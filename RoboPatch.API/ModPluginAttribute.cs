using System;

// =============================================================================
//  ModPluginAttribute.cs  -  MOD METADATA ATTRIBUTE
//
//  Optional attribute for your IMod class. Provides metadata that
//  RoboPatch can use for display and dependency resolution in the future.
//
//  USAGE:
//    [ModPlugin("MyMod", "1.0.0")]
//    public class MyMod : IModPlugin { ... }
// =============================================================================

namespace RoboPatch
{
    /// <summary>
    /// Optional metadata attribute for IMod implementations.
    /// Attach this to your mod class to declare its name and version.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModPluginAttribute : Attribute
    {
        /// <summary>Display name of your mod.</summary>
        public string Name { get; }

        /// <summary>Version string (e.g. "1.0.0").</summary>
        public string Version { get; }

        public ModPluginAttribute(string name, string version)
        {
            Name = name;
            Version = version;
        }
    }
}
