using System;
using UnityEngine;
using UnityEngine.UI;
using YanderesFrequency.Core;

namespace YanderesFrequency.Mechanics
{
    public enum CharacterExpression
    {
        Idle,
        Affectionate,
        Impatient,
        Hostile,
        EerieCommand,
        EnragedStrike,
        Blush
    }

    [Serializable]
    public struct ExpressionSpriteMap
    {
        public CharacterExpression expression;
        public Sprite sprite;
    }

    public class CharacterExpressionManager : MonoBehaviour
    {
        [SerializeField] private Image characterImage;
        [SerializeField] private ExpressionSpriteMap[] expressionMap;

        [Header("System References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MorseInputHandler inputHandler;
        [SerializeField] private PatienceAndHealthSystem healthSystem;

        private void Start()
        {
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
            if (inputHandler == null) inputHandler = FindObjectOfType<MorseInputHandler>();
            if (healthSystem == null) healthSystem = FindObjectOfType<PatienceAndHealthSystem>();

            if (gameManager != null)
            {
                gameManager.OnDialogueStarted += OnDialogueStarted;
            }

            if (inputHandler != null)
            {
                inputHandler.OnWordCompleted += OnWordCompleted;
                inputHandler.OnDontMoveStateChanged += OnDontMoveStateChanged;
            }

            if (healthSystem != null)
            {
                healthSystem.OnHesitationStateChanged += OnHesitationStateChanged;
                healthSystem.OnDamageTaken += OnDamageTaken;
            }

            SetExpression(CharacterExpression.Idle);
        }

        private void OnDestroy()
        {
            if (gameManager != null) gameManager.OnDialogueStarted -= OnDialogueStarted;
            if (inputHandler != null)
            {
                inputHandler.OnWordCompleted -= OnWordCompleted;
                inputHandler.OnDontMoveStateChanged -= OnDontMoveStateChanged;
            }
            if (healthSystem != null)
            {
                healthSystem.OnHesitationStateChanged -= OnHesitationStateChanged;
                healthSystem.OnDamageTaken -= OnDamageTaken;
            }
        }

        private void OnDialogueStarted(DialogueEntry entry)
        {
            CancelInvoke(nameof(ResetToIdle));
            SetExpression(CharacterExpression.Idle);
        }

        private void OnWordCompleted(string word)
        {
            CancelInvoke(nameof(ResetToIdle));
            
            if (gameManager != null)
            {
                if (gameManager.CurrentActiveChoiceType == ChoiceType.Green)
                {
                    SetExpression(CharacterExpression.Affectionate);
                }
                else if (gameManager.CurrentActiveChoiceType == ChoiceType.Red)
                {
                    SetExpression(CharacterExpression.Hostile);
                }
            }
        }

        private void OnDontMoveStateChanged(bool isActive)
        {
            if (isActive)
            {
                SetExpression(CharacterExpression.EerieCommand);
            }
            else
            {
                SetExpression(CharacterExpression.Idle);
            }
        }

        private void OnHesitationStateChanged(bool isHesitating)
        {
            if (isHesitating)
            {
                SetExpression(CharacterExpression.Impatient);
            }
            else
            {
                // Return to Idle (unless WordCompleted is about to override this)
                SetExpression(CharacterExpression.Idle);
            }
        }

        private void OnDamageTaken()
        {
            SetExpression(CharacterExpression.EnragedStrike);
            // Revert back to Idle after 1.5 seconds of jumpscare/strike
            CancelInvoke(nameof(ResetToIdle));
            Invoke(nameof(ResetToIdle), 1.5f);
        }

        private void ResetToIdle()
        {
            SetExpression(CharacterExpression.Idle);
        }

        // Can be called by external events (e.g., Bad Ending UI Button)
        public void SetExpression(CharacterExpression expression)
        {
            if (characterImage == null || expressionMap == null) return;

            foreach (var map in expressionMap)
            {
                if (map.expression == expression)
                {
                    characterImage.sprite = map.sprite;
                    return;
                }
            }
        }
    }
}
