using UnityEngine;

namespace Weapons.Projectiles
{
    public class Projectile : MonoBehaviour
    {
        private Vector3 _startPos;
        private IProjectileEffect _effect;
        private ProjectileConfig _config;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Init(ProjectileConfig config, Vector3 direction, IProjectileEffect effect)
        {
            _config = config;
            _effect = effect;
            _startPos = transform.position;
            transform.forward = direction;

            // Physikbewegung setzen
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;       // Reset wichtig für Reuse
                _rb.angularVelocity = Vector3.zero;
                _rb.linearVelocity = direction * _config.speed;
            }
        }

        private void Update()
        {
            // Nur falls kein Rigidbody da ist → manuelle Bewegung
            if (_rb == null)
                transform.position += transform.forward * (_config.speed * Time.deltaTime);

            // Reichweite checken
            if (Vector3.Distance(_startPos, transform.position) > _config.range)
                Despawn();
        }

        private void OnTriggerEnter(Collider other)
        {
            _effect?.OnHit(other.gameObject, this);
            Despawn();
        }

        private void Despawn()
        {
            PoolManager.Instance.DespawnAuto(gameObject);
        }

        public void ResetProjectile()
        {
            // Reset für Pool reuse
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }

            _effect = null;
            _config = null;
            _startPos = Vector3.zero;
        }
    }
}