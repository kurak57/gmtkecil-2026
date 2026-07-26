using System;
using UnityEngine;
using UnityEngine.UI;

namespace YanderesFrequency.Mechanics
{
    [Serializable]
    public class PhaseBackgroundData
    {
        public HazardPhase phase;
        public Sprite backgroundSprite;
    }

    public class PhaseBackgroundManager : MonoBehaviour
    {
        [Header("Target Renderer")]
        [Tooltip("Assign an Image component if the background is a UI element.")]
        [SerializeField] private Image targetImage;
        
        [Tooltip("Assign a SpriteRenderer component if the background is a 2D object in the scene.")]
        [SerializeField] private SpriteRenderer targetSpriteRenderer;

        [Header("Phase Backgrounds")]
        [SerializeField] private PhaseBackgroundData[] backgroundData;

        private void Start()
        {
            if (HazardManager.Instance != null)
            {
                HazardManager.Instance.OnPhaseChanged += HandlePhaseChanged;
                // Set initial background based on the current phase at startup
                HandlePhaseChanged(HazardManager.Instance.CurrentPhase);
            }
            else
            {
                Debug.LogWarning("[PhaseBackgroundManager] HazardManager Instance is not found!");
            }
        }

        private void OnDestroy()
        {
            if (HazardManager.Instance != null)
            {
                HazardManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            }
        }

        private void HandlePhaseChanged(HazardPhase newPhase)
        {
            Sprite newSprite = null;

            // Find matching sprite for the new phase
            foreach (var data in backgroundData)
            {
                if (data.phase == newPhase)
                {
                    newSprite = data.backgroundSprite;
                    break;
                }
            }

            if (newSprite != null)
            {
                if (targetImage != null)
                {
                    targetImage.sprite = newSprite;
                }
                
                if (targetSpriteRenderer != null)
                {
                    targetSpriteRenderer.sprite = newSprite;
                }
                
                Debug.Log($"[PhaseBackgroundManager] Background updated for phase: {newPhase}");
            }
        }
    }
}
