using UnityEngine;

namespace Turbo.DragNDrop
{
    [RequireComponent(typeof(Collider2D))]
    public class DropTarget : MonoBehaviour
    {
        [SerializeField] private int priority;
        [SerializeField] private Transform snapPoint;

        public int Priority => priority;
        public Transform SnapPoint => snapPoint;

        public virtual bool CanAccept(Draggable draggable)
        {
            return true;
        }

        public virtual void Accept(Draggable draggable)
        {
            if (draggable == null)
                return;

            Transform targetPoint = snapPoint != null ? snapPoint : transform;
            draggable.transform.position = targetPoint.position;
        }
    }
}
