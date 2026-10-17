using Turbo.DragNDrop;
using Turbo.ToolTip;
using UnityEngine;
using UnityEngine.Serialization;

namespace Signal
{
public class PlaceableItem : Draggable
{
    private const string ItemNameColor = "#FFD166";
    private const string DescriptionColor = "#FFFFFF";
    private const string PriceColor = "#7BD389";

    private ToolTipInfo _toolTipInfo;
    
    [SerializeField] private string itemName;
    [SerializeField] private string description;
    [SerializeField] private int price;
    [SerializeField] private int sellPrice;
    [SerializeField] private bool canBeSold = true;
    [SerializeField] private GameDropTarget startTarget;
    [SerializeField] private Vibrate vibrate;
    
    private GameDropTarget _currentTarget;
    private StoreSlot _storeSlot;
    private bool _isPurchased;

    public int Price => price;
    public int SellPrice => sellPrice;
    public bool CanBeSold => canBeSold;
    public bool IsPurchased => _isPurchased;
    public GameDropTarget CurrentTarget => _currentTarget;
    public StoreSlot StoreSlot => _storeSlot;
    public string ItemName => itemName;

    protected override void Awake()
    {
        base.Awake();
        
        if (vibrate == null)
        {
            vibrate = GetComponent<Vibrate>();
        }
        
        _toolTipInfo = GetComponent<ToolTipInfo>();
        UpdateToolTipText();
    }

    private void Start()
    {
        if (startTarget == null)
        {
            return;
        }

        _isPurchased = true;
        _storeSlot = null;
        _currentTarget = null;
        startTarget.PlaceItem(this);
        UpdateToolTipText();
    }
    
    private void UpdateToolTipText()
    {
        if (_toolTipInfo == null)
        {
            return;
        }

        _toolTipInfo.SetToolTipText(BuildToolTipText());
    }

    private string BuildToolTipText()
    {
        string priceLabel = _isPurchased ? "Sell price" : "Price";
        int value = _isPurchased ? sellPrice : price;

        return
            $"<color={ItemNameColor}>{itemName}</color>\n" +
            $"<color={DescriptionColor}>{description}</color>\n" +
            $"<color={PriceColor}>{priceLabel}: {value}</color>";
    }
    
    public void SetAnchorPoint(Transform anchorPoint)
    {
        if (vibrate == null)
        {
            return;
        }

        vibrate.SetAnchorPoint(anchorPoint);
    }

    public void SetStoreSlot(StoreSlot storeSlot)
    {
        _storeSlot = storeSlot;
    }

    public void SetCurrentTarget(GameDropTarget target)
    {
        _currentTarget = target;
        _storeSlot = null;
        UpdateToolTipText();
    }

    public void ReturnToContainer()
    {
        if (_currentTarget != null)
        {
            _currentTarget.Accept(this);
            return;
        }

        if (_storeSlot != null)
        {
            _storeSlot.SnapItem(this);
        }
    }

    protected override void OnDragStarted()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsRoundActive)
        {
            CancelDrag();
            return;
        }

        if (!_isPurchased && GameManager.Instance != null && GameManager.Instance.Money < price)
        {
            CancelDrag();
        }
    }

    protected override void OnDragAccepted(DropTarget target)
    {
        if (target is SellTarget sellTarget)
        {
            sellTarget.Accept(this);
            return;
        }

        if (target is not GameDropTarget gameDropTarget)
        {
            ReturnToContainer();
            return;
        }

        if (!_isPurchased)
        {
            HandlePurchaseDrop(gameDropTarget);
            return;
        }

        HandleMoveDrop(gameDropTarget);
    }

    protected override void OnDragRejected()
    {
        ReturnToContainer();
    }

    private void HandlePurchaseDrop(GameDropTarget target)
    {
        if (!target.IsEmpty)
        {
            ReturnToContainer();
            return;
        }

        if (GameManager.Instance == null)
        {
            ReturnToContainer();
            return;
        }

        if (!GameManager.Instance.TrySpendMoney(price))
        {
            ReturnToContainer();
            return;
        }

        _isPurchased = true;
        UpdateToolTipText();

        if (_storeSlot != null)
        {
            _storeSlot.ClearItem(this);
            _storeSlot = null;
        }

        target.PlaceItem(this);
    }

    private void HandleMoveDrop(GameDropTarget target)
    {
        if (_currentTarget == null)
        {
            ReturnToContainer();
            return;
        }

        if (target == _currentTarget)
        {
            _currentTarget.Accept(this);
            return;
        }

        if (target.IsEmpty)
        {
            GameDropTarget fromTarget = _currentTarget;

            fromTarget.ClearItem(this);
            target.PlaceItem(this);

            return;
        }

        PlaceableItem otherItem = target.Item;
        GameDropTarget from = _currentTarget;

        target.ClearItem(otherItem);
        from.ClearItem(this);

        target.PlaceItem(this);
        from.PlaceItem(otherItem);
    }

    private void CancelDrag()
    {
        if (DragNDropManager.Instance == null)
        {
            return;
        }

        DragNDropManager.Instance.EndDrag(Vector2.zero);
    }
}
}