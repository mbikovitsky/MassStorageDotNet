using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CommandLine;
using DiscUtils;
using DiscUtils.Complete;
using DiscUtils.Raw;
using DiscUtils.Streams;
using DokanDiscUtils;
using DokanNet;
using MassStorageDotNet.MassStorage;
using MassStorageDotNet.Usb;
using FileAccess = System.IO.FileAccess;

namespace TestApp
{
    internal static class Program
    {
        private static readonly MassStorageFactory Factory = new MassStorageFactory(new UsbManagerAdapter());

#pragma warning disable CA1812
        [Verb("devices", HelpText = "Get info on connected mass-storage devices.")]
        private class DevicesOptions
        {
        }

        [Verb("dump", HelpText = "Create a dump of a mass-storage device.")]
        private class DumpOptions
        {
            public DumpOptions(string device, string output)
            {
                Device = device;
                Output = output;
            }

            [Option('d', "device",
                Required = true,
                HelpText = "Device to dump")]
            public string Device { get; }

            [Option('o', "output",
                Default = null,
                HelpText = "Output file. Omit to output to stdout.")]
            public string Output { get; }
        }

        [Verb("part", HelpText = "Print partition information of a mass-storage device.")]
        private class PartOptions
        {
            public PartOptions(string device)
            {
                Device = device;
            }

            [Option('d', "device",
                Required = true,
                HelpText = "Device to parse")]
            public string Device { get; }
        }

        [Verb("dirlist", HelpText = "Print a directory listing of a filesystem on a mass-storage device.")]
        private class DirListOptions
        {
            public DirListOptions(string device, int partition, int filesystem)
            {
                Device = device;
                Partition = partition;
                Filesystem = filesystem;
            }

            [Option('d', "device",
                Required = true,
                HelpText = "Device to parse")]
            public string Device { get; }

            [Option('p', "partition",
                Default = 0,
                HelpText = "Partition to parse")]
            public int Partition { get; }

            [Option('f', "filesystem",
                Default = 0,
                HelpText = "Filesystem inside the partition to parse")]
            public int Filesystem { get; }
        }

        [Verb("read", HelpText = "Read a file")]
        private class ReadOptions
        {
            public ReadOptions(string device, int partition, int filesystem, string input, string output)
            {
                Device = device;
                Partition = partition;
                Filesystem = filesystem;
                Input = input;
                Output = output;
            }

            [Option('d', "device",
                Required = true,
                HelpText = "Device to parse")]
            public string Device { get; }

            [Option('p', "partition",
                Default = 0,
                HelpText = "Partition to parse")]
            public int Partition { get; }

            [Option('f', "filesystem",
                Default = 0,
                HelpText = "Filesystem inside the partition to parse")]
            public int Filesystem { get; }

            [Option('i', "input",
                Required = true,
                HelpText = "Full path of the file to read")]
            public string Input { get; }

            [Option('o', "output",
                Default = null,
                HelpText = "Output file. Omit to output to stdout.")]
            public string Output { get; }
        }

        [Verb("write", HelpText = "Write a file")]
        private class WriteOptions
        {
            public WriteOptions(string device, int partition, int filesystem, string input, string output)
            {
                Device = device;
                Partition = partition;
                Filesystem = filesystem;
                Input = input;
                Output = output;
            }

            [Option('d', "device",
                Required = true,
                HelpText = "Device to parse")]
            public string Device { get; }

            [Option('p', "partition",
                Default = 0,
                HelpText = "Partition to parse")]
            public int Partition { get; }

            [Option('f', "filesystem",
                Default = 0,
                HelpText = "Filesystem inside the partition to parse")]
            public int Filesystem { get; }

            [Option('i', "input",
                Default = null,
                HelpText = "Input file. Omit to read from stdin.")]
            public string Input { get; }

            [Option('o', "output",
                Required = true,
                HelpText = "Full path of the file to write")]
            public string Output { get; }
        }

        [Verb("mount", HelpText = "Mount mass-storage as a virtual drive")]
        private class MountOptions
        {
            public MountOptions(string device, int partition, int filesystem, char driveLetter)
            {
                Device = device;
                Partition = partition;
                Filesystem = filesystem;
                DriveLetter = driveLetter;
            }

