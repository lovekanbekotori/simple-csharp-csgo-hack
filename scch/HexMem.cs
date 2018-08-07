using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace scch
{
    public class HexMem
    {
        /*
         * Please note this memory class is very old, and i no longer use it in my private cheats (therefore it is messy af)
         * I simply use it here to avoid any sig detections that may happen with this hack.
         * Don't want anything overflowing into my private ;)
         * (for those wondering, i now use generics and the entire class is about 1/5th the size of this one)
         */
        #region Dll Imports
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, uint lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, uint lpNumberOfBytesRead);

        //I use 2 dif rpms here because i was nub at the time.
        //i copy pastad Traxin's pat scan func and couldnt be bothered fixing the errors presented with my current rpm import so i used both lmao


        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hObject);
        #endregion
        public string processName = string.Empty;
        public Process process = null;
        public IntPtr hProc = IntPtr.Zero;
        private ProcessAccessFlags OriginalFlags;
        public HexMem(string procName)
        {
            processName = procName;
        }
        public HexMem(string procName, ProcessAccessFlags flags)
        {
            processName = procName;
            OpenProcess(flags);
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000,
            ReadControl = 0x00020000
        }
        public void monitorThread()
        {
            for (;;)
            {
                if (process.HasExited)
                {
                    Environment.Exit(1);
                }
                Thread.Sleep(500);
            }
        }
        public void OpenProcess(ProcessAccessFlags flags)
        {
            OriginalFlags = flags;
            Thread t = new Thread(monitorThread);
            while (process == null)
            {
                try
                {
                    process = Process.GetProcessesByName(processName)[0];
                }
                catch
                {
                    process = null;
                    Thread.Sleep(500);
                }
            }
            if (process != null)
            {
                hProc = OpenProcess((uint)flags, false, process.Id);
                t.Start();
            }
        }
        public void closeHandle()
        {
            int val;
            val = CloseHandle(hProc);
            if (val == 0)
            {
                MessageBox.Show("Could not Close Handle sucessfully.");
                throw new Exception("Could not Close Handle sucessfully.");
            }
        }

        public bool WriteByteArray(long address,byte[] buffer, uint pSize)
        {
            if (hProc == IntPtr.Zero)
            {
                MessageBox.Show("Handle to process not aquired before writeing.");
                return false;
            }

            try
            {
                 
                return WriteProcessMemory(hProc, (IntPtr)address, buffer, pSize, 0U);
            }
            catch
            {
                MessageBox.Show("Exception caught when writeing!");
                return false;
            }
        }

        #region Readers
        public byte[] ReadByteArray(long address, uint pSize)
        {
            if (hProc == IntPtr.Zero)
            {
                MessageBox.Show("Handle to process not aquired before reading.");
                return null;
            }

            byte[] buffer = new byte[pSize];
            try
            {
                ReadProcessMemory(hProc, (IntPtr)address, buffer, pSize, 0U);
            }
            catch
            {
                MessageBox.Show("Exception caught when reading!");
            }
            return buffer;
        }
        public byte ReadByte(long address)  //Needs testing
        {
            if (hProc == IntPtr.Zero)
            {
                MessageBox.Show("Handle to process not aquired before reading.");
                return 0;
            }

            byte[] buffer = new byte[1];


            try
            {
                ReadProcessMemory(hProc, (IntPtr)address, buffer, 1, 0U);
            }
            catch
            {
                MessageBox.Show("Exception caught when reading!");
                return 0;
            }
            return buffer[0];
        }
        public char ReadChar(long address)
        {
            return BitConverter.ToChar(this.ReadByteArray(address, sizeof(Int16)), 0);
        }
        public bool ReadBool(long address)
        {
            return BitConverter.ToBoolean(this.ReadByteArray(address, sizeof(Int16)), 0);
        }
        public Int16 ReadInt16(long address)
        {
            return BitConverter.ToInt16(this.ReadByteArray(address, sizeof(Int16)), 0);
        }
        public UInt16 ReadUInt16(long address)
        {
            return BitConverter.ToUInt16(this.ReadByteArray(address, sizeof(Int16)), 0);
        }
        public Int32 ReadInt32(long address)
        {
            return BitConverter.ToInt32(this.ReadByteArray(address, sizeof(Int32)), 0);
        }
        public UInt32 ReadUInt32(long address)
        {
            return BitConverter.ToUInt32(this.ReadByteArray(address, sizeof(Int16)), 0);
        }
        public Int64 ReadInt64(long address)
        {
            return BitConverter.ToInt64(this.ReadByteArray(address, sizeof(Int64)), 0);
        }
        public UInt64 ReadUInt64(long address)
        {
            return BitConverter.ToUInt64(this.ReadByteArray(address, sizeof(Int16)), 0);
        }
        public float ReadFloat(long address)
        {
            return BitConverter.ToSingle(this.ReadByteArray(address, sizeof(float)), 0);
        }


        public bool WriteFloat(long address,float w)
        {
            return WriteByteArray(address, BitConverter.GetBytes(w), sizeof(float));
        }


        public double ReadDouble(long address)
        {
            return BitConverter.ToDouble(this.ReadByteArray(address, sizeof(double)), 0);
        }
        public string ReadStringAscii(long address, uint size)
        {
            return Encoding.ASCII.GetString(this.ReadByteArray(address, size));
        }

        public string ReadStringAsciiLine(long address, uint size)
        {
            int i = 0;
            byte[] tmp =this.ReadByteArray(address, size);
            for (; i<size;i++ )
            {
                if (tmp[i] == 00)
                    break;
            }
            return Encoding.ASCII.GetString(tmp,0,i);
        }

        public string ReadStringUnicode(long address, uint size)
        {
            return Encoding.Unicode.GetString(this.ReadByteArray(address, size));
        }

        public string ReadStringUTF8(long address, uint size)
        {
            return Encoding.UTF8.GetString(this.ReadByteArray(address, size));
        }

        public Vec3 ReadVec3(long address)
        {
            Vec3 temp = new Vec3();
            temp.x = BitConverter.ToSingle(this.ReadByteArray(address, sizeof(float)), 0);
            temp.y = BitConverter.ToSingle(this.ReadByteArray(address + 4, sizeof(float)), 0);
            temp.z = BitConverter.ToSingle(this.ReadByteArray(address + 8, sizeof(float)), 0);
            return temp;
        }

        public Vec2 ReadVec2(long address)
        {
            Vec2 temp = new Vec2();
            temp.x = BitConverter.ToSingle(this.ReadByteArray(address, sizeof(float)), 0);
            temp.y = BitConverter.ToSingle(this.ReadByteArray(address + 4, sizeof(float)), 0);
            return temp;
        }

        #endregion
        public long GetModuleAddress(string moduleName)
        {
            if (hProc != IntPtr.Zero)
            {
                for (int i = 0; i < process.Modules.Count; i++)
                {
                    if (process.Modules[i].ModuleName.Equals(moduleName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return (long)process.Modules[i].BaseAddress;
                    }
                }
            }
            return 0;
        }
        public ProcessModule GetModule(string moduleName)
        {
            if (hProc != IntPtr.Zero)
            {
                for (int i = 0; i < process.Modules.Count; i++)
                {
                    if (process.Modules[i].ModuleName.Equals(moduleName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return process.Modules[i];
                    }
                }
            }
            return null;
        }
        #region Patter Scanner
        //Thanks to Traxin from GH for this!!
        private byte[] Read(IntPtr MemoryAddress, uint bytesToRead, out int bytesRead)
        {
            byte[] buffer = new byte[bytesToRead];
            IntPtr ptrBytesRead;
            ReadProcessMemory(hProc, MemoryAddress, buffer, bytesToRead, out ptrBytesRead);
            bytesRead = ptrBytesRead.ToInt32();
            return buffer;
        }
        private bool CheckPattern(string pattern, byte[] array2check)
        {
            int len = array2check.Length;
            string[] strBytes = pattern.Split(' ');
            int x = 0;
            foreach (byte b in array2check)
            {
                if (strBytes[x] == "?" || strBytes[x] == "??")
                {
                    x++;
                }
                else if (byte.Parse(strBytes[x], NumberStyles.HexNumber) == b)
                {
                    x++;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        public IntPtr PatternScanMod(ProcessModule pMod, string pattern)
        {
            IntPtr baseAddy = pMod.BaseAddress;
            uint dwSize = (uint)pMod.ModuleMemorySize;
            int br;
            byte[] memDump = Read(baseAddy, dwSize, out br);
            string[] pBytes = pattern.Split(' ');
            try
            {
                for (int y = 0; y < memDump.Length; y++)
                {
                    if (memDump[y] == byte.Parse(pBytes[0], NumberStyles.HexNumber))
                    {
                        byte[] checkArray = new byte[pBytes.Length];
                        for (int x = 0; x < pBytes.Length; x++)
                        {
                            checkArray[x] = memDump[y + x];
                        }
                        if (CheckPattern(pattern, checkArray))
                        {
                            long temp = (long)baseAddy;
                            temp += y;
                            return (IntPtr)temp;
                        }
                        else
                        {
                            //y += pBytes.Length - (pBytes.Length / 2);
                            y += 1;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return (IntPtr)0x11;
            }
            return (IntPtr)0;
        }

        public IntPtr PatternScanRange(IntPtr baseAddress, long size, string pattern)
        {
            IntPtr baseAddy = baseAddress;
            uint dwSize = (uint)size;

            int br;
            byte[] memDump = Read(baseAddy, dwSize, out br);
            string[] pBytes = pattern.Split(' ');
            try
            {
                for (int y = 0; y < memDump.Length; y++)
                {
                    if (memDump[y] == byte.Parse(pBytes[0], NumberStyles.HexNumber))
                    {
                        byte[] checkArray = new byte[pBytes.Length];
                        for (int x = 0; x < pBytes.Length; x++)
                        {
                            checkArray[x] = memDump[y + x];
                        }
                        if (CheckPattern(pattern, checkArray))
                        {
                            return baseAddy + y;
                        }
                        else
                        {
                            y += 1;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
            return IntPtr.Zero;
        }

        public int checkSpaces(string toCheck)
        {
            int spaceCount = 0;
            foreach (char c in toCheck)
            {
                if (c == ' ')
                {
                    spaceCount++;
                }
            }
            return spaceCount + 1;
        }
        #endregion
    }
}