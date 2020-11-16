using System;

namespace MassStorageDotNet.Usb
{
    public interface IUsbConnection : IDisposable
    {
        // TODO: Verify against Android interface

        /// <summary>
        /// Indicates the maximum transfer size for a bulk transfer. Buffers larger than
        /// this size may be truncated. A value of 0 indicates there is no cap.
        /// </summary>
        int BulkTransferCap { get; }

        bool ClaimInterface(IUsbInterface usbInterface, bool force);

        bool ReleaseInterface(IUsbInterface usbInterface);

        bool SetConfiguration(IUsbConfiguration configuration);

        bool SetInterface(IUsbInterface usbInterface);

        /// <summary>
        /// Performs a bulk transaction on the given endpoint. The direction of the transfer is determined by the direction of the endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint for this transaction</param>
        /// <param name="buffer">Buffer for data to send or receive</param>
        /// <param name="offset">The index of the first byte in the buffer to send or receive</param>
        /// <param name="length">The length of the data to send or receive. Values larger than <see cref="BulkTransferCap"/> may be truncated.</param>
        /// <param name="timeout">In milliseconds, 0 is infinite</param>
        /// <returns>Length of data transferred (or zero) for success, or negative value for failure</returns>
        int BulkTransfer(IUsbEndpoint endpoint, byte[] buffer, int offset, int length, int timeout);

        /// <summary>
        /// Performs a bulk transaction on the given endpoint. The direction of the transfer is determined by the direction of the endpoint.
        /// This method transfers data starting from index 0 in the buffer.
        /// </summary>
        /// <param name="endpoint">The endpoint for this transaction</param>
        /// <param name="buffer">Buffer for data to send or receive</param>
        /// <param name="length">The length of the data to send or receive. Values larger than <see cref="BulkTransferCap"/> may be truncated.</param>
        /// <param name="timeout">In milliseconds, 0 is infinite</param>
        /// <returns>Length of data transferred (or zero) for success, or negative value for failure</returns>
        int BulkTransfer(IUsbEndpoint endpoint, byte[] buffer, int length, int timeout);

        /// <summary>
        /// Performs a control transaction on endpoint zero for this device. The direction of the transfer is determined by the request type.
        /// </summary>
        /// <param name="requestType">Request type for this transaction</param>
        /// <param name="request">Request ID for this transaction</param>
        /// <param name="value">Value field for this transaction</param>
        /// <param name="index">Index field for this transaction</param>
        /// <param name="buffer">Buffer for data portion of transaction, or null if no data needs to be sent or received</param>
        /// <param name="offset">The index of the first byte in the buffer to send or receive</param>
        /// <param name="length">The length of the data to send or receive</param>
        /// <param name="timeout">In milliseconds</param>
        /// <returns>Length of data transferred (or zero) for success, or negative value for failure</returns>
        int ControlTransfer(
            byte requestType,
            byte request,
            ushort value,
            ushort index,
            byte[]? buffer,
            int offset,
            ushort length,
            int timeout);

        /// <summary>
        /// Performs a control transaction on endpoint zero for this device. The direction of the transfer is determined by the request type.
        /// This method transfers data starting from index 0 in the buffer.
        /// </summary>
        /// <param name="requestType">Request type for this transaction</param>
        /// <param name="request">Request ID for this transaction</param>
        /// <param name="value">Value field for this transaction</param>
        /// <param name="index">Index field for this transaction</param>
        /// <param name="buffer">Buffer for data portion of transaction, or null if no data needs to be sent or received</param>
        /// <param name="length">The length of the data to send or receive</param>
        /// <param name="timeout">In milliseconds</param>
        /// <returns>Length of data transferred (or zero) for success, or negative value for failure</returns>
        int ControlTransfer(
            byte requestType,
            byte request,
            ushort value,
            ushort index,
            byte[]? buffer,
            ushort length,
            int timeout);

        bool ClearStall(IUsbEndpoint endpoint);
    }
}
