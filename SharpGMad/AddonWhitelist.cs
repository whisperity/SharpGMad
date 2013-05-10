using System.Text.RegularExpressions;

namespace Addon
{
    class Whitelist
    {
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

        //
        // Call on a filename including relative path to determine
        // whether file is allowed to be in the addon.
        //
        public static bool Check(string strName)
        {
            bool bValid = false;

            for (int i = 0; ; i++)
            {
                if (bValid || Wildcard[i] == null)
                {
                    break;
                }

                bValid = TestWildcard(Wildcard[i], strName);
            }

            return bValid;
        }

        public static bool TestWildcard(string wildcard, string strName)
        {
            string pattern = "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return (regex.IsMatch(strName));
        }
    }
}