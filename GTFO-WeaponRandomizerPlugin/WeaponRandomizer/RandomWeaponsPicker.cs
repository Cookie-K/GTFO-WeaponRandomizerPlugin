using Gear;
using GTFO_WeaponRandomizerPlugin_Net6.WeaponRandomizer.@enum;
using Player;
using SNetwork;
using WeaponRandomizerPlugin.WeaponRandomizer.@enum;

namespace WeaponRandomizerPlugin.WeaponRandomizer
{

    /// <summary>
    /// Handles weapon selection randomly or semi randomly
    /// Random selection will select any weapons at random
    /// Semi random will use a buffer to store previous weapons and select the next time from a queue to avoid
    /// the same weapons from being picked multiple times in a row
    ///
    /// Treat sentry as one will reduce the chances of selecting a sentry such that other tool types are seen just as often 
    /// </summary>
    public class RandomWeaponsSelector
    {
        private static readonly Random Rng = new Random();
        private static readonly int MinWeapons = 4;
        private static readonly List<InventorySlot> SlotsToRandomize = new List<InventorySlot>();
        
        private static SelectionType _type;
        private static bool _treatSentryAsOne;
        private static bool _treatHammerAsOne;
        
        private readonly Dictionary<InventorySlot, List<GearIDRange>> _gearIdsBySlot;
        private readonly Dictionary<string, Dictionary<InventorySlot, RngList>> _gearListPerPlayer;

        public RandomWeaponsSelector(Dictionary<InventorySlot, List<GearIDRange>> gearIdsBySlot, SelectionType type)
        {
            _type = type;
            _gearIdsBySlot = gearIdsBySlot;
            _treatSentryAsOne = ConfigManager.TreatSentriesAsOne;
            _treatHammerAsOne = ConfigManager.TreatHammersAsOne;
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
                _gearListPerPlayer[playerName][slot] = new RngList(_gearIdsBySlot[slot], slot);
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
                    } while (_gearListPerPlayer[player.NickName][slot].Count() >= MinWeapons && pickedGearIds.Contains(itemId));
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
            private readonly Queue<GearIDRange> _wepQueue = new ();
            private readonly List<GearIDRange> _prevChosen = new ();
            private readonly List<GearIDRange> _items;
            private readonly InventorySlot _slot;
            private readonly SelectionType _selectionType;
            private readonly int _nUniqueItems;
            private const int BufferMin = 2;

            public RngList(IEnumerable<GearIDRange> items, InventorySlot slot)
            {
                _slot = slot;
                _items = items.OrderBy(_ => Rng.Next()).ToList();
                _nUniqueItems = _items.Count;
                if (_treatSentryAsOne)
                {
                    _nUniqueItems -= _items.Count(id => IsSentry(id.PlayfabItemId)) - 1;
                }
                if (_treatHammerAsOne)
                {
                    _nUniqueItems -= _items.Count(id => IsHammer(id.PlayfabItemId)) - 1;
                }

                _selectionType = _nUniqueItems <= BufferMin ? SelectionType.Random : _type;
                var bufferLimit = _nUniqueItems / 2;
                foreach (var item in _items)
                {
                    if (_prevChosen.Count >= bufferLimit)
                    {
                        _wepQueue.Enqueue(item);
                    }
                    else
                    {
                        _prevChosen.Add(item);
                    }
                }
            }

            public GearIDRange Next()
            {
                GearIDRange next;
                var index = Rng.Next(0, _prevChosen.Count - 1);

                if (_selectionType == SelectionType.SemiRandom)
                {
                    next = _wepQueue.Dequeue();
                    while (!CanPickItem(next.PlayfabItemId))
                    {
                        _wepQueue.Enqueue(next);
                        next = _wepQueue.Dequeue();
                    }
                    _wepQueue.Enqueue(_prevChosen[index]);
                }
                else
                {
                    do
                    {
                        next = _items[Rng.Next(0, _items.Count)];
                    } while (!CanPickItem(next.PlayfabItemId));
                }
                
                _prevChosen.RemoveAt(index);
                _prevChosen.Add(next);
                
                return next;
            }

            public int Count()
            {
                return _items.Count;
            }

            private static bool IsSentry(string id)
            {
                return id.IndexOf("sentry", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            
            private static bool IsHammer(string id)
            {
                var ids = Enum.GetNames(typeof(Hammers));
                return ids.Contains(id);
            }
            
            private bool CanPickItem(string next)
            {
                if (_slot != InventorySlot.GearClass && _slot != InventorySlot.GearMelee)
                {
                    return true;
                }
                
                var isSentry = IsSentry(next);
                var isHammer = IsHammer(next);
                var probabilityCheck = Rng.Next(_nUniqueItems) == 0;

                if (_type == SelectionType.SemiRandom)
                {
                    var isSentryInPrev = _prevChosen.Any(id => IsSentry(id.PlayfabItemId));
                    var isHammerInPrev = _prevChosen.Any(id => IsHammer(id.PlayfabItemId));
                    return _slot == InventorySlot.GearClass ? (!_treatSentryAsOne || !isSentry || !isSentryInPrev) : (!_treatHammerAsOne || !isHammer || !isHammerInPrev) && probabilityCheck;
                }

                return _slot == InventorySlot.GearClass ? (!_treatSentryAsOne || !isSentry) : (!_treatHammerAsOne || !isHammer) && probabilityCheck;
            }

        }
    }
}