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
        
        if (roomPrefabs == null || roomPrefabs.Count == 0)
        {
            Debug.LogError("❌ Нет префабов комнат!");
            return;
        }
        
        if (startRoomPrefab == null)
        {
            Debug.LogError("❌ Нет префаба стартовой комнаты!");
            return;
        }
        
        // Создаём стартовую комнату
        RoomPrefab startRoom = InstantiateRoom(startRoomPrefab, Vector2.zero);
        placedRooms.Add(startRoom);
        PlayerSpawnPosition = startRoom.GetSpawnPoint();
        
        Debug.Log($"✅ Стартовая комната создана! Position: {startRoom.transform.position}, Center: {startRoom.Center}");
        Debug.Log($"   Доступные стороны: {string.Join(", ", startRoom.GetAvailableSides())}");
        
        // Добавляем только доступные стороны в очередь
        foreach (RoomSide side in startRoom.GetAvailableSides())
        {
            pendingConnections.Enqueue(new RoomConnection(startRoom, side));
        }
        
        // Генерируем
        int attempts = 0;
        int failedAttempts = 0;
        
        while (placedRooms.Count < maxRooms && pendingConnections.Count > 0 && attempts < maxGenerationAttempts)
        {
            attempts++;
            
            if (failedAttempts > 50)
            {
                Debug.LogWarning($"⚠️ Слишком много неудачных попыток ({failedAttempts}), прерываем генерацию");
                break;
            }
            
            RoomConnection connection = pendingConnections.Dequeue();
            
            // Проверяем, нужно ли создавать тупик
            if (ShouldCreateDeadEnd(connection))
            {
                Debug.Log($"  🚫 Создан тупик на стороне {connection.side}");
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
        
        Debug.Log($"🎯 Генерация завершена! Всего комнат: {placedRooms.Count}, попыток: {attempts}");
        Debug.Log($"📊 Ожидающих соединений: {pendingConnections.Count}");
        
        // Проверяем, не осталось ли висячих соединений
        if (pendingConnections.Count > 0)
        {
            Debug.Log($"⚠️ Осталось {pendingConnections.Count} неиспользованных соединений");
        }
        
        SpawnPlayer();
    }
    
    private bool ShouldCreateDeadEnd(RoomConnection connection)
    {
        // Не создаем тупик, если это единственное доступное соединение
        if (placedRooms.Count <= 1)
            return false;
        
        // Если у родительской комнаты только одна доступная сторона (кроме текущей)
        RoomPrefab parentRoom = connection.parentRoom;
        List<RoomSide> availableSides = parentRoom.GetAvailableSides();
        availableSides.Remove(connection.side);
        
        // Если после удаления текущей стороны у комнаты не осталось других сторон
        if (availableSides.Count == 0)
            return false;
        
        // Создаем тупик с определенным шансом
        float chance = deadEndChance;
        
        // Увеличиваем шанс тупика, если комната уже имеет много соединений
        int connectionCount = parentRoom.GetAvailableSides().Count;
        if (connectionCount >= 3)
        {
            chance += 0.2f;
        }
        
        // Уменьшаем шанс, если комнат еще мало
        if (placedRooms.Count < maxRooms * 0.3f)
        {
            chance *= 0.5f;
        }
        
        return Random.value < chance;
    }
    
    private bool TryPlaceRoom(RoomConnection connection)
    {
        // Получаем позицию двери родительской комнаты
        Vector2 doorPos = connection.parentRoom.GetDoorPosition(connection.side);
        
        // Перемешиваем префабы
        List<RoomPrefab> shuffledPrefabs = roomPrefabs.OrderBy(x => Random.value).ToList();
        
        // Пробуем разные комнаты
        foreach (RoomPrefab newRoomPrefab in shuffledPrefabs)
        {
            // Проверяем, есть ли у новой комнаты сторона для соединения
            RoomSide oppositeSide = GetOppositeSide(connection.side);
            if (!newRoomPrefab.HasConnection(oppositeSide))
            {
                continue;
            }
            
            // Вычисляем позицию для новой комнаты
            Vector2 newRoomPos = CalculateRoomPosition(doorPos, connection.side, newRoomPrefab);
            
            // Проверяем пересечения
            if (!IsOverlapping(newRoomPos, newRoomPrefab.Width, newRoomPrefab.Height))
            {
                // Создаём комнату
                RoomPrefab newRoom = InstantiateRoom(newRoomPrefab, newRoomPos);
                placedRooms.Add(newRoom);
                
                // Устанавливаем сторону, которой приклеилась комната
                newRoom.SetAttachedSide(oppositeSide);
                
                // Добавляем новые соединения (только доступные стороны)
                AddNewConnections(newRoom, oppositeSide);
                
                Debug.Log($"  ✅ Комната {placedRooms.Count} создана! Позиция: {newRoomPos}, Center: {newRoom.Center}");
                Debug.Log($"     Доступные стороны: {string.Join(", ", newRoom.GetAvailableSides())}");
                return true;
            }
        }
        
        return false;
    }
    
    private Vector2 CalculateRoomPosition(Vector2 doorPos, RoomSide side, RoomPrefab newRoom)
    {
        // Получаем позицию двери относительно центра новой комнаты
        RoomSide attachSide = GetOppositeSide(side);
        Vector2 doorLocalPos = GetDoorLocalPosition(newRoom, attachSide);
        
        // Вычисляем позицию pivot новой комнаты
        Vector2 centerPos = doorPos - doorLocalPos;
        Vector2 pivotPos = centerPos - newRoom.PivotOffset;
        
        return pivotPos;
    }
    
    private Vector2 GetDoorLocalPosition(RoomPrefab room, RoomSide side)
    {
        // Получаем позицию двери относительно центра комнаты
        Vector2 doorWorldPos = room.GetDoorPosition(side);
        Vector2 centerPos = room.Center;
        
        Vector2 localPos = doorWorldPos - centerPos;
        
        // Проверяем, что смещение корректное
        if (Mathf.Abs(localPos.x) > room.Width || Mathf.Abs(localPos.y) > room.Height)
        {
            // Используем стандартные позиции
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
        // Получаем все доступные стороны комнаты
        List<RoomSide> availableSides = room.GetAvailableSides();
        
        // Удаляем сторону, которой приклеилась комната
        availableSides.Remove(attachedSide);
        
        // Перемешиваем
        availableSides = availableSides.OrderBy(x => Random.value).ToList();
        
        // Проверяем, нужно ли добавлять все стороны или только некоторые
        int maxNewConnections;
        
        // Если это последняя комната или осталось мало комнат, создаем меньше соединений
        if (placedRooms.Count >= maxRooms - 2)
        {
            maxNewConnections = Mathf.Min(availableSides.Count, 1);
        }
        else
        {
            // Создаем от 0 до 2 новых соединений
            maxNewConnections = Mathf.Min(availableSides.Count, Random.Range(1, Mathf.Min(availableSides.Count, 3)));
        }
        
        // Добавляем соединения
        for (int i = 0; i < maxNewConnections; i++)
        {
            pendingConnections.Enqueue(new RoomConnection(room, availableSides[i]));
        }
        
        // Если комната не имеет доступных сторон, это тупик
        if (availableSides.Count == 0)
        {
            Debug.Log($"  🚫 Комната {placedRooms.Count} - тупик (нет доступных сторон)");
        }
        else if (maxNewConnections == 0)
        {
            Debug.Log($"  🚫 Комната {placedRooms.Count} - тупик (все стороны заблокированы)");
        }
        else
        {
            Debug.Log($"  🔗 Добавлено {maxNewConnections} новых соединений");
        }
    }
    
    private bool IsOverlapping(Vector2 position, float width, float height)
    {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        
        // Создаем прямоугольник для новой комнаты
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
    
    private void OnDrawGizmos()
    {
        if (placedRooms == null || placedRooms.Count == 0) return;
        
        foreach (var room in placedRooms)
        {
            if (room != null)
            {
                Vector2 center = room.Center;
                
                // Рамка
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(center, new Vector3(room.Width, room.Height, 0));
                
                // Центр
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(center, 0.2f);
                
                // Pivot
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(room.transform.position, 0.15f);
                
                // Доступные двери
                foreach (RoomSide side in System.Enum.GetValues(typeof(RoomSide)))
                {
                    Vector2 doorPos = room.GetDoorPosition(side);
                    if (room.HasConnection(side))
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(doorPos, 0.25f);
                    }
                    else
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawSphere(doorPos, 0.15f);
                    }
                }
            }
        }
        
        // Рисуем ожидающие соединения
        Gizmos.color = Color.yellow;
        foreach (var connection in pendingConnections)
        {
            if (connection.parentRoom != null)
            {
                Vector2 doorPos = connection.parentRoom.GetDoorPosition(connection.side);
                Gizmos.DrawSphere(doorPos, 0.3f);
            }
        }
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