
using UnityEngine;
using Game.Events;

public class RoomController : MonoBehaviour
{
    public GameObject[] rooms;

    [SerializeField] private RoomChangedEventChannelSO _roomChanged;

    private int currentRoom = 0;

    void Start()
    {
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

        _roomChanged.Raise(currentRoom);
    }

    public void GoToPreviousRoom()
    {
        currentRoom--;

        if (currentRoom < 0)
        {
            currentRoom = rooms.Length - 1;
        }

        ShowRoom(currentRoom);

        _roomChanged.Raise(currentRoom);
    }

    private void ShowRoom(int roomIndex)
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i].SetActive(i == roomIndex);
        }
    }
}
