using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement Details")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;

    private bool canDoubleJump;

    [Header("Colliision Info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;
    private bool isAirBorne;
    private bool isWallDetected;

    private float xInput;
    private float yInput;

    private bool facingRight = true;
    private int facingDirection = 1;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        UpdateAirborneStatus();

        HandleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleColission();
        HandleAnimations();
    }

    private void HandleWallSlide()
    {
        bool canWallSlide = isWallDetected && rb.velocity.y < 0;

        float yModifier = yInput < 0 ? 1 : 0.05f;

        if (canWallSlide == false) return;

       

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * yModifier);

    }
    private void UpdateAirborneStatus()
    {
        if (isAirBorne && isGrounded)
            HandleLanding();

        if (!isGrounded && !isAirBorne)
            BecomeAirborne();
    }

    private void BecomeAirborne()
    {
        isAirBorne = true;
    }

    private void HandleLanding()
    {
        isAirBorne = false;
        canDoubleJump = true;
    }

    private void HandleFlip()
    {
        if (xInput < 0 && facingRight || xInput > 0 && !facingRight)
            Flip();
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
            jumpButton();
    }

    private void jumpButton()
    {
        if (isGrounded)
        {
            Jump();
        }
        else if (canDoubleJump)
        {
            DoubleJump();
        }


    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }
    private void DoubleJump()
    {
        canDoubleJump = false;
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
    }

    private void HandleColission()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
    }


    void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallDetected", isWallDetected);
    }
    private void HandleMovement()
    {
        if (isWallDetected) return;
        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
    }

    private void Flip()
    {
        facingDirection *= -1;
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDirection), transform.position.y));
    }

}
