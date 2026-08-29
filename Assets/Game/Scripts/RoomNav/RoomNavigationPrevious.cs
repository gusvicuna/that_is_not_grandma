
using UnityEngine;

public class RoomNavigationPrevious : MonoBehaviour
{
    public RoomController roomController;

    private void OnMouseDown()
    {
        roomController.GoToPreviousRoom();
    }
}


