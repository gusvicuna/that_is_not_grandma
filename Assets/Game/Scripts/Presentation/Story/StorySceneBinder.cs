using System.Collections.Generic;
using Game.Data;
using Game.Domain;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Turns a fired beat's effects into scene changes. The only component in this feature that
    /// touches objects; the director itself never sees a GameObject.
    /// </summary>
    public class StorySceneBinder : MonoBehaviour
    {
        [Header("Scene")]
        [Tooltip("Every story actor in the scene, including the ones inside rooms that start disabled.")]
        [SerializeField] private StoryActor[] _actors;

        [Tooltip("Every NPC the story can re-script. One entry per NPC copy, per room.")]
        [SerializeField] private NpcInteractable[] _npcs;

        [Header("Channels")]
        [SerializeField] private DialogueEventChannelSO _dialogueRequested;
        [SerializeField] private TensionLevelEventChannelSO _tensionChanged;

        [Header("Modal state (the dialogue queue waits on these)")]
        [SerializeField] private DialogueController _dialogueController;
        [SerializeField] private ExchangeController _exchangeController;
        [Tooltip("The call panel stays open after the verdict so the player can read it.")]
        [SerializeField] private PoliceCallController _policeCallController;

        private readonly Queue<DialogueSO> _pendingDialogues = new();
        private StoryDirector _director;

        /// <summary>Called by StoryDirectorBehaviour in Awake — SetFlag effects need the director.</summary>
        public void Bind(StoryDirector director)
        {
            _director = director;
        }

        private void Awake()
        {
            Wiring.Require(this, _dialogueRequested, nameof(_dialogueRequested));
            Wiring.Require(this, _tensionChanged, nameof(_tensionChanged));
        }

        public void Apply(StoryBeatSO beat)
        {
            if (beat == null || beat.Effects == null)
            {
                return;
            }
            foreach (StoryEffectData effect in beat.Effects)
            {
                Apply(effect, beat);
            }
        }

        private void Update()
        {
            if (_pendingDialogues.Count == 0 || IsBusy)
            {
                return;
            }
            _dialogueRequested.Raise(_pendingDialogues.Dequeue());
        }

        /// <summary>
        /// A clue share raises its channel while the share panel is still open, and the police panel
        /// stays open on purpose after a call. Playing a dialogue there would stack it on an open
        /// panel — and DialogueController drops a request made while another dialogue runs, without
        /// an error. So PlayDialogue queues and waits.
        /// </summary>
        private bool IsBusy
        {
            get
            {
                if (_dialogueController != null && _dialogueController.IsDialogueActive)
                {
                    return true;
                }
                if (_exchangeController != null && _exchangeController.IsExchangeActive)
                {
                    return true;
                }
                return _policeCallController != null && _policeCallController.IsCallPanelActive;
            }
        }

        private void Apply(StoryEffectData effect, StoryBeatSO beat)
        {
            switch (effect.Kind)
            {
                case StoryEffectKind.ShowActor:
                    SetActorsVisible(effect.ActorId, true);
                    break;
                case StoryEffectKind.HideActor:
                    SetActorsVisible(effect.ActorId, false);
                    break;
                case StoryEffectKind.MoveActor:
                    MoveActor(effect.ActorId, effect.Room);
                    break;
                case StoryEffectKind.SetNpcDialogue:
                    SetNpcDialogue(effect.Npc, effect.Dialogue);
                    break;
                case StoryEffectKind.PlayDialogue:
                    if (effect.Dialogue != null)
                    {
                        _pendingDialogues.Enqueue(effect.Dialogue);
                    }
                    else
                    {
                        Debug.LogWarning($"Beat '{beat.Id}' has a PlayDialogue effect with no dialogue.", beat);
                    }
                    break;
                case StoryEffectKind.SetTension:
                    _tensionChanged.Raise(effect.Tension);
                    break;
                case StoryEffectKind.SetFlag:
                    _director.SetFlag(effect.Flag);
                    break;
            }
        }

        private void SetActorsVisible(string actorId, bool visible)
        {
            bool found = false;
            foreach (StoryActor actor in _actors)
            {
                if (actor == null || actor.Id != actorId)
                {
                    continue;
                }
                actor.SetVisible(visible);
                found = true;
            }
            WarnIfMissing(found, actorId);
        }

        private void MoveActor(string actorId, RoomId room)
        {
            bool found = false;
            foreach (StoryActor actor in _actors)
            {
                if (actor == null || actor.Id != actorId)
                {
                    continue;
                }
                actor.SetVisible(actor.Room == room);
                found = actor.Room == room || found;
            }
            if (!found)
            {
                Debug.LogWarning($"No story actor '{actorId}' exists in {room} — it is now hidden everywhere.", this);
            }
        }

        private void SetNpcDialogue(NpcSO npc, DialogueSO dialogue)
        {
            if (npc == null || dialogue == null)
            {
                Debug.LogWarning("A SetNpcDialogue effect is missing its NPC or its dialogue.", this);
                return;
            }
            foreach (NpcInteractable npcInteractable in _npcs)
            {
                if (npcInteractable != null && npcInteractable.Npc == npc)
                {
                    npcInteractable.SetDialogue(dialogue);
                }
            }
        }

        private void WarnIfMissing(bool found, string actorId)
        {
            if (!found)
            {
                Debug.LogWarning($"No story actor with id '{actorId}' is wired into the binder.", this);
            }
        }
    }
}
