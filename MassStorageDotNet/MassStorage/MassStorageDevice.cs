using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using MassStorageDotNet.Scsi;
using MassStorageDotNet.Usb;
using Polly;
using Utils;

namespace MassStorageDotNet.MassStorage
{
    public sealed class MassStorageDevice : IDisposable
    {
        private const int Timeout = 1000;
        private const int ReadWriteRetryCount = 5;

        private delegate byte[] CreateTransferCdb(uint logicalBlockAddress, ushort blocks, out bool deviceToHost);

        private delegate byte[] CreateExtendedTransferCdb(ulong logicalBlockAddress, uint blocks, out bool deviceToHost);

        private bool _disposed;
        private readonly bool _writable;
        private readonly IUsbConnection _connection;
        private readonly IUsbInterface _interface;
        private readonly IUsbEndpoint _inEndpoint;
        private readonly IUsbEndpoint _outEndpoint;
        private readonly Lazy<CapacityData> _capacity;
        private readonly Lazy<byte> _maxLun;
        private readonly byte[] _cswBuffer = new byte[Marshal.SizeOf<CommandStatusWrapper>()];
        private readonly byte[] _senseBuffer = new byte[Marshal.SizeOf<ScsiSenseFixed>()];

        private struct CapacityData
        {
            public ulong LastLogicalBlockAddress;
            public uint BlockSize;
        }

        private struct ScsiSense
        {
            public byte SenseKey;
            public byte AdditionalSenseCode;
            public byte AdditionalSenseCodeQualifier;
        }

        private MassStorageDevice(IUsbConnection connection, MassStorageDeviceInfo deviceInfo, bool writable)
        {
            _writable = writable;
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));

            if (!_connection.SetConfiguration(deviceInfo.Configuration))
            {
                // TODO: better exception
                throw new Exception("Can't set device configuration");
            }

            _interface = deviceInfo.Interface;
            if (!_connection.ClaimInterface(_interface, true))
            {
                // TODO: better exception
                throw new Exception("Can't claim interface");
            }

            if (!_connection.SetInterface(_interface))
            {
                // TODO: better exception
                throw new Exception("Can't set interface alt setting");
            }

            _inEndpoint = deviceInfo.BulkInEndpoint;
            _outEndpoint = deviceInfo.BulkOutEndpoint;

