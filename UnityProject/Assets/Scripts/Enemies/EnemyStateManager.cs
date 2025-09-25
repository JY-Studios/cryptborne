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
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;

            controller = GetComponent<CharacterController>();
            if (controller == null)
                controller = gameObject.AddComponent<CharacterController>();

            health = GetComponent<EnemyHealth>();

            // Startzustand
            SwitchState(idleState);
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