using System.Linq;

namespace SharpGMad
{
    /// <summary>
    /// Provides methods to use the internal tag database.
    /// </summary>
    static class Tags
    {
        /// <summary>
        /// Contains a list of addon types.
        /// Addons may have ONE of these values.
        /// </summary>
        public static string[] Type = new string[]{
            "gamemode",
            "map",
            "weapon",
            "vehicle",
            "npc",
            "tool",
            "effects",
            "model",
            "servercontent"
        };

        /// <summary>
        /// Gets whether the specified type is a valid type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type if valid, false otherwise.</returns>
        public static bool TypeExists(string type) { return Type.Contains(type); }

        /// <summary>
        /// Contains a list of addon tags.
        /// Addons may have up to TWO of these values.
        /// </summary>
        public static string[] Misc = new string[]{
            "fun",
            "roleplay",
            "scenic",
            "movie",
            "realism",
            "cartoon",
            "water",
            "comic",
            "build"
        };

        /// <summary>
        /// Gets whether the specified tag is a valid one.
        /// </summary>
        /// <param name="tag">The tag to check.</param>
        /// <returns>True if the tag is valid, false otherwise.</returns>
        public static bool TagExists(string tag) { return Misc.Contains(tag); }
    }
}
