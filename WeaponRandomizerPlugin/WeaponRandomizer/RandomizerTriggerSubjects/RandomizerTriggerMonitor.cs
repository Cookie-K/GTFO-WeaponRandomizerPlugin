using System;
using System.Threading;
using System.Timers;
using UnityEngine;
using Object = System.Object;

namespace WeaponRandomizerPlugin.WeaponRandomizer.RandomizerTriggerSubjects
{
    public class RandomizerTriggerMonitor : MonoBehaviour
    {

        public static RandomizerTriggerMonitor Instance { get; private set; }
        private static KeyCode KeyTrigger = KeyCode.Alpha0;
        
        private static bool RandomizeOnSecDoorOpen = true;
        private static bool TimerStarted;
        private static bool SwapedOnPrevFrame; 
        private static bool Swaped;
        private static float TimeCounter;
        private static int TimerDuration = 10; // every 10s
        
        public RandomizerTriggerMonitor(IntPtr intPtr) : base(intPtr)
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyTrigger))
            {
                WeaponRandomizer.Instance.Randomize();
            }

            if (TimerStarted)
            {
                TimeCounter += Time.deltaTime;
                if (Math.Round(TimeCounter) % TimerDuration == 0)
                {
                    if (!SwapedOnPrevFrame)
                    {
                        Swaped = true;
                        WeaponRandomizer.Instance.Randomize();
                    }
                }
                else
                {
                    Swaped = false;
                }

                SwapedOnPrevFrame = Swaped;
            }
            
            
        }

        public void OnGameStateChanged(eGameStateName state)
        {
            switch (state)
            {
                case eGameStateName.StopElevatorRide:
                    break;
                case eGameStateName.InLevel:
                    StartTimer();
                    break;
                case eGameStateName.AfterLevel:
                    EndTimer();
                    break;
            }
        }

        public void OnSecurityDoorOpen()
        {
            if (RandomizeOnSecDoorOpen)
            {
                WeaponRandomizer.Instance.Randomize();
            }
        }
        
        private void StartTimer()
        {
            TimerStarted = true;
            TimeCounter = 0;
        }

        private void EndTimer()
        {
            TimerStarted = false;
            TimeCounter = 0;
        }
        
    }
}