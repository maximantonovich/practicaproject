using System;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;

public class square_move : MonoBehaviour
{

    [SerializeField] private float moving_speed = 5f;

    private Rigidbody2D rb;
    private Vector2 move;
    private Animator _animator;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");
        _animator.SetFloat("Vertical", move.y);
        _animator.SetFloat("Horizontal", move.x);
        _animator.SetFloat("Speed", move.sqrMagnitude);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = move.normalized * moving_speed;
    }
}
