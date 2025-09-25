using Enemies.States;
using UnityEngine;

namespace Enemies
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
            // Beim Pool-Recycling neu initialisieren
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

            // Startzustand setzen
            currentState = null;
            SwitchState(idleState);
        }
        
        public void ResetEnemy()
        {
            // Combat Timer zurücksetzen
            nextAttackTime = 0f;
            
            // State komplett zurücksetzen
            if (currentState != null)
                currentState.Exit(this);
            
            currentState = null;
            
            if (controller != null)
            {
                controller.enabled = false;
                // KEINE Position-Änderung hier!
                // transform.position wird vom PoolManager gesetzt
            }
            
            // Player-Referenz neu holen
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            
            // Zurück zum Idle State
            SwitchState(idleState);
            
            // CharacterController wieder enablen NACH State-Wechsel
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