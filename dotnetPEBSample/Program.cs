using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KernelStructOffset;

namespace dotnetPEBSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Install-Package KernelStructOffset

            IntPtr pebAddress = EnvironmentBlockInfo.GetPebAddress(out IntPtr tebAddress);

            var peb = EnvironmentBlockInfo.GetPeb(pebAddress);

            _PEB_LDR_DATA ldrData = _PEB_LDR_DATA.Create(peb.Ldr);
            IntPtr memoryOrderLink = ldrData.InMemoryOrderModuleList.Flink;

            Console.WriteLine("InMemoryOrderModuleList: " + memoryOrderLink.ToString("x"));

            _LDR_DATA_TABLE_ENTRY item = _LDR_DATA_TABLE_ENTRY.Create(memoryOrderLink);

            while (true)
            {
                string fullDllName = item.FullDllName.GetText();
                Console.WriteLine(fullDllName);

                if (item.InMemoryOrderLinks.Flink == memoryOrderLink)
                {
                    break;
                }

                item = _LDR_DATA_TABLE_ENTRY.Create(item.InMemoryOrderLinks.Flink);
            }

            Console.ReadKey();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct _PEB_LDR_DATA
    {
        public fixed byte Reserved1[8];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public IntPtr[] Reserved2;
        public _LIST_ENTRY InMemoryOrderModuleList;

        public static _PEB_LDR_DATA Create(IntPtr ldrAddress)
        {
            _PEB_LDR_DATA ldrData = (_PEB_LDR_DATA)Marshal.PtrToStructure(ldrAddress, typeof(_PEB_LDR_DATA));
            return ldrData;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct _LDR_DATA_TABLE_ENTRY
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public IntPtr[] Reserved1;
        public _LIST_ENTRY InMemoryOrderLinks;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public IntPtr[] Reserved2;
        public IntPtr DllBase;
        public IntPtr EntryPoint;
        public IntPtr SizeOfImage;
        public _UNICODE_STRING FullDllName;
        public fixed byte Reserved4[8];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public IntPtr[] Reserved5;
        public IntPtr Reserved6;
        public uint TimeDateStamp;

        public static _LDR_DATA_TABLE_ENTRY Create(IntPtr memoryOrderLink)
        {
            IntPtr head = memoryOrderLink - Marshal.SizeOf(typeof(_LIST_ENTRY));

            _LDR_DATA_TABLE_ENTRY entry = (_LDR_DATA_TABLE_ENTRY)Marshal.PtrToStructure(
                head, typeof(_LDR_DATA_TABLE_ENTRY));

            return entry;
        }
    }
}
