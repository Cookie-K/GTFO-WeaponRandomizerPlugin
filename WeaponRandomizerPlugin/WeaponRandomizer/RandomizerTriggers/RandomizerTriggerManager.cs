using System;
using SNetwork;
using UnityEngine;
using Random = System.Random;
using Timer = System.Threading.Timer;

namespace WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggers
{
    public class RandomizerTriggerManager : MonoBehaviour
    {
        private static readonly Random Rng = new Random();
        private static readonly bool RandomizeOnSecDoorOpen = ConfigManager.RandomizeOnSecDoorOpen;
        private static readonly bool RandomizeOnTimer = ConfigManager.RandomizeByInterval > 0;
        private static readonly int TimerDuration = 1000 * ConfigManager.RandomizeByInterval;
        private static readonly int TimerFuzz = 1000 * ConfigManager.IntervalFuzz;
        private static Timer _timer;
        private static bool _triggerTimedRandomize;
        private static bool _triggerSecDoorRandomize;

        public RandomizerTriggerManager(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

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
            if (TimerDuration > 0 && TimerFuzz > 0)
            {
                WeaponRandomizerManager.OnRandomize += FuzzInterval;
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

        private void StartTimer()
        {
            _timer = new Timer(arg => _triggerTimedRandomize = true, null, 0, TimerDuration);
        }

        private void FuzzInterval()
        {
            _timer.Change(0, TimerDuration + Rng.Next(-TimerFuzz, TimerFuzz));
        }

        private void EndTimer()
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
            if (TimerDuration > 0 && TimerFuzz > 0)
            {
                WeaponRandomizerManager.OnRandomize -= FuzzInterval;
            }
        }
    }
}