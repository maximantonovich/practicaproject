using UnityEngine;

public class sword_attack : MonoBehaviour
{
    

    public GameObject melee;
    bool is_attacking = false;
    float atk_duration= 0.25f;
    float atk_timer = 0f; 
    public Animator animator;


    void Start()
    {
        // Если не назначен в инспекторе, ищем на родителе
        if (animator == null)
            animator = GetComponentInParent<Animator>();
    }


    // Update is called once per frame
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
            melee.SetActive(true);



             if (animator != null)
            {
                // Получаем направление движения из Animator
                float horizontal = animator.GetFloat("Horizontal");
                float vertical = animator.GetFloat("Vertical");
                
                // Устанавливаем параметры для анимации атаки
                animator.SetFloat("AttackHorizontal", horizontal);
                animator.SetFloat("AttackVertical", vertical);
                animator.SetTrigger("Attack");
            }
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
                animator.SetTrigger("Attack_done");
            }   
        }
    }
}



