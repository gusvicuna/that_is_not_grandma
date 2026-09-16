using System.Collections;
using Game.Domain;
using Game.Events;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Presentation
{
    /// <summary>
    /// The night, as the player experiences it: the house fades to black, a line says night has
    /// come, and a few seconds later the run either continues into the next morning or ends here.
    ///
    /// It also owns the pacing of the loop. `DayNightCycle` no longer starts the next day when the
    /// night resolves — it waits for the finished channel this raises after the screen fades back
    /// in, so no daylight is spent behind a black screen.
    /// </summary>
    public class NightSequenceView : MonoBehaviour
    {
        [Header("Scene")]
        [Tooltip("Full-screen black image. Alpha 0 and blocksRaycasts off at rest.")]
        [SerializeField] private CanvasGroup _blackout;
        [SerializeField] private TMP_Text _messageText;
        [FormerlySerializedAs("_nightResultUI")]
        [Tooltip("Draws the ending. This component decides when it appears.")]
        [SerializeField] private GameEndView _gameEndView;

        [Header("Timing (seconds)")]
        [SerializeField] private float _fadeDuration = 0.75f;
        [SerializeField] private float _nightMessageSeconds = 2.5f;
        [SerializeField] private float _nextDayMessageSeconds = 2f;

        [Header("Text — placeholders until a human writes the real lines")]
        [SerializeField] private string _nightMessage = "[NIGHT_FALLS]";
        [SerializeField] private string _nextDayMessage = "[NEXT_DAY]";

        [Header("Channels")]
        [SerializeField] private VoidEventChannelSO _nightStarted;
        [SerializeField] private BoolEventChannelSO _nightResolved;
        [SerializeField] private GameLostEventChannelSO _gameLost;
        [Tooltip("The right accusation. It arrives during the day, never during a night.")]
        [SerializeField] private VoidEventChannelSO _gameWon;
        [Tooltip("Raised once the screen has faded back in. DayNightCycle waits for this to start the next day.")]
        [SerializeField] private VoidEventChannelSO _nightSequenceFinished;

        private bool? _survived;
        private LossReason _lossReason;
        private bool _won;

        /// <summary>True while the screen is taken over. ClickRouter checks this.</summary>
        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            Wiring.Require(this, _blackout, nameof(_blackout));
            Wiring.Require(this, _messageText, nameof(_messageText));
            Wiring.Require(this, _nightSequenceFinished, nameof(_nightSequenceFinished));
            Wiring.Require(this, _gameEndView, nameof(_gameEndView));
            SetBlackout(0f);
            ShowMessage(null);
        }

        private void OnEnable()
        {
            if (_nightStarted != null) _nightStarted.Raised += OnNightStarted;
            if (_nightResolved != null) _nightResolved.Raised += OnNightResolved;
            if (_gameLost != null) _gameLost.Raised += OnGameLost;
            if (_gameWon != null) _gameWon.Raised += OnGameWon;
        }

        private void OnDisable()
        {
            if (_nightStarted != null) _nightStarted.Raised -= OnNightStarted;
            if (_nightResolved != null) _nightResolved.Raised -= OnNightResolved;
            if (_gameLost != null) _gameLost.Raised -= OnGameLost;
            if (_gameWon != null) _gameWon.Raised -= OnGameWon;
        }

        /// <summary>
        /// The whole night resolves synchronously inside this event, and the order the two
        /// listeners run in is not ours to choose — so the outcome is only read later, once the
        /// first fade has finished and the chain is certainly over.
        /// </summary>
        private void OnNightStarted()
        {
            if (IsPlaying)
            {
                return;
            }
            // _survived is deliberately NOT cleared here: the checker may already have resolved
            // this night before this handler ran, and clearing would throw that answer away.
            StartCoroutine(RunNight());
        }

        private void OnNightResolved(bool survived)
        {
            _survived = survived;
        }

        private void OnGameLost(LossReason reason)
        {
            _lossReason = reason;
            _won = false;
            if (!IsPlaying)
            {
                StartCoroutine(RunEndingUnlessANightTakesOver());
            }
        }

        private void OnGameWon()
        {
            _won = true;
            if (!IsPlaying)
            {
                StartCoroutine(RunEndingUnlessANightTakesOver());
            }
        }

        private IEnumerator RunNight()
        {
            IsPlaying = true;

            yield return Fade(0f, 1f);

            ShowMessage(_nightMessage);
            yield return new WaitForSeconds(_nightMessageSeconds);

            if (!_survived.HasValue)
            {
                Debug.LogError(
                    "The night started but nothing resolved it — is NightSurvivalChecker in the scene " +
                    "and wired to the same channels? Treating it as survived so the run can continue.",
                    this);
                _survived = true;
            }

            bool survived = _survived.Value;
            _survived = null; // consumed: the next night must resolve itself, not inherit this one

            if (survived)
            {
                ShowMessage(_nextDayMessage);
                yield return new WaitForSeconds(_nextDayMessageSeconds);
                ShowMessage(null);
                yield return Fade(1f, 0f);
                IsPlaying = false;
                _nightSequenceFinished.Raise();
                yield break;
            }

            ShowMessage(null);
            ShowEnding();
        }

        /// <summary>
        /// An ending can reach this component before the night sequence has even been asked to
        /// start: the whole night resolves synchronously inside CH_NightStarted, and if
        /// NightSurvivalChecker subscribed first, its CH_GameLost arrives while IsPlaying is still
        /// false. Waiting one frame tells the two cases apart — if a night took over, it owns the
        /// ending and shows it after its own message. What gets here are the endings that happen
        /// in broad daylight: the police believing you, or running out of patience.
        /// </summary>
        private IEnumerator RunEndingUnlessANightTakesOver()
        {
            yield return null;
            if (IsPlaying)
            {
                yield break;
            }
            IsPlaying = true;
            yield return Fade(0f, 1f);
            ShowEnding();
        }

        /// <summary>The screen stays black behind the panel: the run is over, there is nothing to go back to.</summary>
        private void ShowEnding()
        {
            if (_gameEndView == null)
            {
                Debug.LogError("The run ended but no GameEndView is assigned — the player is left staring at a black screen.", this);
                return;
            }
            if (_won)
            {
                _gameEndView.ShowWin();
                return;
            }
            _gameEndView.ShowLoss(_lossReason);
        }

        private IEnumerator Fade(float from, float to)
        {
            if (_fadeDuration <= 0f)
            {
                SetBlackout(to);
                yield break;
            }

            float elapsed = 0f;
            SetBlackout(from);
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                SetBlackout(Mathf.Lerp(from, to, elapsed / _fadeDuration));
                yield return null;
            }
            SetBlackout(to);
        }

        private void SetBlackout(float alpha)
        {
            if (_blackout == null)
            {
                return;
            }
            _blackout.alpha = alpha;
            _blackout.blocksRaycasts = alpha > 0.01f;
        }

        private void ShowMessage(string message)
        {
            if (_messageText == null)
            {
                return;
            }
            _messageText.text = message ?? string.Empty;
            _messageText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }
    }
}
