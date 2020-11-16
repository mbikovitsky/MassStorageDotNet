using System;
using System.Diagnostics;
using System.IO;
using Utils;

namespace MassStorageDotNet.MassStorage
{
    public class MassStorageStream : Stream
    {
        private readonly MassStorageDevice _massStorageDevice;
        private long _position;

        private delegate bool TransferSingleBlock(
            byte[] block, int blockOffset, ulong logicalBlockAddress,
            byte[] buffer, int bufferOffset, int count);

        private delegate uint TransferMultipleBlocks(
            ulong logicalBlockAddress,
            byte[] buffer, int offset,
            uint numberOfBlocks);

        private MassStorageStream(MassStorageDevice massStorageDevice)
        {
            _massStorageDevice = massStorageDevice;
        }

        public static MassStorageStream Create(MassStorageDevice massStorageDevice)
        {
            try
            {
                return new MassStorageStream(massStorageDevice);
            }
            catch
            {
                massStorageDevice?.Dispose();
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _massStorageDevice.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            // TODO: Can we even flush?
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
            {
                throw new NotSupportedException();
            }

            static bool TransferSingleBlock(
                byte[] block, int blockOffset, ulong logicalBlockAddress,
                byte[] buffer, int bufferOffset, int count)
            {
                Params.ValidateBuffer(block, blockOffset, count);
                Params.ValidateBuffer(buffer, bufferOffset, count);

                // Copy the required portion from the block
                Array.Copy(
                    block,
                    blockOffset,
                    buffer,
                    bufferOffset,
                    count);

                return true;
            }

            var read = ReadWrite(buffer, offset, count, TransferSingleBlock, MassStorageDevice.ReadBlocks);
            Position += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var newPosition = origin switch
            {
                SeekOrigin.Begin => offset,

                SeekOrigin.Current => _position + offset,

                SeekOrigin.End => Length - 1 + offset,

                _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null),
            };
            if (newPosition < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
            }

            _position = newPosition;
            return newPosition;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
            {
                throw new NotSupportedException();
            }

            bool TransferSingleBlock(
                byte[] block, int blockOffset, ulong logicalBlockAddress,
                byte[] buffer, int bufferOffset, int count)
            {
                Params.ValidateBuffer(block, blockOffset, count);
                Params.ValidateBuffer(buffer, bufferOffset, count);

                // Copy the required portion into the block
                Array.Copy(
                    buffer,
                    bufferOffset,
                    block,
                    blockOffset,
                    count);

                // Write it back
                return MassStorageDevice.WriteBlocks(logicalBlockAddress, block, 0) == 1;
            }

            var written = ReadWrite(buffer, offset, count, TransferSingleBlock, MassStorageDevice.WriteBlocks);
            Position += written;
        }

        public override bool CanRead => true;  // TODO: Are there non-readable devices?

        public override bool CanSeek => true;

        public override bool CanWrite => !MassStorageDevice.ReadOnly;

        public override long Length => checked((long) (MassStorageDevice.NumberOfBlocks * BlockSize));

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public uint BlockSize => MassStorageDevice.BlockSize;

        private MassStorageDevice MassStorageDevice => _massStorageDevice ?? throw new ObjectDisposedException(nameof(MassStorageStream));

        private int ReadWrite(byte[] buffer, int offset, int count, TransferSingleBlock transferSingleBlock, TransferMultipleBlocks transferMultipleBlocks)
        {
            Params.ValidateBuffer(buffer, offset, count);

            if (Position >= Length || count == 0)
            {
                return 0;
            }

            int totalTransferred = 0;

            CalculateBlockSpan(ref count, out var startLba, out var endLba);

            var block = new byte[BlockSize];

            // Read the first block
            if (MassStorageDevice.ReadBlocks(startLba, block, 0) != 1)
            {
                return 0;
            }

            // TODO: Overflow check?
            var offsetInFirstBlock = (int)(Position % BlockSize);
            var bytesFromFirstBlock = (int)Math.Min(BlockSize - offsetInFirstBlock, count);

            // Transfer the required portion of the first block
            if (!transferSingleBlock(block, offsetInFirstBlock, startLba, buffer, offset, bytesFromFirstBlock))
            {
                return 0;
            }

            totalTransferred += bytesFromFirstBlock;

            // Transfer the intermediate blocks
            var currentOffset = offset + bytesFromFirstBlock;
            var currentLba = startLba + 1;
            while (currentLba < endLba)
            {
                var blocksToRead = (uint)Math.Min(endLba - currentLba, uint.MaxValue);
                var blocksRead = transferMultipleBlocks(currentLba, buffer, currentOffset, blocksToRead);
                var bytesRead = (int)(blocksRead * BlockSize);

                totalTransferred += bytesRead;

                if (blocksRead != blocksToRead)
                {
                    return totalTransferred;
                }

                currentOffset += bytesRead;
                currentLba += blocksRead;
            }

            // Read the last block
            if (MassStorageDevice.ReadBlocks(endLba, block, 0) != 1)
            {
                return totalTransferred;
            }

            // Transfer the required portion of the last block
            var bytesFromLastBlock = count - (currentOffset - offset);
            if (transferSingleBlock(block, 0, endLba, buffer, currentOffset, bytesFromLastBlock))
            {
                totalTransferred += bytesFromLastBlock;
            }

            return totalTransferred;
        }

        private void CalculateBlockSpan(ref int bytesToTransfer, out ulong startLba, out ulong endLba)
        {
            Debug.Assert(bytesToTransfer >= 0);

            if (Length - Position < bytesToTransfer)
            {
                bytesToTransfer = (int) (Length - Position);
            }

            startLba = LbaFromByteAddress((ulong) Position);
            endLba = LbaFromByteAddress((ulong) Position + (uint) bytesToTransfer - 1);

            Debug.Assert(startLba < MassStorageDevice.NumberOfBlocks);
            Debug.Assert(endLba < MassStorageDevice.NumberOfBlocks);
            Debug.Assert(endLba >= startLba);
        }

        private ulong LbaFromByteAddress(ulong byteAddress)
        {
            return byteAddress / BlockSize;
        }
    }
}
