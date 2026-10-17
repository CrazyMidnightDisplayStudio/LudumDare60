using System;
using Turbo.Pause;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Turbo.DragNDrop
{
    public sealed class DragNDropManagerIPointer : MonoBehaviour
    {
        public static DragNDropManagerIPointer Instance { get; private set; }
        public static bool IsDragging => Instance != null && Instance._currentDraggable != null;
        public static Draggable CurrentDraggable => Instance != null ? Instance._currentDraggable : null;

        [SerializeField] private Camera worldCamera;

        private Draggable _currentDraggable;
        private Vector3 _dragOffset;
        private Camera _camera;

        public event Action<bool> DragStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _camera = worldCamera != null ? worldCamera : Camera.main;
        }

        private void OnEnable()
        {
            if (PauseManager.Instance != null)
                PauseManager.Instance.PauseStateChangedAction += OnPauseStateChanged;
        }

        private void OnDisable()
        {
            if (PauseManager.Instance != null)
                PauseManager.Instance.PauseStateChangedAction -= OnPauseStateChanged;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (_currentDraggable == null || PauseManager.IsPaused)
                return;

            Vector3 pointerWorldPosition = GetPointerWorldPosition(_currentDraggable.transform.position.z);
            _currentDraggable.SetDragPosition(pointerWorldPosition + _dragOffset);
        }

        public bool TryBeginDrag(Draggable draggable, Vector2 screenPosition)
        {
            if (PauseManager.IsPaused)
                return false;

            if (_currentDraggable != null || draggable == null)
                return false;

            Vector3 pointerWorldPosition = GetPointerWorldPosition(draggable.transform.position.z, screenPosition);

            _currentDraggable = draggable;
            _dragOffset = draggable.transform.position - pointerWorldPosition;

            _currentDraggable.BeginDrag();
            DragStateChanged?.Invoke(true);

            return true;
        }

        public void EndDrag(Vector2 screenPosition)
        {
            if (_currentDraggable == null)
                return;

            if (PauseManager.IsPaused)
            {
                CancelDrag();
                return;
            }

            DropTarget target = ResolveTarget(_currentDraggable, screenPosition);

            if (target != null)
            {
                _currentDraggable.EndDragAccepted(target);
                target.Accept(_currentDraggable);
            }
            else
            {
                _currentDraggable.EndDragRejected();
            }

            _currentDraggable = null;
            _dragOffset = Vector3.zero;
            DragStateChanged?.Invoke(false);
        }

        public void CancelDrag()
        {
            if (_currentDraggable == null)
                return;

            _currentDraggable.EndDragRejected();
            _currentDraggable = null;
            _dragOffset = Vector3.zero;
            DragStateChanged?.Invoke(false);
        }

        private void OnPauseStateChanged(bool isPaused)
        {
            if (isPaused)
                CancelDrag();
        }

        private DropTarget ResolveTarget(Draggable draggable, Vector2 screenPosition)
        {
            Vector3 pointerWorldPosition = GetPointerWorldPosition(draggable.transform.position.z, screenPosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(pointerWorldPosition);

            DropTarget bestTarget = null;
            float bestDistanceSqr = float.MaxValue;
            int bestPriority = int.MinValue;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];

                if (hit == null || hit == draggable.Collider)
                    continue;

                if (!hit.TryGetComponent(out DropTarget target))
                    continue;

                if (!target.CanAccept(draggable))
                    continue;

                int priority = target.Priority;
                float distanceSqr = ((Vector2)hit.bounds.center - (Vector2)pointerWorldPosition).sqrMagnitude;

                if (bestTarget == null || priority > bestPriority || priority == bestPriority && distanceSqr < bestDistanceSqr)
                {
                    bestTarget = target;
                    bestPriority = priority;
                    bestDistanceSqr = distanceSqr;
                }
            }

            return bestTarget;
        }

        private Vector3 GetPointerWorldPosition(float targetZ)
        {
            Vector2 screenPosition = Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
            return GetPointerWorldPosition(targetZ, screenPosition);
        }

        private Vector3 GetPointerWorldPosition(float targetZ, Vector2 screenPosition)
        {
            if (_camera == null)
                return Vector3.zero;

            float distance = Mathf.Abs(targetZ - _camera.transform.position.z);
            Vector3 screenPoint = new(screenPosition.x, screenPosition.y, distance);
            Vector3 worldPoint = _camera.ScreenToWorldPoint(screenPoint);
            worldPoint.z = targetZ;

            return worldPoint;
        }
    }
}
