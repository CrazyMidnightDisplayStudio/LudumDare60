using System.Collections;
using Turbo.GameTime;
using UnityEngine;

namespace Signal
{
    public class Vibrate : MonoBehaviour
    {
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private float amplitude = 0.03f;
        [SerializeField] private float retargetInterval = 0.025f;
        [SerializeField] private float smoothTime = 0.02f;

        private Transform _anchorPoint;
        private Coroutine _vibrationCoroutine;

        private void OnDisable()
        {
            Stop();
        }

        private void OnValidate()
        {
            if (duration < 0.01f)
            {
                duration = 0.01f;
            }

            if (amplitude < 0f)
            {
                amplitude = 0f;
            }

            if (retargetInterval < 0.001f)
            {
                retargetInterval = 0.001f;
            }

            if (smoothTime < 0.001f)
            {
                smoothTime = 0.001f;
            }
        }

        public void SetAnchorPoint(Transform anchorPoint)
        {
            _anchorPoint = anchorPoint;

            if (_anchorPoint != null && _vibrationCoroutine == null)
            {
                transform.position = _anchorPoint.position;
            }
        }

        public void Play()
        {
            if (_anchorPoint == null)
            {
                return;
            }

            Stop();
            transform.position = _anchorPoint.position;
            _vibrationCoroutine = StartCoroutine(VibrateRoutine());
        }

        public void Stop()
        {
            if (_vibrationCoroutine != null)
            {
                StopCoroutine(_vibrationCoroutine);
                _vibrationCoroutine = null;
            }

            if (_anchorPoint != null)
            {
                transform.position = _anchorPoint.position;
            }
        }

        private IEnumerator VibrateRoutine()
        {
            float remainingTime = duration;
            float retargetTimer = 0f;

            Vector3 currentOffset = Vector3.zero;
            Vector3 targetOffset = Vector3.zero;
            Vector3 offsetVelocity = Vector3.zero;

            while (remainingTime > 0f)
            {
                if (_anchorPoint == null)
                {
                    yield break;
                }

                GameTimeManager gameTimeManager = GameTimeManager.Instance;

                if (gameTimeManager == null)
                {
                    yield return null;
                    continue;
                }

                float gameDeltaTime = gameTimeManager.GameDeltaTime;

                if (gameDeltaTime <= 0f)
                {
                    transform.position = _anchorPoint.position;
                    yield return null;
                    continue;
                }

                remainingTime -= gameDeltaTime;
                retargetTimer -= gameDeltaTime;

                if (retargetTimer <= 0f)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * amplitude;
                    targetOffset = new Vector3(randomOffset.x, randomOffset.y, 0f);
                    retargetTimer = retargetInterval;
                }

                currentOffset = Vector3.SmoothDamp(
                    currentOffset,
                    targetOffset,
                    ref offsetVelocity,
                    smoothTime,
                    Mathf.Infinity,
                    gameDeltaTime
                );

                transform.position = _anchorPoint.position + currentOffset;

                yield return null;
            }

            targetOffset = Vector3.zero;

            while ((currentOffset - targetOffset).sqrMagnitude > 0.000001f)
            {
                if (_anchorPoint == null)
                {
                    yield break;
                }

                GameTimeManager gameTimeManager = GameTimeManager.Instance;

                if (gameTimeManager == null)
                {
                    yield return null;
                    continue;
                }

                float gameDeltaTime = gameTimeManager.GameDeltaTime;

                if (gameDeltaTime <= 0f)
                {
                    transform.position = _anchorPoint.position;
                    yield return null;
                    continue;
                }

                currentOffset = Vector3.SmoothDamp(
                    currentOffset,
                    targetOffset,
                    ref offsetVelocity,
                    smoothTime,
                    Mathf.Infinity,
                    gameDeltaTime
                );

                transform.position = _anchorPoint.position + currentOffset;

                yield return null;
            }

            transform.position = _anchorPoint.position;
            _vibrationCoroutine = null;
        }
    }
}
