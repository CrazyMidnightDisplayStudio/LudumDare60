using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Turbo.DragNDrop
{
    public class DragNDropManager : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private LayerMask draggableMask;
        [SerializeField] private LayerMask dropTargetMask;
        
        private static DragNDropManager _instance;
        private static Draggable _currentDraggable;
        
        public static bool IsDragging => _currentDraggable != null;
        public static DragNDropManager Instance => _instance;
        public static Draggable CurrentDraggable => _currentDraggable;

        private void Awake()
        {
            _instance = this;

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (targetCamera == null)
            {
                return;
            }

            Mouse mouse = Mouse.current;

            if (mouse == null)
            {
                return;
            }

            Vector2 screenPosition = mouse.position.ReadValue();
            Vector3 worldPosition = GetWorldPosition(screenPosition);

            if (mouse.leftButton.wasPressedThisFrame)
            {
                TryBeginDrag(worldPosition);
            }

            if (_currentDraggable != null)
            {
                _currentDraggable.SetDragPosition(worldPosition);

                if (mouse.leftButton.wasReleasedThisFrame)
                {
                    EndDrag(worldPosition);
                }
            }
        }

        private void TryBeginDrag(Vector3 worldPosition)
        {
            if (_currentDraggable != null)
            {
                return;
            }

            Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition, draggableMask);

            if (hitCollider == null)
            {
                return;
            }

            if (!hitCollider.TryGetComponent(out Draggable draggable))
            {
                return;
            }

            _currentDraggable = draggable;
            _currentDraggable.BeginDrag();
        }

        public void EndDrag(Vector3 worldPosition)
        {
            if (_currentDraggable == null)
            {
                return;
            }

            DropTarget target = FindBestDropTarget(worldPosition);

            if (target != null && target.CanAccept(_currentDraggable))
            {
                Draggable draggable = _currentDraggable;
                _currentDraggable = null;
                draggable.EndDragAccepted(target);
                return;
            }

            Draggable rejectedDraggable = _currentDraggable;
            _currentDraggable = null;
            rejectedDraggable.EndDragRejected();
        }

        private DropTarget FindBestDropTarget(Vector3 worldPosition)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition, dropTargetMask);

            DropTarget bestTarget = null;
            int bestPriority = int.MinValue;

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D hitCollider = colliders[i];

                if (hitCollider == null)
                {
                    continue;
                }

                if (!hitCollider.TryGetComponent(out DropTarget target))
                {
                    continue;
                }

                if (target.Priority <= bestPriority)
                {
                    continue;
                }

                bestTarget = target;
                bestPriority = target.Priority;
            }

            return bestTarget;
        }

        private Vector3 GetWorldPosition(Vector2 screenPosition)
        {
            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;
            return worldPosition;
        }
    }
}
