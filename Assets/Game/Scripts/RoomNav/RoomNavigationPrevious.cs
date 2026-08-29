using Game.Presentation;
using UnityEngine;

/// <summary>
/// The arrow that walks back to the previous room. See RoomNavigation for why this goes through
/// ClickRouter instead of OnMouseDown.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomNavigationPrevious : MonoBehaviour, IInteractable
{
    public RoomController roomController;

    public void Interact()
    {
        roomController.GoToPreviousRoom();
    }
}
