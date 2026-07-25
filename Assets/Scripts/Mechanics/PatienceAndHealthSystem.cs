using System;
using UnityEngine;

namespace YanderesFrequency.Mechanics
{
    public class PatienceAndHealthSystem : MonoBehaviour
    {
        [Header("Health (Battery)")]
        [SerializeField] private int maxHP = 5;
        public int MaxHP => maxHP;
        private int currentHP;

        [Header("Patience (Kesabaran)")]
        [SerializeField] private float maxPatience = 100f;
        [SerializeField] private float baseDrainRate = 10f; // per second
        private float currentPatience;
        
        private bool isFrozen = true;
        private bool isHesitating = false;
        private float baseDrainMultiplier = 1f;

        public event Action<int> OnHPChanged;
        public event Action<float, float> OnPatienceChanged; // current, max
        public event Action OnGameOver;
        public event Action<bool> OnHesitationStateChanged;
        public event Action OnDamageTaken;

        public float MaxPatience => maxPatience;

        private void Start()
        {
            currentHP = maxHP;
            currentPatience = maxPatience;
            
            OnHPChanged?.Invoke(currentHP);
            OnPatienceChanged?.Invoke(currentPatience, maxPatience);
        }

        private void Update()
        {
            if (isFrozen || currentPatience <= 0) return;

            // Drains 2x faster if hesitating, and applies baseDrainMultiplier (for Red choices)
            float drainMultiplier = (isHesitating ? 2f : 1f) * baseDrainMultiplier;
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
            if (isHesitating != hesitating)
            {
                isHesitating = hesitating;
                OnHesitationStateChanged?.Invoke(hesitating);
            }
        }

        public void SetBaseDrainMultiplier(float multiplier)
        {
            baseDrainMultiplier = multiplier;
        }

        public void ResetPatience()
        {
            currentPatience = maxPatience;
            OnPatienceChanged?.Invoke(currentPatience, maxPatience);
        }

        public void ReducePatience(float amount)
        {
            currentPatience -= amount;
            if (currentPatience <= 0)
            {
                currentPatience = 0;
                TakeDamage();
            }
            OnPatienceChanged?.Invoke(currentPatience, maxPatience);
        }

        public void GainHP(int amount)
        {
            currentHP += amount;
            if (currentHP > maxHP) currentHP = maxHP;
            OnHPChanged?.Invoke(currentHP);
        }

        public void TakeDamage()
        {
            if (currentHP <= 0) return;

            currentHP--;
            OnHPChanged?.Invoke(currentHP);
            OnDamageTaken?.Invoke();

            if (currentHP <= 0)
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
