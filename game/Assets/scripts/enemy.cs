using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Настройки врага")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1f;
    
    public Transform target;
    public float speed = 3f;
    
    private Animator animator;
    private float lastAttackTime;
    private bool isAttacking = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        // Если цель не задана, ищем игрока автоматически
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void Update()
    {
        if (target == null) return;
        
        // Вычисляем расстояние до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        
        // Проверяем, находится ли игрок в радиусе обнаружения
        if (distanceToPlayer > detectionRange)
        {
            // Игрок слишком далеко - враг стоит на месте
            animator.SetFloat("Speed", 0f);
            return;
        }
        
        // Движение к герою
        Vector3 direction = (target.position - transform.position).normalized;
        
        // Проверяем, может ли враг атаковать
        if (distanceToPlayer <= attackRange)
        {
            // Враг в радиусе атаки
            if (Time.time > lastAttackTime + attackCooldown && !isAttacking)
            {
                Attack();
            }
            // Не двигаемся во время атаки
            if (!isAttacking)
            {
                transform.position += direction * speed * Time.deltaTime;
            }
        }
        else
        {
            // Враг двигается к игроку
            transform.position += direction * speed * Time.deltaTime;
        }
        
        // Поворот врага
        if (direction.x < 0)
            transform.localScale = new Vector3(-0.4f, 0.4f, 0.4f); 
        else if (direction.x > 0)
            transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        
        // Анимация скорости (если враг не атакует)
        if (!isAttacking)
        {
            animator.SetFloat("Speed", Mathf.Abs(direction.x));
        }
    }
    
    private void Attack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        
        // Анимация атаки
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Наносим урон игроку
        if (target != null)
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($" Враг атаковал! Урон: {damage}");
            }
        }
        
        // Сбрасываем состояние атаки через время
        Invoke("ResetAttackState", 0.5f);
    }
    
    private void ResetAttackState()
    {
        isAttacking = false;
    }
    
    // Метод для установки цели извне (используется EnemySpawner)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    // Визуализация для отладки
    private void OnDrawGizmosSelected()
    {
        // Радиус обнаружения (желтый)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Радиус атаки (красный)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}