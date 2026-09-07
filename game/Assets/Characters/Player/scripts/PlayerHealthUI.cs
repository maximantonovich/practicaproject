using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    
    private PlayerHealth playerHealth;
    
    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth не найден!");
            return;
        }
        
        // Если слайдер не назначен, ищем в дочерних объектах
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();
        
        if (healthText == null)
            healthText = GetComponentInChildren<Text>();
        
        // Обновляем UI
        UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        
        // Подписываемся на события
        playerHealth.OnHealthChanged += UpdateHealthUI;
    }
    
    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
    
    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthUI;
    }
}