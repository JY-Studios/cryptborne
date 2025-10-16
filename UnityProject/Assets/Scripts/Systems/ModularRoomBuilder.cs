using UnityEngine;
using System.Collections.Generic;

public class ModularRoomBuilder : MonoBehaviour
{
    [Header("Floor Modules - NUR 4x4 Tiles verwenden!")]
    public GameObject floorTile1;           // floor_tile_large (4x4 Units)
    public GameObject floorTile2;           // floor_tile_big_grate (4x4 Units)
    public GameObject floorTile3;           // floor_tile_big_grate_open (4x4 Units)
    
    [Header("Wall Modules")]
    public GameObject wallNormal;           // wall (4 Units breit)
    public GameObject wallCorner;           // wall_corner (4x4 Units)
    public GameObject wallDoorway;          // wall_doorway (4 Units breit)
    
    [Header("Props - Add a few of each")]
    public GameObject barrel;               // barrel_large
    public GameObject crate;                // box_large
    public GameObject pillar;               // pillar
    
    [Header("Decorations (optional)")]
    public GameObject torch;                // torch_lit
    public GameObject banner;               // banner_thin_red (oder eine andere Farbe)
    public bool spawnBanners = false;       // Banner erstmal deaktiviert zum Testen
    
    [Header("Door Settings")]
    public bool northDoor = false;
    public bool southDoor = false;
    public bool eastDoor = false;
    public bool westDoor = false;
    
    [Header("Room Settings")]
    public int roomWidth = 24;              // Breite in Units (Vielfache von 4)
    public int roomDepth = 24;              // Tiefe in Units (Vielfache von 4)
    public float tileSize = 4f;             // WICHTIG: 4 für KayKit 4x4 Tiles!
    
    [Header("Decoration Density")]
    [Range(0f, 0.3f)] public float propDensity = 0.1f;
    [Range(0f, 1f)] public float wallDecoDensity = 0.2f;
    
    void Start()
    {
        BuildRoom();
    }
    
    void BuildRoom()
    {
        // ===== BODEN =====
        int tilesX = Mathf.CeilToInt(roomWidth / tileSize);
        int tilesZ = Mathf.CeilToInt(roomDepth / tileSize);
        
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, z * tileSize);
                GameObject floorPrefab = GetRandomFloorTile();
                
                if (floorPrefab != null)
                {
                    GameObject floor = Instantiate(floorPrefab, pos, Quaternion.Euler(0, Random.Range(0, 4) * 90, 0), transform);
                    SetTagRecursive(floor, "Ground");
                    EnsureCollider(floor);
                }
                
