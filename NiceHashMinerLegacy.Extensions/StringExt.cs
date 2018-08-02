using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMinerLegacy.Extensions
{
    public static class StringExt
    {
        public static string AfterFirstOccurence(this string s, string occ)
        {
            var i = occ.Length;
            for (; i < s.Length; i++)
            {
                if (s.Substring(i - occ.Length, i) == occ)
                    break;
            }

            return s.Substring(i);
        }
    }
}
