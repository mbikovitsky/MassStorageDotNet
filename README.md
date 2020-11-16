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


Licensing information:
- All source code in this repository is licensed under the [MIT license](LICENSE).
- The included `libusb-1.0.dll` binary is part of the [libusb] project, which is
  licensed under the [LGPL-2.1](LICENSE.libusb).


[DiscUtils]: https://github.com/DiscUtils/DiscUtils
[Dokan]: https://dokan-dev.github.io/
[libusb]: https://libusb.info/
