using System;
using System.Text;
using System.Runtime.InteropServices;

namespace ClantagChanger
{
    public static class NTAPI
    {
        public const int NT_SUCCESS = 0;

        public static uint STATUS_SUCCESS = 0x00000000;
        public static uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;

        public delegate bool CallBack(int hwnd,int lParam);

        #region Signatures

        #region kernel32.dll

        [DllImport("kernel32.dll",SetLastError = true,ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess,IntPtr lpAddress,uint dwSize,AllocationType flAllocationType,MemoryProtection flProtect);

        [DllImport("kernel32.dll",SetLastError = true,ExactSpelling = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess,IntPtr lpAddress,int dwSize,AllocationType dwFreeType);

        [DllImport("kernel32.dll",SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess,IntPtr lpBaseAddress,byte[] lpBuffer,int nSize,out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll",SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess,IntPtr lpBaseAddress,[Out] byte[] lpBuffer,int dwSize,out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll",SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess,IntPtr lpThreadAttributes,uint dwStackSize,IntPtr lpStartAddress,IntPtr lpParameter,uint dwCreationFlags,out IntPtr lpThreadId);

        [DllImport("kernel32.dll",SetLastError = true)]
        public static extern int WaitForSingleObject(IntPtr hHandle,int dwMilliseconds);

        #endregion

        #endregion

        #region Flags

        public const int INFINITE = -1;
        public const int WAIT_ABANDONED = 0x80;
        public const int WAIT_OBJECT_0 = 0x00;
        public const int WAIT_TIMEOUT = 0x102;
        public const int WAIT_FAILED = -1;

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        #endregion
    }
}
