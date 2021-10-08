using GearSwapPlugin;
using GearSwapPlugin.GearSwap;
using HarmonyLib;
using UnityEngine;
using WeaponRandomizerPlugin.WeaponRandomizer;
using WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggers;
using WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggerSubjects;

namespace WeaponRandomizerPlugin
{
    [HarmonyPatch]
    public class Entry
    {
        private static GameObject _go;

        [HarmonyPatch(typeof(GameStateManager), "ChangeState", typeof(eGameStateName))]
        public static void Postfix(eGameStateName nextState) => AddWeaponRandomizerComponents(nextState);

        private static void AddWeaponRandomizerComponents(eGameStateName? state = null)
        {
            switch (state)
            {
                case null:
                    return;
                case eGameStateName.StopElevatorRide:
                {
                    WeaponRandomizerCore.log.LogMessage("Initializing " + WeaponRandomizerCore.NAME);

                    var gameObject = new GameObject(WeaponRandomizerCore.AUTHOR + " - " + WeaponRandomizerCore.NAME);

                    Object.DontDestroyOnLoad(gameObject);

                    gameObject.AddComponent<WeaponRandomizer.WeaponRandomizer>();
                    gameObject.AddComponent<RandomizerTriggerManager>();
                    gameObject.AddComponent<RandomizerSync>();

                    _go = gameObject;
                    break;
                }
                case eGameStateName.AfterLevel:
                    WeaponRandomizerCore.log.LogMessage("Closing " + WeaponRandomizerCore.NAME);
                    Object.Destroy(_go);
                    break;
            }
        }
    }
}