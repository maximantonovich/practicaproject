using System;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;

public class square_move : MonoBehaviour
{



    public Transform aim;
    bool is_walking = false;




    [SerializeField] private float moving_speed = 5f;

    private Rigidbody2D rb;
    private Vector2 move;
    private Animator _animator;


    private readonly Quaternion upRotation = Quaternion.Euler(0, 0, 0);
    private readonly Quaternion downRotation = Quaternion.Euler(0, 0, 180); // углы для атаки
    private readonly Quaternion leftRotation = Quaternion.Euler(0, 0, 90);
    private readonly Quaternion rightRotation = Quaternion.Euler(0, 0, -90);



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");

        is_walking = move.sqrMagnitude > 0.01f;

        _animator.SetFloat("Vertical", move.y);
        _animator.SetFloat("Horizontal", move.x);
        _animator.SetFloat("Speed", move.sqrMagnitude);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = move.normalized * moving_speed;
        if(is_walking)
        {
            if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            {
                
                aim.rotation = move.x > 0 ? rightRotation : leftRotation;
            }
            else
            {
                
                aim.rotation = move.y > 0 ? upRotation : downRotation;
            }
        }

    }





}
