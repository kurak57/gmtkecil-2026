using System;
using System.Collections.Generic;
using UnityEngine;
using YanderesFrequency.Mechanics;

namespace YanderesFrequency.Core
{
    public enum ChoiceType
    {
        Green,
        Red
    }

    [Serializable]
    public class ChoiceData
    {
        public string word;
        public ChoiceType type;
    }

    [Serializable]
    public class DialogueEntry
    {
        public int shift = 1;
        [TextArea(2, 4)]
        public string message;
        public ChoiceData greenChoice;
        public ChoiceData redChoice;
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Narrative;

        [Header("System References")]
        [SerializeField] private PatienceAndHealthSystem healthSystem;
        [SerializeField] private MorseInputHandler morseInputHandler;

        [Header("Storyline Data")]
        [SerializeField] private List<DialogueEntry> dialogues = new List<DialogueEntry>();
        private int currentDialogueIndex = 0;
        private ChoiceType currentActiveChoiceType;
        private bool isGameStarted = false;

        [Header("CSV Importer")]
        [SerializeField] private TextAsset dialogueCsvFile;

        [Header("Debug")]
        [SerializeField] private int startShift = 1;

        [Header("Game Modes")]
        [SerializeField] private bool isExtremeMode = false;
        public bool IsExtremeMode => isExtremeMode;

        [Header("UI Panels / Objects to Toggle")]
        [SerializeField] private GameObject[] objectsToEnableOnStart;
        [SerializeField] private GameObject[] objectsToDisableOnStart;

        public event Action<DialogueEntry> OnDialogueStarted;
        public event Action OnGameWon;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Auto-add HazardManager if missing
                if (GetComponent<HazardManager>() == null)
                {
                    gameObject.AddComponent<HazardManager>();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (healthSystem == null) healthSystem = FindObjectOfType<PatienceAndHealthSystem>();
            if (morseInputHandler == null) morseInputHandler = FindObjectOfType<MorseInputHandler>();

            LoadDefaultDialoguesIfEmpty();
            // StartGame() is now called by the UI mode buttons
        }

        private void LoadDefaultDialoguesIfEmpty()
        {
            if (dialogues == null || dialogues.Count == 0)
            {
                dialogues = new List<DialogueEntry>();
                dialogues.Add(new DialogueEntry
                {
                    shift = 1,
                    message = "Hey baby, you promised to use the pager I gave you.",
                    greenChoice = new ChoiceData { word = "YEP", type = ChoiceType.Green },
                    redChoice = new ChoiceData { word = "FORCED", type = ChoiceType.Red }
                });
                dialogues.Add(new DialogueEntry
                {
                    shift = 1,
                    message = "It's so late. Why are you still awake?",
                    greenChoice = new ChoiceData { word = "YOU", type = ChoiceType.Green },
                    redChoice = new ChoiceData { word = "GAMING", type = ChoiceType.Red }
                });
                dialogues.Add(new DialogueEntry
                {
                    shift = 1,
                    message = "You're not talking to any other girls, right?",
                    greenChoice = new ChoiceData { word = "NO", type = ChoiceType.Green },
                    redChoice = new ChoiceData { word = "ANYONE", type = ChoiceType.Red }
                });
            }
        }

        public void StartGame()
        {
            if (isGameStarted) return;
            isGameStarted = true;

            // Toggle UI/Objects
            if (objectsToEnableOnStart != null)
            {
                foreach (var obj in objectsToEnableOnStart) { if (obj != null) obj.SetActive(true); }
            }
            if (objectsToDisableOnStart != null)
            {
                foreach (var obj in objectsToDisableOnStart) { if (obj != null) obj.SetActive(false); }
            }

            // Find the starting dialogue index based on the startShift debug setting
            currentDialogueIndex = 0;
            for (int i = 0; i < dialogues.Count; i++)
            {
                if (dialogues[i].shift >= startShift)
                {
                    currentDialogueIndex = i;
                    break;
                }
            }
            
            EnterNarrativePhase();
        }

        public void EnterNarrativePhase()
        {
            CurrentState = GameState.Narrative;
            Debug.Log("Entered Narrative Phase (Time Frozen)");
            
            if (healthSystem != null)
            {
                healthSystem.SetFrozen(true);
            }

            if (currentDialogueIndex < dialogues.Count)
            {
                int currentShift = dialogues[currentDialogueIndex].shift;
                if (HazardManager.Instance != null)
                {
                    HazardManager.Instance.UpdatePhaseBasedOnShift(currentShift);
                }

                OnDialogueStarted?.Invoke(dialogues[currentDialogueIndex]);
            }
            else
            {
                Debug.Log("Game Completed!");
                OnGameWon?.Invoke();
            }
        }

        public void StartActionPhase(ChoiceData choice)
        {
            CurrentState = GameState.Action;
            currentActiveChoiceType = choice.type;
            Debug.Log($"Entered Action Phase. Target Word: {choice.word}, Type: {choice.type}");

            if (healthSystem != null)
            {
                healthSystem.SetFrozen(false);
                healthSystem.ResetPatience();

                if (choice.type == ChoiceType.Red)
                {
                    // Red choice: Loses 20% patience instantly and drains 1.5x faster
                    healthSystem.SetBaseDrainMultiplier(1.5f);
                    healthSystem.ReducePatience(healthSystem.MaxPatience * 0.2f);
                }
                else
                {
                    // Green choice: Normal
                    healthSystem.SetBaseDrainMultiplier(1.0f);
                }
            }

            if (morseInputHandler != null)
            {
                morseInputHandler.SetTargetWord(choice.word);
            }
        }

        // Called by UI buttons directly
        public void OnChoiceSelected(ChoiceData choice)
        {
            StartActionPhase(choice);
        }

        public void CompleteActionPhase()
        {
            Debug.Log("Action Phase Completed! Returning to Narrative Phase.");
            
            if (currentActiveChoiceType == ChoiceType.Red && healthSystem != null)
            {
                healthSystem.GainHP(1); // Grants +1 Battery on success
            }

            currentDialogueIndex++;
            EnterNarrativePhase();
        }

        public void SetNormalMode()
        {
            if (isGameStarted) return;
            isExtremeMode = false;
            Debug.Log("Game Mode: NORMAL (Morse Helper Enabled)");
            StartGame();
        }

        public void SetExtremeMode()
        {
            if (isGameStarted) return;
            isExtremeMode = true;
            Debug.Log("Game Mode: EXTREME (Morse Helper Disabled)");
            StartGame();
        }

        [ContextMenu("Load Dialogues From CSV")]
        public void LoadDialoguesFromCSV()
        {
            if (dialogueCsvFile == null)
            {
                Debug.LogError("No CSV file assigned!");
                return;
            }

            dialogues.Clear();
            
            string[] lines = dialogueCsvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                // RegEx to split by comma, ignoring commas inside quotes
                string[] columns = System.Text.RegularExpressions.Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                
                if (columns.Length >= 6)
                {
                    DialogueEntry entry = new DialogueEntry();
                    
                    int parsedShift = 1;
                    int.TryParse(columns[0], out parsedShift);
                    entry.shift = parsedShift;

                    entry.message = columns[1].Trim('\"'); // Remove quotes
                    
                    entry.greenChoice = new ChoiceData();
                    entry.greenChoice.word = columns[2].Trim('\"');
                    
                    entry.redChoice = new ChoiceData();
                    entry.redChoice.word = columns[4].Trim('\"');
                    
                    dialogues.Add(entry);
                }
            }
            
            Debug.Log($"Loaded {dialogues.Count} dialogues from CSV.");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
