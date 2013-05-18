using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpGMad
{
    /// <summary>
    /// Represents an error with files checking against the whitelist.
    /// </summary>
    [Serializable]
    class WhitelistException : Exception
    {
        public WhitelistException() { }
        public WhitelistException(string message) : base(message) { }
        public WhitelistException(string message, Exception inner) : base(message, inner) { }
        protected WhitelistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Provides methods to use the internal global whitelist.
    /// </summary>
    static class Whitelist
    {
        /// <summary>
        /// A list of string patterns of allowed files.
        /// </summary>
        private static string[] Wildcard = new string[]{
            "maps/*.bsp",
			"maps/*.png",
			"maps/*.nav",
			"maps/*.ain",
			"sound/*.wav",
			"sound/*.mp3",
			"lua/*.lua",
			"materials/*.vmt",
			"materials/*.vtf",
			"materials/*.png",
			"models/*.mdl",
			"models/*.vtx",
			"models/*.phy",
			"models/*.ani",
			"models/*.vvd",
			"gamemodes/*.txt",
			"gamemodes/*.lua",
			"scenes/*.vcd",
			"particles/*.pcf",
			"gamemodes/*/backgrounds/*.jpg",
			"gamemodes/*/icon24.png",
			"gamemodes/*/logo.png",
			"scripts/vehicles/*.txt",
			"resource/fonts/*.ttf",
			null
        };

        /// <summary>
        /// Checks a file against the internal whitelist determining whether it's allowed or not.
        /// </summary>
        /// <param name="path">The relative path of the filename to determine.</param>
        /// <returns>True if the file is allowed, false if not.</returns>
        public static bool Check(string path)
        {
            return Wildcard.Any(a => TestWildcard(a, path));
        }

        /// <summary>
        /// Tests the specified input string against the specified wildcard.
        /// </summary>
        /// <param name="wildcard">The wildcard to check against.</param>
        /// <param name="input">The string to check.</param>
        /// <returns>True if there is match, false if not.</returns>
        public static bool TestWildcard(string wildcard, string input)
        {
            string pattern = "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return (regex.IsMatch(input));
        }
    }
}
