﻿using Gear;
using GearSwapPlugin.GearSwap;
using Player;
using SNetwork;
using UnityEngine;
using WeaponRandomizerPlugin.WeaponRandomizer.@enum;
using WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggers;

namespace WeaponRandomizerPlugin.WeaponRandomizer
{
    public class WeaponRandomizerManager : MonoBehaviour
    {
        public static event Action OnRandomize;

        private static RandomWeaponsSelector _rngWeaponsSelector;
        private static readonly List<InventorySlot> SlotsToRandomize = new List<InventorySlot>();
        private static readonly Dictionary<InventorySlot, List<GearIDRange>> GearIdsBySlot = new Dictionary<InventorySlot, List<GearIDRange>>();
        private static readonly SelectionType SelectionType = ConfigManager.SelectionType;
        private static readonly DistributionType DistributionType = ConfigManager.DistributionType;
        private static readonly bool RandomizeMelee = ConfigManager.RandomizeMelee;
        private static readonly bool RandomizePrimary = ConfigManager.RandomizePrimary;
        private static readonly bool RandomizeSecondary = ConfigManager.RandomizeSecondary;
        private static readonly bool RandomizeTool = ConfigManager.RandomizeTool;
        private static readonly bool PickUpSentryOnSwitch = ConfigManager.PickUpSentryOnSwitch;

        private void Start()
        {
            if (RandomizeMelee) SlotsToRandomize.Add(InventorySlot.GearMelee);
            if (RandomizePrimary) SlotsToRandomize.Add(InventorySlot.GearStandard);
            if (RandomizeSecondary) SlotsToRandomize.Add(InventorySlot.GearSpecial);
            if (RandomizeTool) SlotsToRandomize.Add(InventorySlot.GearClass);
            
            foreach (var slot in SlotsToRandomize)
            {
                GearIdsBySlot[slot] = new List<GearIDRange>();
                foreach (var gearId in GearManager.GetAllGearForSlot(slot))
                {
                    GearIdsBySlot[slot].Add(gearId);
                }
            }

            GearSwapManager.SetPickUpSentryOnToolChange(PickUpSentryOnSwitch);
            _rngWeaponsSelector = new RandomWeaponsSelector(GearIdsBySlot, SelectionType);
        }

        /// <summary>
        /// Randomise the players equipped weapons
        /// If player is host, it will equip it self with the new weapons and broad cast the weapons to equip to clients
        /// If player is a client, it will only equip weapons when message from host is received  
        /// </summary>
        public static void Randomize()
        {
            WeaponRandomizerCore.log.LogInfo("Randomizing weapons");

            Dictionary<string, string> gearIdsByPlayer;
            switch (DistributionType)
            {
                case DistributionType.Equal:
                    gearIdsByPlayer = _rngWeaponsSelector.PickNextEqualIds();
                    break;
                case DistributionType.Unique:
                    gearIdsByPlayer = _rngWeaponsSelector.PickNextUniqueIds();
                    break;
                default:
                    gearIdsByPlayer = _rngWeaponsSelector.PickNextRandomIds();
                    break;
            }
            
            foreach (var (playerName, ids) in gearIdsByPlayer)
            {
                var data = new RandomizerSync.WeaponRandomizerData {PlayerName = playerName, GearIds = ids};
                if (playerName == SNet.LocalPlayer.NickName)
                {
                    EquipFromPacket(data);
                }
                else
                {
                    RandomizerSync.SyncRandomize(data);
                }
            }
            OnRandomize?.Invoke();
        }

        internal static void EquipFromPacket(RandomizerSync.WeaponRandomizerData data)
        {
            if (SNet.LocalPlayer.NickName != data.PlayerName) return;
            
            foreach (var gearId in data.GearIds.Split(','))
            {
                GearSwapManager.RequestToEquip(GearIdsBySlot.SelectMany(id => id.Value).ToList().Find(id => gearId == id.PlayfabItemId));
            }
        }

        private void OnDestroy()
        {
            GearIdsBySlot.Clear();
        }
    }
}