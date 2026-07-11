using UnityEngine;
using System.Collections.Generic;

public class RoomPrefab : MonoBehaviour
{
    [Header("Размеры комнаты (реальные размеры в юнитах)")]
    [SerializeField] private float width = 21.5f;
    [SerializeField] private float height = 39.75f;
    
    [Header("Смещение pivot (от центра комнаты)")]
    [SerializeField] private Vector2 pivotOffset = Vector2.zero;
    
    [Header("Точки проходов (стен)")]
    [SerializeField] private Transform northDoor;
    [SerializeField] private Transform southDoor;
    [SerializeField] private Transform westDoor;
    [SerializeField] private Transform eastDoor;
    
    [Header("Доступные стороны для соединения")]
    [SerializeField] private bool hasNorthConnection = true;
    [SerializeField] private bool hasSouthConnection = true;
    [SerializeField] private bool hasWestConnection = true;
    [SerializeField] private bool hasEastConnection = true;
    
    [Header("Точка спавна игрока")]
    [SerializeField] private Transform playerSpawn;
    
    public float Width => width;
    public float Height => height;
    public Vector2 PivotOffset => pivotOffset;
    public RoomSide AttachedSide { get; private set; }
    
    // Центр комнаты (с учетом смещения pivot)
    public Vector2 Center => (Vector2)transform.position + pivotOffset;
    
    // Проверка доступности стороны
    public bool HasConnection(RoomSide side)
    {
        switch (side)
        {
            case RoomSide.North: return hasNorthConnection && northDoor != null;
            case RoomSide.South: return hasSouthConnection && southDoor != null;
            case RoomSide.West: return hasWestConnection && westDoor != null;
            case RoomSide.East: return hasEastConnection && eastDoor != null;
            default: return false;
        }
    }
    
    // Получить все доступные стороны
    public List<RoomSide> GetAvailableSides()
    {
        List<RoomSide> sides = new List<RoomSide>();
        if (hasNorthConnection && northDoor != null) sides.Add(RoomSide.North);
        if (hasSouthConnection && southDoor != null) sides.Add(RoomSide.South);
        if (hasWestConnection && westDoor != null) sides.Add(RoomSide.West);
        if (hasEastConnection && eastDoor != null) sides.Add(RoomSide.East);
        return sides;
    }
    
    private void Awake()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        if (playerSpawn == null)
        {
            GameObject spawn = new GameObject("PlayerSpawn");
            spawn.transform.parent = transform;
            spawn.transform.localPosition = pivotOffset;
            playerSpawn = spawn.transform;
        }
    }
    
    public Vector2 GetDoorPosition(RoomSide side)
    {
        Transform door = GetDoorTransform(side);
        if (door != null)
        {
            return (Vector2)door.position;
        }
        
        // Если дверь не задана, вычисляем от центра комнаты
        Vector2 center = Center;
        switch (side)
        {
            case RoomSide.North: 
                return new Vector2(center.x, center.y + height / 2f);
            case RoomSide.South: 
                return new Vector2(center.x, center.y - height / 2f);
            case RoomSide.West: 
                return new Vector2(center.x - width / 2f, center.y);
            case RoomSide.East: 
                return new Vector2(center.x + width / 2f, center.y);
            default: 
                return center;
        }
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
    
    public Vector2 GetSpawnPoint()
    {
        if (playerSpawn != null)
            return (Vector2)playerSpawn.position;
        else
            return Center;
    }
    
    public void SetAttachedSide(RoomSide side)
    {
        AttachedSide = side;
    }
    
    [ContextMenu("Auto Detect Pivot")]
    public void AutoDetectPivot()
    {
        // Автоматически определяем смещение pivot
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            width = spriteRenderer.sprite.bounds.size.x;
            height = spriteRenderer.sprite.bounds.size.y;
            
            Vector2 spritePivot = spriteRenderer.sprite.pivot / spriteRenderer.sprite.pixelsPerUnit;
            Vector2 spriteCenter = new Vector2(
                spriteRenderer.sprite.bounds.size.x / 2f,
                spriteRenderer.sprite.bounds.size.y / 2f
            );
            
            pivotOffset = (Vector2)transform.position - (spritePivot - spriteCenter);
            
            Debug.Log($"Автоопределены размеры: Width={width}, Height={height}, Pivot Offset={pivotOffset}");
        }
        else
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                width = collider.size.x;
                height = collider.size.y;
                pivotOffset = collider.offset;
                Debug.Log($"Размеры из коллайдера: Width={width}, Height={height}, Offset={pivotOffset}");
            }
            else
            {
                Debug.LogWarning("Не удалось автоматически определить размер. Установите вручную.");
            }
        }
    }
    
    // Отладочная визуализация
    private void OnDrawGizmosSelected()
    {
        DrawDebugVisualization();
    }
    
    private void OnDrawGizmos()
    {
        DrawDebugVisualization();
    }
    
    private void DrawDebugVisualization()
    {
        Vector2 center = Center;
        Vector2 position = transform.position;
        
        // === 1. РАМКА КОМНАТЫ ===
        Gizmos.color = Color.green;
        Vector3 rectCenter = new Vector3(center.x, center.y, 0);
        Vector3 rectSize = new Vector3(width, height, 0);
        Gizmos.DrawWireCube(rectCenter, rectSize);
        
        // === 2. ЗАЛИВКА ===
        Gizmos.color = new Color(0, 1, 0, 0.05f);
        Gizmos.DrawCube(rectCenter, rectSize);
        
        // === 3. CENTER ===
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(center, 0.4f);
        
        // === 4. PIVOT ===
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(position, 0.3f);
        
        // === 5. ЛИНИЯ ОТ PIVOT К ЦЕНТРУ ===
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(position, center);
        
        // === 6. ДВЕРИ (только доступные) ===
        DrawDoorGizmo(RoomSide.North, "N", hasNorthConnection);
        DrawDoorGizmo(RoomSide.South, "S", hasSouthConnection);
        DrawDoorGizmo(RoomSide.West, "W", hasWestConnection);
        DrawDoorGizmo(RoomSide.East, "E", hasEastConnection);
        
        // === 7. ТОЧКА СПАВНА ===
        Gizmos.color = Color.cyan;
        if (playerSpawn != null)
        {
            Gizmos.DrawSphere(playerSpawn.position, 0.3f);
        }
        
        // === 8. ПОДПИСИ ===
        #if UNITY_EDITOR
        string availableSides = "";
        if (hasNorthConnection) availableSides += "N";
        if (hasSouthConnection) availableSides += "S";
        if (hasWestConnection) availableSides += "W";
        if (hasEastConnection) availableSides += "E";
        
        UnityEditor.Handles.Label(new Vector3(center.x, center.y + height/2f + 1.5f, 0), 
            $"W:{width:F1} H:{height:F1} [{availableSides}]");
        UnityEditor.Handles.Label(new Vector3(center.x + width/2f + 1f, center.y, 0), 
            "Center");
        UnityEditor.Handles.Label(new Vector3(position.x, position.y - 1f, 0), 
            $"Pivot (Offset: {pivotOffset})");
        #endif
    }
    
    private void DrawDoorGizmo(RoomSide side, string label, bool hasConnection)
    {
        Vector2 pos = GetDoorPosition(side);
        
        if (hasConnection)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pos, 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pos, 0.6f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(new Vector3(pos.x, pos.y + 0.8f, 0), label + " ✓");
            #endif
        }
        else
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(pos, 0.3f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(new Vector3(pos.x, pos.y + 0.8f, 0), label + " ✗");
            #endif
        }
    }
}

public enum RoomSide
{
    North, South, West, East
}