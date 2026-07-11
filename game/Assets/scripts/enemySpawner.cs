using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    [SerializeField] private bool spawnEnemies = true;
    [SerializeField] private int minEnemies = 3;
    [SerializeField] private int maxEnemies = 8;
    [SerializeField] private float minDistanceFromPlayer = 3f;
    [SerializeField] private int enemiesPerRoom = 1;
    
    [Header("Префабы врагов")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    
    [Header("Сложность")]
    [SerializeField] private float difficultyMultiplier = 1f;
    
    [Header("Родительский объект")]
    [SerializeField] private Transform enemiesParent;
    
    private DungeonGenerator dungeonGenerator;
    private List<RoomPrefab> placedRooms;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private Transform playerTransform;
    
    private void Start()
    {
        dungeonGenerator = FindAnyObjectByType<DungeonGenerator>();
        if (dungeonGenerator == null)
        {
            Debug.LogWarning("DungeonGenerator не найден!");
            return;
        }
        
        // Задержка перед спавном, чтобы подземелье успело сгенерироваться
        Invoke("SpawnEnemies", 0.5f);
    }
    
    public void SpawnEnemies()
    {
        if (!spawnEnemies) return;
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("Нет префабов врагов!");
            return;
        }
        
        // Находим игрока
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogWarning("Игрок не найден! Убедитесь, что у игрока есть тег 'Player'");
            return;
        }
        
        // Получаем комнаты
        placedRooms = GetRooms();
        if (placedRooms == null || placedRooms.Count < 2)
        {
            Debug.LogWarning("Недостаточно комнат для спавна врагов!");
            return;
        }
        
        // Очищаем старых врагов
        ClearEnemies();
        
        // Исключаем стартовую комнату (там спавнится игрок)
        List<RoomPrefab> spawnRooms = placedRooms.Skip(1).ToList();
        if (spawnRooms.Count == 0)
        {
            Debug.LogWarning("Нет комнат для спавна врагов!");
            return;
        }
        
        // Определяем общее количество врагов
        int totalEnemies = Random.Range(minEnemies, maxEnemies + 1);
        totalEnemies = Mathf.RoundToInt(totalEnemies * difficultyMultiplier);
        
        Debug.Log($" Спавн {totalEnemies} врагов в {spawnRooms.Count} комнатах");
        
        // Перемешиваем комнаты для случайного распределения
        spawnRooms = spawnRooms.OrderBy(x => Random.value).ToList();
        
        // Спавним врагов
        int enemiesSpawned = 0;
        int roomIndex = 0;
        
        while (enemiesSpawned < totalEnemies && roomIndex < spawnRooms.Count)
        {
            RoomPrefab room = spawnRooms[roomIndex];
            
            // Определяем сколько врагов спавнить в этой комнате
            int enemiesInRoom = Random.Range(0, enemiesPerRoom + 1);
            
            // Если осталось мало врагов, спавним оставшихся
            if (enemiesSpawned + enemiesInRoom > totalEnemies)
            {
                enemiesInRoom = totalEnemies - enemiesSpawned;
            }
            
            if (enemiesInRoom > 0)
            {
                int spawned = SpawnEnemiesInRoom(room, enemiesInRoom);
                enemiesSpawned += spawned;
                Debug.Log($" Комната {roomIndex + 1}: спавнено {spawned} врагов");
            }
            
            roomIndex++;
        }
        
        Debug.Log($"Всего спавнено {enemiesSpawned} врагов");
    }
    
    private int SpawnEnemiesInRoom(RoomPrefab room, int count)
    {
        int spawned = 0;
        
        for (int i = 0; i < count; i++)
        {
            // Выбираем случайного врага из списка
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            
            // Получаем позицию для спавна
            Vector2 spawnPos = GetValidSpawnPosition(room);
            
            if (spawnPos != Vector2.zero)
            {
                // Спавним врага
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, enemiesParent);
                spawnedEnemies.Add(enemy);
                spawned++;
                
                
            }
        }
        
        return spawned;
    }
    
    private Vector2 GetValidSpawnPosition(RoomPrefab room)
    {
        Vector2 center = room.Center;
        float halfWidth = room.Width / 2f - 1f;
        float halfHeight = room.Height / 2f - 1f;
        
        // Пробуем найти место для спавна
        for (int attempt = 0; attempt < 20; attempt++)
        {
            // Случайная позиция внутри комнаты (с отступом от стен)
            float x = center.x + Random.Range(-halfWidth, halfWidth);
            float y = center.y + Random.Range(-halfHeight, halfHeight);
            Vector2 pos = new Vector2(x, y);
            
            // Проверяем расстояние до игрока
            if (playerTransform != null)
            {
                float distance = Vector2.Distance(pos, playerTransform.position);
                if (distance < minDistanceFromPlayer)
                {
                    continue; // Слишком близко к игроку
                }
            }
            
            // Проверяем, что позиция не занята другими врагами
            bool positionFree = true;
            foreach (var enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    float distance = Vector2.Distance(pos, enemy.transform.position);
                    if (distance < 1f)
                    {
                        positionFree = false;
                        break;
                    }
                }
            }
            
            if (positionFree)
            {
                return pos;
            }
        }
        
        // Если не нашли место, используем центр комнаты (но смещаем немного)
        return center + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }
    
    private List<RoomPrefab> GetRooms()
    {
        // Пытаемся получить комнаты из DungeonGenerator через рефлексию
        if (dungeonGenerator != null)
        {
            var field = dungeonGenerator.GetType().GetField("placedRooms", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                var rooms = field.GetValue(dungeonGenerator) as List<RoomPrefab>;
                if (rooms != null && rooms.Count > 0)
                {
                    return rooms;
                }
            }
        }
        
        // Альтернативный способ: ищем все комнаты в родительском объекте
        if (dungeonGenerator != null)
        {
            // Ищем DungeonParent как дочерний объект
            Transform parent = dungeonGenerator.transform.Find("DungeonParent");
            if (parent == null)
            {
                // Или используем dungeonParent из DungeonGenerator
                var parentField = dungeonGenerator.GetType().GetField("dungeonParent",
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (parentField != null)
                {
                    parent = parentField.GetValue(dungeonGenerator) as Transform;
                }
            }
            
            if (parent != null)
            {
                List<RoomPrefab> rooms = new List<RoomPrefab>();
                foreach (Transform child in parent)
                {
                    RoomPrefab room = child.GetComponent<RoomPrefab>();
                    if (room != null)
                    {
                        rooms.Add(room);
                    }
                }
                return rooms;
            }
        }
        
        // Последний вариант: ищем все RoomPrefab в сцене
        return FindObjectsByType<RoomPrefab>().ToList();
    }
    
    private void ClearEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }
    
    public void ClearAllEnemies()
    {
        ClearEnemies();
    }
    
    public List<GameObject> GetSpawnedEnemies()
    {
        return spawnedEnemies;
    }
    
    public int GetEnemyCount()
    {
        int count = 0;
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null) count++;
        }
        return count;
    }
    
    // Вызывается при перезапуске подземелья
    public void OnDungeonRestart()
    {
        ClearEnemies();
        Invoke("SpawnEnemies", 0.5f);
    }
}