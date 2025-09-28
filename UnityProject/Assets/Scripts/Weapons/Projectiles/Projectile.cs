using UnityEngine;

namespace Weapons.Projectiles
{
    public class Projectile : MonoBehaviour
    {
        private Vector3 _startPos;
        private IProjectileEffect _effect;
        private ProjectileConfig _config;
        private bool _hasHit = false;  // Verhindert mehrfache Treffer

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
            _hasHit = false;  // Bei Init zurücksetzen
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
            // Nur einmal treffen erlauben
            if (_hasHit) return;
            
            // Nur Enemies treffen (verhindert Treffer von anderen Objekten)
            if (!other.CompareTag("Enemy")) return;
            
            _hasHit = true;  // Sofort als getroffen markieren
            _effect?.OnHit(other.gameObject, this);
            Despawn();
        }

        private void Despawn()
        {
            // Sicherstellen, dass das Bullet deaktiviert wird
            _hasHit = true;  // Extra Sicherheit
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

            _hasHit = false;  // WICHTIG - Reset des Hit-Flags!
            _effect = null;
            _config = null;
            _startPos = Vector3.zero;
        }
    }
}