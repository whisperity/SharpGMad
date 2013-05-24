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
        /// Check a path against the internal whitelist determining whether it's allowed or not.
        /// </summary>
        /// <param name="path">The relative path of the filename to determine.</param>
        /// <returns>True if the file is allowed, false if not.</returns>
        public static bool Check(string path)
        {
            return Wildcard.Any(wildcard => Check(wildcard, path));
        }

        /// <summary>
        /// Check a path against the specified wildcard determining whether it's allowed or not.
        /// </summary>
        /// <param name="wildcard">The wildcard to check against. (e.g.: files/*.file)</param>
        /// <param name="input">The path to check.</param>
        /// <returns>True if there is match, false if not.</returns>
        public static bool Check(string wildcard, string input)
        {
            if (wildcard == null)
                return false;

            string pattern = "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return (regex.IsMatch(input));
        }

        /// <summary>
        /// Check a path against the internal whitelist and returns the first matching substring.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>The matching substring or String.Empty if there was no match.</returns>
        public static string GetMatchingString(string path)
        {
            string match = String.Empty;

            foreach (string wildcard in Wildcard)
            {
                if (match != String.Empty)
                    break;
                if (wildcard == null || wildcard == String.Empty)
                    break;

                match = GetMatchingString(wildcard, path);
            }

            return match;
        }

        /// <summary>
        /// Check a path against the specified wildcard and returns the matching substring.
        /// </summary>
        /// <param name="wildcard">The wildcard to check against (e.g.: files/*.file)</param>
        /// <param name="path">The path to check.</param>
        /// <returns>The matching substring or String.Empty if there was no match.</returns>
        public static string GetMatchingString(string wildcard, string path)
        {
            string pattern = Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".");
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = regex.Match(path);

            if (match.Success == false)
                return String.Empty;
            else
                return match.Value;
        }
    }
}
