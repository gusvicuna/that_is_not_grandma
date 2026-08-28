using UnityEngine;

public class RoomController : MonoBehaviour
{
    public GameObject[] rooms;
    private int currentRoom = 0;
    void Start()
    {
        ShowRoom(currentRoom);
    }
    public void GoToNextRoom(){
        currentRoom++;
        if(currentRoom >= rooms.Length){
            currentRoom= 0;
        }
        ShowRoom(currentRoom);
        }
        void ShowRoom(int roomIndex)
        {
            for(int i=0; i < rooms.Length;i++)
            {
                rooms[i].SetActive(i==roomIndex);
            }
        }
}
