using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapons;
using Weapons.Behaviours.Ranged;
using Weapons.Data;
using Core.Events;

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
        
        private Weapon<RangedWeaponData> currentWeapon;
        private PlayerController playerController;
        
        // Events für UI Updates
        public static event System.Action<List<RangedWeaponData>, int> OnWeaponInventoryChanged;
        public static event System.Action<RangedWeaponData, int> OnWeaponSwitched;
        
        void Start()
        {
            playerController = GetComponent<PlayerController>();
    
            Debug.Log($"WeaponInventory Start - Available weapons: {availableWeapons.Count}");
    
            // Debug: Liste alle Waffen auf
            for(int i = 0; i < availableWeapons.Count; i++)
            {
                if(availableWeapons[i] != null)
                    Debug.Log($"Weapon {i}: {availableWeapons[i].weaponName}");
            }
    
            // Erste Waffe ausrüsten
            if (availableWeapons.Count > 0)
            {
                EquipWeapon(0);
        
                // WICHTIG: Force initial UI update!
                Invoke("ForceInitialUIUpdate", 0.1f);
            }
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
            
            // Zahlentasten 1-4 für Waffenwechsel
            if (keyboard.digit1Key.wasPressedThisFrame && availableWeapons.Count >= 1)
                EquipWeapon(0);
            if (keyboard.digit2Key.wasPressedThisFrame && availableWeapons.Count >= 2)
                EquipWeapon(1);
            if (keyboard.digit3Key.wasPressedThisFrame && availableWeapons.Count >= 3)
                EquipWeapon(2);
            if (keyboard.digit4Key.wasPressedThisFrame && availableWeapons.Count >= 4)
                EquipWeapon(3);
            
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
        
        void HandleWeaponShooting()
        {
            if (currentWeapon == null || currentWeaponData == null) return;
            
            bool inputPressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
            currentWeapon.TryAttack(transform, inputPressed);
        }
        
        void EquipWeapon(int index)
        {
            if (index < 0 || index >= availableWeapons.Count) return;
            if (availableWeapons[index] == null) return;
            
            currentWeaponIndex = index;
            currentWeaponData = availableWeapons[index];
            
            // Neue Waffe erstellen mit dem passenden Behavior
            var behavior = new ShootNearestBehaviour();
            currentWeapon = new Weapon<RangedWeaponData>(currentWeaponData, behavior);
            
            Debug.Log($"Equipped weapon: {currentWeaponData.weaponName}");
            
            // Event für UI Update
            OnWeaponSwitched?.Invoke(currentWeaponData, currentWeaponIndex);
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
            OnWeaponInventoryChanged?.Invoke(availableWeapons, currentWeaponIndex);
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
            
            OnWeaponInventoryChanged?.Invoke(availableWeapons, currentWeaponIndex);
        }
        
        void ForceInitialUIUpdate()
        {
            Debug.Log("Forcing initial UI update!");
            OnWeaponInventoryChanged?.Invoke(availableWeapons, currentWeaponIndex);
    
            if (currentWeaponData != null)
            {
                Debug.Log($"Current weapon: {currentWeaponData.weaponName}");
                OnWeaponSwitched?.Invoke(currentWeaponData, currentWeaponIndex);
            }
        }
    }
}