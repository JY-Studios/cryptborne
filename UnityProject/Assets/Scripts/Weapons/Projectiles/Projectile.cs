using System.Collections.Generic;
using UnityEngine;
using Weapons.VFX;

namespace Weapons.Projectiles
{
    public class Projectile : MonoBehaviour
    {
        private Vector3 _startPos;
        private IProjectileEffect _effect;
        private ProjectileConfig _config;
        private bool _hasHit = false;
        private Rigidbody _rb;

        // Orbit-spezifische Variablen
        private bool _isOrbiting = false;
        private Transform _orbitTarget;
        private float _orbitRadius;
        private float _orbitAngle;
        private float _orbitSpeed;
        private float _orbitLifetime;
        private float _orbitTimer;

        // Hit-Cooldown System für Orbit-Projektile
        private Dictionary<GameObject, float> _enemyHitCooldowns = new Dictionary<GameObject, float>();
        private const float HIT_COOLDOWN_DURATION = 0.5f;

        // Public Property für Tracking
        public bool IsOrbiting => _isOrbiting;

        // STATISCHE LISTE aller aktiven Orbit-Projektile
        private static List<Projectile> _allOrbitProjectiles = new List<Projectile>();
        public static List<Projectile> AllOrbitProjectiles => _allOrbitProjectiles;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Init(ProjectileConfig config, Vector3 direction, IProjectileEffect effect)
        {
            _config = config;
            _effect = effect;
            _startPos = transform.position;
            _hasHit = false;
            
            InitNormalProjectile(direction);
        }
        
        public void InitOrbit(ProjectileConfig config, float startAngle, Transform target, IProjectileEffect effect)
        {
            _config = config;
            _effect = effect;
            _startPos = transform.position;
            _hasHit = false;
            _isOrbiting = true;
            
            _orbitTarget = target;
            _orbitRadius = config.orbitRadius;
            _orbitAngle = startAngle;
            _orbitSpeed = config.orbitSpeed;
            
            _orbitLifetime = config.range / config.speed;
            _orbitTimer = 0f;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }

            if (!_allOrbitProjectiles.Contains(this))
            {
                _allOrbitProjectiles.Add(this);
                Debug.Log($"Orbit projectile registered. Total orbit projectiles: {_allOrbitProjectiles.Count}");
            }

            UpdateOrbitPosition();
        }

        private void InitNormalProjectile(Vector3 direction)
        {
            _isOrbiting = false;
            transform.forward = direction;

            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.linearVelocity = direction * _config.speed;
            }
        }

        private void Update()
        {
            if (_isOrbiting)
            {
                UpdateOrbit();
            }
            else
            {
                UpdateNormalProjectile();
            }
            
            CleanupHitCooldowns();
        }

        private void UpdateOrbit()
        {
            if (_orbitTarget == null)
            {
                Debug.LogWarning("Orbit projectile lost target, despawning");
                Despawn();
                return;
            }

            _orbitTimer += Time.deltaTime;
            
            if (_orbitTimer >= _orbitLifetime)
            {
                Despawn();
                return;
            }

            _orbitAngle += _orbitSpeed * Time.deltaTime;
            UpdateOrbitPosition();
        }

        private void UpdateOrbitPosition()
        {
            if (_orbitTarget == null)
            {
                Debug.LogWarning("Orbit target is null in UpdateOrbitPosition");
                Despawn();
                return;
            }
            
            float angleRad = _orbitAngle * Mathf.Deg2Rad;
            Vector3 orbitCenter = _orbitTarget.position;
            
            Vector3 newPos = orbitCenter + new Vector3(
                Mathf.Cos(angleRad) * _orbitRadius,
                0f,
                Mathf.Sin(angleRad) * _orbitRadius
            );
            
            transform.position = newPos;
            
            float tangentAngle = _orbitAngle + 90f;
            transform.rotation = Quaternion.Euler(0f, tangentAngle, 0f);
        }

        private void UpdateNormalProjectile()
        {
            if (_rb == null)
                transform.position += transform.forward * (_config.speed * Time.deltaTime);

            if (Vector3.Distance(_startPos, transform.position) > _config.range)
                Despawn();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;
            
            Debug.Log($"Projectile hit enemy: {other.gameObject.name}");
            
            // Impact Position berechnen
            Vector3 impactPosition = transform.position;
            Vector3 impactNormal = (transform.position - other.transform.position).normalized;
            
            // Normale Projektile: Einmal treffen und despawnen
            if (!_isOrbiting)
            {
                if (_hasHit) return;
                
                _hasHit = true;
                _effect?.OnHit(other.gameObject, this);
                
                // Impact Particle spawnen
                Debug.Log($"Spawning impact effect at {impactPosition}");
                VFXManager.SpawnImpactEffect(impactPosition, impactNormal);
                
                Despawn();
                return;
            }
            
            // Orbit-Projektile: Hit-Cooldown System
            GameObject enemy = other.gameObject;
            
            if (CanHitEnemy(enemy))
            {
                _effect?.OnHit(enemy, this);
                
                // Impact Particle spawnen auch für Orbit-Projektile
                Debug.Log($"Spawning impact effect (orbit) at {impactPosition}");
                VFXManager.SpawnImpactEffect(impactPosition, impactNormal);
                
                RegisterHit(enemy);
            }
        }

        private bool CanHitEnemy(GameObject enemy)
        {
            if (!_enemyHitCooldowns.ContainsKey(enemy))
                return true;
            
            return Time.time >= _enemyHitCooldowns[enemy];
        }

        private void RegisterHit(GameObject enemy)
        {
            _enemyHitCooldowns[enemy] = Time.time + HIT_COOLDOWN_DURATION;
        }

        private void CleanupHitCooldowns()
        {
            List<GameObject> toRemove = new List<GameObject>();
            
            foreach (var kvp in _enemyHitCooldowns)
            {
                if (kvp.Key == null || Time.time >= kvp.Value + 5f)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in toRemove)
            {
                _enemyHitCooldowns.Remove(key);
            }
        }

        private void Despawn()
        {
            _hasHit = true;
            
            if (_isOrbiting)
            {
                _allOrbitProjectiles.Remove(this);
                Debug.Log($"Orbit projectile despawned. Remaining: {_allOrbitProjectiles.Count}");
            }
            
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.DespawnAuto(gameObject);
            }
            else
            {
                Debug.LogWarning("PoolManager not available, destroying projectile directly");
                Destroy(gameObject);
            }
        }

        public void ResetProjectile()
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = false;
            }

            if (_isOrbiting)
            {
                _allOrbitProjectiles.Remove(this);
            }

            _hasHit = false;
            _effect = null;
            _config = null;
            _startPos = Vector3.zero;
            
            _isOrbiting = false;
            _orbitTarget = null;
            _orbitRadius = 0f;
            _orbitAngle = 0f;
            _orbitSpeed = 0f;
            _orbitLifetime = 0f;
            _orbitTimer = 0f;
            
            _enemyHitCooldowns.Clear();
        }

        public static void DespawnAllOrbitProjectiles()
        {
            Debug.Log($"Despawning {_allOrbitProjectiles.Count} orbit projectiles");
            
            var projectilesToDespawn = new List<Projectile>(_allOrbitProjectiles);
            
            foreach (var proj in projectilesToDespawn)
            {
                if (proj != null && proj.gameObject != null)
                {
                    proj.Despawn();
                }
            }
            
            _allOrbitProjectiles.Clear();
        }
    }
}