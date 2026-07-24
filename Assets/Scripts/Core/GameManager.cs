using UnityEngine;
using YanderesFrequency.Mechanics;

namespace YanderesFrequency.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Narrative;

        [Header("System References")]
        [SerializeField] private PatienceAndHealthSystem healthSystem;
        [SerializeField] private MorseInputHandler morseInputHandler;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
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
        }

        // Call this when the player clicks a UI Choice Button (e.g. "LARI")
        public void StartActionPhase(string chosenWord)
        {
            CurrentState = GameState.Action;
            Debug.Log($"Entered Action Phase. Target Word: {chosenWord}");

            if (healthSystem != null)
            {
                healthSystem.SetFrozen(false);
                healthSystem.ResetPatience();
            }

            if (morseInputHandler != null)
            {
                morseInputHandler.SetTargetWord(chosenWord);
            }
        }

        // Called by MorseInputHandler when the word is successfully completed
        public void CompleteActionPhase()
        {
            Debug.Log("Action Phase Completed! Returning to Narrative Phase.");
            EnterNarrativePhase();
            
            // In a full game, this is where you'd trigger the next narrative dialogue
            // e.g. DialogueManager.ShowNextLine();
        }
    }
}
