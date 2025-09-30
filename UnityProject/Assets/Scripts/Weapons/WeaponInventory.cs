using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapons;
using Weapons.Behaviours.Ranged;
using Weapons.Data;
using Weapons.Projectiles;

namespace Characters.Player
{
    public class WeaponInventory : MonoBehaviour
    {
        [Header("Weapon Slots")]
        [SerializeField] private List<RangedWeaponData> availableWeapons = new List<RangedWeaponData>();
        [SerializeField] private int maxWeaponSlots = 4;

        [Header("Current State")]
        [SerializeField] private int currentWeaponIndex = 0;
        [SerializeField] private RangedWeaponData currentWeaponData;
        [SerializeField] private bool isOrbitWeapon = false;

        private Weapon<RangedWeaponData> currentWeapon;
        private PlayerController playerController;
        
        // Cooldown Tracking für jede Waffe
        private Dictionary<int, float> weaponLastAttackTimes = new Dictionary<int, float>();

        // Events für UI Updates
        public static event System.Action<List<RangedWeaponData>, int> OnWeaponInventoryChanged;
        public static event System.Action<RangedWeaponData, int> OnWeaponSwitched;
        
        // Public Properties für UI
        public int WeaponCount => availableWeapons.Count;

        void Start()
        {
            playerController = GetComponent<PlayerController>();

            Debug.Log($"WeaponInventory Start - Available weapons: {availableWeapons.Count}");

            // Erste Waffe ausrüsten
            if (availableWeapons.Count > 0)
            {
                EquipWeapon(0);
            }
            
            // UI Update direkt triggern
            TriggerUIUpdate();
        }

        void Update()
        {
            HandleWeaponInput();
            HandleWeaponShooting();
        }

        void HandleWeaponInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Dynamische Zahlentasten nur bis maxWeaponSlots
            for (int i = 0; i < Mathf.Min(maxWeaponSlots, availableWeapons.Count); i++)
            {
                Key key = GetNumberKey(i + 1);
                if (keyboard[key].wasPressedThisFrame)
                {
                    EquipWeapon(i);
                }
            }

            // Mausrad für Waffenwechsel
            var mouse = Mouse.current;
            if (mouse != null && mouse.scroll.ReadValue().y != 0)
            {
                int direction = mouse.scroll.ReadValue().y > 0 ? -1 : 1;
                CycleWeapon(direction);
            }

            // Q und E für schnellen Wechsel
            if (keyboard.qKey.wasPressedThisFrame)
                CycleWeapon(-1);
            if (keyboard.eKey.wasPressedThisFrame)
                CycleWeapon(1);
        }

        Key GetNumberKey(int number)
        {
            switch (number)
            {
                case 1: return Key.Digit1;
                case 2: return Key.Digit2;
                case 3: return Key.Digit3;
                case 4: return Key.Digit4;
                case 5: return Key.Digit5;
                case 6: return Key.Digit6;
                case 7: return Key.Digit7;
                case 8: return Key.Digit8;
                case 9: return Key.Digit9;
                default: return Key.Digit1;
            }
        }

        void HandleWeaponShooting()
        {
            if (currentWeapon == null || currentWeaponData == null) return;

            // Orbit-Waffen spawnen nur beim Equip, nie danach
            if (isOrbitWeapon) return;

            bool inputPressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
            
            // Attack versuchen und bei Erfolg Cooldown tracken
            bool didAttack = currentWeapon.TryAttack(transform, inputPressed);
            if (didAttack)
            {
                weaponLastAttackTimes[currentWeaponIndex] = Time.time;
            }
        }
        
