using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Presentation
{
    /// <summary>
    /// The panel that ends a run, however it ends: the three ways to lose and the one way to win.
    ///
    /// It listens to nothing. `NightSequenceView` decides *when* an ending appears — after the
    /// screen has faded and the night has had its say — and this only draws it. That split exists
    /// because the loss channel fires the instant the night resolves, and a panel that reacted to
    /// it directly appeared before the screen had even gone black.
    /// </summary>
    public class GameEndView : MonoBehaviour
    {
        [FormerlySerializedAs("panel")]
        [SerializeField] private GameObject _panel;

        [FormerlySerializedAs("resultText")]
        [SerializeField] private TMP_Text _resultText;

        [Header("Endings")]
        [Tooltip("Placeholder until a human writes the real line — the jam forbids AI-written player-facing text.")]
        [SerializeField] private string _winMessage = "[WIN_TEXT]";

        [Tooltip("The day ran out with the player still in the open. Written by Janhavi.")]
        [SerializeField] private string _dayClockExpiredMessage = "You ran out of time.";

        [Tooltip("The player hid in a room whose clues had reached the Uncle. Written by Janhavi.")]
        [SerializeField] private string _hidInLeakedRoomMessage = "This room was leaked!";

        [Tooltip("The third wrong accusation. Written by Janhavi.")]
        [SerializeField] private string _policeTrustLostMessage = "You lost the police's trust.";

        [Tooltip("Shown if a new LossReason is ever added and nobody writes a line for it. Written by Janhavi.")]
        [SerializeField] private string _unknownLossMessage = "You didn't survive the night.";

        private void Awake()
        {
            Wiring.Require(this, _panel, nameof(_panel));
            Wiring.Require(this, _resultText, nameof(_resultText));
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        public void ShowWin()
        {
            Show(_winMessage);
        }

        public void ShowLoss(LossReason lossReason)
        {
            Show(MessageFor(lossReason));
        }

        /// <summary>
        /// The lines live on the component so they can be edited in the inspector without a
        /// recompile. Their defaults are the ones Janhavi wrote — a fresh GameEndView, and every
        /// one already in a scene, starts with her text rather than with an empty box.
        /// </summary>
        private string MessageFor(LossReason lossReason)
        {
            switch (lossReason)
            {
                case LossReason.DayClockExpired:
                    return _dayClockExpiredMessage;

                case LossReason.HidInLeakedRoom:
                    return _hidInLeakedRoomMessage;

                case LossReason.PoliceTrustLost:
                    return _policeTrustLostMessage;

                default:
                    return _unknownLossMessage;
            }
        }

        private void Show(string message)
        {
            if (_panel == null || _resultText == null)
            {
                Debug.LogError("GameEndView cannot show the ending: its panel or its text is not assigned.", this);
                return;
            }
            _resultText.text = message;
            _panel.SetActive(true);
        }
    }
}
