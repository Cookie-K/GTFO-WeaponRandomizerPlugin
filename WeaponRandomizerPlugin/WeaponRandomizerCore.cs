using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using UnhollowerRuntimeLib;
using WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggers;
using WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggerSubjects;

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
            VERSION = "0.0.0";

        public static ManualLogSource log;

        public Harmony HarmonyPatches { get; private set; }

        public static WeaponRandomizerCore Instance { get; private set; }

        public override void Load()
        {
            log = Log;
            Instance = this;

            ClassInjector.RegisterTypeInIl2Cpp<WeaponRandomizer.WeaponRandomizer>();
            ClassInjector.RegisterTypeInIl2Cpp<RandomizerTriggerManager>();
            ClassInjector.RegisterTypeInIl2Cpp<RandomizerSync>();

            HarmonyPatches = new Harmony(GUID);
            HarmonyPatches.PatchAll();
            
            foreach (var method in HarmonyPatches.GetPatchedMethods())
            {
                log.LogInfo("Patched method: " + method);
            }
        }
    }
}