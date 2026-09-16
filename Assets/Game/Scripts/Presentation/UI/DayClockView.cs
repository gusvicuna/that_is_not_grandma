using Game.Domain;
using Game.Events;
using TMPro;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Shows the day clock as an hour on the house clock, and pulls the tension up once the
    /// evening arrives. The player should feel the day running out by reading a time, not a bar.
    /// </summary>
    public class DayClockView : MonoBehaviour
    {
        [SerializeField] private DayNightCycle _dayNightCycle;
        [SerializeField] private TMP_Text _clockText;

        [Tooltip("Optional second label, for the day number. Leave empty and nothing else changes.")]
        [SerializeField] private TMP_Text _dayText;
        [Tooltip("How the day number reads. {0} is the number. '[DAY]' is a placeholder — a human writes the real word, the jam forbids AI-written player-facing text.")]
        [SerializeField] private string _dayFormat = "[DAY] {0}";
        [Tooltip("The label stays hidden until this day. 2 keeps the first day from announcing that there will be others — the player should discover the loop by surviving a night, not by reading 'Day 1'.")]
        [SerializeField] private int _firstVisibleDay = 2;

        [Tooltip("The clock face stays hidden until the first day starts, so the intro plays with no clock on screen. CurrentDay is 0 until DayNightCycle announces the first morning.")]
        [SerializeField] private bool _hideUntilDayStarts = true;

        [Header("Clock face")]
        [Tooltip("The hour the day starts. 8 = 8 AM.")]
        [SerializeField] private int _startHour = 8;
        [Tooltip("The hour night falls. 20 = 8 PM.")]
        [SerializeField] private int _endHour = 20;
        [Tooltip("Minutes are rounded down to a multiple of this. A 180-second day covers 12 hours, so one minute lasts a quarter of a second — without a step the digits are a blur.")]
        [SerializeField] private int _minuteStep = 5;

        [Header("Running out of time")]
        [Tooltip("The colour the clock keeps for most of the day. Set it to whatever the TMP component was authored with.")]
        [SerializeField] private Color _normalColor = Color.white;
        [Tooltip("Placeholder — replace with the palette's colour. Note the GDD reserves one colour for Not Grandma alone.")]
        [SerializeField] private Color _warningColor = new Color(0.78f, 0.24f, 0.24f);
        [Tooltip("From this hour on the clock takes the warning colour. 18 = 6 PM. Independent of the tension hour below, so you can make the clock turn before or after the music does.")]
        [SerializeField] private int _warningHour = 18;

        [Header("Evening tension")]
        [Tooltip("The hour that pulls the tension up, once per day. 18 = 6 PM.")]
        [SerializeField] private int _tensionHour = 18;
        [SerializeField] private TensionLevel _tensionLevel = TensionLevel.Uneasy;
        [SerializeField] private TensionLevelEventChannelSO _tensionChanged;
        [Tooltip("Re-arms the evening tension every morning.")]
        [SerializeField] private IntEventChannelSO _dayStarted;

        private bool _tensionRaisedToday;
        private string _shownText;
        private bool? _warningShown;
        private int _shownDay = -1;

        private void Awake()
        {
            Wiring.Require(this, _dayNightCycle, nameof(_dayNightCycle));
            Wiring.Require(this, _clockText, nameof(_clockText));
        }

        private void OnEnable()
        {
            if (_dayNightCycle != null)
            {
                _dayNightCycle.OnClockChanged += Refresh;
            }
            if (_dayStarted != null)
            {
                _dayStarted.Raised += OnDayStarted;
            }
            Refresh();
        }

        private void OnDisable()
        {
            if (_dayNightCycle != null)
            {
                _dayNightCycle.OnClockChanged -= Refresh;
            }
            if (_dayStarted != null)
            {
                _dayStarted.Raised -= OnDayStarted;
            }
        }

        private void OnDayStarted(int day)
        {
            _tensionRaisedToday = false;
            Refresh();
        }

        private void Refresh()
        {
            if (_dayNightCycle == null || _clockText == null)
            {
                return;
            }

            RefreshDay();
            RefreshClockVisibility();

            TimeOfDay now = TimeOfDay.FromDayProgress(
                1f - _dayNightCycle.NormalizedRemaining,
                _startHour,
                _endHour,
                _minuteStep);

            // OnClockChanged fires every frame; assigning TMP text rebuilds its mesh, so only
            // write when the reading actually changed.
            string text = now.ToString();
            if (text != _shownText)
            {
                _shownText = text;
                _clockText.text = text;
            }

            // Nullable so the first refresh always paints, and so a new morning repaints back to
            // normal without a special case.
            bool warning = now.Hour24 >= _warningHour;
            if (_warningShown != warning)
            {
                _warningShown = warning;
                _clockText.color = warning ? _warningColor : _normalColor;
            }

            if (!_tensionRaisedToday && now.Hour24 >= _tensionHour && _tensionChanged != null)
            {
                _tensionRaisedToday = true;
                _tensionChanged.Raise(_tensionLevel);
            }
        }

        /// <summary>
        /// The clock face is switched off until the run actually begins. It hides the text object
        /// rather than this one: a component on a disabled object stops listening, and the clock
        /// would never come back.
        /// </summary>
        private void RefreshClockVisibility()
        {
            if (!_hideUntilDayStarts || _clockText.gameObject == gameObject)
            {
                return;
            }

            bool visible = _dayNightCycle.CurrentDay > 0;
            if (_clockText.gameObject.activeSelf != visible)
            {
                _clockText.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// Written only when the number changes — three times in a whole run. string.Format
        /// allocates, and this is called every frame.
        /// </summary>
        private void RefreshDay()
        {
            if (_dayText == null)
            {
                return;
            }

            int day = _dayNightCycle.CurrentDay;
            if (day == _shownDay)
            {
                return;
            }
            _shownDay = day;

            // Hidden on day 1 by design: a counter that starts at 1 tells the player there is a
            // day 2 before they have survived a single night. CurrentDay is also 0 until the first
            // morning is announced, which the same comparison covers.
            bool visible = day >= _firstVisibleDay;
            _dayText.text = visible ? string.Format(_dayFormat, day) : string.Empty;

            // Also hide the object, so a frame or a background drawn as its child goes with it —
            // unless the label happens to live on this very component, which would switch the
            // clock off along with it.
            if (_dayText.gameObject != gameObject)
            {
                _dayText.gameObject.SetActive(visible);
            }
        }
    }
}
