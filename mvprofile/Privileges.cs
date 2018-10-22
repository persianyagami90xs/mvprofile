using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace mvprofile
{
    class Privileges
    {
        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;

        [Flags]
        enum TokenAccess : uint
        {
            AdjustPrivileges = 0x0020
        }

        public struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public UInt32 Attributes;
        }

        public struct LUID
        {

            public UInt32 LowPart;
            public Int32 HighPart;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AdjustTokenPrivileges(IntPtr hTok,
            [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            UInt32 BufferLengthInBytes,
            ref TOKEN_PRIVILEGES PreviousState,
            out UInt32 ReturnLengthInBytes);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AdjustTokenPrivileges(IntPtr hTok,
            [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            UInt32 BufferLengthInBytes,
            IntPtr PreviousState,
            IntPtr ReturnLengthInBytes);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LookupPrivilegeValue(string lpsystemname, string lpname, [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        public static bool EnablePrivileges(string privilege)
        {
            LUID luid = new LUID();
            if (LookupPrivilegeValue(null, privilege, ref luid))
            {
                var tokenPrivileges = new TOKEN_PRIVILEGES();
                tokenPrivileges.PrivilegeCount = 1;
                tokenPrivileges.Privileges = new LUID_AND_ATTRIBUTES[1];
                tokenPrivileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
                tokenPrivileges.Privileges[0].Luid = luid;

                IntPtr hProc = GetCurrentProcess();

                if (hProc != null)
                {
                    if (OpenProcessToken(hProc, (uint)TokenAccess.AdjustPrivileges, out IntPtr hToken))
                    {
                        return AdjustTokenPrivileges(hToken, false, ref tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero);
                    }
                }
            }
            return false;
        }
    }
}
