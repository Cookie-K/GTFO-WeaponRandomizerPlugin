using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using WeaponRandomizerPlugin.WeaponRandomizer;
using WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggers;

namespace WeaponRandomizerPlugin
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInProcess("GTFO.exe")]
    public class WeaponRandomizerCore : BasePlugin
    {
        public const string
            NAME = "Weapon Randomizer Plugin",
            MODNAME = "WeaponRandomizer",
            AUTHOR = "Cookie_K",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.1";

        public static ManualLogSource log;

        private Harmony HarmonyPatches { get; set; }

        public override void Load()
        {
            log = Log;

            ClassInjector.RegisterTypeInIl2Cpp<WeaponRandomizerManager>();
            ClassInjector.RegisterTypeInIl2Cpp<RandomizerTriggerManager>();
            ClassInjector.RegisterTypeInIl2Cpp<RandomizerSync>();

            HarmonyPatches = new Harmony(GUID);
            HarmonyPatches.PatchAll();
        }
    }
}