using Game.Interfaces;
using UnityEngine;

namespace Game.Gameplay.Characters
{    
    public class Entity : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth;
        public int Health { get; private set; }

        protected virtual void Start()
        {
            Health = maxHealth;
        }
        public void TakeDamage(int damage)
        {
            if (damage < 0)
            {
                return;
            }

            Health -= damage;
            if (Health <= 0)
            {
                Die();
            }
        }
        protected virtual void Die()
        {
            Debug.Log("Умер");
        }

    }
}