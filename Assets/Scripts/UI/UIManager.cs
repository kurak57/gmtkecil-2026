using UnityEngine;
using UnityEngine.UI;
using YanderesFrequency.Core;
using YanderesFrequency.Mechanics;

namespace YanderesFrequency.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text hpText; // Could be images of candles in full game
        [SerializeField] private Image patienceBar;
        [SerializeField] private Text targetWordText;
        [SerializeField] private Text currentInputText;
        [SerializeField] private Text morseHelperText;
        
        [Header("Choice Buttons")]
        [SerializeField] private GameObject choicesPanel;
        [SerializeField] private Button choiceButton1;
        [SerializeField] private Button choiceButton2;

        [Header("System References")]
        [SerializeField] private PatienceAndHealthSystem healthSystem;
        [SerializeField] private MorseInputHandler inputHandler;

        private void Start()
        {
            if (healthSystem != null)
            {
                healthSystem.OnCandlesChanged += UpdateHPUI;
                healthSystem.OnPatienceChanged += UpdatePatienceUI;
                healthSystem.OnGameOver += HandleGameOver;
            }

            if (inputHandler != null)
            {
                inputHandler.OnTargetWordSet += InitializeWordUI;
                inputHandler.OnLetterProgress += UpdateInputProgressUI;
                inputHandler.OnWordCompleted += HandleWordCompleted;
            }

            // Setup demo choice buttons
            if (choiceButton1 != null)
            {
                choiceButton1.onClick.AddListener(() => OnChoiceSelected("DIAM"));
            }
            if (choiceButton2 != null)
            {
                choiceButton2.onClick.AddListener(() => OnChoiceSelected("LARI"));
            }
            
            ShowChoices(); // Start in narrative phase
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (healthSystem != null)
            {
                healthSystem.OnCandlesChanged -= UpdateHPUI;
                healthSystem.OnPatienceChanged -= UpdatePatienceUI;
                healthSystem.OnGameOver -= HandleGameOver;
            }
            if (inputHandler != null)
            {
                inputHandler.OnTargetWordSet -= InitializeWordUI;
                inputHandler.OnLetterProgress -= UpdateInputProgressUI;
                inputHandler.OnWordCompleted -= HandleWordCompleted;
            }
        }

        private void OnChoiceSelected(string word)
        {
            choicesPanel.SetActive(false);
            GameManager.Instance.StartActionPhase(word);
        }

        private void ShowChoices()
        {
            choicesPanel.SetActive(true);
            if (targetWordText != null) targetWordText.text = "";
            if (currentInputText != null) currentInputText.text = "";
            if (morseHelperText != null) morseHelperText.text = "";
        }

        private void InitializeWordUI(string word)
        {
            if (targetWordText != null) targetWordText.text = word;
            if (currentInputText != null) currentInputText.text = "";
            
            // Show the morse code for the first letter as a helper
            if (morseHelperText != null && word.Length > 0)
            {
                morseHelperText.text = $"Expected: {MorseDictionary.GetMorse(word[0])}";
            }
        }

        private void UpdateInputProgressUI(int letterIndex, string currentInput)
        {
            if (currentInputText != null)
            {
                currentInputText.text = currentInput;
            }

            // Update helper text to show expected morse for current letter
            if (morseHelperText != null && inputHandler != null)
            {
                // We use reflection or just access the target word from UI.
                // For simplicity, we just use the UI text.
                string word = targetWordText != null ? targetWordText.text : "";
                if (letterIndex < word.Length)
                {
                     morseHelperText.text = $"Expected: {MorseDictionary.GetMorse(word[letterIndex])}";
                }
            }
        }

        private void HandleWordCompleted(string word)
        {
            if (targetWordText != null) targetWordText.text = "SUCCESS!";
            if (currentInputText != null) currentInputText.text = "";
            if (morseHelperText != null) morseHelperText.text = "";
            
            // Simulate returning to narrative
            Invoke(nameof(ShowChoices), 2f);
        }

        private void UpdateHPUI(int candles)
        {
            if (hpText != null)
            {
                hpText.text = $"Candles: {candles}";
            }
        }

        private void UpdatePatienceUI(float current, float max)
        {
            if (patienceBar != null)
            {
                patienceBar.fillAmount = current / max;
            }
        }

        private void HandleGameOver()
        {
            if (targetWordText != null) targetWordText.text = "GAME OVER";
            choicesPanel.SetActive(false);
        }
    }
}
