using System;
using UnityEngine;
using UnityEngine.EventSystems;
using YanderesFrequency.Core;

namespace YanderesFrequency.Mechanics
{
    public class MorseInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Settings")]
        [SerializeField] private float dashThreshold = 0.2f; // Time in seconds to hold for a Dash
        [SerializeField] private float hesitationThreshold = 1.5f;

        [Header("References")]
        [SerializeField] private PatienceAndHealthSystem healthSystem;

        private GameManager gameManager;
        private string targetWord;
        private int currentLetterIndex = 0;
        private string expectedMorseForLetter;
        private string currentMorseInput = "";
        
        private float pressStartTime = 0f;
        private float lastInputTime = 0f;
        private bool isPressing = false;

        public event Action<string> OnTargetWordSet;
        public event Action<int, string> OnLetterProgress; // index, morse inputted so far
        public event Action<string> OnWordCompleted;

        private void Start()
        {
            gameManager = GameManager.Instance;
            if (healthSystem == null)
            {
                healthSystem = FindObjectOfType<PatienceAndHealthSystem>();
            }
        }

        public void SetTargetWord(string word)
        {
            targetWord = word.ToUpper();
            currentLetterIndex = 0;
            currentMorseInput = "";
            lastInputTime = Time.time;
            
            UpdateExpectedMorse();
            OnTargetWordSet?.Invoke(targetWord);
        }

        private void UpdateExpectedMorse()
        {
            if (currentLetterIndex < targetWord.Length)
            {
                expectedMorseForLetter = MorseDictionary.GetMorse(targetWord[currentLetterIndex]);
            }
        }

        private void Update()
        {
            if (gameManager != null && gameManager.CurrentState != GameState.Action) 
                return;

            if (!isPressing && string.IsNullOrEmpty(targetWord) == false && currentLetterIndex < targetWord.Length)
            {
                // Check for hesitation penalty
                if (Time.time - lastInputTime > hesitationThreshold)
                {
                    healthSystem.SetHesitating(true);
                }
                else
                {
                    healthSystem.SetHesitating(false);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (gameManager != null && gameManager.CurrentState != GameState.Action) return;

            isPressing = true;
            pressStartTime = Time.time;
            healthSystem.SetHesitating(false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressing) return;
            isPressing = false;

            float duration = Time.time - pressStartTime;
            lastInputTime = Time.time;

            char inputChar = (duration >= dashThreshold) ? '-' : '.';
            ProcessInput(inputChar);
        }

        private void ProcessInput(char inputChar)
        {
            currentMorseInput += inputChar;
            OnLetterProgress?.Invoke(currentLetterIndex, currentMorseInput);

            // Check if input matches expected so far
            if (!expectedMorseForLetter.StartsWith(currentMorseInput))
            {
                // Wrong input!
                Debug.Log($"Wrong Input! Expected: {expectedMorseForLetter}, Got: {currentMorseInput}");
                healthSystem.TakeDamage(); // Candle -1
                
                // Reset input for the current letter so player can try again
                currentMorseInput = "";
                OnLetterProgress?.Invoke(currentLetterIndex, currentMorseInput);
                return;
            }

            // Check if letter is completed
            if (currentMorseInput == expectedMorseForLetter)
            {
                Debug.Log($"Letter {targetWord[currentLetterIndex]} completed!");
                currentLetterIndex++;
                currentMorseInput = "";
                healthSystem.ResetPatience(); // Reset patience on correct letter

                if (currentLetterIndex >= targetWord.Length)
                {
                    // Word completed!
                    healthSystem.SetHesitating(false);
                    OnWordCompleted?.Invoke(targetWord);
                    gameManager.CompleteActionPhase(); // Return to Narrative phase
                }
                else
                {
                    UpdateExpectedMorse();
                    OnLetterProgress?.Invoke(currentLetterIndex, currentMorseInput);
                }
            }
        }
    }
}
