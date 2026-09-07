using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;

    private square_move player;

    private void Start()
    {
        player = FindAnyObjectByType<square_move>();

        if (player == null)
        {
            Debug.LogWarning("Игрок со скриптом square_move не найден на сцене!");
            return;
        }

        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();

        if (healthText == null)
            healthText = GetComponentInChildren<Text>();
    }

    private void Update()
    {
        if (player == null) return;
        if (healthSlider != null)
        {
            healthSlider.maxValue = player.maxHealth;
            healthSlider.value = player.currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{player.currentHealth} / {player.maxHealth}";
        }
    }
}