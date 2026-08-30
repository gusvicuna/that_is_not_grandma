using Game.Domain;
using Game.Events;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public GameObject[] rooms;

    [Tooltip("Which RoomId each entry of Rooms is, in the same order. Audio, the story director and the night check all speak RoomId — an index into this array means nothing to them, and a mismatched order would leak the wrong room.")]
    [SerializeField] private RoomId[] roomIds;

    [SerializeField] private RoomIdEventChannelSO _roomChanged;

    private int currentRoom = 0;

    private void Awake()
    {
        int roomCount = rooms != null ? rooms.Length : 0;
        int idCount = roomIds != null ? roomIds.Length : 0;
        if (roomCount != idCount)
        {
            Debug.LogError(
                $"RoomController on '{name}': Rooms has {roomCount} entries and Room Ids has {idCount}. " +
                "They must line up one to one, or the game announces the wrong room.",
                this);
        }

        for (int i = 0; i < roomCount; i++)
        {
            if (rooms[i] == null)
            {
                Debug.LogError(
                    $"RoomController on '{name}': Rooms[{i}] is empty or points at a deleted object. " +
                    "Changing rooms will throw as soon as it reaches that entry.",
                    this);
            }
        }
    }

    void Start()
    {
        // The scene is authored with one room already active and Start does not touch SetActive:
        // whichever room is enabled in the editor is the one the run begins in. It is only picked
        // up here so the arrows keep walking from the right place.
        currentRoom = FindActiveRoom();

        // The starting room is still announced: without it the first room's ambience never begins
        // and a beat waiting on that room can never fire.
        AnnounceRoom(currentRoom);
    }

    /// <summary>
    /// The index of the room left active in the editor. Falls back to the first one — a scene with
    /// every room disabled is an authoring mistake, and a black screen is a worse way to report it.
    /// </summary>
    private int FindActiveRoom()
    {
        int active = -1;
        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] == null || !rooms[i].activeSelf)
            {
                continue;
            }
            if (active < 0)
            {
                active = i;
            }
            else
            {
                Debug.LogWarning(
                    $"RoomController on '{name}': Rooms[{i}] is active on top of Rooms[{active}]. " +
                    "Only one room may start enabled; the first one wins.",
                    this);
            }
        }

        if (active >= 0)
        {
            return active;
        }

        Debug.LogError(
            $"RoomController on '{name}': no room is active in the scene. Enable the starting room " +
            "in the editor. Falling back to the first entry.",
            this);
        if (rooms.Length > 0 && rooms[0] != null)
        {
            rooms[0].SetActive(true);
        }
        return 0;
    }

    public void GoToNextRoom()
    {
        currentRoom++;

        if (currentRoom >= rooms.Length)
        {
            currentRoom = 0;
        }

        ShowRoom(currentRoom);
    }

    public void GoToPreviousRoom()
    {
        currentRoom--;

        if (currentRoom < 0)
        {
            currentRoom = rooms.Length - 1;
        }

        ShowRoom(currentRoom);
    }

    private void ShowRoom(int roomIndex)
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i].SetActive(i == roomIndex);
        }

        AnnounceRoom(roomIndex);
    }

    private void AnnounceRoom(int roomIndex)
    {
        if (_roomChanged == null || roomIds == null || roomIndex >= roomIds.Length)
        {
            return;
        }
        _roomChanged.Raise(roomIds[roomIndex]);
    }
}
