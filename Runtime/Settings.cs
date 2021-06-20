using System;
using UnityEngine;

namespace Popcron.Referencer
{
    public class Settings : ScriptableObject
    {
        private static Settings current;

        //constants
        public const string UniqueIdentifier = "Popcron.Referencer.Settings";
        public const string GameObjectNameKey = UniqueIdentifier + ".GameObject";

        /// <summary>
        /// The current settings data being used.
        /// </summary>
        public static Settings Current
        {
            get
            {
                if (!current)
                {
                    current = Utils.GetOrCreate<Settings>("Assets/Referencer Settings.asset");
                }

                return current;
            }
        }

        [SerializeField]
        private string[] blacklistFilter = {
            "Resources/",
            "Game/",
            "Editor/",
            "Plugins/",
            ".html",
            ".shader",
            ".cs"
        };

        [SerializeField]
        private LogVerbosity verbosity = 0;

        /// <summary>
        /// The current blacklist filter.
        /// </summary>
        public string[] BlacklistFilter
        {
            get => blacklistFilter;
            set => blacklistFilter = value;
        }

        public LogVerbosity Verbosity => verbosity;

        private void OnEnable()
        {
            current = this;
        }

        /// <summary>
        /// Returns true if this path has overlap with the blacklist filter.
        /// </summary>
        public bool IsBlacklisted(string path)
        {
            int length = blacklistFilter.Length;
            for (int i = length - 1; i >= 0; i--)
            {
                if (path.IndexOf(blacklistFilter[i], StringComparison.OrdinalIgnoreCase) != -1)
                {
                    return true;
                }
            }

            return false;
        }

        [Flags]
        public enum LogVerbosity : int
        {
            LogLoadCount = 1 << 0,
            LogLoadReasons = 1 << 1,
            Errors = 1 << 2,
            LogSettings = 1 << 3
        }
    }
}
