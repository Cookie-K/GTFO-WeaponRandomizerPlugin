using System.Runtime.InteropServices;
using GTFO.API;
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

        private void Start()
        {
            if (!NetworkAPI.IsEventRegistered(TriggerEventName))
            {
                NetworkAPI.RegisterEvent<WeaponRandomizerData>(TriggerEventName,  (senderId, packet) => 
                {
                    WeaponRandomizerCore.log.LogInfo($"Weapon Randomizer packet received for {packet.PlayerName}: {packet.GearIds}");
                    WeaponRandomizerManager.EquipFromPacket(packet);
                });
            }
        }

        internal static void SyncRandomize(WeaponRandomizerData data)
        {
            WeaponRandomizerCore.log.LogInfo($"Broadcasting weapon randomize signal for {data.PlayerName}: {data.GearIds}");
            NetworkAPI.InvokeEvent(TriggerEventName, data);
        }

    }
}