        /// <summary>
        /// Gibt den Cooldown Progress für eine Waffe zurück (0 = ready, 1 = full cooldown)
        /// </summary>
        public float GetWeaponCooldownProgress(int weaponIndex)
        {
            if (weaponIndex < 0 || weaponIndex >= availableWeapons.Count)
                return 0f;
            
            RangedWeaponData weaponData = availableWeapons[weaponIndex];
            if (weaponData == null)
                return 0f;
            
            // Wenn Waffe noch nie benutzt wurde
            if (!weaponLastAttackTimes.ContainsKey(weaponIndex))
                return 0f;
            
            float lastAttackTime = weaponLastAttackTimes[weaponIndex];
            float cooldownDuration = weaponData.attackSpeed;
            float timeSinceLastAttack = Time.time - lastAttackTime;
            
            if (timeSinceLastAttack >= cooldownDuration)
                return 0f; // Cooldown fertig
            
            // Cooldown Progress: 1 (gerade geschossen) -> 0 (bereit)
            float progress = 1f - (timeSinceLastAttack / cooldownDuration);
            return Mathf.Clamp01(progress);
        }

        void EquipWeapon(int index)
        {
            if (index < 0 || index >= availableWeapons.Count) return;
            if (availableWeapons[index] == null) return;

            // WICHTIG: Prevent re-equipping the same weapon
            if (index == currentWeaponIndex && currentWeapon != null)
            {
                Debug.Log($"Weapon {availableWeapons[index].weaponName} is already equipped. Ignoring.");
                return;
            }

            // WICHTIG: Cleanup ALLE Orbit-Projektile BEVOR neue Waffe equipped wird
            CleanupOrbitProjectiles();

            currentWeaponIndex = index;
            currentWeaponData = availableWeapons[index];

            var behavior = new ShootNearestBehaviour();
            currentWeapon = new Weapon<RangedWeaponData>(currentWeaponData, behavior);
            
            // Initialize last attack time für neue Waffe (als ob gerade geschossen)
            if (!weaponLastAttackTimes.ContainsKey(index))
            {
                weaponLastAttackTimes[index] = Time.time - currentWeaponData.attackSpeed; // Sofort bereit
            }

            Debug.Log($"Equipped weapon: {currentWeaponData.weaponName}");

            // Prüfen ob Orbit-Waffe
            isOrbitWeapon = false;
            if (currentWeaponData.projectiles != null)
            {
                foreach (var proj in currentWeaponData.projectiles)
                {
                    if (proj.pattern == SpreadPattern.Orbit)
                    {
                        isOrbitWeapon = true;
                        break;
                    }
                }
            }

            // Orbit-Waffen einmal spawnen
            if (isOrbitWeapon)
            {
                Debug.Log("Spawning orbit projectiles...");
                behavior.Attack(currentWeaponData, transform);
            }

            OnWeaponSwitched?.Invoke(currentWeaponData, currentWeaponIndex);
        }

        void CleanupOrbitProjectiles()
        {
            // Benutze die statische Methode aus Projectile.cs
            Debug.Log("Cleaning up orbit projectiles via static method...");
            Projectile.DespawnAllOrbitProjectiles();
        }

        void CycleWeapon(int direction)
        {
            if (availableWeapons.Count <= 1) return;

            int newIndex = currentWeaponIndex + direction;

            // Wrap around
            if (newIndex < 0)
                newIndex = availableWeapons.Count - 1;
            else if (newIndex >= availableWeapons.Count)
                newIndex = 0;

            EquipWeapon(newIndex);
        }

        public void AddWeapon(RangedWeaponData weaponData)
        {
            if (availableWeapons.Count >= maxWeaponSlots)
            {
                Debug.Log("Inventory full!");
                return;
            }

            availableWeapons.Add(weaponData);
            TriggerUIUpdate();
        }

        public void RemoveWeapon(int index)
        {
            if (index < 0 || index >= availableWeapons.Count) return;

            availableWeapons.RemoveAt(index);

            // Wenn aktuelle Waffe entfernt wurde, neue ausrüsten
            if (currentWeaponIndex >= availableWeapons.Count)
            {
                EquipWeapon(availableWeapons.Count - 1);
            }

            TriggerUIUpdate();
        }

        void TriggerUIUpdate()
        {
            Debug.Log("Triggering UI update");
            OnWeaponInventoryChanged?.Invoke(availableWeapons, currentWeaponIndex);

            if (currentWeaponData != null)
            {
                OnWeaponSwitched?.Invoke(currentWeaponData, currentWeaponIndex);
            }
        }

        void OnDestroy()
        {
            // Cleanup beim Destroy
            CleanupOrbitProjectiles();
        }
    }
}