            [Option('d', "device",
                Required = true,
                HelpText = "Device to parse")]
            public string Device { get; }

            [Option('p', "partition",
                Default = 0,
                HelpText = "Partition to parse")]
            public int Partition { get; }

            [Option('f', "filesystem",
                Default = 0,
                HelpText = "Filesystem inside the partition to parse")]
            public int Filesystem { get; }

            [Option('m', "drive-letter",
                Required = true,
                HelpText = "Drive letter to assign the mounted filesystem")]
            public char DriveLetter { get; }
        }
#pragma warning restore CA1812

        private static void Main(string[] args)
        { 
            SetupHelper.SetupComplete();

            Parser.Default.ParseArguments<DevicesOptions, DumpOptions, PartOptions, DirListOptions, ReadOptions, WriteOptions, MountOptions>(args)
                .WithParsed<DevicesOptions>(HandleDevices)
                .WithParsed<DumpOptions>(HandleDump)
                .WithParsed<PartOptions>(HandlePart)
                .WithParsed<DirListOptions>(HandleDirList)
                .WithParsed<ReadOptions>(HandleRead)
                .WithParsed<WriteOptions>(HandleWrite)
                .WithParsed<MountOptions>(HandleMount);
        }

        private static void HandleDevices(DevicesOptions options)
        {
            foreach (var device in Factory.GetSupportedDevices())
            {
                var address = FormatDeviceAddress((UsbDeviceAdapter)device);
                var vidPid = $"{device.VendorId:X4}:{device.ProductId:X4}";
                Console.WriteLine(
                    $"{address} - {vidPid} - {device.Manufacturer} - {device.ProductName} - {device.SerialNumber}");
            }
        }

        private static void HandleDump(DumpOptions options)
        {
            var device = FindDevice(Factory, options.Device);

            using var output = OpenOutput(options.Output);
            using var massStorage = Factory.Create(device, false);

            var numberOfBlocks = massStorage.NumberOfBlocks;
            var blockSize = massStorage.BlockSize;

            var buffer = new byte[0x1000];

            var blocksToRead = (ulong) (buffer.Length / blockSize);
            if (blocksToRead > uint.MaxValue)
            {
                // Lolwut?
                Console.Error.WriteLine("Falling back to reading block-by-block");
                blocksToRead = 1;
            }

            for (ulong blockAddress = 0; blockAddress < numberOfBlocks; blockAddress += blocksToRead)
            {
                var returned = massStorage.ReadBlocks(blockAddress, buffer, 0, (uint) blocksToRead);
                output.Write(buffer, 0, (int) (returned * massStorage.BlockSize));

                PrintDumpProgress(blockAddress + blocksToRead, numberOfBlocks, blockSize);
            }
        }

        private static void HandlePart(PartOptions options)
        {
            static void PrintPartitions(Disk disk)
            {
                for (int i = 0; i < disk.Partitions.Count; i++)
                {
                    var partition = disk.Partitions[i];
                    Console.WriteLine($"{i}: {partition.TypeAsString} ({partition.FirstSector} - {partition.LastSector})");

                    using var partitionStream = partition.Open();
                    var fileSystems = FileSystemManager.DetectFileSystems(partitionStream);
                    for (int j = 0; j < fileSystems.Length; j++)
                    {
                        Console.WriteLine($"{Indent(1)}{j}: {fileSystems[j].Name} - {fileSystems[j].Description}");
                    }
                }
            }

            WithDisk(options.Device, false, PrintPartitions);
        }

        private static void HandleDirList(DirListOptions options)
        {
            static void DirList(DiscFileSystem fileSystem)
            {
                PrintDirectoryTree(fileSystem.Root, Console.Out);
            }

            WithFilesystem(options.Device, false, options.Partition, options.Filesystem, DirList);
        }

        private static void HandleRead(ReadOptions options)
        {
            void ReadFile(DiscFileSystem fileSystem)
            {
                using var sourceFile = fileSystem.OpenFile(options.Input, FileMode.Open, FileAccess.Read);
                using var destinationFile = OpenOutput(options.Output);
                sourceFile.CopyTo(destinationFile);
            }

            WithFilesystem(options.Device, false, options.Partition, options.Filesystem, ReadFile);
        }

