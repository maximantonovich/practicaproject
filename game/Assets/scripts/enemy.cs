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
        
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        if (distanceToPlayer > detectionRange)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }
        

        Vector3 direction = (target.position - transform.position).normalized;

        if (distanceToPlayer <= attackRange)
        {
   
            if (Time.time > lastAttackTime + attackCooldown && !isAttacking)
            {
                Attack();
            }

            if (!isAttacking)
            {
                transform.position += direction * speed * Time.deltaTime;
            }
        }
        else
        {
            
            transform.position += direction * speed * Time.deltaTime;
        }
    
        if (direction.x < 0)
            transform.localScale = new Vector3(-0.4f, 0.4f, 0.4f); 
        else if (direction.x > 0)
            transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
      
        if (!isAttacking)
        {
            animator.SetFloat("Speed", Mathf.Abs(direction.x));
        }
    }

    private void Attack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;

       
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (target != null)
        {
          
            square_move player = target.GetComponent<square_move>();

            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"Враг атаковал! Урон: {damage}");
            }
            else
            {
                Debug.LogWarning("Скрипт square_move не найден на игроке!");
            }
        }

      
        Invoke("ResetAttackState", 0.5f);
    }

    private void ResetAttackState()
    {
        isAttacking = false;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    
    
}