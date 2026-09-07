using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Настройки генерации")]
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int maxGenerationAttempts = 200;
    [SerializeField] private float roomSpacing = 0.5f;

    [Header("Настройки формы лабиринта")]
    [Range(0f, 1f)]
    [Tooltip("1.0 - длинная кишка (змея). 0.0 - круглая клякса. 0.7 - идеальный лабиринт с ветками.")]
    [SerializeField] private float pathStraightness = 0.75f;

    [Header("Префабы комнат")]
    [SerializeField] private List<RoomPrefab> roomPrefabs;
    [SerializeField] private RoomPrefab startRoomPrefab;

    [Header("Родительские объекты")]
    [SerializeField] private Transform dungeonParent;
    [SerializeField] private Transform playerSpawnParent;

    [Header("Игрок")]
    [SerializeField] private GameObject playerPrefab;

    private List<RoomPrefab> placedRooms = new List<RoomPrefab>();
    private List<RoomConnection> openConnections = new List<RoomConnection>();

    private GameObject currentPlayer;

    private Dictionary<RoomPrefab, Dictionary<RoomSide, Vector2>> doorOffsetsCache = new Dictionary<RoomPrefab, Dictionary<RoomSide, Vector2>>();
    private Dictionary<RoomPrefab, Vector2> centerOffsetsCache = new Dictionary<RoomPrefab, Vector2>();

    public Vector2 PlayerSpawnPosition { get; private set; }

    private void Start()
    {
        GenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        ClearDungeon();
        CacheRoomData();

        placedRooms.Clear();
        openConnections.Clear();

        RoomPrefab startRoom = InstantiateRoom(startRoomPrefab, Vector2.zero);
        placedRooms.Add(startRoom);
        PlayerSpawnPosition = startRoom.GetSpawnPoint();

        foreach (RoomSide side in startRoom.GetAvailableSides())
        {
            openConnections.Add(new RoomConnection(startRoom, side));
        }

        int attempts = 0;


        while (placedRooms.Count < maxRooms && openConnections.Count > 0 && attempts < maxGenerationAttempts)
        {
            attempts++;


            RoomConnection connection = ChooseNextConnection();

            if (TryPlaceRoom(connection))
            {

                openConnections.Remove(connection);
            }
            else
            {

                openConnections.Remove(connection);
                connection.parentRoom.CloseDoor(connection.side);
            }
        }




        SpawnPlayer();
    }


    private RoomConnection ChooseNextConnection()
    {

        bool keepGoingStraight = Random.value < pathStraightness;

        if (keepGoingStraight)
        {

            return openConnections[openConnections.Count - 1];
        }
        else
        {

            int randomIndex = Random.Range(0, openConnections.Count);
            return openConnections[randomIndex];
        }
    }

    private void CacheRoomData()
    {
        doorOffsetsCache.Clear();
        centerOffsetsCache.Clear();

        List<RoomPrefab> allPrefabs = new List<RoomPrefab>(roomPrefabs);
        if (!allPrefabs.Contains(startRoomPrefab)) allPrefabs.Add(startRoomPrefab);

        RoomSide[] allSides = { RoomSide.North, RoomSide.South, RoomSide.East, RoomSide.West };

        foreach (RoomPrefab prefab in allPrefabs)
        {
            doorOffsetsCache[prefab] = new Dictionary<RoomSide, Vector2>();

            GameObject tempObj = Instantiate(prefab.gameObject, new Vector3(-10000, -10000, 0), Quaternion.identity);
            RoomPrefab tempRoom = tempObj.GetComponent<RoomPrefab>();
            Vector2 pivot = tempObj.transform.position;

            centerOffsetsCache[prefab] = tempRoom.Center - pivot;

            foreach (RoomSide side in allSides)
            {
                if (tempRoom.HasConnection(side))
                {
                    Vector2 actualDoorPos = tempRoom.GetDoorPosition(side);
                    doorOffsetsCache[prefab][side] = actualDoorPos - pivot;
                }
            }
            DestroyImmediate(tempObj);
        }
    }

    private bool TryPlaceRoom(RoomConnection connection)
    {
        Vector2 parentDoorPos = connection.parentRoom.GetDoorPosition(connection.side);


        List<RoomPrefab> shuffledPrefabs = roomPrefabs.OrderBy(x => Random.value).ToList();

        foreach (RoomPrefab prefabToPlace in shuffledPrefabs)
        {
            RoomSide attachSide = GetOppositeSide(connection.side);


            if (!prefabToPlace.HasConnection(attachSide)) continue;

            Vector2 localDoorOffset = doorOffsetsCache[prefabToPlace][attachSide];
            Vector2 targetPosition = parentDoorPos - localDoorOffset;

            if (!IsOverlapping(prefabToPlace, targetPosition))
            {

                RoomPrefab newRoom = InstantiateRoom(prefabToPlace, targetPosition);
                placedRooms.Add(newRoom);

                newRoom.SetAttachedSide(attachSide);
                connection.parentRoom.SetAttachedSide(connection.side);


                AddNewConnections(newRoom, attachSide);

                return true;
            }
        }

        return false;
    }

    private void AddNewConnections(RoomPrefab room, RoomSide attachedSide)
    {
        List<RoomSide> availableSides = room.GetAvailableSides();


        if (availableSides.Contains(attachedSide))
        {
            availableSides.Remove(attachedSide);
        }


        availableSides = availableSides.OrderBy(x => Random.value).ToList();

        foreach (RoomSide side in availableSides)
        {
            openConnections.Add(new RoomConnection(room, side));
        }
    }


    private bool IsOverlapping(RoomPrefab prefabToCheck, Vector2 targetPivotPos)
    {
        Vector2 localCenterOffset = centerOffsetsCache[prefabToCheck];
        Vector2 expectedWorldCenter = targetPivotPos + localCenterOffset;

        float halfWidth = prefabToCheck.Width / 2f;
        float halfHeight = prefabToCheck.Height / 2f;

        Rect expectedRect = new Rect(
            expectedWorldCenter.x - halfWidth - roomSpacing,
            expectedWorldCenter.y - halfHeight - roomSpacing,
            prefabToCheck.Width + roomSpacing * 2,
            prefabToCheck.Height + roomSpacing * 2
        );

        foreach (RoomPrefab placedRoom in placedRooms)
        {
            Vector2 placedWorldCenter = placedRoom.Center;
            float placedHalfWidth = placedRoom.Width / 2f;
            float placedHalfHeight = placedRoom.Height / 2f;

            Rect existingRect = new Rect(
                placedWorldCenter.x - placedHalfWidth,
                placedWorldCenter.y - placedHalfHeight,
                placedRoom.Width,
                placedRoom.Height
            );

            if (expectedRect.Overlaps(existingRect))
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
        return side switch
        {
            RoomSide.North => RoomSide.South,
            RoomSide.South => RoomSide.North,
            RoomSide.West => RoomSide.East,
            RoomSide.East => RoomSide.West,
            _ => RoomSide.North,
        };
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        if (currentPlayer != null) DestroyImmediate(currentPlayer);

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
            {
                DestroyImmediate(dungeonParent.GetChild(i).gameObject);
            }
        }
        if (currentPlayer != null) DestroyImmediate(currentPlayer);
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