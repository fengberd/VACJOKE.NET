using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

using static NameChanger.NTAPI;

namespace NameChanger
{
    public static class Utils
    {
        public static byte[] zeroInt = new byte[] { 0x00,0x00,0x00,0x00 };
        public static IntPtr cvarTable, nameCVar, dwClientCMD;
        public static CharCodeTable charcodeTable;

        public static byte[] ReadMemory(IntPtr hProcess,IntPtr address,int length)
        {
            byte[] data = new byte[length];
            if(!ReadProcessMemory(hProcess,address,data,data.Length,out IntPtr unused))
            {
                return null;
            }
            return data;
        }

        public static T ReadMemory<T>(IntPtr hProcess,IntPtr address)
        {
            var data = ReadMemory(hProcess,address,Marshal.SizeOf(typeof(T)));
            if(data == null)
            {
                return default(T);
            }
            var handle = GCHandle.Alloc(data,GCHandleType.Pinned);
            var result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),typeof(T));
            handle.Free();
            return result;
        }

        public static string ReadText(IntPtr hProcess,IntPtr address)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                int offset = 0;
                byte read;
                while((read = ReadMemory(hProcess,address + offset,1)[0]) != 0)
                {
                    ms.WriteByte(read);
                    offset++;
                }
                var data = ms.ToArray();
                return Encoding.UTF8.GetString(data,0,data.Length);
            }
        }

        public static string LoadData(Process process)
        {
            if(process == null || process.HasExited)
            {
                return null;
            }
            IntPtr hProcess = process.Handle;
            ProcessModule engineDll = null, vstdlibDll = null;
            foreach(ProcessModule module in process.Modules)
            {
                switch(Path.GetFileName(module.FileName))
                {
                case "engine.dll":
                    engineDll = module;
                    continue;
                case "vstdlib.dll":
                    vstdlibDll = module;
                    continue;
                }
            }
            if(engineDll == null)
            {
                return "Failed to get base address of engine.dll";
            }
            if(vstdlibDll == null)
            {
                return "Failed to get base address of vstdlib.dll";
            }
            var scanner = new SigScan(hProcess);
            // Find dwClientCMD
            if(!scanner.SelectModule(engineDll.BaseAddress,engineDll.ModuleMemorySize))
            {
                return "Failed to select engine.dll";
            }
            int scan = (int)scanner.FindPattern("55 8B EC A1 ? ? ? ? 33 C9 8B 55 08",out long time);
            if(scan == 0)
            {
                return "Can't find dwClientCMD";
            }
            dwClientCMD = new IntPtr(scan);
            // Find cvar chars table
            if(!scanner.SelectModule(vstdlibDll.BaseAddress,vstdlibDll.ModuleMemorySize))
            {
                return "Failed to select vstdlib.dll";
            }
            scan = (int)scanner.FindPattern("8B 3C 85",out time);
            if(scan == 0)
            {
                return "Can't find chars table";
            }
            try
            {
                charcodeTable = ReadMemory<CharCodeTable>(process.Handle,ReadMemory<IntPtr>(hProcess,new IntPtr(scan + 3)));
            }
            catch
            {
                return "Failed to read chars table address(0x" + scan.ToString("X") + ")";
            }
            // Find cvars table
            scan = (int)scanner.FindPattern("8B 0D ? ? ? ? C7 05",out time);
            if(scan == 0)
            {
                return "Failed to scan cvars table";
            }
            cvarTable = ReadMemory<IntPtr>(hProcess,ReadMemory<IntPtr>(hProcess,new IntPtr(scan + 2)));
            nameCVar = GetCVarAddress(hProcess,"name");
            if(nameCVar == IntPtr.Zero)
            {
                return "Failed to get name cvar";
            }
            return null;
        }

        public static IntPtr GetCVarAddress(IntPtr hProcess,string name)
        {
            int someMagicVariable1 = 0, someMagicVariable2 = 0;
            for(int i = 0;i < name.Length;i += 2)
            {
                someMagicVariable2 = charcodeTable.table[someMagicVariable1 ^ char.ToUpper(name[i])];
                if(i + 1 == name.Length)
                {
                    break;
                }
                someMagicVariable1 = charcodeTable.table[someMagicVariable2 ^ char.ToUpper(name[i + 1])];
            }
            int hash = someMagicVariable1 | (someMagicVariable2 << 8);
            IntPtr pointer = ReadMemory<IntPtr>(hProcess,ReadMemory<IntPtr>(hProcess,cvarTable + 0x34) + ((byte)hash * 4));
            while(pointer != IntPtr.Zero)
            {
                if(ReadMemory<int>(hProcess,pointer) == hash)
                {
                    IntPtr cvarPointer = ReadMemory<IntPtr>(hProcess,pointer + 0x4);
                    if(ReadText(hProcess,ReadMemory<IntPtr>(hProcess,cvarPointer + 0xC)) == name)
                    {
                        return cvarPointer;
                    }
                }
                pointer = ReadMemory<IntPtr>(hProcess,pointer + 0xC);
            }
            return IntPtr.Zero;
        }

        public static string SetName(Process process,string name)
        {
            if(process == null || process.HasExited)
            {
                return null;
            }
            IntPtr hProcess = process.Handle;
            WriteProcessMemory(hProcess,nameCVar + 0x44 + 0xC,zeroInt,zeroInt.Length,out int unused);

            byte[] cmd = Encoding.UTF8.GetBytes("name \"" + name + "\"");
            IntPtr parameter = VirtualAllocEx(hProcess,IntPtr.Zero,(uint)cmd.Length,AllocationType.Commit | AllocationType.Reserve,MemoryProtection.ExecuteReadWrite);
            WriteProcessMemory(hProcess,parameter,cmd,cmd.Length,out unused);

            IntPtr hThread = CreateRemoteThread(hProcess,IntPtr.Zero,0,dwClientCMD,parameter,0,out IntPtr unused2);
            WaitForSingleObject(hThread,INFINITE);

            VirtualFreeEx(hProcess,parameter,0,AllocationType.Release);
            return null;
        }

        [StructLayout(LayoutKind.Sequential,Pack = 1)]
        public struct CharCodeTable
        {
            [MarshalAs(UnmanagedType.ByValArray,SizeConst = 255)]
            public int[] table;
        };
    }
}
