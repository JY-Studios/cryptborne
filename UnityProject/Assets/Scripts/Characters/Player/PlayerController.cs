using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 15f;
        public float dashSpeed = 15f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 1f;
        
        [Header("Rotation Settings")]
        public float rotationSpeed = 720f; // Grad pro Sekunde
    
        private Vector2 moveInput;
        private bool isDashing;
        private bool canDash = true;
        private CharacterController controller;
        private Animator animator;
        private string currentAnimation = "";
        private Vector3 movementDirection; // Aktuelle Bewegungsrichtung
        private bool isMoving;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<CharacterController>();
            }

            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("Kein Animator gefunden! Auch nicht in Children.");
            }
        }

        void Update()
        {
            HandleInput();
            HandleMovement();
            UpdateAnimation();
        }
        
        void LateUpdate()
        {
            // IMMER wenn wir Bewegungsinput haben, zur Bewegungsrichtung drehen
            if (isMoving && movementDirection.magnitude > 0.01f)
            {
                // Sanfte Rotation zur Bewegungsrichtung (optional: sofort mit Quaternion.LookRotation)
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
        }
        
        void HandleInput()
        {
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
        }
        
        void HandleMovement()
        {
            if (!isDashing)
            {
                Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
                
                if (move.magnitude > 0.1f)
                {
                    // Bewegungsrichtung speichern
                    movementDirection = move.normalized;
                    isMoving = true;
                    
                    // Bewegen
                    controller.Move(move * moveSpeed * Time.deltaTime);
                }
                else
                {
                    isMoving = false;
                }
            }
        }
        
        void UpdateAnimation()
        {
            if (animator == null) return;

            string targetAnimation;

            if (moveInput.magnitude > 0.1f)
            {
                targetAnimation = "Running_B";
            }
            else
            {
                targetAnimation = "Idle";
            }

            if (currentAnimation != targetAnimation)
            {
                animator.CrossFade(targetAnimation, 0.1f);
                currentAnimation = targetAnimation;
            }
        }
    
        System.Collections.IEnumerator Dash()
        {
            isDashing = true;
            canDash = false;
        
            float elapsed = 0;
            while (elapsed < dashDuration)
            {
                Vector3 dashMove = new Vector3(moveInput.x, 0, moveInput.y) * (dashSpeed * Time.deltaTime);
                controller.Move(dashMove);
                elapsed += Time.deltaTime;
                yield return null;
            }
        
            isDashing = false;
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }
        
        // NEU: Public Property damit andere Scripts wissen ob wir uns bewegen
        public bool IsMoving => isMoving;
    }
}