            _capacity = new Lazy<CapacityData>(GetCapacity, LazyThreadSafetyMode.ExecutionAndPublication);
            _maxLun = new Lazy<byte>(GetMaxLun, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        internal static MassStorageDevice Create(IUsbConnection connection, MassStorageDeviceInfo deviceInfo, bool writable)
        {
            try
            {
                return new MassStorageDevice(connection, deviceInfo, writable);
            }
            catch
            {
                connection?.Dispose();
                throw;
            }
        }

        public ulong NumberOfBlocks => _capacity.Value.LastLogicalBlockAddress + 1;

        public uint BlockSize => _capacity.Value.BlockSize;

        public byte MaxLun => _maxLun.Value;

        public bool ReadOnly => !_writable;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _connection.ReleaseInterface(_interface);
            _connection.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Reads logical blocks from the device.
        /// </summary>
        /// <param name="logicalBlockAddress">LBA to start reading from</param>
        /// <param name="destination">Buffer to fill with the blocks</param>
        /// <param name="offset">Offset in the buffer to write the blocks to</param>
        /// <param name="numberOfBlocks">Number of blocks to read</param>
        /// <returns>Number of blocks actually read</returns>
        public uint ReadBlocks(ulong logicalBlockAddress, byte[] destination, int offset,
            uint numberOfBlocks = 1)
        {
            static byte[] CreateTransferCdb(uint address, ushort blocks, out bool deviceToHost)
            {
                deviceToHost = true;
                var read10Cdb = new Read10Cdb {LogicalBlockAddress = address, TransferLength = blocks};
                return Native.StructureToBytes(read10Cdb);
            }

            static byte[] CreateExtendedTransferCdb(ulong address, uint blocks, out bool deviceToHost)
            {
                deviceToHost = true;
                var read16Cdb = new Read16Cdb {LogicalBlockAddress = address, TransferLength = blocks};
                return Native.StructureToBytes(read16Cdb);
            }

            return ReadWriteBlocks(
                CreateTransferCdb,
                CreateExtendedTransferCdb,
                logicalBlockAddress,
                destination,
                offset,
                numberOfBlocks);
        }

        public uint WriteBlocks(ulong logicalBlockAddress, byte[] source, int offset,
            uint numberOfBlocks = 1)
        {
            static byte[] CreateTransferCdb(uint address, ushort blocks, out bool deviceToHost)
            {
                deviceToHost = false;
                var read10Cdb = new Write10Cdb { LogicalBlockAddress = address, TransferLength = blocks };
                return Native.StructureToBytes(read10Cdb);
            }

            static byte[] CreateExtendedTransferCdb(ulong address, uint blocks, out bool deviceToHost)
            {
                deviceToHost = false;
                var read16Cdb = new Write16Cdb { LogicalBlockAddress = address, TransferLength = blocks };
                return Native.StructureToBytes(read16Cdb);
            }

            return ReadWriteBlocks(
                CreateTransferCdb,
                CreateExtendedTransferCdb,
                logicalBlockAddress,
                source,
                offset,
                numberOfBlocks);
        }

        private CapacityData GetCapacity()
        {
            EnsureNotDisposed();

            var read10Result = SendCommand<ReadCapacity10Cdb, ReadCapacity10CdbResult>(new ReadCapacity10Cdb());
            if (read10Result.BlockLength != uint.MaxValue)
            {
                return new CapacityData
                {
                    LastLogicalBlockAddress = read10Result.LogicalBlockAddress,
                    BlockSize = read10Result.BlockLength
                };
            }

            var read16Result = SendCommand<ReadCapacity16Cdb, ReadCapacity16CdbResult>(new ReadCapacity16Cdb
            { AllocationLength = (uint)Marshal.SizeOf<ReadCapacity16CdbResult>() });
            return new CapacityData
            {
                LastLogicalBlockAddress = read16Result.LogicalBlockAddress,
                BlockSize = read16Result.BlockLength
            };
        }

        private byte GetMaxLun()
        {
            EnsureNotDisposed();

            var result = new byte[1];
            int transferred;
            try
            {
                transferred = _connection.ControlTransfer(
                    0b10100001,
                    0xFE,
                    0,
                    _interface.InterfaceNumber,
                    result,
                    0,
                    (ushort) result.Length,
                    Timeout);
            }
            catch (StallException)
            {
                // Some devices send a STALL instead of the actual value.
                // In such cases we should set lun to 0.
                // (See xusb.c in libusb samples.)
                return 0;
            }
            if (transferred != result.Length)
            {
                // TODO: Better exception
                throw new Exception("Control transfer failed");
            }
            return result[0];
        }

        private uint ReadWriteBlocks(
            CreateTransferCdb createTransferCdb,
            CreateExtendedTransferCdb createExtendedTransferCdb,
            ulong logicalBlockAddress,
            byte[] buffer,
            int offset,
            uint numberOfBlocks)
        {
            EnsureNotDisposed();

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
            }

            if (numberOfBlocks == 0)
            {
                return 0;
            }

            var transferLength = numberOfBlocks * BlockSize;
            if (offset + transferLength > buffer.Length)
            {
                throw new ArgumentException("Buffer too small", nameof(buffer));
            }

            if (_connection.BulkTransferCap > 0 && transferLength > _connection.BulkTransferCap)
            {
                var blocksChunk = (uint) _connection.BulkTransferCap / BlockSize;
                uint totalTransferred = 0;
                while (numberOfBlocks > 0 && logicalBlockAddress < NumberOfBlocks)
                {
                    uint transferredBlocks;
                    try
                    {
                        transferredBlocks = ReadWriteBlocks(
                            createTransferCdb, createExtendedTransferCdb,
                            logicalBlockAddress, buffer, offset,
                            Math.Min(blocksChunk, numberOfBlocks));
                    }
                    catch (MassStorageException)
                    {
                        if (totalTransferred == 0)
                        {
                            throw;
                        }

                        return totalTransferred;
                    }

                    numberOfBlocks -= transferredBlocks;
                    logicalBlockAddress += transferredBlocks;
                    totalTransferred += transferredBlocks;
                    offset += (int) (transferredBlocks * BlockSize);
                }

                return totalTransferred;
            }

            byte[] cdb;
            bool deviceToHost;
            if (logicalBlockAddress > uint.MaxValue || numberOfBlocks > ushort.MaxValue)
            {
                cdb = createExtendedTransferCdb(logicalBlockAddress, numberOfBlocks, out deviceToHost);
            }
            else
            {
                cdb = createTransferCdb((uint) logicalBlockAddress, (ushort) numberOfBlocks, out deviceToHost);
            }

            //
            // Send command and retry if LOGICAL UNIT NOT READY received.
            //

            uint Action()
            {
                var transferred = SendCommand(cdb, buffer, offset, (int) transferLength, deviceToHost);
                return (uint) (transferred / BlockSize);
            }

            return Policy
                .Handle<ScsiException>(exception => exception.Key == 0x0B && exception.Code == 0x04)
                .Retry(ReadWriteRetryCount)
                .Execute(Action);
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MassStorageDevice));
            }
        }

        private TResult SendCommand<TCommand, TResult>(TCommand command, bool requestSense = true) where TCommand : ICdb
        {
            var bytes = new byte[Marshal.SizeOf<TResult>()];
            if (SendCommand(command, bytes, 0, bytes.Length, true, requestSense) < bytes.Length)
            {
                // TODO: Better exception
                throw new Exception("Command failed");
            }

            return Native.BytesToStructure<TResult>(bytes);
        }

        private int SendCommand<T>(T command, byte[] buffer, int offset, int length, bool deviceToHost,
            bool requestSense = true) where T : ICdb
        {
            return SendCommand(Native.StructureToBytes(command), buffer, offset, length, deviceToHost, requestSense);
        }

        private int SendCommand(byte[] command, byte[]? buffer, int offset, int length,
            bool deviceToHost, bool requestSense = true)
        {
            if (null != buffer)
            {
                Params.ValidateBuffer(buffer, offset, length);
            }

            var cbw = new CommandBlockWrapper
            {
                Flags =
                    deviceToHost
                    ? CommandBlockWrapperFlags.DeviceToHost
                    : CommandBlockWrapperFlags.HostToDevice,
                CommandBlock = command,
                DataTransferLength = (uint) length
            };

            var cbwBytes = Native.StructureToBytes(cbw);

            try
            {
                BulkTransferExact(_outEndpoint, cbwBytes);
            }
            catch (Exception exception)
            {
                ResetRecovery();
                throw new MassStorageException("CBW transfer failed", exception);
            }

            int transferred = 0;
            if (buffer != null)
            {
                var endpoint = deviceToHost ? _inEndpoint : _outEndpoint;
                try
                {
                    transferred = BulkTransfer(endpoint, buffer, offset, length);
                }
                catch (StallException)
                {
                    _connection.ClearStall(endpoint);
                }
                catch (Exception exception)
                {
                    ResetRecovery();
                    throw new MassStorageException("Data transfer failed", exception);
                }
            }

            var status = ReadStatus(cbw.Tag);
            switch (status)
            {
                case CommandStatus.Success:
                    return transferred;

                case CommandStatus.PhaseError:
                    throw new MassStorageException("Phase error");

                case CommandStatus.Failure:
                default:
                    // TODO: Can it be anything else? Do we care?
                    Debug.Assert(status == CommandStatus.Failure);
                    break;
            }

            if (!requestSense)
            {
                throw new MassStorageException("Command failed. No sense requested.");
            }

            ScsiSense sense;
            try
            {
                sense = RequestSense();
            }
            catch (Exception exception)
            {
                throw new MassStorageException("Command failed. Sense retrieval failed.", exception);
            }

            throw new ScsiException(sense.SenseKey, sense.AdditionalSenseCode, sense.AdditionalSenseCodeQualifier);
        }

        private CommandStatus ReadStatus(uint expectedTag)
        {
            try
            {
                BulkTransferExact(_inEndpoint, _cswBuffer);
            }
            catch (UsbException)
            {
                _connection.ClearStall(_inEndpoint);
                try
                {
                    BulkTransferExact(_inEndpoint, _cswBuffer);
                }
                catch (Exception exception)
                {
                    ResetRecovery();
                    throw new MassStorageException("Could not read status", exception);
                }
            }
            catch
            {
                ResetRecovery();
                throw;
            }

            var csw = Native.BytesToStructure<CommandStatusWrapper>(_cswBuffer);
            if (!csw.IsValid(expectedTag))
            {
                ResetRecovery();
                throw new MassStorageException("Could not read status");
            }
            else if (csw.Status == CommandStatus.PhaseError)
            {
                ResetRecovery();
            }

            return csw.Status;
        }

        private ScsiSense RequestSense()
        {
            var cdb = new RequestSenseCdb
            {
                AllocationLength = (byte) _senseBuffer.Length
            };

            // Read sense data from the device. We don't actually care about the returned
            // size, since we can unmarshal either way. If the device returns a garbled sense,
            // there's not much we can do about it, since we're already using this function for
            // error handling. On total failure, SendCommand throws.
            SendCommand(cdb, _senseBuffer, 0, _senseBuffer.Length, true, false);

            var sense = Native.BytesToStructure<ScsiSenseFixed>(_senseBuffer);

            return new ScsiSense
            {
                SenseKey = sense.SenseKey,
                AdditionalSenseCode = sense.AdditionalSenseCode,
                AdditionalSenseCodeQualifier = sense.AdditionalSenseCodeQualifier
            };
        }

        private void BulkTransferExact(IUsbEndpoint endpoint, byte[] buffer)
        {
            BulkTransferExact(endpoint, buffer, 0, buffer.Length);
        }

        private void BulkTransferExact(IUsbEndpoint endpoint, byte[] buffer, int offset, int length)
        {
            var transferred = BulkTransfer(endpoint, buffer, offset, length);
            if (transferred != length)
            {
                // TODO: better exception
                throw new IOException("Partial transfer");
            }
        }

        private int BulkTransfer(IUsbEndpoint endpoint, byte[] buffer, int offset, int length)
        {
            Debug.Assert(_connection.BulkTransferCap <= 0 || length <= _connection.BulkTransferCap);
            return _connection.BulkTransfer(endpoint, buffer, offset, length, Timeout);
        }

        private void ResetRecovery()
        {
            BulkOnlyMassStorageReset();
            _connection.ClearStall(_inEndpoint);
            _connection.ClearStall(_outEndpoint);
        }

        private void BulkOnlyMassStorageReset()
        {
            _connection.ControlTransfer(
                0b00100001,
                0xFF,
                0,
                _interface.InterfaceNumber,
                null,
                0,
                Timeout);
        }
    }
}
