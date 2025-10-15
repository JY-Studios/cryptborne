using Characters.Enemies.States;
using UnityEngine;

namespace Characters.Enemies
{
    public class EnemyStateManager : MonoBehaviour
    {
        private EnemyBaseState currentState;
        
        public EnemyIdleState idleState = new EnemyIdleState();
        public EnemyChaseState chaseState = new EnemyChaseState();
        public EnemyAttackState attackState = new EnemyAttackState();
        public EnemyDeadState deadState = new EnemyDeadState();
        
        [HideInInspector] public Transform player;
        [HideInInspector] public CharacterController controller;
        [HideInInspector] public EnemyHealth health;
        [HideInInspector] public Animator animator; // NEU: Animator-Referenz
        
        [Header("Movement")]
        public float moveSpeed = 2f;
        public float detectionRange = 8f;
        public float attackRange = 1.5f;
        public float separationRadius = 1f;

        [Header("Combat")]
        public int attackDamage = 1;
        public float attackCooldown = 1.5f;
        [HideInInspector] public float nextAttackTime = 0f;
        
        void Start()
        {
            Initialize();
        }
        
        void OnEnable()
        {
            Initialize();
        }
        
        void Initialize()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;

            if (controller == null)
                controller = GetComponent<CharacterController>();
                
            if (controller == null)
                controller = gameObject.AddComponent<CharacterController>();

            if (health == null)
                health = GetComponent<EnemyHealth>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>(); // ← GENAU WIE BEIM PLAYER!

            if (animator != null)
            {
                Debug.Log($"Enemy Animator found on: {animator.gameObject.name}");
            }
            else
            {
                Debug.LogError("No Animator found in children!");
            }

            // Startzustand setzen
            currentState = null;
            SwitchState(idleState);
        }
        
        public void ResetEnemy()
        {
            nextAttackTime = 0f;
            
            if (currentState != null)
                currentState.Exit(this);
            
            currentState = null;
            
            if (controller != null)
            {
                controller.enabled = false;
            }
            
            // Animator zurücksetzen
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }
            
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            
            SwitchState(idleState);
            
            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        void Update()
        {
            if (currentState != null)
                currentState.Update(this);
        }

        public void SwitchState(EnemyBaseState newState)
        {
            if (currentState != null)
                currentState.Exit(this);

            currentState = newState;

            if (currentState != null)
                currentState.Enter(this);
        }
    }
}