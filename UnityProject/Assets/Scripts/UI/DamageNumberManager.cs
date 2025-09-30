using UnityEngine;
using TMPro;
using System.Collections;

namespace UI
{
    public class DamageNumberManager : MonoBehaviour
    {
        private static DamageNumberManager _instance;
        public static DamageNumberManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DamageNumberManager");
                    _instance = go.AddComponent<DamageNumberManager>();
                }
                return _instance;
            }
        }

        [Header("Prefab")]
        public GameObject damageNumberPrefab;
        
        [Header("Settings")]
        public float floatSpeed = 2f;
        public float floatDistance = 1.5f;
        public float lifetime = 1f;
        public float fadeStartTime = 0.5f;

        private Camera mainCamera;
        private Canvas canvas;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            mainCamera = Camera.main;
            canvas = FindObjectOfType<Canvas>();
            
            if (canvas == null)
            {
                Debug.LogError("DamageNumberManager: No Canvas found in scene!");
            }
        }

        public void ShowDamage(Vector3 worldPosition, float damage)
        {
            if (damageNumberPrefab == null)
            {
                Debug.LogWarning("DamageNumberManager: damageNumberPrefab is not assigned!");
                return;
            }

            if (canvas == null || mainCamera == null)
            {
                Debug.LogWarning("DamageNumberManager: Canvas or Camera not found!");
                return;
            }

            // Instantiate damage number
            GameObject damageNumberObj = Instantiate(damageNumberPrefab, canvas.transform);
            TextMeshProUGUI text = damageNumberObj.GetComponent<TextMeshProUGUI>();
            
            if (text == null)
            {
                Debug.LogError("DamageNumberManager: Prefab has no TextMeshProUGUI component!");
                Destroy(damageNumberObj);
                return;
            }

            // Set damage text
            text.text = Mathf.RoundToInt(damage).ToString();

            // Position leicht zufällig versetzen für bessere Lesbarkeit bei vielen Treffern
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(0f, 0.5f),
                0f
            );
            worldPosition += randomOffset;

            // Start animation
            StartCoroutine(AnimateDamageNumber(damageNumberObj, text, worldPosition));
        }

        IEnumerator AnimateDamageNumber(GameObject obj, TextMeshProUGUI text, Vector3 startWorldPos)
        {
            float elapsed = 0f;
            Color originalColor = text.color;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;

                // World position nach oben bewegen
                float progress = elapsed / lifetime;
                Vector3 currentWorldPos = startWorldPos + Vector3.up * (floatDistance * progress);

                // World position zu Screen position konvertieren
                Vector3 screenPos = mainCamera.WorldToScreenPoint(currentWorldPos);

                // Screen position zu Canvas position
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    screenPos,
                    canvas.worldCamera,
                    out Vector2 canvasPos
                );

                // Position setzen
                obj.transform.localPosition = canvasPos;

                // Fade out nach fadeStartTime
                if (elapsed > fadeStartTime)
                {
                    float fadeProgress = (elapsed - fadeStartTime) / (lifetime - fadeStartTime);
                    Color newColor = originalColor;
                    newColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
                    text.color = newColor;
                }

                yield return null;
            }

            Destroy(obj);
        }
    }
}