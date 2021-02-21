﻿using System;

namespace Assets.Scripts.SystemExtensions
{
    public static class ByteArrayExtensions
    {
        public static string ToHexDump(this Byte[] buf)
        {
            string s = "";
            string line = "";
            string ascii = "";
            int address = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                if (i > 0 && (i % 8 == 0))
                {
                    line += " ";
                }

                if (i > 0 && (i % 16 == 0))
                {
                    s += $"{address:x4}   {line} {ascii}\n";
                    line = "";
                    ascii = "";
                    address = i;
                }

                line += $"{buf[i]:x2} ";
                ascii += (buf[i] < 0x20 || buf[i] >= 0x7f) ? '.' : (char)buf[i];
            }

            if (s.Length > 0)
            {
                s += $"{address:x4}   {line,-50} {ascii}\n";
            }
            return s;
        }
    }
}
