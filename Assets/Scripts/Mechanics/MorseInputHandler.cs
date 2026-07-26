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
        
        public int CurrentLetterIndex => currentLetterIndex;

        [Header("Hazards State")]
        private bool isRebooting = false;
        private float rebootProgress = 0f;
        private float rebootDuration = 3f;
        private bool isDontMoveActive = false;
        private float dontMoveTimer = 0f;
        private float dontMoveDuration = 2f;
        private float nextHazardCheckTime = 0f;

        public event Action<string> OnTargetWordSet;
        public event Action<int, string> OnLetterProgress; // index, morse inputted so far
        public event Action<string> OnWordCompleted;

        public event Action<bool> OnRebootStateChanged;
        public event Action<float> OnRebootProgress;
        public event Action<bool> OnDontMoveStateChanged;

        public float CurrentHoldProgress
        {
            get
            {
                if (!isPressing) return 0f;
                return Mathf.Clamp01((Time.time - pressStartTime) / dashThreshold);
            }
        }

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
            nextHazardCheckTime = Time.time + UnityEngine.Random.Range(3f, 6f);
            isRebooting = false;
            isDontMoveActive = false;
            OnRebootStateChanged?.Invoke(false);
            OnDontMoveStateChanged?.Invoke(false);
            
            UpdateExpectedMorse();
            OnTargetWordSet?.Invoke(targetWord);
        }

        private void UpdateExpectedMorse()
        {
            while (currentLetterIndex < targetWord.Length)
            {
                expectedMorseForLetter = MorseDictionary.GetMorse(targetWord[currentLetterIndex]);
                if (!string.IsNullOrEmpty(expectedMorseForLetter))
                {
                    return; // Found a valid letter
                }
                // Character doesn't exist in Morse dictionary (like a space), skip it
                currentLetterIndex++;
            }
            expectedMorseForLetter = ""; // No more valid letters
        }

        private void Update()
        {
            if (gameManager != null && gameManager.CurrentState != GameState.Action) 
                return;

            if (isRebooting)
            {
                if (isPressing)
                {
                    rebootProgress += Time.deltaTime;
                    OnRebootProgress?.Invoke(rebootProgress / rebootDuration);
                    if (rebootProgress >= rebootDuration)
                    {
                        isRebooting = false;
                        rebootProgress = 0f;
                        isPressing = false;
                        OnRebootStateChanged?.Invoke(false);
                        Debug.Log("Pager Rebooted!");
                        lastInputTime = Time.time; // Reset hesitation
                    }
                }
                else if (rebootProgress > 0)
                {
                    rebootProgress = 0f;
                    OnRebootProgress?.Invoke(0f);
                }
                return; // Block normal processing
            }

            if (isDontMoveActive)
            {
                dontMoveTimer -= Time.deltaTime;
                if (dontMoveTimer <= 0)
                {
                    isDontMoveActive = false;
                    OnDontMoveStateChanged?.Invoke(false);
                    Debug.Log("Don't Move phase ended.");
                    lastInputTime = Time.time;
                }
                return; // Normal input blocked during this time (handled in PointerDown)
            }

            // Hazard random triggers
            if (Time.time > nextHazardCheckTime)
            {
                nextHazardCheckTime = Time.time + UnityEngine.Random.Range(5f, 10f);
                if (HazardManager.Instance != null)
                {
                    bool canReboot = HazardManager.Instance.IsHazardUnlocked(HazardPhase.PagerReboot);
                    bool canDontMove = HazardManager.Instance.IsHazardUnlocked(HazardPhase.DontMove);

                    if (canReboot && canDontMove)
                    {
                        if (UnityEngine.Random.value < 0.25f) TriggerReboot();
                        else if (UnityEngine.Random.value < 0.25f) TriggerDontMove();
                    }
                    else if (canReboot && UnityEngine.Random.value < 0.3f) TriggerReboot();
                    else if (canDontMove && UnityEngine.Random.value < 0.3f) TriggerDontMove();
                }
            }

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

        private void TriggerReboot()
        {
            isRebooting = true;
            rebootProgress = 0f;
            isPressing = false;
            OnRebootStateChanged?.Invoke(true);
            OnRebootProgress?.Invoke(0f);
            Debug.Log("Hazard: Pager Requires Reboot!");
        }

        private void TriggerDontMove()
        {
            isDontMoveActive = true;
            dontMoveTimer = dontMoveDuration;
            isPressing = false;
            OnDontMoveStateChanged?.Invoke(true);
            Debug.Log("Hazard: DON'T MOVE!");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            HandlePointerDown();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            HandlePointerUp();
        }

        // --- Added for easier EventTrigger hooking in Inspector ---
        public void OnButtonPressed()
        {
            HandlePointerDown();
        }

        public void OnButtonReleased()
        {
            HandlePointerUp();
        }
        // ----------------------------------------------------------

        private void HandlePointerDown()
        {
            if (gameManager != null && gameManager.CurrentState != GameState.Action) return;

            isPressing = true;
            pressStartTime = Time.time;
            healthSystem.SetHesitating(false);

            if (isDontMoveActive)
            {
                isDontMoveActive = false;
                OnDontMoveStateChanged?.Invoke(false);
                isPressing = false;
                Debug.Log("Player moved during DON'T MOVE! Taking damage.");
                healthSystem.TakeDamage();
            }
        }

        private void HandlePointerUp()
        {
            if (!isPressing) return;
            isPressing = false;

            if (isRebooting)
            {
                rebootProgress = 0f;
                OnRebootProgress?.Invoke(0f);
                return;
            }
            if (isDontMoveActive) return;

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

                UpdateExpectedMorse();

                if (currentLetterIndex >= targetWord.Length)
                {
                    // Word completed!
                    healthSystem.SetHesitating(false);
                    OnWordCompleted?.Invoke(targetWord);
                    Invoke(nameof(FinishActionPhase), 1.5f); // 1.5s delay before returning to Narrative
                }
                else
                {
                    OnLetterProgress?.Invoke(currentLetterIndex, currentMorseInput);
                }
            }
        }

        private void FinishActionPhase()
        {
            if (gameManager != null)
            {
                gameManager.CompleteActionPhase(); // Return to Narrative phase
            }
        }
    }
}
