using Game.Presentation;
using UnityEngine;

/// <summary>
/// The arrow that walks to the next room. Routed through ClickRouter rather than OnMouseDown:
/// OnMouseDown is a physics callback that ignores every guard the router applies, so with it the
/// player could change rooms during a conversation, with the share panel open, and — because a
/// CanvasGroup only blocks uGUI raycasts — behind the black screen of the night sequence.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomNavigation : MonoBehaviour, IInteractable
{
    public RoomController roomController;

    public void Interact()
    {
        roomController.GoToNextRoom();
    }
}
