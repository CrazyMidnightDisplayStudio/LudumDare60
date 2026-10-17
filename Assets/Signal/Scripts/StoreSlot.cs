using UnityEngine;

namespace Signal
{
    public class StoreSlot : MonoBehaviour
    {
        [SerializeField] private Transform point;

        private PlaceableItem _item;

        public PlaceableItem Item => _item;

        public void PlaceItem(PlaceableItem item)
        {
            _item = item;
            item.SetStoreSlot(this);

            Transform targetPoint = point != null ? point : transform;
            item.transform.position = targetPoint.position;
            item.SetAnchorPoint(targetPoint);
        }

        public void ClearItem(PlaceableItem item)
        {
            if (_item != item)
            {
                return;
            }

            _item = null;
        }

        public void SnapItem(PlaceableItem item)
        {
            Transform targetPoint = point != null ? point : transform;
            item.transform.position = targetPoint.position;
            item.SetAnchorPoint(targetPoint);
        }
    }
}
