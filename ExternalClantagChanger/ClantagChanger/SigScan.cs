using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace ClantagChanger
{
    public class SigScan
    {
        private IntPtr g_hProcess { get; set; }
        private byte[] g_arrModuleBuffer { get; set; }
        private ulong g_lpModuleBase { get; set; }

        private Dictionary<string, string> g_dictStringPatterns { get; }

        public SigScan(IntPtr hProc)
        {
            g_hProcess = hProc;
            g_dictStringPatterns = new Dictionary<string, string>();
        }

        public bool SelectModule(IntPtr hModule, int SizeOfImage)
        {
            g_lpModuleBase = (ulong)hModule;
            g_arrModuleBuffer = new byte[SizeOfImage];

            g_dictStringPatterns.Clear();

            return NTAPI.ReadProcessMemory(g_hProcess,hModule, g_arrModuleBuffer, SizeOfImage,out IntPtr a);
        }

        public void AddPattern(string szPatternName, string szPattern)
        {
            g_dictStringPatterns.Add(szPatternName, szPattern);
        }

        private bool PatternCheck(int nOffset, byte[] arrPattern)
        {
            for (int i = 0; i < arrPattern.Length; i++)
            {
                if (arrPattern[i] == 0x0)
                    continue;

                if (arrPattern[i] != this.g_arrModuleBuffer[nOffset + i])
                    return false;
            }

            return true;
        }

        public ulong FindPattern(string szPattern, out long lTime)
        {
            if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
                throw new Exception("Selected module is null");

            Stopwatch stopwatch = Stopwatch.StartNew();

            byte[] arrPattern = ParsePatternString(szPattern);

            for (int nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
            {
                if (this.g_arrModuleBuffer[nModuleIndex] != arrPattern[0])
                    continue;

                if (PatternCheck(nModuleIndex, arrPattern))
                {
                    lTime = stopwatch.ElapsedMilliseconds;
                    return g_lpModuleBase + (ulong)nModuleIndex;
                }
            }

            lTime = stopwatch.ElapsedMilliseconds;
            return 0;
        }
        public Dictionary<string, ulong> FindPatterns(out long lTime)
        {
            if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
                throw new Exception("Selected module is null");

            Stopwatch stopwatch = Stopwatch.StartNew();

            byte[][] arrBytePatterns = new byte[g_dictStringPatterns.Count][];
            ulong[] arrResult = new ulong[g_dictStringPatterns.Count];

            // PARSE PATTERNS
            for (int nIndex = 0; nIndex < g_dictStringPatterns.Count; nIndex++)
                arrBytePatterns[nIndex] = ParsePatternString(g_dictStringPatterns.ElementAt(nIndex).Value);

            // SCAN FOR PATTERNS
            for (int nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
            {
                for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
                {
                    if (arrResult[nPatternIndex] != 0)
                        continue;

                    if (this.PatternCheck(nModuleIndex, arrBytePatterns[nPatternIndex]))
                        arrResult[nPatternIndex] = g_lpModuleBase + (ulong)nModuleIndex;
                }
            }

            Dictionary<string, ulong> dictResultFormatted = new Dictionary<string, ulong>();

            // FORMAT PATTERNS
            for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
                dictResultFormatted[g_dictStringPatterns.ElementAt(nPatternIndex).Key] = arrResult[nPatternIndex];

            lTime = stopwatch.ElapsedMilliseconds;
            return dictResultFormatted;
        }

        private byte[] ParsePatternString(string szPattern)
        {
            List<byte> patternbytes = new List<byte>();

            foreach (var szByte in szPattern.Split(' '))
                patternbytes.Add(szByte == "?" ? (byte)0x0 : Convert.ToByte(szByte, 16));

            return patternbytes.ToArray();
        }
    }
}
