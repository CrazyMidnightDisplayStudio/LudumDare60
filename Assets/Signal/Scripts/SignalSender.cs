using System.Collections.Generic;
using Turbo.GameTime;
using UnityEngine;

namespace Signal
{
    public class SignalSender : MonoBehaviour
    {
        [SerializeField] private float waveSpeed = 5f;
        [SerializeField] private float maxRadius = 10f;
        [SerializeField] private float waveWidth = 0.15f;
        [SerializeField] private int circleSegments = 64;
        [SerializeField] private Color waveColor = Color.white;
        [SerializeField] private Material waveMaterial;
        [SerializeField] private LayerMask receiverMask = ~0;
        [SerializeField] private AudioClip audioClip;
        
        private static readonly HashSet<SignalSender> _allSignalSenders = new();

        private static int _nextSourceId = 1;

        private readonly List<Wave> _waves = new();
        private readonly List<Collider2D> _overlapResults = new();

        private ContactFilter2D _contactFilter;
        private AudioSource _audioSource;
        private int _sourceId;
        private PlaceableItem _placeableItem;

        public int SourceId => _sourceId;
        
        private string SourceName
        {
            get
            {
                if (_placeableItem == null)
                {
                    return string.Empty;
                }

                return _placeableItem.ItemName;
            }
        }

        private void Awake()
        {
            _placeableItem = GetComponent<PlaceableItem>();
            _audioSource = GetComponent<AudioSource>();
            _sourceId = _nextSourceId;
            _nextSourceId++;

            RebuildContactFilter();
        }

        private void OnEnable()
        {
            _allSignalSenders.Add(this);
        }

        private void OnDisable()
        {
            ClearSignals();
            _allSignalSenders.Remove(this);
        }

        private void OnDestroy()
        {
            _allSignalSenders.Remove(this);
        }

        private void Update()
        {
            GameTimeManager gameTimeManager = GameTimeManager.Instance;

            if (gameTimeManager == null)
            {
                return;
            }

            float gameDeltaTime = gameTimeManager.GameDeltaTime;

            if (gameDeltaTime <= 0f)
            {
                return;
            }

            UpdateWaves(gameDeltaTime);
        }

        private void OnValidate()
        {
            if (waveSpeed < 0.01f)
            {
                waveSpeed = 0.01f;
            }

            if (maxRadius < 0.01f)
            {
                maxRadius = 0.01f;
            }

            if (waveWidth < 0.01f)
            {
                waveWidth = 0.01f;
            }

            if (circleSegments < 3)
            {
                circleSegments = 3;
            }

            RebuildContactFilter();
        }

        public static void ClearAllSignals()
        {
            foreach (SignalSender signalSender in _allSignalSenders)
            {
                if (signalSender == null)
                {
                    continue;
                }

                signalSender.ClearSignals();
            }
        }
        
        public void SendSignal()
        {
            SendSignal(null);
        }

        public void SendSignal(ISignalWave parentWave)
        {
            _audioSource.PlayOneShot(audioClip);

            Vector2 origin = transform.position;
            LineRenderer view = CreateWaveView(origin);

            int repeaterChainCount = 0;

            if (parentWave != null)
            {
                repeaterChainCount = parentWave.RepeaterChainCount + 1;
            }

            Wave wave = new Wave(
                _sourceId,
                SourceName,
                repeaterChainCount,
                origin,
                view,
                waveSpeed,
                maxRadius,
                waveWidth,
                circleSegments,
                waveColor,
                waveMaterial,
                _contactFilter
            );

            _waves.Add(wave);
            UpdateWaveView(wave);
        }

        public void ClearSignals()
        {
            for (int i = 0; i < _waves.Count; i++)
            {
                _waves[i].DestroyView();
            }

            _waves.Clear();
            _overlapResults.Clear();
        }

        private void RebuildContactFilter()
        {
            _contactFilter = new ContactFilter2D();
            _contactFilter.useLayerMask = true;
            _contactFilter.layerMask = receiverMask;
            _contactFilter.useTriggers = true;
        }

        private LineRenderer CreateWaveView(Vector2 origin)
        {
            GameObject waveObject = new GameObject("Wave");
            waveObject.transform.position = origin;

            LineRenderer lineRenderer = waveObject.AddComponent<LineRenderer>();
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = circleSegments;
            lineRenderer.widthMultiplier = waveWidth;
            lineRenderer.startColor = waveColor;
            lineRenderer.endColor = waveColor;

            if (waveMaterial != null)
            {
                lineRenderer.material = waveMaterial;
            }

            return lineRenderer;
        }

