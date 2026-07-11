using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Настройки")]
    public float invincibilityTime = 0.5f;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;


    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
                isInvincible = false;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($" {gameObject.name} получил {damage} урона. HP: {currentHealth}/{maxHealth}");

       
        if (invincibilityTime > 0)
        {
            isInvincible = true;
            invincibilityTimer = invincibilityTime;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}