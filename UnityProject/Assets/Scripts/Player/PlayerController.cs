using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 15f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    
    private Vector2 moveInput;
    private bool isDashing;
    private bool canDash = true;
    private CharacterController controller;
    
    void Start()
    {
        // CharacterController statt Rigidbody verwenden
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }
    }
    
    void Update()
    {
        // Input
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            Vector2 input = Vector2.zero;
            if (keyboard.wKey.isPressed) input.y += 1;
            if (keyboard.sKey.isPressed) input.y -= 1;
            if (keyboard.aKey.isPressed) input.x -= 1;
            if (keyboard.dKey.isPressed) input.x += 1;
            moveInput = input.normalized;
            
            if (keyboard.spaceKey.wasPressedThisFrame && canDash && !isDashing)
            {
                StartCoroutine(Dash());
            }
        }
        
        // Bewegung direkt in Update für perfekte Synchronisation
        if (!isDashing)
        {
            Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * Time.deltaTime;
            controller.Move(move);
        }
    }
    
    System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        
        float elapsed = 0;
        while (elapsed < dashDuration)
        {
            Vector3 dashMove = new Vector3(moveInput.x, 0, moveInput.y) * dashSpeed * Time.deltaTime;
            controller.Move(dashMove);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}