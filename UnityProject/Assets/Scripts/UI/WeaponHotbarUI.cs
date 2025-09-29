using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Characters.Player;
using Weapons.Data;

public class WeaponHotbarUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject slotPrefab; // Prefab für einen Slot
    public Transform slotsContainer; // Parent für die Slots
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;
    
    [Header("Slot Settings")]
    public int maxSlots = 4;
    
    private List<GameObject> slotObjects = new List<GameObject>();
    private List<Image> slotBackgrounds = new List<Image>();
    private List<Image> weaponIcons = new List<Image>();
    private List<TextMeshProUGUI> slotNumbers = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> weaponNames = new List<TextMeshProUGUI>();
    
    void OnEnable()
    {
        WeaponInventory.OnWeaponSwitched += OnWeaponSwitched;
        WeaponInventory.OnWeaponInventoryChanged += OnInventoryChanged;
    }
    
    void OnDisable()
    {
        WeaponInventory.OnWeaponSwitched -= OnWeaponSwitched;
        WeaponInventory.OnWeaponInventoryChanged -= OnInventoryChanged;
    }
    
    void Start()
    {
        CreateSlots();
    }
    
    void CreateSlots()
    {
        // Lösche alte Slots
        foreach (var slot in slotObjects)
        {
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
        }
    }
    
    void OnWeaponSwitched(RangedWeaponData weapon, int index)
    {
        // Update selected slot visual
        for (int i = 0; i < slotBackgrounds.Count; i++)
        {
            if (slotBackgrounds[i] != null)
            {
                slotBackgrounds[i].color = (i == index) ? selectedColor : normalColor;
            }
        }
    }
    
    void OnInventoryChanged(List<RangedWeaponData> weapons, int currentIndex)
    {
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
    }
}