using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YanderesFrequency.Core;
using YanderesFrequency.Mechanics;

namespace YanderesFrequency.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject mainGameplayUI; // The parent containing all the gameplay HUD
        [SerializeField] private GameObject introPanel; // The panel shown during the intro sequence
        [SerializeField] private TextMeshProUGUI hpText; // Obsolete (kept for compatibility)
        [SerializeField] private Image hpImageBar; // Used for the heart images (Filled Horizontal)
        [SerializeField] private Image patienceBar;
        [SerializeField] private TextMeshProUGUI targetWordText;
        [SerializeField] private TextMeshProUGUI currentInputText;
        [SerializeField] private TextMeshProUGUI morseHelperText;
        [SerializeField] private TextMeshProUGUI narrativeMessageText; // Added for story messages
        [SerializeField] private Image[] holdProgressBars; // Added array for tap/hold visual feedback (e.g. left and right images)
        
        [Header("Hazard UI")]
        [SerializeField] private GameObject hazardAlertPanel;
        [SerializeField] private TextMeshProUGUI hazardText;
        [SerializeField] private Image hazardProgressBar;

        [Header("Word Highlight Colors")]
        [SerializeField] private Color typedLetterColor = Color.green;
        [SerializeField] private Color currentLetterColor = Color.white;
        [SerializeField] private Color untypedLetterColor = Color.red;
        [SerializeField] private Color currentInputColor = Color.black;

        [Header("Choice Buttons")]
        [SerializeField] private GameObject choicesPanel;
        [SerializeField] private Button choiceButton1; // Green Choice
        [SerializeField] private Button choiceButton2; // Red Choice

        [Header("System References")]
        [SerializeField] private PatienceAndHealthSystem healthSystem;
        [SerializeField] private MorseInputHandler inputHandler;

        [Header("Ending Screen")]
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private Image endingImage;
        [SerializeField] private TextMeshProUGUI endingTitleText;
        [SerializeField] private TextMeshProUGUI endingDescriptionText;
        
        [Header("Ending Contents")]
        [SerializeField] private Sprite gameOverSprite;
        [SerializeField] private string gameOverTitle = "GAME OVER";
        [SerializeField, TextArea(2, 4)] private string gameOverDescription = "She got you...";
        
        [SerializeField] private Sprite trueEndingSprite;
        [SerializeField] private string trueEndingTitle = "YOU SURVIVED";
        [SerializeField, TextArea(2, 4)] private string trueEndingDescription = "The police arrived. She fled into the night...\nYou are safe. (True Ending)";
        
        [SerializeField] private Sprite badEndingSprite;
        [SerializeField] private string badEndingTitle = "DOOR OPENED";
        [SerializeField, TextArea(2, 4)] private string badEndingDescription = "She is inside. There is nowhere to run...\nForever Yours. (Bad Ending)";

        private Vector3 originalTargetWordPos;
        private Vector3 originalInputTextPos;
        private float paranoiaGlitchTimer = 0f;

        private void Start()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHPChanged += UpdateHPUI;
                healthSystem.OnPatienceChanged += UpdatePatienceUI;
                healthSystem.OnGameOver += HandleGameOver;
            }

            if (inputHandler != null)
            {
                inputHandler.OnTargetWordSet += InitializeWordUI;
                inputHandler.OnLetterProgress += UpdateInputProgressUI;
                inputHandler.OnWordCompleted += HandleWordCompleted;
                inputHandler.OnRebootStateChanged += HandleRebootState;
                inputHandler.OnRebootProgress += HandleRebootProgress;
                inputHandler.OnDontMoveStateChanged += HandleDontMoveState;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnIntroStarted += HandleIntroStarted;
                GameManager.Instance.OnDialogueStarted += HandleDialogueStarted;
                GameManager.Instance.OnTrueEnding += HandleTrueEnding;
                GameManager.Instance.OnBadEnding += HandleBadEnding;
            }

            if (hazardAlertPanel != null) hazardAlertPanel.SetActive(false);

            if (targetWordText != null) originalTargetWordPos = targetWordText.rectTransform.localPosition;
            if (currentInputText != null) originalInputTextPos = currentInputText.rectTransform.localPosition;
        }

        private void Update()
        {
            if (holdProgressBars != null && inputHandler != null)
            {
                float progress = inputHandler.CurrentHoldProgress;
                foreach (var bar in holdProgressBars)
                {
                    if (bar != null)
                    {
                        bar.fillAmount = progress;
                    }
                }
            }

            // Phase 2: Panic Blur / Jitter Effect
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Action)
            {
                bool hasBlur = HazardManager.Instance != null && HazardManager.Instance.IsHazardUnlocked(HazardPhase.PanicBlur);
                bool hasParanoia = HazardManager.Instance != null && HazardManager.Instance.IsHazardUnlocked(HazardPhase.Paranoia);

                if (hasBlur)
                {
                    ApplyJitterEffect();
                }
                else
                {
                    ResetJitterEffect();
                }

                if (hasParanoia)
                {
                    paranoiaGlitchTimer += Time.deltaTime;
                    if (paranoiaGlitchTimer > 0.1f) // 10 fps glitch
                    {
                        paranoiaGlitchTimer = 0f;
                        UpdateWordHighlight(inputHandler.CurrentLetterIndex);
                    }
                }
            }
            else
            {
                ResetJitterEffect();
            }
        }

        private void ApplyJitterEffect()
        {
            float jitterAmount = 3f;
            if (targetWordText != null)
                targetWordText.rectTransform.localPosition = originalTargetWordPos + (Vector3)UnityEngine.Random.insideUnitCircle * jitterAmount;
            if (currentInputText != null)
                currentInputText.rectTransform.localPosition = originalInputTextPos + (Vector3)UnityEngine.Random.insideUnitCircle * jitterAmount;
        }

        private void ResetJitterEffect()
        {
            if (targetWordText != null && targetWordText.rectTransform.localPosition != originalTargetWordPos)
                targetWordText.rectTransform.localPosition = originalTargetWordPos;
            if (currentInputText != null && currentInputText.rectTransform.localPosition != originalInputTextPos)
                currentInputText.rectTransform.localPosition = originalInputTextPos;
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (healthSystem != null)
            {
                healthSystem.OnHPChanged -= UpdateHPUI;
                healthSystem.OnPatienceChanged -= UpdatePatienceUI;
                healthSystem.OnGameOver -= HandleGameOver;
            }
            if (inputHandler != null)
            {
                inputHandler.OnTargetWordSet -= InitializeWordUI;
                inputHandler.OnLetterProgress -= UpdateInputProgressUI;
                inputHandler.OnWordCompleted -= HandleWordCompleted;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnIntroStarted -= HandleIntroStarted;
                GameManager.Instance.OnDialogueStarted -= HandleDialogueStarted;
                GameManager.Instance.OnTrueEnding -= HandleTrueEnding;
                GameManager.Instance.OnBadEnding -= HandleBadEnding;
            }
        }

        private void HandleRebootState(bool isActive)
        {
            if (hazardAlertPanel != null) hazardAlertPanel.SetActive(isActive);
            if (isActive && hazardText != null) hazardText.text = "PAGER ERROR\nHOLD TO REBOOT";
            if (hazardProgressBar != null)
            {
                hazardProgressBar.gameObject.SetActive(isActive);
                hazardProgressBar.fillAmount = 0f;
            }
        }

        private void HandleRebootProgress(float progress)
        {
            if (hazardProgressBar != null) hazardProgressBar.fillAmount = progress;
        }

        private void HandleDontMoveState(bool isActive)
        {
            if (hazardAlertPanel != null) hazardAlertPanel.SetActive(isActive);
            if (isActive && hazardText != null) hazardText.text = "DON'T MOVE!";
            if (hazardProgressBar != null) hazardProgressBar.gameObject.SetActive(false);
        }

        private void HandleIntroStarted()
        {
            if (introPanel != null) introPanel.SetActive(true);
            if (mainGameplayUI != null) mainGameplayUI.SetActive(false);
            if (choicesPanel != null) choicesPanel.SetActive(false);
        }

        private void HandleDialogueStarted(DialogueEntry dialogue)
        {
            if (introPanel != null) introPanel.SetActive(false);
            if (mainGameplayUI != null) mainGameplayUI.SetActive(true);
            choicesPanel.SetActive(true);

            if (targetWordText != null) targetWordText.text = "";
            if (currentInputText != null) currentInputText.text = "";
            if (morseHelperText != null) morseHelperText.text = "";

            if (narrativeMessageText != null)
            {
                narrativeMessageText.text = dialogue.message;
            }

            if (choiceButton1 != null)
            {
                choiceButton1.onClick.RemoveAllListeners();
                choiceButton1.onClick.AddListener(() => OnChoiceSelected(dialogue.greenChoice));
                var btn1Text = choiceButton1.GetComponentInChildren<TextMeshProUGUI>();
                if (btn1Text != null) btn1Text.text = dialogue.greenChoice.word + " (Green)";
            }
            
            if (choiceButton2 != null)
            {
                choiceButton2.onClick.RemoveAllListeners();
                choiceButton2.onClick.AddListener(() => OnChoiceSelected(dialogue.redChoice));
                var btn2Text = choiceButton2.GetComponentInChildren<TextMeshProUGUI>();
                if (btn2Text != null) btn2Text.text = dialogue.redChoice.word + " (Red)";
            }
        }

        private void OnChoiceSelected(ChoiceData choice)
        {
            choicesPanel.SetActive(false);
            if (narrativeMessageText != null) narrativeMessageText.text = "";
            
            GameManager.Instance.OnChoiceSelected(choice);
            AudioManager.Instance.PlayOneShotSFX(AudioName.SFXButtonClick);
        }

        private string currentTargetWord = "";

        private void InitializeWordUI(string word)
        {
            currentTargetWord = word;
            int startIndex = (inputHandler != null) ? inputHandler.CurrentLetterIndex : 0;
            UpdateWordHighlight(startIndex);
            
            if (currentInputText != null) currentInputText.text = "";
            
            // Show the morse code for the first letter as a helper
            if (morseHelperText != null && word.Length > 0)
            {
                if (GameManager.Instance != null && GameManager.Instance.IsExtremeMode)
                {
                    morseHelperText.text = "";
                }
                else
                {
                    morseHelperText.text = $"{MorseDictionary.GetMorse(word[0])}";
                }
            }
        }

        private void UpdateWordHighlight(int letterIndex)
        {
            if (targetWordText == null || string.IsNullOrEmpty(currentTargetWord)) return;
            
            if (letterIndex >= currentTargetWord.Length)
            {
                targetWordText.text = currentTargetWord;
                return;
            }

            string before = currentTargetWord.Substring(0, letterIndex);
            string current = currentTargetWord.Substring(letterIndex, 1);
            string after = currentTargetWord.Substring(letterIndex + 1);

            // Phase 5: Paranoia text glitch
            if (HazardManager.Instance != null && HazardManager.Instance.IsHazardUnlocked(HazardPhase.Paranoia))
            {
                char[] arr = after.ToCharArray();
                for (int i = 0; i < arr.Length; i++)
                {
                    if (UnityEngine.Random.value < 0.3f)
                    {
                        arr[i] = (char)UnityEngine.Random.Range(65, 91); // A-Z
                    }
                }
                after = new string(arr);
            }

            // Highlight colors based on inspector settings
            string hexTyped = ColorUtility.ToHtmlStringRGBA(typedLetterColor);
            string hexCurrent = ColorUtility.ToHtmlStringRGBA(currentLetterColor);
            string hexUntyped = ColorUtility.ToHtmlStringRGBA(untypedLetterColor);

            targetWordText.text = $"<color=#{hexTyped}>{before}</color><color=#{hexCurrent}><u><b>{current}</b></u></color><color=#{hexUntyped}>{after}</color>";
        }

        private void UpdateInputProgressUI(int letterIndex, string currentInput)
        {
            if (currentInputText != null)
            {
                currentInputText.text = currentInput;
                currentInputText.color = currentInputColor;
            }

            UpdateWordHighlight(letterIndex);

            // Update helper text to show expected morse for current letter
            if (morseHelperText != null && inputHandler != null)
            {
                if (GameManager.Instance != null && GameManager.Instance.IsExtremeMode)
                {
                    morseHelperText.text = "";
                }
                else if (letterIndex < currentTargetWord.Length)
                {
                     morseHelperText.text = $"{MorseDictionary.GetMorse(currentTargetWord[letterIndex])}";
                }
            }
        }

        private void HandleWordCompleted(string word)
        {
            if (targetWordText != null) targetWordText.text = "SUCCESS!";
            if (currentInputText != null) currentInputText.text = "";
            if (morseHelperText != null) morseHelperText.text = "";
        }

        private void UpdateHPUI(int hp)
        {
            if (hpText != null)
            {
                hpText.text = $"Battery/HP: {hp}";
            }
            if (hpImageBar != null && healthSystem != null)
            {
                hpImageBar.fillAmount = (float)hp / healthSystem.MaxHP;
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
            if (choicesPanel != null) choicesPanel.SetActive(false);
            if (mainGameplayUI != null) mainGameplayUI.SetActive(false);
            
            if (endingImage != null && gameOverSprite != null)
            {
                endingImage.gameObject.SetActive(true);
                endingImage.sprite = gameOverSprite;
            }
            if (endingTitleText != null) endingTitleText.text = gameOverTitle;
            if (endingDescriptionText != null) endingDescriptionText.text = gameOverDescription;
            if (endingPanel != null) endingPanel.SetActive(true);
        }

        private void HandleTrueEnding()
        {
            if (choicesPanel != null) choicesPanel.SetActive(false);
            if (mainGameplayUI != null) mainGameplayUI.SetActive(false);
            
            if (endingImage != null && trueEndingSprite != null)
            {
                endingImage.gameObject.SetActive(true);
                endingImage.sprite = trueEndingSprite;
            }
            if (endingTitleText != null) endingTitleText.text = trueEndingTitle;
            if (endingDescriptionText != null) endingDescriptionText.text = trueEndingDescription;
            if (endingPanel != null) endingPanel.SetActive(true);
        }

        private void HandleBadEnding()
        {
            if (choicesPanel != null) choicesPanel.SetActive(false);
            if (mainGameplayUI != null) mainGameplayUI.SetActive(false);
            
            if (endingImage != null && badEndingSprite != null)
            {
                endingImage.gameObject.SetActive(true);
                endingImage.sprite = badEndingSprite;
            }
            if (endingTitleText != null) endingTitleText.text = badEndingTitle;
            if (endingDescriptionText != null) endingDescriptionText.text = badEndingDescription;
            if (endingPanel != null) endingPanel.SetActive(true);
        }
    }
}
