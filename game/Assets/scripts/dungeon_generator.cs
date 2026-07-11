using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Настройки генерации")]
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int maxGenerationAttempts = 200;
    [SerializeField] private float roomSpacing = 0.5f;
    [SerializeField] private float maxBranching = 0.6f;
    [SerializeField] private float deadEndChance = 0.1f; // Шанс создать тупик
    
    [Header("Префабы комнат")]
    [SerializeField] private List<RoomPrefab> roomPrefabs;
    [SerializeField] private RoomPrefab startRoomPrefab;
    
    [Header("Родительские объекты")]
    [SerializeField] private Transform dungeonParent;
    [SerializeField] private Transform playerSpawnParent;
    
    [Header("Игрок")]
    [SerializeField] private GameObject playerPrefab;
    
    private List<RoomPrefab> placedRooms = new List<RoomPrefab>();
    private Queue<RoomConnection> pendingConnections = new Queue<RoomConnection>();
    private GameObject currentPlayer;
    
    public Vector2 PlayerSpawnPosition { get; private set; }
    
    private void Start()
    {
        GenerateDungeon();
    }
    
    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        ClearDungeon();
        placedRooms.Clear();
        pendingConnections.Clear();
        
        
        
        
        
        
        RoomPrefab startRoom = InstantiateRoom(startRoomPrefab, Vector2.zero);
        placedRooms.Add(startRoom);
        PlayerSpawnPosition = startRoom.GetSpawnPoint();
        
        
        
       
        foreach (RoomSide side in startRoom.GetAvailableSides())
        {
            pendingConnections.Enqueue(new RoomConnection(startRoom, side));
        }
        
        
        int attempts = 0;
        int failedAttempts = 0;
        
        while (placedRooms.Count < maxRooms && pendingConnections.Count > 0 && attempts < maxGenerationAttempts)
        {
            attempts++;
            
            
            
            RoomConnection connection = pendingConnections.Dequeue();
            
           
            if (ShouldCreateDeadEnd(connection))
            {
                continue;
            }
            
            if (TryPlaceRoom(connection))
            {
                failedAttempts = 0;
            }
            else
            {
                failedAttempts++;
            }
        }
        
       
        
       
        
        SpawnPlayer();
    }
    
    private bool ShouldCreateDeadEnd(RoomConnection connection)
    {
       
        if (placedRooms.Count <= 1)
            return false;
        
       
        RoomPrefab parentRoom = connection.parentRoom;
        List<RoomSide> availableSides = parentRoom.GetAvailableSides();
        availableSides.Remove(connection.side);
        
       
        if (availableSides.Count == 0)
            return false;
        
        
        float chance = deadEndChance;
        
        
        int connectionCount = parentRoom.GetAvailableSides().Count;
        if (connectionCount >= 3)
        {
            chance += 0.2f;
        }
        
      
        if (placedRooms.Count < maxRooms * 0.3f)
        {
            chance *= 0.5f;
        }
        
        return Random.value < chance;
    }
    
    private bool TryPlaceRoom(RoomConnection connection)
    {
       
        Vector2 doorPos = connection.parentRoom.GetDoorPosition(connection.side);
        
       
        List<RoomPrefab> shuffledPrefabs = roomPrefabs.OrderBy(x => Random.value).ToList();
        
       
        foreach (RoomPrefab newRoomPrefab in shuffledPrefabs)
        {
            
            RoomSide oppositeSide = GetOppositeSide(connection.side);
            if (!newRoomPrefab.HasConnection(oppositeSide))
            {
                continue;
            }
            
            
            Vector2 newRoomPos = CalculateRoomPosition(doorPos, connection.side, newRoomPrefab);
            
           
            if (!IsOverlapping(newRoomPos, newRoomPrefab.Width, newRoomPrefab.Height))
            {
               
                RoomPrefab newRoom = InstantiateRoom(newRoomPrefab, newRoomPos);
                placedRooms.Add(newRoom);
                
               
                newRoom.SetAttachedSide(oppositeSide);
                
                
                AddNewConnections(newRoom, oppositeSide);
        

                return true;
            }
        }
        
        return false;
    }
    
    private Vector2 CalculateRoomPosition(Vector2 doorPos, RoomSide side, RoomPrefab newRoom)
    {
       
        RoomSide attachSide = GetOppositeSide(side);
        Vector2 doorLocalPos = GetDoorLocalPosition(newRoom, attachSide);
        
        
        Vector2 centerPos = doorPos - doorLocalPos;
        Vector2 pivotPos = centerPos - newRoom.PivotOffset;
        
        return pivotPos;
    }
    
    private Vector2 GetDoorLocalPosition(RoomPrefab room, RoomSide side)
    {
       
        Vector2 doorWorldPos = room.GetDoorPosition(side);
        Vector2 centerPos = room.Center;
        
        Vector2 localPos = doorWorldPos - centerPos;
        
        
        if (Mathf.Abs(localPos.x) > room.Width || Mathf.Abs(localPos.y) > room.Height)
        {
           
            switch (side)
            {
                case RoomSide.North: return new Vector2(0, room.Height / 2f);
                case RoomSide.South: return new Vector2(0, -room.Height / 2f);
                case RoomSide.West: return new Vector2(-room.Width / 2f, 0);
                case RoomSide.East: return new Vector2(room.Width / 2f, 0);
                default: return Vector2.zero;
            }
        }
        
        return localPos;
    }
    
    private void AddNewConnections(RoomPrefab room, RoomSide attachedSide)
    {
       
        List<RoomSide> availableSides = room.GetAvailableSides();
        
        availableSides.Remove(attachedSide);
        
       
        availableSides = availableSides.OrderBy(x => Random.value).ToList();
        
        
        int maxNewConnections;
        
        
        if (placedRooms.Count >= maxRooms - 2)
        {
            maxNewConnections = Mathf.Min(availableSides.Count, 1);
        }
        else
        {
            
            maxNewConnections = Mathf.Min(availableSides.Count, Random.Range(1, Mathf.Min(availableSides.Count, 3)));
        }
        
       
        for (int i = 0; i < maxNewConnections; i++)
        {
            pendingConnections.Enqueue(new RoomConnection(room, availableSides[i]));
        }
        
       
       
    }
    
    private bool IsOverlapping(Vector2 position, float width, float height)
    {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        
        
        Rect newRect = new Rect(
            position.x - halfWidth - roomSpacing,
            position.y - halfHeight - roomSpacing,
            width + roomSpacing * 2,
            height + roomSpacing * 2
        );
        
        foreach (RoomPrefab room in placedRooms)
        {
            Vector2 roomPos = room.transform.position;
            float roomHalfWidth = room.Width / 2f;
            float roomHalfHeight = room.Height / 2f;
            
            Rect existingRect = new Rect(
                roomPos.x - roomHalfWidth,
                roomPos.y - roomHalfHeight,
                room.Width,
                room.Height
            );
            
            if (newRect.Overlaps(existingRect))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private RoomPrefab InstantiateRoom(RoomPrefab prefab, Vector2 position)
    {
        GameObject roomObject = Instantiate(prefab.gameObject, position, Quaternion.identity, dungeonParent);
        RoomPrefab room = roomObject.GetComponent<RoomPrefab>();
        room.Initialize();
        return room;
    }
    
    private RoomSide GetOppositeSide(RoomSide side)
    {
        switch (side)
        {
            case RoomSide.North: return RoomSide.South;
            case RoomSide.South: return RoomSide.North;
            case RoomSide.West: return RoomSide.East;
            case RoomSide.East: return RoomSide.West;
            default: return RoomSide.North;
        }
    }
    
    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;
        
        if (currentPlayer != null)
            Destroy(currentPlayer);
        
        currentPlayer = Instantiate(playerPrefab, PlayerSpawnPosition, Quaternion.identity, playerSpawnParent);
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(PlayerSpawnPosition.x, PlayerSpawnPosition.y, -10f);
        }
    }
    
    private void ClearDungeon()
    {
        if (dungeonParent != null)
        {
            for (int i = dungeonParent.childCount - 1; i >= 0; i--)
                DestroyImmediate(dungeonParent.GetChild(i).gameObject);
        }
        
        if (currentPlayer != null)
            DestroyImmediate(currentPlayer);
    }
    
    public void RestartDungeon()
    {
        GenerateDungeon();
    }
    
  


[System.Serializable]
public class RoomConnection
{
    public RoomPrefab parentRoom;
    public RoomSide side;
    
    public RoomConnection(RoomPrefab room, RoomSide side)
    {
        this.parentRoom = room;
        this.side = side;
    }
}

}