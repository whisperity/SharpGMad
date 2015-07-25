using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGMad.Shell
{
    /// <summary>
    /// A set of flags the current executing context can have
    /// </summary>
    [Flags]
    enum Availability : byte
    {
        /// <summary>
        /// Default value
        /// Context: the environment has nothing special associated
        /// Command: the command is always available for execution
        /// </summary>
        AlwaysAvailable     = 0x00, //   0
        AddonUnloaded       = 0x01, //   1
        AddonLoaded         = 0x02, //   2
        AddonWriteable      = 0x04, //   4
        AddonModified       = 0x08, //   8
        /// <summary>Indicates if there is at least one file exported</summary>
        ExportExists        = 0x10, //  16
        /// <summary>Indicates that at least one exports are modified and can be pulled.</summary>
        // (This should imply ExportExists...)
        ExportPullable      = 0x20, //  32
        /// <summary>Indicates that the global addon content whitelist has been overriden to open illegal addons</summary>
        WhitelistOverridden = 0x40, //  64
        //Unused            = 0x80  // 128
    }

    /// <summary>
    /// A rule which dictates a set of "must have" and "must not have" availabilities
    /// </summary>
    struct AvailabilityRule
    {
        /// <summary>
        /// The availability flags that the context MUST HAVE to conform this rule
        /// </summary>
        public Availability Include;
        /// <summary>
        /// The availability flags that the context MUST NOT HAVE to conform this rule
        /// </summary>
        public Availability Exclude;

        /// <summary>
        /// Check is a given context conforms to the current rule
        /// </summary>
        public bool CheckContext(Availability context)
        {
            return CheckContext(context, this.Include, this.Exclude);
        }

        /// <summary>
        /// Check if a given context conforms the given ruleset
        /// </summary>
        /// <param name="context">The context to check</param>
        /// <param name="include">The flags the context MUST HAVE to mark conforming</param>
        /// <param name="exclude">The flags the context MUST NOT HAVE to mark conforming</param>
        public static bool CheckContext(Availability context, Availability include, Availability exclude)
        {
            bool inc = false, exc = false;
            // Check inclusively. The current context must match ALL flags to include
            if ((include & context) == include)
                inc = true;

            // Check exclusively. The current context must NOT match ANY flags to exclude
            if ((exclude & context) == 0)
                exc = true;

            return (inc & exc);
        }
    }
}
