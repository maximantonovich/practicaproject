using UnityEngine;

public class EnemyFlip : MonoBehaviour
{
    public Transform target;
    public float speed = 3f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (target == null) return;

        // Движение к герою
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;


        if (direction.x < 0)
            transform.localScale = new Vector3(-0.4f, 0.4f, 0.4f); 
        else if (direction.x > 0)
            transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);  

        
        animator.SetFloat("Speed", Mathf.Abs(direction.x)); 
    }
}