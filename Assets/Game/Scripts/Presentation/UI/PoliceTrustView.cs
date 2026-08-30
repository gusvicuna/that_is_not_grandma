using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    /// <summary>
    /// The police's patience, drawn as a row of eyes — one open eye per remaining trust. A wrong
    /// accusation closes the last one still open; when the last eye closes, the run is over.
    ///
    /// The row sizes itself from the trust the run actually starts with, so authoring three eyes
    /// and leaving <see cref="PoliceCallController"/> at two hides the spare instead of lying about
    /// how many chances are left. Read-only — it never calls back into the controller.
    ///
    /// This component must sit on an object that stays active: it hides the row itself, and an
    /// object that switches itself off stops hearing the event that would switch it back on.
    /// </summary>
    public class PoliceTrustView : MonoBehaviour
    {
        [SerializeField] private PoliceCallController _policeCallController;

        [Tooltip("One Image per eye, in the order they close — the last element closes first, so author them left to right and they burn out right to left.")]
        [SerializeField] private Image[] _eyes;

        [Header("Eye states")]
        [Tooltip("Optional. Assign both sprites to swap the drawing; leave them empty and only the colours below separate open from closed.")]
        [SerializeField] private Sprite _openSprite;
        [SerializeField] private Sprite _closedSprite;
        [SerializeField] private Color _openColor = Color.white;
        [Tooltip("Placeholder — replace with the palette's colour. A dimmed tint reads well enough on its own while there is no closed-eye sprite.")]
        [SerializeField] private Color _closedColor = new Color(1f, 1f, 1f, 0.3f);

        [Header("Closing")]
        [Tooltip("Seconds for one eye to close: it squashes shut over the first half, changes to the closed state while flat, and opens back to that drawing over the second half. 0 skips the animation.")]
        [SerializeField] private float _closeDuration = 0.35f;

        [Header("Visibility")]
        [Tooltip("Optional. A CanvasGroup covering the row, faded instead of deactivating the eyes — use it if the row has a frame or a label that should hide with them.")]
        [SerializeField] private CanvasGroup _group;
        [Tooltip("Keeps the row hidden until the phone can actually be called (day 2 by default). The eyes mean nothing before the police are an option, and showing them on day 1 announces a system the player has not met yet.")]
        [SerializeField] private bool _hideUntilPhoneAvailable = true;

        // -1 until the controller has built its case: how many eyes this run started with.
        private int _capacity = -1;
        private int _shownTrust = -1;
        private bool _visible = true;
        private bool _visibilityApplied;
        private Vector3[] _authoredScales;
        private Coroutine _closing;
        private bool _started;

        private void Awake()
        {
            Wiring.Require(this, _policeCallController, nameof(_policeCallController));

            if (_eyes == null || _eyes.Length == 0)
            {
                Debug.LogError($"{nameof(PoliceTrustView)} on '{name}': {nameof(_eyes)} is empty.", this);
                _eyes = new Image[0];
            }

            // The closing animation squashes an eye and restores it, and what it restores to has to
            // be the scale the prefab was authored with — never whatever a stopped animation left.
            _authoredScales = new Vector3[_eyes.Length];
            for (int i = 0; i < _eyes.Length; i++)
            {
                if (_eyes[i] != null)
                {
                    _authoredScales[i] = _eyes[i].transform.localScale;
                }
            }
        }

        private void OnEnable()
        {
            if (_policeCallController != null)
            {
                _policeCallController.OnAvailabilityChanged += Refresh;
            }

            // Before Start the controller may not have run its own Awake yet, and TrustRemaining
            // would read 0 — every eye closed on the first frame of the run.
            if (_started)
            {
                Refresh();
            }
        }

        private void OnDisable()
        {
            if (_policeCallController != null)
            {
                _policeCallController.OnAvailabilityChanged -= Refresh;
            }

            // A coroutine stopped mid-squash would leave an eye flat forever.
            if (_closing != null)
            {
                StopCoroutine(_closing);
                _closing = null;
                ApplyEyes(_shownTrust);
            }
        }

        private void Start()
        {
            _started = true;
            Refresh();
        }

        private void Refresh()
        {
            if (_policeCallController == null)
            {
                return;
            }

            if (_capacity < 0 && !TakeCapacity())
            {
                return;
            }

            int trust = Mathf.Clamp(_policeCallController.TrustRemaining, 0, _capacity);
            bool visible = !_hideUntilPhoneAvailable || _policeCallController.IsPhoneAvailable;

            if (trust == _shownTrust)
            {
                SetVisible(visible);
                return;
            }

            int previous = _shownTrust;
            _shownTrust = trust;

            if (_closing != null)
            {
                StopCoroutine(_closing);
                _closing = null;
            }

            if (previous <= trust || _closeDuration <= 0f || !isActiveAndEnabled)
            {
                ApplyEyes(trust);
                SetVisible(visible);
                return;
            }

            // Shown before the coroutine starts, so the eyes are on screen while they close: the
            // call that spends the last trust also makes the phone unavailable, and the row would
            // otherwise vanish on the same frame as the eye the player was meant to watch close.
            SetVisible(true);
            _closing = StartCoroutine(CloseEyes(previous, trust, visible));
        }

        /// <summary>
        /// The first non-zero reading is the starting trust — nothing can be spent before the phone
        /// is available. Returns false while the controller has not built its case yet.
        /// </summary>
        private bool TakeCapacity()
        {
            int trust = _policeCallController.TrustRemaining;
            if (trust <= 0)
            {
                return false;
            }

            if (trust > _eyes.Length)
            {
                Debug.LogWarning(
                    $"{nameof(PoliceTrustView)} on '{name}': the run starts with {trust} trust " +
                    $"but only {_eyes.Length} eyes are wired.", this);
            }

            _capacity = Mathf.Min(trust, _eyes.Length);
            ApplyEyes(_capacity);
            return true;
        }

        private IEnumerator CloseEyes(int from, int to, bool visibleAfter)
        {
            for (int i = from - 1; i >= to; i--)
            {
                yield return CloseEye(i);
            }

            _closing = null;
            SetVisible(visibleAfter);
        }

        private IEnumerator CloseEye(int index)
        {
            if (index < 0 || index >= _eyes.Length || _eyes[index] == null)
            {
                yield break;
            }

            Transform eye = _eyes[index].transform;
            Vector3 scale = _authoredScales[index];
            float half = _closeDuration * 0.5f;

            for (float elapsed = 0f; elapsed < half; elapsed += Time.deltaTime)
            {
                eye.localScale = new Vector3(scale.x, scale.y * (1f - elapsed / half), scale.z);
                yield return null;
            }

            eye.localScale = new Vector3(scale.x, 0f, scale.z);
            ApplyEye(index, false);

            for (float elapsed = 0f; elapsed < half; elapsed += Time.deltaTime)
            {
                eye.localScale = new Vector3(scale.x, scale.y * (elapsed / half), scale.z);
                yield return null;
            }

            eye.localScale = scale;
        }

        /// <summary>Paints every eye for the given trust, with no animation.</summary>
        private void ApplyEyes(int trust)
        {
            for (int i = 0; i < _eyes.Length; i++)
            {
                if (_eyes[i] == null)
                {
                    continue;
                }

                _eyes[i].transform.localScale = _authoredScales[i];
                ApplyEye(i, i < trust);
            }
            ApplySpares();
        }

        private void ApplyEye(int index, bool open)
        {
            Image eye = _eyes[index];
            Sprite sprite = open ? _openSprite : _closedSprite;
            if (sprite != null)
            {
                eye.sprite = sprite;
            }
            eye.color = open ? _openColor : _closedColor;
        }

        private void SetVisible(bool visible)
        {
            // The first call always paints: a CanvasGroup authored at alpha 0 has to be turned on
            // even though the field started out saying the row was visible.
            if (_visible == visible && _visibilityApplied)
            {
                return;
            }
            _visible = visible;
            _visibilityApplied = true;

            if (_group != null)
            {
                _group.alpha = visible ? 1f : 0f;
            }
            ApplySpares();
        }

        /// <summary>
        /// Switches off the eyes the run does not use — the ones past the starting trust, and every
        /// one of them while the row is hidden. A CanvasGroup, when there is one, does the hiding
        /// instead, so there the spares are the only thing left to deactivate.
        /// </summary>
        private void ApplySpares()
        {
            for (int i = 0; i < _eyes.Length; i++)
            {
                if (_eyes[i] == null)
                {
                    continue;
                }

                bool used = _capacity < 0 || i < _capacity;
                bool active = used && (_visible || _group != null);
                if (_eyes[i].gameObject.activeSelf != active)
                {
                    _eyes[i].gameObject.SetActive(active);
                }
            }
        }
    }
}
