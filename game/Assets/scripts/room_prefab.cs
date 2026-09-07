using UnityEngine;
using System.Collections.Generic;

public class RoomPrefab : MonoBehaviour
{
    [Header("Размеры комнаты (реальные размеры в юнитах)")]
    [SerializeField] private float width = 21.5f;
    [SerializeField] private float height = 39.75f;

    [Header("Смещение pivot (от центра комнаты)")]
    [SerializeField] private Vector2 pivotOffset = Vector2.zero;

    [Header("Точки проходов (где должны быть двери)")]
    [SerializeField] private Transform northDoor;
    [SerializeField] private Transform southDoor;
    [SerializeField] private Transform westDoor;
    [SerializeField] private Transform eastDoor;

    [Header("Объекты глухих стен (По умолчанию должны быть ВКЛЮЧЕНЫ)")]
    [SerializeField] private GameObject northWall;
    [SerializeField] private GameObject southWall;
    [SerializeField] private GameObject westWall;
    [SerializeField] private GameObject eastWall;

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

    public Vector2 Center => (Vector2)transform.position + pivotOffset;

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

    // МЕТОД ИЗМЕНЕН: Теперь он УБИРАЕТ стену, создавая проход
    public void SetAttachedSide(RoomSide side)
    {
        AttachedSide = side;

        // Помечаем, что сторона занята
        switch (side)
        {
            case RoomSide.North: hasNorthConnection = false; break;
            case RoomSide.South: hasSouthConnection = false; break;
            case RoomSide.West: hasWestConnection = false; break;
            case RoomSide.East: hasEastConnection = false; break;
        }

        // ВЫКЛЮЧАЕМ стену, чтобы можно было пройти в соседнюю комнату
        switch (side)
        {
            case RoomSide.North:
                if (northWall != null) northWall.SetActive(false);
                break;
            case RoomSide.South:
                if (southWall != null) southWall.SetActive(false);
                break;
            case RoomSide.West:
                if (westWall != null) westWall.SetActive(false);
                break;
            case RoomSide.East:
                if (eastWall != null) eastWall.SetActive(false);
                break;
        }
    }

    public void CloseDoor(RoomSide side)
    {
        // Помечаем проход как недоступный
        switch (side)
        {
            case RoomSide.North: hasNorthConnection = false; break;
            case RoomSide.South: hasSouthConnection = false; break;
            case RoomSide.West: hasWestConnection = false; break;
            case RoomSide.East: hasEastConnection = false; break;
        }

        // Поскольку стены у нас теперь стоят по дефолту, 
        // мы просто страхуемся и убеждаемся, что стена точно включена.
        switch (side)
        {
            case RoomSide.North:
                if (northWall != null) northWall.SetActive(true);
                break;
            case RoomSide.South:
                if (southWall != null) southWall.SetActive(true);
                break;
            case RoomSide.West:
                if (westWall != null) westWall.SetActive(true);
                break;
            case RoomSide.East:
                if (eastWall != null) eastWall.SetActive(true);
                break;
        }
    }

    [ContextMenu("Auto Detect Pivot")]
    public void AutoDetectPivot()
    {
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
        }
        else
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                width = collider.size.x;
                height = collider.size.y;
                pivotOffset = collider.offset;
            }
        }
    }
}
    public enum RoomSide
    {
        North, South, West, East
    }