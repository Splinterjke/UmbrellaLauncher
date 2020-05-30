using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UmbrellaProject
{
    internal class RegExpressions
    {
        public static Lazy<Regex> LuaPathRegex = new Lazy<Regex>(() => new Regex(@"^https?://github.com/\S+.lua$", RegexOptions.Compiled));
        public static Lazy<Regex> LuaRawPathRegex = new Lazy<Regex>(() => new Regex(@"^https?://raw.githubusercontent.com/\S+.lua$", RegexOptions.Compiled));
    }
}
