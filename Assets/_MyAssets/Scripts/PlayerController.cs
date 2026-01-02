using System;
using Unity.Netcode;
using UnityEngine;
using PrimeTween;
using Unity.Netcode.Components;

public class PlayerController : NetworkBehaviour
{
    public bool canControl = true;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Vector2 groundedOffset;
    [SerializeField] private LayerMask groundLayer;

    [Header("Debug")]
    [SerializeField] private bool showDebug;

    private Rigidbody2D rb;
    private NetworkAnimator networkAnimator;
    private bool isGrounded;
    private float horizontalInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        networkAnimator = GetComponent<NetworkAnimator>();
    }

    private void Update()
    {
        PlayerFacing();
        if (!IsOwner) return;
        if (!canControl)
        {
            horizontalInput = 0f;
            return;
        }

        horizontalInput = Input.GetAxis("Horizontal");

        bool isMoving = Mathf.Abs(horizontalInput) > 0.1f;
        networkAnimator.Animator.SetBool("IsMoving", isMoving);

        isGrounded = CheckGrounded();
        networkAnimator.Animator.SetBool("IsJumping", isGrounded ? false : true);

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void PlayerFacing()
    {
        // Flip character based on movement direction
        if (rb.linearVelocity.x > 0.1f)
        {
            // Tween.Scale(transform, new Vector3(1f, 1f, 1f), 0.2f);
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (rb.linearVelocity.x < -0.1f)
        {
            // Tween.Scale(transform, new Vector3(-1f, 1f, 1f), 0.2f);
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    private void FixedUpdate()
    {
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(horizontalInput * (speed / (isGrounded ? 1f : 2f)), rb.linearVelocity.y);
    }


    private bool CheckGrounded()
    {
        return Physics2D.Raycast(transform.position + (Vector3)groundedOffset, Vector3.down, 0.1f, groundLayer);
    }

    private void OnDrawGizmos()
    {
        if (!showDebug) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position + (Vector3)groundedOffset, Vector3.down * 0.1f);
    }
}
