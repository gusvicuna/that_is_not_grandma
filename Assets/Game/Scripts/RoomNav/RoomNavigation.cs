
using UnityEngine;

public class RoomNavigation : MonoBehaviour
{
    public RoomController roomController;

    private void OnMouseDown()
    {
        roomController.GoToNextRoom();
    }
}