                // Props im Raum (nicht zu nah an Wänden)
                if (x > 0 && x < tilesX - 1 && z > 0 && z < tilesZ - 1)
                {
                    if (Random.value < propDensity)
                    {
                        // Zufällige Position innerhalb der Tile
                        float randomX = Random.Range(-tileSize * 0.3f, tileSize * 0.3f);
                        float randomZ = Random.Range(-tileSize * 0.3f, tileSize * 0.3f);
                        Vector3 randomOffset = new Vector3(randomX, 0.01f, randomZ);
                        SpawnRandomProp(pos + randomOffset);
                    }
                }
            }
        }
        
        // ===== WÄNDE =====
        // Offset für die Ecken
        float wallOffset = -2f;
        
        // NORD-WAND (oben) - von x=0 bis x=24, bei z=24
        BuildWallLine(0, roomDepth, roomWidth, true, northDoor, 0);
        
        // SÜD-WAND (unten) - von x=0 bis x=24, bei z=wallOffset
        BuildWallLine(0, wallOffset, roomWidth, true, southDoor, 180);
        
        // OST-WAND (rechts) - von z=0 bis z=24, bei x=24
        BuildWallLine(roomWidth, 0, roomDepth, false, eastDoor, 90);
        
        // WEST-WAND (links) - von z=0 bis z=24, bei x=wallOffset
        BuildWallLine(wallOffset, 0, roomDepth, false, westDoor, 270);
        
        // ===== ECKEN =====
        if (wallCorner != null)
        {
            // Südwest-Ecke (unten links)
            GameObject corner1 = Instantiate(wallCorner, 
                new Vector3(wallOffset, 0, wallOffset), 
                Quaternion.Euler(0, 0, 0), transform);
            
            // Südost-Ecke (unten rechts)
            GameObject corner2 = Instantiate(wallCorner, 
                new Vector3(roomWidth, 0, wallOffset), 
                Quaternion.Euler(0, 270, 0), transform);
            
            // Nordwest-Ecke (oben links)
            GameObject corner3 = Instantiate(wallCorner, 
                new Vector3(wallOffset, 0, roomDepth), 
                Quaternion.Euler(0, 90, 0), transform);
            
            // Nordost-Ecke (oben rechts)
            GameObject corner4 = Instantiate(wallCorner, 
                new Vector3(roomWidth, 0, roomDepth), 
                Quaternion.Euler(0, 180, 0), transform);
            
            SetTagRecursive(corner1, "Wall");
            SetTagRecursive(corner2, "Wall");
            SetTagRecursive(corner3, "Wall");
            SetTagRecursive(corner4, "Wall");
            
            EnsureCollider(corner1);
            EnsureCollider(corner2);
            EnsureCollider(corner3);
            EnsureCollider(corner4);
        }
    }

    void BuildWallLine(float xPos, float zPos, int length, bool isHorizontal, bool hasDoor, float rotation)
    {
        // +1 um bis zur Ecke zu reichen!
        int wallCount = (length / 4) + 1;

        for (int i = 0; i < wallCount; i++)
        {
            Vector3 pos = isHorizontal ? new Vector3(xPos + i * 4, 0, zPos) : new Vector3(xPos, 0, zPos + i * 4);

            // Tür in der Mitte
            bool isDoorPosition = hasDoor && i == wallCount / 2;

            if (isDoorPosition)
            {
                if (wallDoorway != null)
                {
                    GameObject door = Instantiate(wallDoorway, pos, Quaternion.Euler(0, rotation, 0), transform);
                    SetTagRecursive(door, "Wall");
                    EnsureCollider(door);
                }
            }
            else
            {
                if (wallNormal != null)
                {
                    GameObject wall = Instantiate(wallNormal, pos, Quaternion.Euler(0, rotation, 0), transform);
                    SetTagRecursive(wall, "Wall");
                    EnsureCollider(wall);

                    // Gelegentlich Wand-Dekoration
                    if (Random.value < wallDecoDensity && (torch != null || (spawnBanners && banner != null)))
                    {
                        SpawnWallDecoration(pos, rotation);
                    }
                }
            }
        }
    }

    void SpawnRandomProp(Vector3 position)
    {
        List<GameObject> props = new List<GameObject>();
        if (barrel != null) props.Add(barrel);
        if (crate != null) props.Add(crate);
        if (pillar != null) props.Add(pillar);

        if (props.Count > 0)
        {
            GameObject prop = props[Random.Range(0, props.Count)];
            float randomRotation = Random.Range(0, 4) * 90f;
            GameObject spawnedProp = Instantiate(prop, position, Quaternion.Euler(0, randomRotation, 0), transform);
            SetTagRecursive(spawnedProp, "Wall");
            EnsureCollider(spawnedProp);
        }
    }
    
    void SpawnWallDecoration(Vector3 wallPos, float wallRotation)
    {
        GameObject deco = torch;
        bool isBanner = false;
        
        // Falls Banner aktiviert sind, manchmal Banner statt Fackel
        if (spawnBanners && banner != null && Random.value < 0.3f)
        {
            deco = banner;
            isBanner = true;
        }
        
        if (deco == null) return;
        
        // Offset nach innen (in den Raum hinein)
        Vector3 forward = Quaternion.Euler(0, wallRotation, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, wallRotation, 0) * Vector3.right;
        
        // Zufällige Position entlang der Wand
        float randomOffset = Random.Range(-1.5f, 1.5f);
        
        Vector3 decoPos;
        if (isBanner)
        {
            // Banner: Weiter in den Raum und etwas tiefer
            decoPos = wallPos + right * randomOffset - forward * 2.0f + Vector3.up * 1.2f;
        }
        else
        {
            // Fackeln: Höher an der Wand
            decoPos = wallPos + right * randomOffset - forward * 0.4f + Vector3.up * 2.5f;
        }
        
        Instantiate(deco, decoPos, Quaternion.Euler(0, wallRotation, 0), transform);
    }
    
    GameObject GetRandomFloorTile()
    {
        List<GameObject> floors = new List<GameObject>();
        if (floorTile1 != null) floors.Add(floorTile1);
        if (floorTile2 != null) floors.Add(floorTile2);
        if (floorTile3 != null) floors.Add(floorTile3);
        
        if (floors.Count == 0) return null;
        return floors[Random.Range(0, floors.Count)];
    }
    
    void SetTagRecursive(GameObject obj, string newTag)
    {
        obj.tag = newTag;
        foreach (Transform child in obj.transform)
        {
            SetTagRecursive(child.gameObject, newTag);
        }
    }

    void EnsureCollider(GameObject obj, bool isTrigger = false)
    {
        // Finde ALLE MeshFilters (auch in Children)
        MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length > 0)
        {
            // Füge MeshCollider zu jedem Child mit Mesh hinzu
            foreach (var meshFilter in meshFilters)
            {
                if (meshFilter.GetComponent<Collider>() == null)
                {
                    MeshCollider collider = meshFilter.gameObject.AddComponent<MeshCollider>();
                    collider.convex = false;
                    collider.isTrigger = isTrigger;
                }
            }
        }
        else if (obj.GetComponent<Collider>() == null)
        {
            // Fallback: BoxCollider am Parent
            BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
            boxCollider.isTrigger = isTrigger;
        }
    }
}