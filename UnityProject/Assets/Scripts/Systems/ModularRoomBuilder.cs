using UnityEngine;

public class ModularRoomBuilder : MonoBehaviour
{
    [Header("Module Prefabs")]
    public GameObject wallModule;
    public GameObject floorModule;
    public GameObject cornerModule;
    public GameObject doorModule;
    
    [Header("Door Settings")]
    public bool northDoor = false;
    public bool southDoor = false;
    public bool eastDoor = false;
    public bool westDoor = false;
    
    [Header("Room Settings")]
    public int roomWidth = 20;
    public int roomDepth = 20;
    
    void Start()
    {
        BuildRoom();
    }
    
    void BuildRoom()
    {
        // BODEN
        for (int x = 0; x < roomWidth; x++)
        {
            for (int z = 0; z < roomDepth; z++)
            {
                Vector3 pos = new Vector3(x, 0, z);
                Instantiate(floorModule, pos, Quaternion.identity, transform);
            }
        }
        
        // NORD-WAND (oben, bei z = roomDepth - 0.5)
        for (int x = 0; x < roomWidth; x++)
        {
            Vector3 pos = new Vector3(x, 0, roomDepth - 0.5f);
            
            // Tür in der Mitte (2 Units breit)
            bool isDoorPosition = northDoor && (x == roomWidth/2 - 1 || x == roomWidth/2);
            
            if (isDoorPosition)
            {
                // Nur beim ERSTEN Tür-Segment spawnen
                if (x == roomWidth/2 - 1 && doorModule != null)
                {
                    Instantiate(doorModule, pos, Quaternion.identity, transform);
                }
                // Beim zweiten Segment nichts tun
            }
            else
            {
                // Normale Wand
                Instantiate(wallModule, pos, Quaternion.identity, transform);
            }
        }
        
        // SÜD-WAND (unten, bei z = -0.5)
        for (int x = 0; x < roomWidth; x++)
        {
            Vector3 pos = new Vector3(x, 0, -0.5f);
            
            bool isDoorPosition = southDoor && (x == roomWidth/2 - 1 || x == roomWidth/2);
            
            if (isDoorPosition)
            {
                if (x == roomWidth/2 - 1 && doorModule != null)
                {
                    Instantiate(doorModule, pos, Quaternion.identity, transform);
                }
            }
            else
            {
                Instantiate(wallModule, pos, Quaternion.identity, transform);
            }
        }
        
        // OST-WAND (rechts, bei x = roomWidth - 0.5)
        for (int z = 0; z < roomDepth; z++)
        {
            Vector3 pos = new Vector3(roomWidth - 0.5f, 0, z);
            
            bool isDoorPosition = eastDoor && (z == roomDepth/2 - 1 || z == roomDepth/2);
            
            if (isDoorPosition)
            {
                if (z == roomDepth/2 - 1 && doorModule != null)
                {
                    Instantiate(doorModule, pos, Quaternion.Euler(0, 90, 0), transform);
                }
            }
            else
            {
                Instantiate(wallModule, pos, Quaternion.Euler(0, 90, 0), transform);
            }
        }
        
        // WEST-WAND (links, bei x = -0.5)
        for (int z = 0; z < roomDepth; z++)
        {
            Vector3 pos = new Vector3(-0.5f, 0, z);
            
            bool isDoorPosition = westDoor && (z == roomDepth/2 - 1 || z == roomDepth/2);
            
            if (isDoorPosition)
            {
                if (z == roomDepth/2 - 1 && doorModule != null)
                {
                    Instantiate(doorModule, pos, Quaternion.Euler(0, 90, 0), transform);
                }
            }
            else
            {
                Instantiate(wallModule, pos, Quaternion.Euler(0, 90, 0), transform);
            }
        }
        
        // ECKEN
        Instantiate(cornerModule, new Vector3(-0.5f, 0, -0.5f), Quaternion.identity, transform);
        Instantiate(cornerModule, new Vector3(roomWidth - 0.5f, 0, -0.5f), Quaternion.identity, transform);
        Instantiate(cornerModule, new Vector3(-0.5f, 0, roomDepth - 0.5f), Quaternion.identity, transform);
        Instantiate(cornerModule, new Vector3(roomWidth - 0.5f, 0, roomDepth - 0.5f), Quaternion.identity, transform);
    }
}