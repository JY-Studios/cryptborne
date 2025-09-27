using System.Collections.Generic;
using Characters.Enemies;
using UnityEngine;
using Weapons.Data;

namespace Weapons.Behaviours.Melee
{
    public class MeleeBehaviorInstant : IWeaponBehavior<MeleeWeaponData>
    {
        private string targetTag;

        public MeleeBehaviorInstant(string targetTag)
        {
            this.targetTag = targetTag;
        }

        public void Attack(MeleeWeaponData data, Transform player)
        {
            var position = player.position;
            var direction = player.forward;
            
            Collider[] hits = Physics.OverlapSphere(position, data.radius);
            var alreadyHit = new HashSet<GameObject>();

            foreach (var c in hits)
            {
                var go = c.gameObject;
                if (!go.CompareTag(targetTag)) continue; // nur Objekte mit diesem Tag
                if (alreadyHit.Contains(go)) continue;

                var dirToTarget = (c.transform.position - position).normalized;
                if (Vector3.Angle(direction, dirToTarget) <= data.arcAngle * 0.5f)
                {
                    alreadyHit.Add(go);

                    var health = go.GetComponent<EnemyHealth>();
                    if (health != null) health.TakeDamage(data.damage);

                    var rb = go.GetComponent<Rigidbody>();
                    if (rb != null) rb.AddForce(dirToTarget * 5f, ForceMode.Impulse);
                }
            }

            if (data.weaponPrefab != null)
                GameObject.Instantiate(data.weaponPrefab, position, Quaternion.LookRotation(direction));
        }
    }
}