        private void UpdateWaves(float gameDeltaTime)
        {
            for (int i = _waves.Count - 1; i >= 0; i--)
            {
                Wave wave = _waves[i];

                wave.PreviousRadius = wave.CurrentRadius;
                wave.CurrentRadius = Mathf.Min(
                    wave.CurrentRadius + wave.WaveSpeed * gameDeltaTime,
                    wave.MaxRadius
                );

                UpdateWaveView(wave);
                NotifyReceivers(wave);

                if (wave.CurrentRadius >= wave.MaxRadius)
                {
                    wave.DestroyView();
                    _waves.RemoveAt(i);
                }
            }
        }

        private void UpdateWaveView(Wave wave)
        {
            LineRenderer view = wave.View;

            if (view == null)
            {
                return;
            }

            if (view.positionCount != wave.CircleSegments)
            {
                view.positionCount = wave.CircleSegments;
            }

            view.widthMultiplier = wave.WaveWidth;
            view.startColor = wave.WaveColor;
            view.endColor = wave.WaveColor;

            if (wave.WaveMaterial != null && view.material != wave.WaveMaterial)
            {
                view.material = wave.WaveMaterial;
            }

            float angleStep = Mathf.PI * 2f / wave.CircleSegments;

            for (int i = 0; i < wave.CircleSegments; i++)
            {
                float angle = angleStep * i;
                float x = Mathf.Cos(angle) * wave.CurrentRadius;
                float y = Mathf.Sin(angle) * wave.CurrentRadius;

                view.SetPosition(i, new Vector3(x, y, 0f));
            }
        }

        private void NotifyReceivers(Wave wave)
        {
            _overlapResults.Clear();

            Physics2D.OverlapCircle(
                wave.Origin,
                wave.CurrentRadius,
                wave.ContactFilter,
                _overlapResults
            );

            for (int i = 0; i < _overlapResults.Count; i++)
            {
                Collider2D hitCollider = _overlapResults[i];

                if (hitCollider == null)
                {
                    continue;
                }

                if (!HasWaveReachedCollider(wave, hitCollider))
                {
                    continue;
                }

                if (!hitCollider.TryGetComponent(typeof(IWaveReceiver), out Component component))
                {
                    continue;
                }

                if (component.gameObject == gameObject)
                {
                    continue;
                }

                if (component is not IWaveReceiver receiver)
                {
                    continue;
                }

                if (!wave.TryMarkReceiver(receiver))
                {
                    continue;
                }

                receiver.ReceiveWave(wave);
            }
        }

        private bool HasWaveReachedCollider(Wave wave, Collider2D hitCollider)
        {
            Vector2 closestPoint = hitCollider.ClosestPoint(wave.Origin);
            float distance = Vector2.Distance(wave.Origin, closestPoint);

            if (wave.PreviousRadius <= 0f)
            {
                return distance <= wave.CurrentRadius;
            }

            return distance > wave.PreviousRadius && distance <= wave.CurrentRadius;
        }

        private sealed class Wave : ISignalWave
        {
            private readonly HashSet<IWaveReceiver> _hitReceivers = new();

            public Wave(
                int sourceId,
                string sourceName,
                int repeaterChainCount,
                Vector2 origin,
                LineRenderer view,
                float waveSpeed,
                float maxRadius,
                float waveWidth,
                int circleSegments,
                Color waveColor,
                Material waveMaterial,
                ContactFilter2D contactFilter)
            {
                SourceId = sourceId;
                SourceName = sourceName;
                RepeaterChainCount = repeaterChainCount;
                Origin = origin;
                View = view;
                WaveSpeed = waveSpeed;
                MaxRadius = maxRadius;
                WaveWidth = waveWidth;
                CircleSegments = circleSegments;
                WaveColor = waveColor;
                WaveMaterial = waveMaterial;
                ContactFilter = contactFilter;
                IsAmplified = false;
            }

            public int SourceId { get; }
            public string SourceName { get; }
            public int RepeaterChainCount { get; }
            public bool IsAmplified { get; private set; }
            public Vector2 Origin { get; }
            public LineRenderer View { get; }
            public float WaveWidth { get; }
            public int CircleSegments { get; }
            public Material WaveMaterial { get; }
            public ContactFilter2D ContactFilter { get; }

            public float PreviousRadius { get; set; }
            public float CurrentRadius { get; set; }
            public float WaveSpeed { get; private set; }
            public float MaxRadius { get; private set; }
            public Color WaveColor { get; private set; }

            public void AddMaxRadius(float value)
            {
                if (value <= 0f)
                {
                    return;
                }

                MaxRadius += value;
            }

            public void MultiplySpeed(float multiplier)
            {
                if (multiplier < 1f)
                {
                    return;
                }

                WaveSpeed *= multiplier;
            }

            public void SetColor(Color value)
            {
                WaveColor = value;
            }

            public void MarkAsAmplified()
            {
                IsAmplified = true;
            }

            public bool TryMarkReceiver(IWaveReceiver receiver)
            {
                return _hitReceivers.Add(receiver);
            }

            public void DestroyView()
            {
                if (View != null)
                {
                    Object.Destroy(View.gameObject);
                }
            }
        }
    }
}
