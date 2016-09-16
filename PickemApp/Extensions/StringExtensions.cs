using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PickemApp.Extensions
{
    public static class StringExtensions
    {
        public static string MakePossessive(this string s)
        {
            return s += (s.EndsWith("s")) ? "'" : "'s";
        }
    }
}