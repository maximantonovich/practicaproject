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

        // Разворот через Scale (работает ВСЕГДА!)
        if (direction.x < 0)
            transform.localScale = new Vector3(-0.4f, 0.4f, 0.4f); // Влево
        else if (direction.x > 0)
            transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);  // Вправо

        // Для Blend Tree (если нужно)
        animator.SetFloat("Speed", Mathf.Abs(direction.x)); // Всегда положительное
    }
}