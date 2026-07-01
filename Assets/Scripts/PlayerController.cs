using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    bool canDoubleJump;

    [Header("Buffer && Coyote Jump")]
    [SerializeField] private float bufferJumpWindow = .25f;
    private float bufferJumpActivated = -1;
    [SerializeField] private float coyoteJumpWindow = .5f;
    private float coyoteJumpActivated = -1;

    [Header("Wall Interactions")]
    [SerializeField] private float wallJumpDuration = .6f;
    [SerializeField] private Vector2 wallJumpForce;
    private bool isWallJumping;

    [Header("KnockBack")]
    [SerializeField] private float knokbackDuration = 1.0f;
    [SerializeField] private Vector2 knockbackPower;
    private bool isKnocked;

    [Header("Colliision")]
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

        if (isKnocked) return;

        HandleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleColission();
        HandleAnimations();
    }

    public void KnockBack()
    {
        if (isKnocked) return;

        StartCoroutine(KnockbackRoutine());
        anim.SetTrigger("knockBack");
        rb.velocity = new Vector2(knockbackPower.x * -facingDirection, knockbackPower.y);
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnocked = true;
        yield return new WaitForSeconds(knokbackDuration);
        isKnocked = false;
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

        if (rb.velocity.y <= 0)
        {
            Debug.Log("Activated Coyote Jump");
            ActivateCoyoteJump();
        }
    }

    private void HandleLanding()
    {
        isAirBorne = false;
        canDoubleJump = true;

        AttemptBufferJump();
    }
    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpButton();
            requestBufferJump();
        }
    }

    #region Buffer & Coyote Jump
    private void requestBufferJump()
    {
        if (isAirBorne)
            bufferJumpActivated = Time.time;
    }

    private void AttemptBufferJump()
    {
        if (Time.time < bufferJumpActivated + bufferJumpWindow)
        {
            bufferJumpActivated = Time.time -1;
            Jump();
            
        }
    }
    private void ActivateCoyoteJump()=> coyoteJumpActivated = Time.time;
    private void CancelCoyoteJump() => coyoteJumpActivated = Time.time - 1;
    #endregion

    private void jumpButton()
    {
        bool coyoteJumpAvaliable = Time.time < coyoteJumpActivated + coyoteJumpWindow;

    
        if (isGrounded || coyoteJumpAvaliable)
        {
          
            Jump();
        }
        else if (isWallDetected && !isGrounded)
        {
            WallJump();
        }
        else if (canDoubleJump && isAirBorne && !coyoteJumpAvaliable) 
        {
            DoubleJump();
        }

        CancelCoyoteJump();
    }

    private void HandleFlip()
    {
        if (xInput < 0 && facingRight || xInput > 0 && !facingRight)
            Flip();
    }
    private void Jump() => rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    private void DoubleJump()
    {
        isWallJumping = false;
        canDoubleJump = false;
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
    }
    
    private void WallJump()
    {
        canDoubleJump = true;
        rb.velocity = new Vector2(wallJumpForce.x * -facingDirection, wallJumpForce.y);
        Flip();

        StopAllCoroutines();
        StartCoroutine(WallJumpRoutine());
    }

    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;

    }

    private void HandleWallSlide()
    {
        bool canWallSlide = isWallDetected && rb.velocity.y < 0;
        float yModifier = yInput < 0 ? 1 : 0.05f;
        if (canWallSlide == false) return;

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * yModifier);

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
        if (isWallJumping) return;
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
