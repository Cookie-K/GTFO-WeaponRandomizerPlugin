using HarmonyLib;
using LevelGeneration;
using SNetwork;
using UnityEngine;

namespace WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggerSubjects
{
    [HarmonyPatch]
    public class RandomizerPatchSubject
    {
        [HarmonyPatch(typeof(GameStateManager), "ChangeState", typeof(eGameStateName))]
        public static void Postfix(eGameStateName nextState) => GameChangeState(nextState);
        
        [HarmonyPatch(typeof(LG_Door_Sync), "AttemptDoorInteraction", typeof(eDoorInteractionType), typeof(float), typeof(float), typeof(Vector3), typeof(SNet_Player))]
        public static void Postfix(
            eDoorInteractionType interaction,
            float val1,
            float val2,
            Vector3 position,
            SNet_Player player,
            LG_Door_Sync __instance) => OnDoorOpen(__instance);
        
        private static void GameChangeState(eGameStateName? state = null)
        {
            if (!(state is null))
            {
                RandomizerTriggerMonitor.Instance.OnGameStateChanged((eGameStateName) state);
            }
        }

        private static void OnDoorOpen(LG_Door_Sync __instance)
        {
            if ((__instance.m_core.DoorType == eLG_DoorType.Security || __instance.m_core.DoorType == eLG_DoorType.Apex) &&
                __instance.m_core.LastStatus == eDoorStatus.Open)
            {
                RandomizerTriggerMonitor.Instance.OnSecurityDoorOpen();
            } 
        }

    }
}