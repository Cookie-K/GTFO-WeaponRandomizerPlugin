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
                    } while (pickedGearIds.Contains(itemId));
                    nextGearIds.Add(itemId);
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
            private readonly Queue<GearIDRange> _wepQueue = new Queue<GearIDRange>();
            private readonly List<GearIDRange> _prevChosen = new List<GearIDRange>();
            private readonly List<GearIDRange> _items;
            private readonly int _bufferLimit;
            private const int BufferMin = 2;
            private const int BufferMax = 3;
            private const int BufferThreshold = 5;

            public RngList(List<GearIDRange> items)
            {
                var distinctIds = items.GroupBy(id => id.PlayfabItemId).Select(id => id.First()).ToList();
                var countDistinct = distinctIds.Count;
                if (_treatSentryAsOne)
                {
                    countDistinct -= distinctIds.Count(id => IsSentry(id.PlayfabItemId));
                }
                _bufferLimit = countDistinct > BufferThreshold ? BufferMax : BufferMin;
                _items = items.OrderBy(_ => Rng.Next()).ToList();
                foreach (var item in _items)
                {
                    _wepQueue.Enqueue(item);
                }
            }

            public GearIDRange Next()
            {
                GearIDRange next;

                if (_type == SelectionType.SemiRandom)
                {
                    var i = 0;
                    do
                    {
                        next = _wepQueue.Dequeue();
                        if (_prevChosen.Count >= _bufferLimit)
                        { 
                            i = Rng.Next(0, _prevChosen.Count);
                            _wepQueue.Enqueue(_prevChosen[i]);
                        }
                        
                    } while (_prevChosen.Any(id => id.PlayfabItemId == next.PlayfabItemId) || _treatSentryAsOne && PrevPickedSentry(next.PlayfabItemId));

                    if (_prevChosen.Count > _bufferLimit)
                    {
                        _prevChosen.RemoveAt(i);
                    }
                    _prevChosen.Add(next);
                }
                else
                {
                    next = _items[Rng.Next(0, _items.Count)];
                }

                return next;
            }

            private bool IsSentry(string id)
            {
                return id.IndexOf("sentry", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            
            private bool PrevPickedSentry(string next)
            {
                return IsSentry(next) && _prevChosen.Any(id => IsSentry(id.PlayfabItemId));
            }

        }
    }
}