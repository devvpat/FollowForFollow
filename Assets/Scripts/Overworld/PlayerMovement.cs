using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            return;
        }
        rb.linearVelocity = moveInput * moveSpeed;
        animator.SetBool("isMoving", rb.linearVelocity != Vector2.zero);
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            animator.SetBool("isMoving", false);
            // animator.SetFloat("LastInputX", moveInput.x);
            // animator.SetFloat("LastInputY", moveInput.y);
        }
        moveInput = context.ReadValue<Vector2>();
        // animator.SetFloat("InputX", moveInput.x);
        // animator.SetFloat("InputY", moveInput.y);
    }
}
