using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 15f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isDashing;
    private bool canDash = true;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                         RigidbodyConstraints.FreezeRotationZ;
    }
    
    void Update()
    {
        // Neues Input System
        moveInput = Keyboard.current.wKey.isPressed ? Vector2.up : Vector2.zero;
        moveInput += Keyboard.current.sKey.isPressed ? Vector2.down : Vector2.zero;
        moveInput += Keyboard.current.aKey.isPressed ? Vector2.left : Vector2.zero;
        moveInput += Keyboard.current.dKey.isPressed ? Vector2.right : Vector2.zero;
        moveInput.Normalize();
        
        // Dash auf Space
        if (Keyboard.current.spaceKey.wasPressedThisFrame && canDash && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }
    
    void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.linearVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, moveInput.y * moveSpeed);
        }
    }
    
    System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        
        Vector3 dashVelocity = new Vector3(moveInput.x, 0, moveInput.y) * dashSpeed;
        dashVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = dashVelocity;
        
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}