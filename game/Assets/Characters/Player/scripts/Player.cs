using UnityEngine;

public class square_move : MonoBehaviour
{
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

    void Update()
    {
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
        rb.linearVelocity = move.normalized * moving_speed;
    }

    void RotateAimToMouse()
    {
        if (mainCamera == null || aim == null) return;

      
        Vector3 mouseScreenPos = Input.mousePosition;
        

        
        float distance = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3( mouseScreenPos.x, mouseScreenPos.y, distance));
        
       

       
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
       

       
        float angle = 0f;
        
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Горизонтально
            angle = direction.x > 0 ? 0f : 180f;
        }
        else
        {
            // Вертикально
            angle = direction.y > 0 ? 90f : -90f;
            
        }

       
        aim.eulerAngles = new Vector3(0, 0, angle);
    }
}