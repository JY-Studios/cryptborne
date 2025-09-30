using UnityEngine;

namespace Weapons.VFX
{
    /// <summary>
    /// Managed alle visuellen Effekte für Waffen (Muzzle Flash, Impact, etc.)
    /// </summary>
    public static class VFXManager
    {
        /// <summary>
        /// Spawnt einen Muzzle Flash Effekt an der angegebenen Position
        /// </summary>
        public static void SpawnMuzzleFlash(Vector3 position, Vector3 direction)
        {
            GameObject muzzleFlash = PoolManager.Instance.Spawn(
                poolName: "MuzzleFlash",
                position: position,
                rotation: Quaternion.LookRotation(direction)
            );

            if (muzzleFlash != null)
            {
                ParticleSystem ps = muzzleFlash.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                    
                    // Auto-Despawn nach Particle Lifetime
                    float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
                    CoroutineHelper.Instance.DelayedAction(() =>
                    {
                        if (muzzleFlash != null)
                            PoolManager.Instance.DespawnAuto(muzzleFlash);
                    }, lifetime);
                }
            }
        }

        /// <summary>
        /// Spawnt einen Impact Effekt an der angegebenen Position
        /// </summary>
        public static void SpawnImpactEffect(Vector3 position, Vector3 normal)
        {
            GameObject impact = PoolManager.Instance.Spawn(
                poolName: "ImpactEffect",
                position: position,
                rotation: Quaternion.LookRotation(normal)
            );

            if (impact != null)
            {
                ParticleSystem ps = impact.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                    
                    float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
                    CoroutineHelper.Instance.DelayedAction(() =>
                    {
                        if (impact != null)
                            PoolManager.Instance.DespawnAuto(impact);
                    }, lifetime);
                }
            }
        }
    }

    /// <summary>
    /// Helper für Coroutines (da statische Klassen keine Coroutines haben können)
    /// </summary>
    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;
        public static CoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineHelper");
                    _instance = go.AddComponent<CoroutineHelper>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public void DelayedAction(System.Action action, float delay)
        {
            StartCoroutine(DelayedActionCoroutine(action, delay));
        }

        private System.Collections.IEnumerator DelayedActionCoroutine(System.Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}