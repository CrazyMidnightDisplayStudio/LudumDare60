using UnityEngine;

namespace Turbo.DragNDrop
{
    [RequireComponent(typeof(Collider2D))]
    public class DraggableIPointer : MonoBehaviour
    {
        [SerializeField] private bool returnToStartPositionIfRejected = true;

        private Collider2D _collider;
        private Vector3 _startPosition;
        private int _startSortingOrder;
        private SpriteRenderer _spriteRenderer;

        public Collider2D Collider => _collider;
        public bool IsDragging => DragNDropManagerIPointer.CurrentDraggable == this;

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void BeginDrag()
        {
            _startPosition = transform.position;

            if (_spriteRenderer != null)
            {
                _startSortingOrder = _spriteRenderer.sortingOrder;
                _spriteRenderer.sortingOrder = _startSortingOrder + 1;
            }

            OnDragStarted();
        }

        public void SetDragPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
            OnDragMoved(worldPosition);
        }

        public void EndDragAccepted(DropTarget target)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = _startSortingOrder;
            }

            OnDragAccepted(target);
        }

        public void EndDragRejected()
        {
            if (returnToStartPositionIfRejected)
            {
                transform.position = _startPosition;
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = _startSortingOrder;
            }

            OnDragRejected();
        }

        protected virtual void OnDragStarted()
        {
        }

        protected virtual void OnDragMoved(Vector3 worldPosition)
        {
        }

        protected virtual void OnDragAccepted(DropTarget target)
        {
        }

        protected virtual void OnDragRejected()
        {
        }
    }
}
