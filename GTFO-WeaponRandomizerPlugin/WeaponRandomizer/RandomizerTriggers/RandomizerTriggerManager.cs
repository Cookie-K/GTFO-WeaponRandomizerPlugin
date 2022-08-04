using SNetwork;
using UnityEngine;
using Random = System.Random;

namespace WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggers
{
    /// <summary>
    /// Manages triggers for the weapon randomizer 
    /// </summary>
    public class RandomizerTriggerManager : MonoBehaviour
    {
        private static readonly Random Rng = new Random();
        private static readonly bool RandomizeOnSecDoorOpen = ConfigManager.RandomizeOnSecDoorOpen;
        private static readonly bool RandomizeOnTimer = ConfigManager.RandomizeByInterval > 0;
        private static readonly int TimerDuration = 1000 * ConfigManager.RandomizeByInterval;
        private static readonly int TimerAlterTime = 1000 * (ConfigManager.AlterInterval > 0 ? ConfigManager.AlterInterval : 0);
        
        private static Timer _timer;
        private static bool _triggerTimedRandomize;
        private static bool _triggerSecDoorRandomize;

        private void Start()
        {
            if (RandomizeOnTimer)
            {
                RandomizerTriggerPatcher.OnGameStateChange += OnGameStateChanged;
            }
            if (RandomizeOnSecDoorOpen)
            {
                RandomizerTriggerPatcher.OnSecDoorOpen += OnSecDoorOpen;
            }
            if (TimerDuration > 0 && TimerAlterTime > 0)
            {
                WeaponRandomizerManager.OnRandomize += AlterInterval;
            }
        }

        private void Update()
        {
            if (!SNet.IsMaster) return;
            if (!_triggerTimedRandomize && !_triggerSecDoorRandomize) return;
            
            WeaponRandomizerManager.Randomize();
            _triggerTimedRandomize = false;
            _triggerSecDoorRandomize = false;
        }
        
        public void OnSecDoorOpen()
        {
            if (RandomizeOnSecDoorOpen)
            {
                _triggerSecDoorRandomize = true;
            }
        }

        public void OnGameStateChanged(eGameStateName state)
        {
            switch (state)
            {
                case eGameStateName.InLevel:
                    StartTimer();
                    break;
                case eGameStateName.AfterLevel:
                    EndTimer();
                    break;
            }
        }

        private static void StartTimer()
        {
            _timer = new Timer(arg => _triggerTimedRandomize = true, null, 0, TimerDuration);
        }

        private static void AlterInterval()
        {
            var timerDuration = TimerDuration + Rng.Next(-TimerAlterTime, TimerAlterTime);
            _timer.Change(timerDuration, timerDuration);
        }

        private static void EndTimer()
        {
            _timer.Dispose();
        }

        private void OnDestroy()
        {
            if (RandomizeOnTimer)
            {
                RandomizerTriggerPatcher.OnGameStateChange -= OnGameStateChanged;
            }
            if (RandomizeOnSecDoorOpen)
            {
                RandomizerTriggerPatcher.OnSecDoorOpen -= OnSecDoorOpen;
            }
            if (TimerDuration > 0 && TimerAlterTime > 0)
            {
                WeaponRandomizerManager.OnRandomize -= AlterInterval;
            }
        }
    }
}