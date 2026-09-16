using UnityEngine;
using Game.Data;
using Game.Domain;
using Game.Events;

namespace Game.Presentation
{
    /// <summary>
    /// Two looping sources crossfaded by the domain Crossfader, which is what stops the classic
    /// "pace between two rooms faster than the fade and now both ambiences play forever" bug.
    /// </summary>
    public class AmbienceController : MonoBehaviour
    {
        private const string _nightTrackId = "night";

        [SerializeField] private AmbienceBankSO _bank;
        [SerializeField] private AudioSource _sourceA;
        [SerializeField] private AudioSource _sourceB;
        [SerializeField] private float _fadeSeconds = 2f;

        [Header("Channels")]
        [SerializeField] private RoomIdEventChannelSO _roomChangedChannel;
        [SerializeField] private VoidEventChannelSO _nightStartedChannel;
        [SerializeField] private IntEventChannelSO _dayStartedChannel;
        [SerializeField] private VoidEventChannelSO _audioUnlockedChannel;

        private Crossfader _crossfader;
        private AudioSource _incomingSource;
        private AudioSource _outgoingSource;
        private float _incomingTargetVolume;
        private float _outgoingTargetVolume;
        private bool _isUnlocked;
        private bool _hasEnteredARoom;
        private RoomId _currentRoom;

        private void Awake()
        {
            _crossfader = new Crossfader(_fadeSeconds);
            _incomingSource = _sourceA;
            _outgoingSource = _sourceB;

            ConfigureSource(_sourceA);
            ConfigureSource(_sourceB);
        }

        private void OnEnable()
        {
            _roomChangedChannel.Raised += OnRoomChanged;
            _nightStartedChannel.Raised += OnNightStarted;
            _dayStartedChannel.Raised += OnDayStarted;
            _audioUnlockedChannel.Raised += OnAudioUnlocked;
        }

        private void OnDisable()
        {
            _roomChangedChannel.Raised -= OnRoomChanged;
            _nightStartedChannel.Raised -= OnNightStarted;
            _dayStartedChannel.Raised -= OnDayStarted;
            _audioUnlockedChannel.Raised -= OnAudioUnlocked;
        }

        private void Update()
        {
            _crossfader.Tick(Time.deltaTime);

            _incomingSource.volume = _crossfader.IncomingWeight * _incomingTargetVolume;
            _outgoingSource.volume = _crossfader.OutgoingWeight * _outgoingTargetVolume;

            if (!_crossfader.IsFading && _outgoingSource.isPlaying)
            {
                _outgoingSource.Stop();
                _outgoingSource.clip = null;
            }
        }

        private void OnRoomChanged(RoomId room)
        {
            _currentRoom = room;
            _hasEnteredARoom = true;
            PlayRoomAmbience(room);
        }

        private void OnNightStarted()
        {
            PlayTrack(_nightTrackId, _bank.NightAmbience, _bank.NightVolume);
        }

        private void OnDayStarted(int dayNumber)
        {
            if (_hasEnteredARoom)
            {
                PlayRoomAmbience(_currentRoom);
            }
        }

        private void OnAudioUnlocked()
        {
            _isUnlocked = true;
            if (_incomingSource.clip != null && !_incomingSource.isPlaying)
            {
                _incomingSource.Play();
            }
        }

        private void PlayRoomAmbience(RoomId room)
        {
            // An unmapped room is legal and means silence, not "keep the last room playing".
            if (_bank.TryGet(room, out AudioClip clip, out float volume))
            {
                PlayTrack(room.ToString(), clip, volume);
            }
            else
            {
                PlayTrack(null, null, 0f);
            }
        }

        private void PlayTrack(string trackId, AudioClip clip, float volume)
        {
            if (_crossfader.IncomingTrackId == trackId)
            {
                return;
            }

            _crossfader.To(trackId);
            SwapSources();

            _incomingSource.Stop();
            _incomingSource.clip = clip;
            _incomingSource.volume = 0f;
            _incomingTargetVolume = volume;

            if (clip != null && _isUnlocked)
            {
                _incomingSource.Play();
            }
        }

        /// <summary>The source that was fading in becomes the one fading out; the free one takes the new clip.</summary>
        private void SwapSources()
        {
            _outgoingSource = _incomingSource;
            _outgoingTargetVolume = _incomingTargetVolume;
            _incomingSource = _outgoingSource == _sourceA ? _sourceB : _sourceA;
        }

        private void ConfigureSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
        }
    }
}
