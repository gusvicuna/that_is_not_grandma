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
    }

    void Start()
    {
        // Announces the starting room too: without it the first room's ambience never begins and a
        // beat waiting on that room can never fire.
        ShowRoom(currentRoom);
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
