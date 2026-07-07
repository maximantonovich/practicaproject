using UnityEngine;

public class sword_attack : MonoBehaviour
{
    public GameObject melee;
    bool is_attacking = false;
    float atk_duration = 0.25f;
    float atk_timer = 0f; 
    public Animator animator;

    public int damage = 20;
    private bool hasDealtDamage = false;

    public Camera mainCamera;

    void Start()
    {
        if (animator == null)
            animator = GetComponentInParent<Animator>();
        
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (melee != null)
        {
            BoxCollider2D col = melee.GetComponent<BoxCollider2D>();
            if (col != null)
                col.isTrigger = true;
            melee.SetActive(false);
        }
    }

    void Update()
    {
        check_melee_time();

        if(Input.GetKeyDown(KeyCode.E) || (Input.GetMouseButtonDown(0)))
        {
            Attack();   
        }
    }

    void Attack()
    {
        if(!is_attacking)
        {
            is_attacking = true;
            hasDealtDamage = false;
            melee.SetActive(true);

            if (animator != null)
            {
                Vector2 attackDirection = GetAttackDirection();
                
               
                
                
                animator.SetFloat("AttackHorizontal", attackDirection.x);
                animator.SetFloat("AttackVertical", attackDirection.y);
                animator.SetTrigger("Attack");
            }
        }
    }

    Vector2 GetAttackDirection()
    {
        
        Vector3 mouseScreenPos = Input.mousePosition;
        
       
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(
            mouseScreenPos.x, 
            mouseScreenPos.y, 
            Mathf.Abs(transform.position.z - mainCamera.transform.position.z) // Важно для 2D!
        ));
        
        
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        
       
        
        return direction;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!is_attacking || hasDealtDamage) return;

        Health enemyHealth = other.GetComponent<Health>();
        
        if (enemyHealth != null && enemyHealth.IsAlive())
        {
            enemyHealth.TakeDamage(damage);
            hasDealtDamage = true;
            Debug.Log($"⚔️ Атакован {other.name} на {damage} урона!");
        }
    }

    void check_melee_time()
    {
        if(is_attacking)
        {
            atk_timer += Time.deltaTime;
            if(atk_timer >= atk_duration)
            {
                is_attacking = false;
                melee.SetActive(false);
                atk_timer = 0;
                hasDealtDamage = false;
                animator.SetTrigger("Attack_done");
            }   
        }
    }
}