using UnityEngine;

public class RoomNavigation : MonoBehaviour
{
    public RoomController roomController;
    private void OnMouseDown(){
        roomController.GoToNextRoom();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
