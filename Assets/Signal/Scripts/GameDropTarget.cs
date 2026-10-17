using System;
using Turbo.DragNDrop;
using UnityEngine;

namespace Signal
{
    public class GameDropTarget : DropTarget
    {
        private Transform _anchorPoint;

        private PlaceableItem _item;

        public PlaceableItem Item => _item;
        public bool IsEmpty => _item == null;

        private void Awake()
        {
            _anchorPoint = SnapPoint;
        }

        public override bool CanAccept(Draggable draggable)
        {
            if (draggable is not PlaceableItem item)
            {
                return false;
            }

            if (GameManager.Instance != null && GameManager.Instance.IsRoundActive)
            {
                return false;
            }

            if (!item.IsPurchased)
            {
                return IsEmpty;
            }

            return true;
        }

        public override void Accept(Draggable draggable)
        {
            if (draggable is not PlaceableItem item)
            {
                return;
            }

            base.Accept(draggable);

            if (_anchorPoint != null)
            {
                item.SetAnchorPoint(_anchorPoint);
            }
        }

        public void PlaceItem(PlaceableItem item)
        {
            _item = item;
            item.SetCurrentTarget(this);
            Accept(item);
        }

        public void ClearItem(PlaceableItem item)
        {
            if (_item != item)
            {
                return;
            }

            _item = null;
        }
    }
}
