using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Characters.Player;
using Weapons.Data;

public class WeaponHotbarUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject slotPrefab;
    public Transform slotsContainer;
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;
    
    [Header("Slot Settings")]
    public int maxSlots = 4;
    
    private List<GameObject> slotObjects = new List<GameObject>();
    private List<Image> slotBackgrounds = new List<Image>();
    private List<Image> weaponIcons = new List<Image>();
    private List<TextMeshProUGUI> slotNumbers = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> weaponNames = new List<TextMeshProUGUI>();
    private bool isInitialized = false;
    
    void OnEnable()
    {
        // Slots SOFORT erstellen bevor Events subscribed werden
        if (!isInitialized)
        {
            CreateSlots();
            isInitialized = true;
        }
        
        // Jetzt Events subscriben - UI ist ready
        WeaponInventory.OnWeaponSwitched += OnWeaponSwitched;
        WeaponInventory.OnWeaponInventoryChanged += OnInventoryChanged;
    }
    
    void OnDisable()
    {
        WeaponInventory.OnWeaponSwitched -= OnWeaponSwitched;
        WeaponInventory.OnWeaponInventoryChanged -= OnInventoryChanged;
    }
    
    void CreateSlots()
    {
        // Lösche alte Slots
        foreach (var slot in slotObjects)
        {
            if (slot != null)
                Destroy(slot);
        }
        slotObjects.Clear();
        slotBackgrounds.Clear();
        weaponIcons.Clear();
        slotNumbers.Clear();
        weaponNames.Clear();
        
        // Erstelle neue Slots
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotsContainer);
            slotObjects.Add(slot);
            
            // Finde UI Komponenten
            Image bg = slot.GetComponent<Image>();
            slotBackgrounds.Add(bg);
            
            Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            weaponIcons.Add(icon);
            
            TextMeshProUGUI number = slot.transform.Find("Number")?.GetComponent<TextMeshProUGUI>();
            if (number != null)
            {
                number.text = (i + 1).ToString();
            }
            slotNumbers.Add(number);
            
            TextMeshProUGUI name = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            weaponNames.Add(name);
            
            // Initial als leer markieren
            if (weaponIcons[i] != null)
                weaponIcons[i].gameObject.SetActive(false);
            if (weaponNames[i] != null)
                weaponNames[i].text = "Empty";
        }
        
        Debug.Log($"WeaponHotbarUI: Created {maxSlots} slots");
    }
    
    void OnWeaponSwitched(RangedWeaponData weapon, int index)
    {
        if (!isInitialized) return;
        
        // Update selected slot visual
        for (int i = 0; i < slotBackgrounds.Count; i++)
        {
            if (slotBackgrounds[i] != null)
            {
                slotBackgrounds[i].color = (i == index) ? selectedColor : normalColor;
            }
        }
        
        Debug.Log($"WeaponHotbarUI: Switched to slot {index} - {weapon.weaponName}");
    }
    
    void OnInventoryChanged(List<RangedWeaponData> weapons, int currentIndex)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("WeaponHotbarUI: OnInventoryChanged called but not initialized!");
            return;
        }
        
        // Update alle Slots
        for (int i = 0; i < maxSlots; i++)
        {
            if (i < weapons.Count && weapons[i] != null)
            {
                // Slot hat eine Waffe
                if (weaponIcons[i] != null)
                {
                    weaponIcons[i].gameObject.SetActive(true);
                    if (weapons[i].icon != null)
                    {
                        weaponIcons[i].sprite = weapons[i].icon;
                    }
                }
                
                if (weaponNames[i] != null)
                {
                    weaponNames[i].text = weapons[i].weaponName;
                }
                
                slotObjects[i].SetActive(true);
            }
            else
            {
                // Leerer Slot
                if (i < slotObjects.Count)
                {
                    if (weaponIcons[i] != null)
                        weaponIcons[i].gameObject.SetActive(false);
                    if (weaponNames[i] != null)
                        weaponNames[i].text = "Empty";
                }
            }
        }
        
        OnWeaponSwitched(weapons[currentIndex], currentIndex);
        Debug.Log($"WeaponHotbarUI: Inventory updated - {weapons.Count} weapons");
    }
}