        private static void HandleWrite(WriteOptions options)
        {
            void WriteFile(DiscFileSystem fileSystem)
            {
                using var sourceFile = OpenInput(options.Input);
                using var destinationFile = fileSystem.OpenFile(options.Output, FileMode.Create, FileAccess.Write);
                sourceFile.CopyTo(destinationFile);
            }

            WithFilesystem(options.Device, true, options.Partition, options.Filesystem, WriteFile);
        }

        private static void HandleMount(MountOptions options)
        {
            using var disk = new DiscUtils.Vhd.Disk(@"E:\temp\vhd.vhd", FileAccess.ReadWrite);
            using var partition = disk.Partitions[options.Partition].Open();
            var fileSystems = FileSystemManager.DetectFileSystems(partition);

            var dokanOperations = DokanOperations.Create(fileSystems[options.Filesystem].Open(partition));

            var dokanOptions = DokanOptions.DisableOplocks;
#if DEBUG
            dokanOptions |= DokanOptions.DebugMode | DokanOptions.StderrOutput;
#endif

            dokanOperations.Mount($"{options.DriveLetter}:\\", dokanOptions);
        }

        private static void WithDisk(string deviceId, bool writable, Action<Disk> callback)
        {
            var device = FindDevice(Factory, deviceId);

            using var stream = MassStorageStream.Create(Factory.Create(device, writable));

            var geometry = Geometry.FromCapacity(stream.Length, checked((int) stream.BlockSize));
            using var disk = new Disk(stream, Ownership.None, geometry);

            callback(disk);
        }

        private static void WithFilesystem(
            string deviceId, bool writable,
            int partition, int filesystem,
            Action<DiscFileSystem> callback)
        {
            void OpenFsAndInvoke(Disk disk)
            {
                using var partitionStream = disk.Partitions[partition].Open();

                var fileSystems = FileSystemManager.DetectFileSystems(partitionStream);
                using var fileSystem = fileSystems[filesystem].Open(partitionStream);

                callback(fileSystem);
            }

            WithDisk(deviceId, writable, OpenFsAndInvoke);
        }

        private static string FormatDeviceAddress(UsbDeviceAdapter device)
        {
            return string.Join(".", device.PortNumbers.Select(b => b.ToString(CultureInfo.InvariantCulture)));
        }

        private static byte[] ParseDeviceAddress(string address)
        {
            return address.Split('.').Select(s => Convert.ToByte(s, CultureInfo.InvariantCulture)).ToArray();
        }

        private static IUsbDevice FindDevice(MassStorageFactory factory, string address)
        {
            var portNumbers = ParseDeviceAddress(address);
            var device = factory.GetSupportedDevices().Single(usbDevice =>
                ((UsbDeviceAdapter) usbDevice).PortNumbers.SequenceEqual(portNumbers));
            return device;
        }

        private static Stream OpenOutput(string? filename)
        {
            return null == filename
                ? Console.OpenStandardOutput()
                : new FileStream(filename, FileMode.Create, FileAccess.Write);
        }

        private static Stream OpenInput(string? filename)
        {
            return null == filename
                ? Console.OpenStandardInput()
                : new FileStream(filename, FileMode.Open, FileAccess.Read);
        }

        private static void PrintDumpProgress(ulong blocksRead, ulong totalBlocks, uint blockSize)
        {
            if (!Console.IsErrorRedirected)
            {
                Console.Error.Write(
                    $"\rRead {blocksRead}/{totalBlocks} blocks ({blocksRead * blockSize}/{totalBlocks * blockSize} bytes)");
            }
        }

        private static void PrintDirectoryTree(DiscDirectoryInfo directory, TextWriter outputWriter, int indent = 0)
        {
            foreach (var fileInfo in directory.GetFiles())
            {
                outputWriter.WriteLine($"{Indent(indent)}{fileInfo.Name}");
            }

            foreach (var directoryInfo in directory.GetDirectories())
            {
                outputWriter.WriteLine($"{Indent(indent)}{directoryInfo.Name}");
                PrintDirectoryTree(directoryInfo, outputWriter, indent + 1);
            }
        }

        private static string Indent(int count)
        {
            return new string(' ', count * 4);
        }
    }
}
