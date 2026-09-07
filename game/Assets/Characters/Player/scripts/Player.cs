using UnityEngine;

public class square_move : MonoBehaviour
{
    [Header("Здоровье")]
    public int maxHealth = 100;
    public int currentHealth;
    private bool isDead = false;

    [Header("Движение и прицел")]
    public Transform aim;
    bool is_walking = false;

    [SerializeField] private float moving_speed = 5f;

    private Rigidbody2D rb;
    private Vector2 move;
    private Animator _animator;

    public Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Start()
    {
 
        currentHealth = maxHealth;
    }

    void Update()
    {
    
        if (isDead) return;

        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");

        is_walking = move.sqrMagnitude > 0.01f;

        _animator.SetFloat("Vertical", move.y);
        _animator.SetFloat("Horizontal", move.x);
        _animator.SetFloat("Speed", move.sqrMagnitude);

        RotateAimToMouse();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        rb.linearVelocity = move.normalized * moving_speed;
    }

    void RotateAimToMouse()
    {
        if (mainCamera == null || aim == null) return;

        Vector3 mouseScreenPos = Input.mousePosition;

        float distance = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, distance));

        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        float angle = 0f;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            angle = direction.x > 0 ? 0f : 180f;
        }
        else
        {
            
            angle = direction.y > 0 ? 90f : -90f;
        }

        aim.eulerAngles = new Vector3(0, 0, angle);
    }




    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Получен урон: {damage}. Осталось HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0;

        if (_animator != null)
        {
            _animator.SetTrigger("Die");
        }

      
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

       
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null)
        {
            coll.enabled = false;
        }

       
       
        this.enabled = false;
    }
}