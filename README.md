# MassStorageDotNet
USB mass storage implementation for .NET.

Repository contents:
- `MassStorageDotNet` - A .NET Standard implementing the USB mass storage protocol
                        (host side).
- `ThreadSafeDiscUtils` - A .NET Standard thread-safe wrapper over the [DiscUtils] 
                          library.
- `DokanDiscUtils` - .NET Framework library implementing the [Dokan] interface using
                     [DiscUtils].
- `LibUsbWrapper` - .NET Framework wrapper for the Windows version of [libusb].
- `TestApp` - Helper application capable of:
    - Mounting a mass storage device with the help of [Dokan], [DiscUtils],
      and the `MassStorageDotNet` library.
    - Creating a dump of a mass storage device.
    - Reading and writing individual files on a mass storage device.
- `Utils` - Various utility functions.


[DiscUtils]: https://github.com/DiscUtils/DiscUtils
[Dokan]: https://dokan-dev.github.io/
[libusb]: https://libusb.info/
