using Characters.Enemies;
using Characters.Player;
using UnityEngine;
using UnityEngine.UI;
using Weapons;
using Weapons.Data;
using TMPro;

namespace Core.Events
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance;

        [Header("UI")]
        public GameObject upgradePanel;
        public Button damageButton;
        public Button attackSpeedButton;
        public Button healButton;

        private WeaponData playerWeapon;

        void Awake()
        {
            Instance = this;
            upgradePanel.SetActive(false);

            // Button-Listener setzen
            damageButton.onClick.AddListener(UpgradeDamage);
            attackSpeedButton.onClick.AddListener(UpgradeAttackSpeed);
            healButton.onClick.AddListener(HealPlayer);
        }

        public void SetPlayerWeapon(WeaponData weapon)
        {
            playerWeapon = weapon;
        }

        /// <summary>
        /// Panel anzeigen & Spiel pausieren
        /// </summary>
        public void ShowUpgradePanel()
        {
            upgradePanel.SetActive(true);
            Time.timeScale = 0f; // Spiel pausieren
        }

        public void UpgradeDamage()
        {
            if (playerWeapon != null)
                playerWeapon.damage = Mathf.CeilToInt(playerWeapon.damage * 1.1f); // +10%
            ClosePanel();
        }

        public void UpgradeAttackSpeed()
        {
            if (playerWeapon != null)
                playerWeapon.attackSpeed *= 0.9f; // 10% schneller
            ClosePanel();
        }

        public void HealPlayer()
        {
            // Spieler direkt heilen
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.Heal(20);
            ClosePanel();
        }

        private void ClosePanel()
        {
            upgradePanel.SetActive(false);
            Time.timeScale = 1f; // Spiel fortsetzen
            
            // Nächste Wave mit Delay starten
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.StartNextWaveAfterDelay();
            }
        }
    }
}