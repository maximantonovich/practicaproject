using System;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;

public class square_move : MonoBehaviour
{

    [SerializeField] private float moving_speed = 5f;

    private Rigidbody2D rb;
    private Vector2 move;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");
    
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = move.normalized * moving_speed;
    }
}
