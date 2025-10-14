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
    
        private Vector2 moveInput;
        private bool isDashing;
        private bool canDash = true;
        private CharacterController controller;
        private Animator animator;
        private string currentAnimation = "";
        private Vector3 lastMoveDirection;
        private bool isMoving; // NEU: Tracking ob wir uns bewegen

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
        
        // NEU: Rotation in LateUpdate - wird NACH allen anderen Updates ausgeführt!
        void LateUpdate()
        {
            // Wenn wir uns bewegen, ERZWINGE Laufrichtung
            if (isMoving && lastMoveDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(lastMoveDirection);
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
                Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * (moveSpeed * Time.deltaTime);
                controller.Move(move);
                
                // Bewegungsrichtung speichern
                if (move.magnitude > 0.1f)
                {
                    lastMoveDirection = move.normalized;
                    isMoving = true;
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
    }
}