using UnityEngine;

public class ModularRoomBuilder : MonoBehaviour
{
    [Header("Module Prefabs")]
    public GameObject wallModule;
    public GameObject floorModule;
    public GameObject cornerModule;
    
    [Header("Room Settings")]
    public int roomWidth = 10;
    public int roomDepth = 10;
    
    void Start()
    {
        BuildRoom();
    }
    
    void BuildRoom()
    {
        // Boden erstellen
        for (int x = 0; x < roomWidth; x++)
        {
            for (int z = 0; z < roomDepth; z++)
            {
                Vector3 position = new Vector3(x, 0, z);
                Instantiate(floorModule, position, Quaternion.identity, transform);
            }
        }
        
        // Wände erstellen - KORRIGIERTE POSITIONEN
        for (int x = 0; x < roomWidth; x++)
        {
            // Nord-Wand (0.1 nach innen verschoben)
            Vector3 northPos = new Vector3(x, 0, roomDepth - 0.5f);
            Instantiate(wallModule, northPos, Quaternion.identity, transform);
            
            // Süd-Wand
            Vector3 southPos = new Vector3(x, 0, -0.5f);
            Instantiate(wallModule, southPos, Quaternion.identity, transform);
        }
        
        for (int z = 0; z < roomDepth; z++)
        {
            // Ost-Wand
            Vector3 eastPos = new Vector3(roomWidth - 0.5f, 0, z);
            Instantiate(wallModule, eastPos, Quaternion.Euler(0, 90, 0), transform);
            
            // West-Wand
            Vector3 westPos = new Vector3(-0.5f, 0, z);
            Instantiate(wallModule, westPos, Quaternion.Euler(0, 90, 0), transform);
        }
        
        // Ecken - auch angepasst
        Instantiate(cornerModule, new Vector3(-0.5f, 0, -0.5f), Quaternion.identity, transform);
        Instantiate(cornerModule, new Vector3(roomWidth - 0.5f, 0, -0.5f), Quaternion.identity, transform);
        Instantiate(cornerModule, new Vector3(-0.5f, 0, roomDepth - 0.5f), Quaternion.identity, transform);
        Instantiate(cornerModule, new Vector3(roomWidth - 0.5f, 0, roomDepth - 0.5f), Quaternion.identity, transform);
    }
}