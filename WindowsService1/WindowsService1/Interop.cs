﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//for DLLImport
using System.Runtime.InteropServices;
//WindowsIdentity
using System.Security.Principal;

namespace WindowsService1
{
    class Interop
    {
        public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        public static void ShowMessageBox(string message, string title)
        {
            int resp = 0;
            WTSSendMessage(
                WTS_CURRENT_SERVER_HANDLE,
                WTSGetActiveConsoleSessionId(),
                title, title.Length,
                message, message.Length,
                0, 0, out resp, false);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern bool WTSSendMessage(
            IntPtr hServer,
            int SessionId,
            String pTitle,
            int TitleLength,
            String pMessage,
            int MessageLength,
            int Style,
            int Timeout,
            out int pResponse,
            bool bWait);
        public static void CreateProcess(string app, string commandLine, string path)
        {
            bool result;
            IntPtr hToken = WindowsIdentity.GetCurrent().Token;
            IntPtr hDupedToken = IntPtr.Zero;

            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.Length = Marshal.SizeOf(sa);

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            //add for screen
            //si.lpDesktop = @"WinSta0\Default"; //Modify as needed
            //si.dwFlags = STARTF_USESHOWWINDOW | STARTF_FORCEONFEEDBACK;
            //si.wShowWindow = SW_HIDE;
            //add for screen end

            int dwSessionID = WTSGetActiveConsoleSessionId();
            result = WTSQueryUserToken(dwSessionID, out hToken);

            if (!result)
            {
                ShowMessageBox("WTSQueryUserToken failed, dwSessionID = "+dwSessionID+",hToken = "+hToken.ToString()+",result="+result, "AlertService Message");
            }

            result = DuplicateTokenEx(
                  hToken,
                  GENERIC_ALL_ACCESS,
                  ref sa,
                  (int)SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
                  (int)TOKEN_TYPE.TokenPrimary,
                  ref hDupedToken
               );

            if (!result)
            {
                ShowMessageBox("DuplicateTokenEx failed", "AlertService Message");
            }

            IntPtr lpEnvironment = IntPtr.Zero;
            result = CreateEnvironmentBlock(out lpEnvironment, hDupedToken, false);

            if (!result)
            {
                ShowMessageBox("CreateEnvironmentBlock failed", "AlertService Message");
            }
            /*
            result = CreateProcessAsUser(
                                 hDupedToken,
                                 app,
                                 String.Empty,
                                 ref sa, ref sa,
                                 false, 0, IntPtr.Zero,
                                 path, ref si, ref pi);
             */
            result = CreateProcessAsUser(
                                 hDupedToken,
                                 app,
                                 commandLine,
                                 ref sa, ref sa,
                                 false, 0, IntPtr.Zero,
                                 path, ref si, ref pi);

            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                string message = String.Format("CreateProcessAsUser Error: {0}", error);
                ShowMessageBox(message, "AlertService Message");
            }

            if (pi.hProcess != IntPtr.Zero)
                CloseHandle(pi.hProcess);
            if (pi.hThread != IntPtr.Zero)
                CloseHandle(pi.hThread);
            if (hDupedToken != IntPtr.Zero)
                CloseHandle(hDupedToken);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public Int32 dwProcessID;
            public Int32 dwThreadID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public Int32 Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        public const int GENERIC_ALL_ACCESS = 0x10000000;
        //add for screen
        private const short SW_HIDE = 0;
        private const short SW_SHOWNOACTIVATE = 4;
        private const short SW_SHOW = 5;
        private const uint TOKEN_QUERY = 0x0008;
        private const uint TOKEN_DUPLICATE = 0x0002;
        private const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        private const int STARTF_USESHOWWINDOW = 0x00000001;
        private const int STARTF_FORCEONFEEDBACK = 0x00000040;
        private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        private const int STARTF_RUNFULLSCREEN = 0x00000020;
        //add for screen end

        [DllImport("kernel32.dll", SetLastError = true,
            CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", SetLastError = true,
            CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandle,
            Int32 dwCreationFlags,
            IntPtr lpEnvrionment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            Int32 dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            Int32 ImpersonationLevel,
            Int32 dwTokenType,
            ref IntPtr phNewToken);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern bool WTSQueryUserToken(
            Int32 sessionId,
            out IntPtr Token);

        [DllImport("userenv.dll", SetLastError = true)]
        static extern bool CreateEnvironmentBlock(
            out IntPtr lpEnvironment,
            IntPtr hToken,
            bool bInherit);
    }
}
