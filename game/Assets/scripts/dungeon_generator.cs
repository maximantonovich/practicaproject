using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Настройки генерации")]
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int maxGenerationAttempts = 100;
    [SerializeField] private float roomSpacing = 0.1f;
    
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
        
        // Создаём стартовую комнату
        RoomPrefab startRoom = InstantiateRoom(startRoomPrefab, Vector2.zero);
        placedRooms.Add(startRoom);
        PlayerSpawnPosition = startRoom.GetSpawnPoint();
        
        // Добавляем все стороны в очередь
        foreach (RoomSide side in System.Enum.GetValues(typeof(RoomSide)))
        {
            pendingConnections.Enqueue(new RoomConnection(startRoom, side));
        }
        
        // Генерируем
        int attempts = 0;
        while (placedRooms.Count < maxRooms && pendingConnections.Count > 0 && attempts < maxGenerationAttempts)
        {
            attempts++;
            RoomConnection connection = pendingConnections.Dequeue();
            
            if (TryPlaceRoom(connection))
            {
                Debug.Log($"✅ Комната {placedRooms.Count} создана!");
            }
        }
        
        Debug.Log($"Генерация завершена! Всего комнат: {placedRooms.Count}");
        
        SpawnPlayer();
    }
    
    private bool TryPlaceRoom(RoomConnection connection)
    {
        if (roomPrefabs == null || roomPrefabs.Count == 0)
        {
            Debug.LogError("Нет префабов комнат!");
            return false;
        }
        
        // Получаем позицию прохода родительской комнаты
        Vector2 doorPos = connection.parentRoom.GetDoorPosition(connection.side);
        
        if (doorPos == Vector2.zero)
        {
            Debug.LogWarning($"Точка двери {connection.side} не назначена в {connection.parentRoom.name}");
            return false;
        }
        
        // Выбираем случайную комнату
        RoomPrefab newRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
        
        // ВЫЧИСЛЯЕМ ПОЗИЦИЮ С ВЫРАВНИВАНИЕМ
        Vector2 newRoomPos = CalculateRoomPosition(doorPos, connection.side, newRoomPrefab);
        
       
        if (IsOverlapping(newRoomPos, newRoomPrefab.Width, newRoomPrefab.Height))
        {
            Debug.Log($"Комната пересекается, пропускаем");
            return false;
        }
        
        // Создаём комнату
        RoomPrefab newRoom = InstantiateRoom(newRoomPrefab, newRoomPos);
        placedRooms.Add(newRoom);
        
        // Определяем, какой стороной приклеилась новая комната
        RoomSide oppositeSide = GetOppositeSide(connection.side);
        newRoom.SetAttachedSide(oppositeSide);
        
        // Добавляем новые стороны в очередь (кроме той, которой приклеились)
        foreach (RoomSide side in System.Enum.GetValues(typeof(RoomSide)))
        {
            if (side != oppositeSide)
            {
                pendingConnections.Enqueue(new RoomConnection(newRoom, side));
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// РАСЧЁТ ПОЗИЦИИ С ПРАВИЛЬНЫМ ВЫРАВНИВАНИЕМ
    /// </summary>
    private Vector2 CalculateRoomPosition(Vector2 doorPos, RoomSide side, RoomPrefab newRoom)
    {
        switch (side)
        {
            case RoomSide.North:
                // Комната сверху: выравниваем по центру двери по X
                // Y = doorPos.y - высота комнаты (чтобы нижняя стена совпала с дверью)
                return new Vector2(
                    doorPos.x - newRoom.Width / 2f,  // ← Центрируем по X
                    doorPos.y - newRoom.Height + 0.1f
                );
                
            case RoomSide.South:
                // Комната снизу: выравниваем по центру двери по X
                // Y = doorPos.y (верхняя стена совпадает с дверью)
                return new Vector2(
                    doorPos.x - newRoom.Width / 2f,  // ← Центрируем по X
                    doorPos.y - 0.1f
                );
                
            case RoomSide.West:
                // Комната слева: выравниваем по центру двери по Y
                // X = doorPos.x - ширина комнаты (правая стена совпадает с дверью)
                return new Vector2(
                    doorPos.x - newRoom.Width + 0.1f,
                    doorPos.y - newRoom.Height / 2f   // ← Центрируем по Y
                );
                
            case RoomSide.East:
                // Комната справа: выравниваем по центру двери по Y
                // X = doorPos.x (левая стена совпадает с дверью)
                return new Vector2(
                    doorPos.x - 0.1f,
                    doorPos.y - newRoom.Height / 2f   // ← Центрируем по Y
                );
                
            default:
                return Vector2.zero;
        }
    }
    
    private bool IsOverlapping(Vector2 position, float width, float height)
    {
        Rect newRect = new Rect(position.x, position.y, width, height);
        
        foreach (RoomPrefab room in placedRooms)
        {
            Rect existingRect = new Rect(
                room.transform.position.x,
                room.transform.position.y,
                room.Width,
                room.Height
            );
            
            // Добавляем отступ
            Rect expandedNewRect = new Rect(
                newRect.x - roomSpacing,
                newRect.y - roomSpacing,
                newRect.width + roomSpacing * 2,
                newRect.height + roomSpacing * 2
            );
            
            if (expandedNewRect.Overlaps(existingRect))
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
        Camera.main.transform.position = new Vector3(PlayerSpawnPosition.x, PlayerSpawnPosition.y, -10f);
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

