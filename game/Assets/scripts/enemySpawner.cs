using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    [SerializeField] private bool spawnEnemies = true;

    [Header("Префабы врагов")]
    [SerializeField] private List<GameObject> enemyPrefabs;

    [Header("Родительский объект")]
    [SerializeField] private Transform enemiesParent;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Start()
    {

        Invoke(nameof(SpawnEnemies), 0.5f);
    }

    public void SpawnEnemies()
    {
        if (!spawnEnemies || enemyPrefabs == null || enemyPrefabs.Count == 0) return;

        ClearEnemies();

        List<RoomPrefab> allRooms = FindObjectsByType<RoomPrefab>(FindObjectsSortMode.None).ToList();

        if (allRooms.Count < 2)
        {
            Debug.LogWarning("Слишком мало комнат для спавна врагов!");
            return;
        }

        RoomPrefab startRoom = allRooms.OrderBy(r => Vector2.Distance(Vector2.zero, r.transform.position)).First();
        allRooms.Remove(startRoom);

        int enemiesSpawned = 0;

        foreach (RoomPrefab room in allRooms)
        {
         
            Transform spawn1 = FindDeepChild(room.transform, "Enemy_spawn_1");
            Transform spawn2 = FindDeepChild(room.transform, "Enemy_spawn_2");

            
            List<Transform> availableSpawns = new List<Transform>();
            if (spawn1 != null) availableSpawns.Add(spawn1);
            if (spawn2 != null) availableSpawns.Add(spawn2);

            foreach (Transform spawnPoint in availableSpawns)
            {
              
                GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

                GameObject enemy = Instantiate(randomEnemyPrefab, spawnPoint.position, Quaternion.identity, enemiesParent);
                spawnedEnemies.Add(enemy);
                enemiesSpawned++;
            }
        }

        Debug.Log($"Спавн завершен! Всего врагов на карте: {enemiesSpawned}");
    }

  
    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null) return result;
        }
        return null;
    }

    public void ClearEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        spawnedEnemies.Clear();
    }

    public void OnDungeonRestart()
    {
        ClearEnemies();
        Invoke(nameof(SpawnEnemies), 0.5f);
    }
}