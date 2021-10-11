using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Nidhogg.Managers;
using UnityEngine;

namespace WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggers
{
    public class RandomizerSync : MonoBehaviour
    {
        private const string TriggerEventName = "Trigger_Weapon_Randomize";

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WeaponRandomizerData {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string PlayerName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string GearIds;
        }
        
        public RandomizerSync(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        private void Start()
        {
            if (!NetworkingManager.IsEventRegistered(TriggerEventName))
            {
                NetworkingManager.RegisterEvent<WeaponRandomizerData>(TriggerEventName,  (senderId, packet) => 
                {
                    WeaponRandomizerCore.log.LogInfo($"Weapon Randomizer packet received for {packet.PlayerName}: {packet.GearIds}");
                    WeaponRandomizerManager.EquipFromPacket(packet);
                });
            }
        }

        internal static void SyncRandomize(WeaponRandomizerData data)
        {
            WeaponRandomizerCore.log.LogInfo($"Broadcasting weapon randomize signal for {data.PlayerName}: {data.GearIds}");
            NetworkingManager.InvokeEvent(TriggerEventName, data);
        }

    }
}