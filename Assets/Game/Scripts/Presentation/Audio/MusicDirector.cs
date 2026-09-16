using UnityEngine;
using UnityEngine.Serialization;
using Game.Domain;
using Game.Events;

namespace Game.Presentation
{
    /// <summary>
    /// Turns tension into layer volumes. Owns the two domain objects and nothing else: every rule
    /// about what "Alert" sounds like lives in TensionDirector.
    /// </summary>
    public class MusicDirector : MonoBehaviour
    {
        // Cached so Update never allocates an enum array.
        private static readonly MusicLayerId[] _layers =
        {
            MusicLayerId.Bed, MusicLayerId.Approach, MusicLayerId.Lie
        };

        [FormerlySerializedAs("_player")]
        [SerializeField] private MusicLayerPlayer _layerPlayer;

        [Header("Fades")]
        [Tooltip("Tension arrives faster than it leaves — asymmetric on purpose.")]
        [SerializeField] private float _fadeInPerSecond = 0.5f;
        [SerializeField] private float _fadeOutPerSecond = 0.2f;
        [SerializeField] private float _lieMotifSeconds = 8f;

        [Header("Channels")]
        [SerializeField] private TensionLevelEventChannelSO _tensionChangedChannel;
        [SerializeField] private VoidEventChannelSO _nightStartedChannel;
        [SerializeField] private IntEventChannelSO _dayStartedChannel;

        private TensionDirector _tensionDirector;
        private MusicLayerMixer _layerMixer;

        public TensionLevel CurrentTension => _tensionDirector.Level;

        private void Awake()
        {
            _tensionDirector = new TensionDirector(_lieMotifSeconds);
            _layerMixer = new MusicLayerMixer(_fadeInPerSecond, _fadeOutPerSecond);
        }

        private void OnEnable()
        {
            _tensionChangedChannel.Raised += SetTension;
            _nightStartedChannel.Raised += OnNightStarted;
            _dayStartedChannel.Raised += OnDayStarted;
        }

        private void OnDisable()
        {
            _tensionChangedChannel.Raised -= SetTension;
            _nightStartedChannel.Raised -= OnNightStarted;
            _dayStartedChannel.Raised -= OnDayStarted;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _tensionDirector.Tick(deltaTime);

            foreach (MusicLayerId layer in _layers)
            {
                _layerMixer.SetTarget(layer, _tensionDirector.GetTarget(layer));
            }

            _layerMixer.Tick(deltaTime);
            _layerPlayer.ApplyWeights(_layerMixer);
        }

        /// <summary>Called by AudioCueRouter for conversations marked with PlaysLieMotif.</summary>
        public void PulseLieMotif()
        {
            _tensionDirector.PulseLieMotif();
        }

        private void SetTension(TensionLevel level)
        {
            _tensionDirector.SetTension(level);
        }

        private void OnNightStarted()
        {
            SetTension(TensionLevel.Alert);
        }

        private void OnDayStarted(int dayNumber)
        {
            // Every morning starts from silence without the day clock having to announce it.
            SetTension(TensionLevel.Calm);
        }
    }
}
