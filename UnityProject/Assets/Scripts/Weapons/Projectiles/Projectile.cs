using UnityEngine;

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
        private Transform _orbitTarget; // Referenz zum Spieler für Orbit
        private float _orbitRadius;
        private float _orbitAngle;
        private float _orbitSpeed;
        private float _orbitLifetime;
        private float _orbitTimer;

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
            
            // Normale Projektile
            InitNormalProjectile(direction);
        }
        
        // Spezielle Init-Methode für Orbit-Projektile
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
            
            // Orbit-Lifetime basierend auf Range (Zeit in Sekunden)
            // Je größer die Range, desto länger kreisen die Projektile
            _orbitLifetime = config.range / config.speed;
            _orbitTimer = 0f;

            // Rigidbody deaktivieren für manuelle Bewegung
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }

            // Initiale Position setzen
            UpdateOrbitPosition();
        }

        private void InitNormalProjectile(Vector3 direction)
        {
            _isOrbiting = false;
            transform.forward = direction;

            // Physikbewegung setzen
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
        }

        private void UpdateOrbit()
        {
            // Timer aktualisieren
            _orbitTimer += Time.deltaTime;
            
            // Lifetime-Check
            if (_orbitTimer >= _orbitLifetime)
            {
                Despawn();
                return;
            }

            // Winkel aktualisieren (rotiert um das Center)
            _orbitAngle += _orbitSpeed * Time.deltaTime;
            
            // Position auf dem Orbit-Kreis berechnen
            UpdateOrbitPosition();
        }

        private void UpdateOrbitPosition()
        {
            if (_orbitTarget == null)
            {
                Despawn();
                return;
            }
            
            float angleRad = _orbitAngle * Mathf.Deg2Rad;
            
            // Orbit-Center ist IMMER die aktuelle Position des Spielers
            Vector3 orbitCenter = _orbitTarget.position;
            
            Vector3 newPos = orbitCenter + new Vector3(
                Mathf.Cos(angleRad) * _orbitRadius,
                0f,
                Mathf.Sin(angleRad) * _orbitRadius
            );
            
            transform.position = newPos;
            
            // Rotation so dass das Projektil in Bewegungsrichtung zeigt (tangential)
            float tangentAngle = _orbitAngle + 90f;
            transform.rotation = Quaternion.Euler(0f, tangentAngle, 0f);
        }

        private void UpdateNormalProjectile()
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
            
            // Nur Enemies treffen
            if (!other.CompareTag("Enemy")) return;
            
            _hasHit = true;
            _effect?.OnHit(other.gameObject, this);
            
            // Orbit-Projektile despawnen nicht bei Treffer
            // Sie können mehrere Enemies treffen während sie kreisen
            if (!_isOrbiting)
            {
                Despawn();
            }
            else
            {
                // Kurze Verzögerung bevor wieder getroffen werden kann
                _hasHit = false;
            }
        }

        private void Despawn()
        {
            _hasHit = true;
            PoolManager.Instance.DespawnAuto(gameObject);
        }

        public void ResetProjectile()
        {
            // Reset für Pool reuse
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = false;
            }

            _hasHit = false;
            _effect = null;
            _config = null;
            _startPos = Vector3.zero;
            
            // Orbit-Reset
            _isOrbiting = false;
            _orbitTarget = null;
            _orbitRadius = 0f;
            _orbitAngle = 0f;
            _orbitSpeed = 0f;
            _orbitLifetime = 0f;
            _orbitTimer = 0f;
        }
    }
}