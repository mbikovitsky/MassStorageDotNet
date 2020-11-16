using System;
using LibUsbWrapper;
using MassStorageDotNet.Usb;
using TimeoutException = MassStorageDotNet.Usb.TimeoutException;

namespace TestApp
{
    internal sealed class UsbConnectionAdapter : IUsbConnection
    {
        private readonly UsbDeviceConnection _connection;

        private UsbConnectionAdapter(UsbDeviceConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public static UsbConnectionAdapter Create(UsbDeviceConnection connection)
        {
            try
            {
                return new UsbConnectionAdapter(connection);
            }
            catch
            {
                connection?.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public int BulkTransferCap => 0;

        public bool ClaimInterface(IUsbInterface usbInterface, bool force)
        {
            try
            {
                _connection.ClaimInterface(usbInterface.InterfaceNumber);
            }
            catch (LibUsbException)
            {
                return false;
            }

            return true;
        }

        public bool ReleaseInterface(IUsbInterface usbInterface)
        {
            try
            {
                _connection.ReleaseInterface(usbInterface.InterfaceNumber);
            }
            catch (LibUsbException)
            {
                return false;
            }

            return true;
        }

        public bool SetConfiguration(IUsbConfiguration configuration)
        {
            try
            {
                _connection.SetConfiguration(configuration.Id);
            }
            catch (LibUsbException)
            {
                return false;
            }

            return true;
        }

        public bool SetInterface(IUsbInterface usbInterface)
        {
            try
            {
                _connection.SetInterface(usbInterface.InterfaceNumber, usbInterface.AlternateSetting);
            }
            catch (LibUsbException)
            {
                return false;
            }

            return true;
        }

        public int BulkTransfer(
            IUsbEndpoint endpoint,
            byte[] buffer,
            int offset,
            int length,
            int timeout)
        {
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), timeout, null);
            }

            int transferred;
            try
            {
                transferred =
                    _connection.BulkTransfer(endpoint.EndpointAddress, buffer, offset, length, (uint) timeout);
            }
            catch (LibUsbException exception) when (exception.Error == LibUsbError.Pipe)
            {
                throw new StallException("Bulk endpoint stalled", exception);
            }
            catch (LibUsbException exception) when (exception.Error == LibUsbError.Timeout)
            {
                throw new TimeoutException("Bulk transfer timed-out", exception);
            }
            catch (Exception exception)
            {
                throw new UsbException("Bulk transfer failed", exception);
            }

            return transferred;
        }

        public int BulkTransfer(IUsbEndpoint endpoint, byte[] buffer, int length, int timeout)
        {
            return BulkTransfer(endpoint, buffer, 0, length, timeout);
        }

        public int ControlTransfer(
            byte requestType,
            byte request,
            ushort value,
            ushort index,
            byte[]? buffer,
            int offset,
            ushort length,
            int timeout)
        {
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), timeout, null);
            }

            int transferred;
            try
            {
                transferred =
                    _connection.ControlTransfer(requestType, request, value, index, buffer, offset, length, (uint) timeout);
            }
            catch (LibUsbException exception) when (exception.Error == LibUsbError.Pipe)
            {
                throw new StallException("Control endpoint stalled", exception);
            }
            catch (LibUsbException exception) when (exception.Error == LibUsbError.Timeout)
            {
                throw new TimeoutException("Control transfer timed-out", exception);
            }
            catch (Exception exception)
            {
                throw new UsbException("Control transfer failed", exception);
            }

            return transferred;
        }

        public int ControlTransfer(
            byte requestType,
            byte request,
            ushort value,
            ushort index,
            byte[]? buffer,
            ushort length,
            int timeout)
        {
            return ControlTransfer(requestType, request, value, index, buffer, 0, length, timeout);
        }

        public bool ClearStall(IUsbEndpoint endpoint)
        {
            try
            {
                _connection.ClearStall(endpoint.EndpointAddress);
            }
            catch (LibUsbException)
            {
                return false;
            }

            return true;
        }
    }
}
