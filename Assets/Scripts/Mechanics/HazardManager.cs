using System;
using UnityEngine;

namespace YanderesFrequency.Mechanics
{
    public enum HazardPhase
    {
        Normal = 1,       // Shift 1-2
        PanicBlur = 2,    // Shift 3-4
        PagerReboot = 3,  // Shift 5-6
        DontMove = 4,     // Shift 7-8
        Paranoia = 5      // Shift 9-10
    }

    public class HazardManager : MonoBehaviour
    {
        public static HazardManager Instance { get; private set; }

        public HazardPhase CurrentPhase { get; private set; } = HazardPhase.Normal;

        public event Action<HazardPhase> OnPhaseChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void UpdatePhaseBasedOnShift(int shift)
        {
            HazardPhase newPhase = HazardPhase.Normal;
            
            if (shift <= 2) newPhase = HazardPhase.Normal;
            else if (shift <= 4) newPhase = HazardPhase.PanicBlur;
            else if (shift <= 6) newPhase = HazardPhase.PagerReboot;
            else if (shift <= 8) newPhase = HazardPhase.DontMove;
            else newPhase = HazardPhase.Paranoia;

            if (CurrentPhase != newPhase)
            {
                CurrentPhase = newPhase;
                Debug.Log($"[HazardManager] Shift {shift} active. Phase updated to: {CurrentPhase}");
                OnPhaseChanged?.Invoke(CurrentPhase);
            }
        }

        public bool IsHazardUnlocked(HazardPhase phase)
        {
            return CurrentPhase >= phase;
        }
    }
}
