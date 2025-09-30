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
    private List<Image> cooldownOverlays = new List<Image>();
    private bool isInitialized = false;
    
    void OnEnable()
    {
        if (!isInitialized)
        {
            CreateSlots();
            isInitialized = true;
        }
        
        WeaponInventory.OnWeaponSwitched += OnWeaponSwitched;
        WeaponInventory.OnWeaponInventoryChanged += OnInventoryChanged;
    }
    
    void OnDisable()
    {
        WeaponInventory.OnWeaponSwitched -= OnWeaponSwitched;
        WeaponInventory.OnWeaponInventoryChanged -= OnInventoryChanged;
    }
    
    void Update()
    {
        UpdateCooldowns();
    }
    
    void CreateSlots()
    {
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
        cooldownOverlays.Clear();
        
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotsContainer);
            slotObjects.Add(slot);
            
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
            
            // Cooldown Overlay Setup - minimal, behält Prefab Settings bei
            Transform cooldownTransform = slot.transform.Find("CooldownOverlay");
            if (cooldownTransform != null)
            {
                Image cooldownOverlay = cooldownTransform.GetComponent<Image>();
                if (cooldownOverlay != null)
                {
                    RectTransform cooldownRect = cooldownTransform as RectTransform;
                    if (cooldownRect != null)
                    {
                        // Nur Pivot und Initial Scale setzen, Rest vom Prefab
                        cooldownRect.pivot = new Vector2(0.5f, 1f); // Pivot oben
                        cooldownRect.localScale = new Vector3(1f, 0f, 1f); // Initial unsichtbar
                    }
                }
                cooldownOverlays.Add(cooldownOverlay);
            }
            else
            {
                Debug.LogWarning($"WeaponHotbarUI: Slot {i} has no CooldownOverlay!");
                cooldownOverlays.Add(null);
            }
            
            if (weaponIcons[i] != null)
                weaponIcons[i].gameObject.SetActive(false);
            if (weaponNames[i] != null)
                weaponNames[i].text = "Empty";
        }
        
        Debug.Log($"WeaponHotbarUI: Created {maxSlots} slots with cooldown tracking");
    }
    
    void UpdateCooldowns()
    {
        var inventory = FindObjectOfType<WeaponInventory>();
        if (inventory == null) return;
        
        for (int i = 0; i < cooldownOverlays.Count && i < inventory.WeaponCount; i++)
        {
            if (cooldownOverlays[i] != null)
            {
                float cooldownProgress = inventory.GetWeaponCooldownProgress(i);
                
                RectTransform rectTransform = cooldownOverlays[i].transform as RectTransform;
                if (rectTransform != null)
                {
                    // Scale Y: 1.0 = voll im Cooldown, 0.0 = bereit
                    rectTransform.localScale = new Vector3(1f, cooldownProgress, 1f);
                }
            }
        }
    }
    
    void OnWeaponSwitched(RangedWeaponData weapon, int index)
    {
        if (!isInitialized) return;
        
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
        if (!isInitialized)
        {
            Debug.LogWarning("WeaponHotbarUI: OnInventoryChanged called but not initialized!");
            return;
        }
        
        for (int i = 0; i < maxSlots; i++)
        {
            if (i < weapons.Count && weapons[i] != null)
            {
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