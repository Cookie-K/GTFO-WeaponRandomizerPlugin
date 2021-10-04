using System;
using System.Collections.Generic;
using System.Linq;
using Gear;
using GearSwapPlugin.GearSwap;
using Player;
using UnityEngine;
using Random = System.Random;

namespace WeaponRandomizerPlugin.WeaponRandomizer
{
    public class WeaponRandomizer : MonoBehaviour
    {
        public static WeaponRandomizer Instance { get; private set; }
        private static readonly Random Rng = new Random();
        private static readonly bool TreatSentryAsOne = true;
        private static readonly List<InventorySlot> SlotsToRandomize = new List<InventorySlot> {InventorySlot.GearMelee, InventorySlot.GearStandard, InventorySlot.GearSpecial, InventorySlot.GearClass};
        private static readonly Dictionary<InventorySlot, List<GearIDRange>> GearIdBySlot = new Dictionary<InventorySlot, List<GearIDRange>>();
        private static readonly Dictionary<InventorySlot, int> IndexBySlot = new Dictionary<InventorySlot, int>();


        public WeaponRandomizer(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            } else {
                Instance = this;
            }
        }

        private void Start()
        {
            Init();
        }
      
        private void Init()
        {
            WeaponRandomizerCore.log.LogInfo("Initializing random list");
            foreach (var slot in SlotsToRandomize)
            {
                GearIdBySlot[slot] = new List<GearIDRange>();
                foreach (var gearId in GearManager.GetAllGearForSlot(slot))
                {
                    GearIdBySlot[slot].Add(gearId);
                }

                GearIdBySlot[slot] = ShuffleList(GearIdBySlot[slot]);
                IndexBySlot[slot] = 0;
            }

            GearLoadingManager.PickUpSentryOnToolChange = false;
        }

        private static List<T> ShuffleList<T>(IEnumerable<T> list)
        {
            return list.OrderBy(x => Rng.Next()).ToList();
        }

        public void Randomize()
        {
            WeaponRandomizerCore.log.LogInfo("Randomize!");
            EquipFromRandomList();
        }
        
        private void EquipFromRandomList()
        {
            foreach (var slot in SlotsToRandomize)
            {
                if (IndexBySlot[slot] >= GearIdBySlot[slot].Count)
                {
                    IndexBySlot[slot] = 0;
                    GearIdBySlot[slot] = ShuffleList(GearIdBySlot[slot]);
                }
                GearSwapper.RequestToEquip(GearIdBySlot[slot][IndexBySlot[slot]]);
                IndexBySlot[slot] = IndexBySlot[slot] + 1;
            }
        }

        private void OnDestroy()
        {
            GearIdBySlot.Clear();
        }
    }
}