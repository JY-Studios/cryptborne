using UnityEngine;

namespace Weapons.Audio
{
    /// <summary>
    /// Verwaltet alle Sound-Effekte für Waffen
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _instance;
        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SoundManager");
                    _instance = go.AddComponent<SoundManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Weapon Sounds")]
        public AudioClip shootSound;
        public AudioClip impactSound;
        
        [Header("Audio Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float weaponVolume = 0.5f;
        [Range(0f, 1f)] public float impactVolume = 0.3f;

        private AudioSource audioSource;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSource();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        void InitializeAudioSource()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; // 2D Sound
        }

        /// <summary>
        /// Spielt einen Schuss-Sound ab
        /// </summary>
        public void PlayShootSound()
        {
            if (shootSound != null)
            {
                PlaySound(shootSound, weaponVolume);
            }
            else
            {
                Debug.LogWarning("SoundManager: shootSound is not assigned!");
            }
        }

        /// <summary>
        /// Spielt einen Impact-Sound ab
        /// </summary>
        public void PlayImpactSound()
        {
            if (impactSound != null)
            {
                PlaySound(impactSound, impactVolume);
            }
            else
            {
                Debug.LogWarning("SoundManager: impactSound is not assigned!");
            }
        }

        /// <summary>
        /// Spielt einen beliebigen Sound mit angegebener Lautstärke ab
        /// </summary>
        public void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (clip == null || audioSource == null) return;

            float finalVolume = volume * masterVolume;
            audioSource.PlayOneShot(clip, finalVolume);
        }

        /// <summary>
        /// Spielt einen Sound an einer bestimmten 3D-Position ab
        /// </summary>
        public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;

            float finalVolume = volume * masterVolume;
            AudioSource.PlayClipAtPoint(clip, position, finalVolume);
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}