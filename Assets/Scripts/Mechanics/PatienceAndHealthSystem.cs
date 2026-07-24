using System;
using UnityEngine;

namespace YanderesFrequency.Mechanics
{
    public class PatienceAndHealthSystem : MonoBehaviour
    {
        [Header("Health (Candles)")]
        [SerializeField] private int maxCandles = 5;
        private int currentCandles;

        [Header("Patience (Kesabaran)")]
        [SerializeField] private float maxPatience = 100f;
        [SerializeField] private float baseDrainRate = 10f; // per second
        private float currentPatience;
        
        private bool isFrozen = true;
        private bool isHesitating = false;

        public event Action<int> OnCandlesChanged;
        public event Action<float, float> OnPatienceChanged; // current, max
        public event Action OnGameOver;

        private void Start()
        {
            currentCandles = maxCandles;
            currentPatience = maxPatience;
            
            OnCandlesChanged?.Invoke(currentCandles);
            OnPatienceChanged?.Invoke(currentPatience, maxPatience);
        }

        private void Update()
        {
            if (isFrozen || currentPatience <= 0) return;

            // Drains 2x faster if hesitating
            float drainMultiplier = isHesitating ? 2f : 1f;
            currentPatience -= baseDrainRate * drainMultiplier * Time.deltaTime;

            if (currentPatience <= 0)
            {
                currentPatience = 0;
                TakeDamage(); // Patience ran out!
            }

            OnPatienceChanged?.Invoke(currentPatience, maxPatience);
        }

        public void SetFrozen(bool frozen)
        {
            isFrozen = frozen;
        }

        public void SetHesitating(bool hesitating)
        {
            isHesitating = hesitating;
        }

        public void ResetPatience()
        {
            currentPatience = maxPatience;
            OnPatienceChanged?.Invoke(currentPatience, maxPatience);
        }

        public void TakeDamage()
        {
            if (currentCandles <= 0) return;

            currentCandles--;
            OnCandlesChanged?.Invoke(currentCandles);

            if (currentCandles <= 0)
            {
                OnGameOver?.Invoke();
            }
            else
            {
                // Reset patience after taking damage to give player a chance
                ResetPatience();
            }
        }
    }
}
