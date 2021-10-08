using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Nidhogg.Managers;
using UnityEngine;

namespace WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggers
{
    public class RandomizerSync : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WepRandomizerData {
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
            NetworkingManager.RegisterEvent<WepRandomizerData>("Trigger_Randomize",  (senderId, packet) => 
            {
                WeaponRandomizerCore.log.LogInfo($"Packet received for {packet.PlayerName}: {packet.GearIds}");
                WeaponRandomizerManager.EquipFromPacket(packet);
            });
        }

        internal static void SyncRandomize(Dictionary<string, WepRandomizerData> dataPerPlayer)
        {
            foreach (var data in dataPerPlayer.Values)
            {
                NetworkingManager.InvokeEvent("Trigger_Randomize", data);
                WeaponRandomizerCore.log.LogInfo($"For {data.PlayerName}");
                WeaponRandomizerCore.log.LogInfo($"{data.GearIds}");
            }
        }
        
        internal static void SyncRandomize(WepRandomizerData data)
        {
            NetworkingManager.InvokeEvent("Trigger_Randomize", data);
            WeaponRandomizerCore.log.LogInfo($"For {data.PlayerName}");
            WeaponRandomizerCore.log.LogInfo($"{data.GearIds}");
        }
    }
}