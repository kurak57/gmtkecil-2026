using System;
using UnityEngine;

namespace YanderesFrequency.Mechanics
{
    public enum HazardPhase
    {
        Normal = 1,       // Phase 1
        PanicBlur = 2,    // Phase 2
        PagerReboot = 3,  // Phase 3
        DontMove = 4,     // Phase 4
        Paranoia = 5      // Phase 5
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

        public void UpdatePhase(int phaseNumber)
        {
            // Clamp phaseNumber between 1 and 5
            int clampedPhase = Mathf.Clamp(phaseNumber, 1, 5);
            HazardPhase newPhase = (HazardPhase)clampedPhase;

            if (CurrentPhase != newPhase)
            {
                CurrentPhase = newPhase;
                Debug.Log($"[HazardManager] Dialogue Phase active. Hazard Phase updated to: {CurrentPhase}");
                OnPhaseChanged?.Invoke(CurrentPhase);
            }
        }

        public bool IsHazardUnlocked(HazardPhase phase)
        {
            return CurrentPhase >= phase;
        }
    }
}
