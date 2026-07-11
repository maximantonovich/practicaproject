using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Настройки здоровья")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    
    public System.Action<int, int> OnHealthChanged; 
    public System.Action OnPlayerDeath;
    
    private void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($" Игрок получил {damage} урона. Осталось здоровья: {currentHealth}/{maxHealth}");
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private void Die()
    {
        Debug.Log("Игрок умер!");
        OnPlayerDeath?.Invoke();
        
        // Можно добавить эффекты смерти
        // gameObject.SetActive(false);
    }
}