using Turbo.DragNDrop;

namespace Signal
{
    public class SellTarget : DropTarget
    {
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

            return item.IsPurchased && item.CanBeSold;
        }

        public override void Accept(Draggable draggable)
        {
            if (draggable is not PlaceableItem item)
            {
                return;
            }

            if (!CanAccept(item))
            {
                item.ReturnToContainer();
                return;
            }

            if (item.CurrentTarget != null)
            {
                item.CurrentTarget.ClearItem(item);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(item.SellPrice);
            }

            Destroy(item.gameObject);
        }
    }
}
