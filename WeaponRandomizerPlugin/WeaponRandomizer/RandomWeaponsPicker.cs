using System;
using System.Collections.Generic;
using System.Linq;
using Gear;
using Player;
using SNetwork;
using WeaponRandomizerPlugin.WeaponRandomizer.@enum;

namespace WeaponRandomizerPlugin.WeaponRandomizer
{

    public class RandomWeaponsSelector
    {
        private static readonly Random Rng = new Random();
        private static readonly List<InventorySlot> SlotsToRandomize = new List<InventorySlot>();
        
        private static SelectionType _type;
        private static bool _treatSentryAsOne;
        
        private readonly Dictionary<InventorySlot, List<GearIDRange>> _gearIdsBySlot;
        private readonly Dictionary<string, Dictionary<InventorySlot, RngList>> _gearListPerPlayer;

        public RandomWeaponsSelector(Dictionary<InventorySlot, List<GearIDRange>> gearIdsBySlot, SelectionType type)
        {
            _type = type;
            _gearIdsBySlot = gearIdsBySlot;
            _treatSentryAsOne = ConfigManager.TreatSentriesAsOne;
            _gearListPerPlayer = new Dictionary<string, Dictionary<InventorySlot, RngList>>();
            SlotsToRandomize.AddRange(gearIdsBySlot.Keys);
            foreach (var player in SNet.LobbyPlayers)
            {
                InitWeaponsDict(player.NickName);
            }
        }

        private void InitWeaponsDict(string playerName)
        {
            _gearListPerPlayer[playerName] = new Dictionary<InventorySlot, RngList>();
            foreach (var slot in SlotsToRandomize)
            {
                _gearListPerPlayer[playerName][slot] = new RngList(_gearIdsBySlot[slot]);
            }
        }
        
        public Dictionary<string, string> PickNextRandomIds()
        {
            var gearIdsByPlayer = new Dictionary<string, string>(); 

            foreach (var player in SNet.LobbyPlayers)
            {
                if (!_gearListPerPlayer.ContainsKey(player.NickName))
                {
                    InitWeaponsDict(player.NickName);
                }
                gearIdsByPlayer.Add(player.NickName, string.Join(",", SlotsToRandomize.Select(slot => _gearListPerPlayer[player.NickName][slot].Next().PlayfabItemId)));
            }

            return gearIdsByPlayer;
        }
        
        public Dictionary<string, string> PickNextUniqueIds()
        {
            var pickedGearIds = new List<string>();
            var gearIdsByPlayer = new Dictionary<string, string>(); 

            foreach (var player in SNet.LobbyPlayers)
            {
                var nextGearIds = new List<string>();
                if (!_gearListPerPlayer.ContainsKey(player.NickName))
                {
                    InitWeaponsDict(player.NickName);
                }
                
                foreach (var slot in SlotsToRandomize)
                {
                    string itemId;
                    do
                    {
                        itemId = _gearListPerPlayer[player.NickName][slot].Next().PlayfabItemId;
                        nextGearIds.Add(itemId);
                    } while (pickedGearIds.Contains(itemId));

                    pickedGearIds.Add(itemId);
                }
                gearIdsByPlayer.Add(player.NickName, string.Join(",", nextGearIds));
            }

            return gearIdsByPlayer;
        }

        public Dictionary<string, string> PickNextEqualIds()
        {
            var gearIdsByPlayer = new Dictionary<string, string>();

            var nextGearIds = SlotsToRandomize.Select(slot => _gearListPerPlayer[SNet.LocalPlayer.NickName][slot].Next().PlayfabItemId).ToList();
            foreach (var player in SNet.LobbyPlayers)
            {
                gearIdsByPlayer.Add(player.NickName, string.Join(",", nextGearIds));
            }

            return gearIdsByPlayer;
        }
       
        private class RngList
        {
            private readonly Queue<string> _prevChosen = new Queue<string>();
            private readonly List<GearIDRange> _list;
            private readonly int _bufferLimit;
            private const int BufferMin = 2;
            private const int BufferMax = 3;
            private const int BufferThreshold = 5;

            public RngList(List<GearIDRange> items)
            {
                var countDistinct = items.GroupBy(id => id.PlayfabItemId).Select(id => id.First()).ToList().Count;
                _bufferLimit = countDistinct > BufferThreshold ? BufferMax : BufferMin;
                _list = items;
            }

            public GearIDRange Next()
            {
                GearIDRange next;

                if (_type == SelectionType.SemiRandom)
                {
                    do
                    {
                        next = _list[Rng.Next(0, _list.Count)];
                    } while (_prevChosen.Contains(next.PlayfabItemId) || _treatSentryAsOne && PrevPickedSentry(next.PlayfabItemId));
                    _prevChosen.Enqueue(next.PlayfabItemId);
                    if (_prevChosen.Count > _bufferLimit)
                    {
                        _prevChosen.Dequeue();
                    }
                }
                else
                {
                    next = _list[Rng.Next(0, _list.Count)];
                }

                return next;
            }

            private bool PrevPickedSentry(string next)
            {
                Func<string, bool> isSentry = str => str.IndexOf("sentry", StringComparison.OrdinalIgnoreCase) >= 0;
                return isSentry.Invoke(next) && _prevChosen.Any(isSentry);
            }

        }
    }
}