using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace HowToFish.BossHpText
{
    [BepInPlugin(Guid, "Boss HP Text", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "alexandre.howtofish.bosshptext";

        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<string> Format;
        internal static ConfigEntry<float> FontSize;
        internal static ConfigEntry<string> TextColor;
        internal static ConfigEntry<float> OutlineWidth;
        internal static ConfigEntry<string> OutlineColor;

        private void Awake()
        {
            Log = Logger;

            Enabled = Config.Bind("General", "Enabled", true,
                "Show the boss's current and maximum HP as text inside the boss health bar.");

            Format = Config.Bind("Display", "Format", "{0}/{1}",
                "How the numbers are written. {0} is current HP, {1} is maximum HP.");

            FontSize = Config.Bind("Display", "FontSize", 0f,
                "Font size in points. 0 copies the size of the boss name label above the bar.");

            TextColor = Config.Bind("Display", "TextColor", "#FFFFFF",
                "Colour of the numbers, as a hex code.");

            OutlineWidth = Config.Bind("Display", "OutlineWidth", 0.2f,
                "Outline thickness, 0 to 1. The bar changes colour between bosses and mini-bosses, "
                + "so some outline keeps the numbers readable on both. 0 disables it.");

            OutlineColor = Config.Bind("Display", "OutlineColor", "#000000",
                "Colour of the outline, as a hex code.");

            new Harmony(Guid).PatchAll();

            Log.LogInfo("Boss HP Text loaded.");
        }
    }
}
