using UnityEngine;
using System.Collections.Generic;

public class RoomPrefab : MonoBehaviour
{
    [Header("Размеры комнаты")]
    [SerializeField] private float width = 21.5f;
    [SerializeField] private float height = 39.75f;
    
    [Header("Точки проходов (стен)")]
    [SerializeField] private Transform northDoor;
    [SerializeField] private Transform southDoor;
    [SerializeField] private Transform westDoor;
    [SerializeField] private Transform eastDoor;
    
    [Header("Точка спавна игрока")]
    [SerializeField] private Transform playerSpawn;
    
    // Свойства
    public float Width => width;
    public float Height => height;
    public RoomSide AttachedSide { get; private set; }
    
    private void Awake()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        // Если точка спавна не назначена, ставим в центр комнаты
        if (playerSpawn == null)
        {
            GameObject spawn = new GameObject("PlayerSpawn");
            spawn.transform.parent = transform;
            spawn.transform.localPosition = new Vector3(width / 2f, height / 2f, 0);
            playerSpawn = spawn.transform;
        }
    }
    
    /// <summary>
    /// Получить позицию прохода по стороне
    /// </summary>
    public Vector2 GetDoorPosition(RoomSide side)
    {
        Transform door = GetDoorTransform(side);
        return door != null ? (Vector2)door.position : (Vector2)transform.position;
    }
    
    private Transform GetDoorTransform(RoomSide side)
    {
        switch (side)
        {
            case RoomSide.North: return northDoor;
            case RoomSide.South: return southDoor;
            case RoomSide.West: return westDoor;
            case RoomSide.East: return eastDoor;
            default: return null;
        }
    }
    
    /// <summary>
    /// Получить точку спавна игрока (без параметров)
    /// </summary>
    public Vector2 GetSpawnPoint()
    {
        if (playerSpawn != null)
            return (Vector2)playerSpawn.position;
        else
            return (Vector2)transform.position + new Vector2(width / 2f, height / 2f);
    }
    
    /// <summary>
    /// Установить сторону, которой приклеилась комната
    /// </summary>
    public void SetAttachedSide(RoomSide side)
    {
        AttachedSide = side;
    }
    
    /// <summary>
    /// Визуализация в редакторе
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Границы комнаты
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + new Vector3(width / 2f, height / 2f, 0), new Vector3(width, height, 0));
        
        // Точки проходов
        Gizmos.color = Color.blue;
        if (northDoor != null) Gizmos.DrawSphere(northDoor.position, 0.3f);
        if (southDoor != null) Gizmos.DrawSphere(southDoor.position, 0.3f);
        if (westDoor != null) Gizmos.DrawSphere(westDoor.position, 0.3f);
        if (eastDoor != null) Gizmos.DrawSphere(eastDoor.position, 0.3f);
        
        // Точка спавна игрока
        Gizmos.color = Color.red;
        if (playerSpawn != null) Gizmos.DrawSphere(playerSpawn.position, 0.2f);
    }
}


public enum RoomSide
{
    North, South, West, East
}