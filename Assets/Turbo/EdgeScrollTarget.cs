using UnityEngine;
using UnityEngine.InputSystem;

public class EdgeScrollTarget : MonoBehaviour
{
    [Header("Камера, которая отображает сцену")]
    [SerializeField]
    private Camera targetCamera;

    [Header("Разрешённые направления")]
    [SerializeField]
    private bool horizontal = true;

    [SerializeField]
    private bool vertical = true;

    [Header("Границы центра камеры в мировых координатах")]
    [SerializeField]
    private Vector2 minPosition = new Vector2(-10f, -5f);

    [SerializeField]
    private Vector2 maxPosition = new Vector2(10f, 5f);

    [Header("Начало перетаскивания")]
    [Tooltip("Сколько секунд нужно удерживать ЛКМ перед перетаскиванием.")]
    [SerializeField, Min(0f)]
    private float dragHoldDelay = 0.2f;

    private float _mousePressedTime;
    private bool _canStartDrag;
    
    [Header("Отображение границ в Scene")]
    [SerializeField]
    private Color boundsColor = Color.green;

    private bool _isDragging;
    private Vector2 _previousMousePosition;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        transform.position = ClampPosition(transform.position);
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (!Application.isFocused ||
            mouse == null ||
            targetCamera == null)
        {
            _isDragging = false;
            _canStartDrag = false;
            return;
        }

        if (!mouse.leftButton.isPressed)
        {
            _isDragging = false;
            _canStartDrag = false;
            return;
        }

        Vector2 mousePosition = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            _canStartDrag = targetCamera.pixelRect.Contains(mousePosition);
            _isDragging = false;

            _mousePressedTime = Time.unscaledTime;
            _previousMousePosition = mousePosition;
            return;
        }

        if (!_canStartDrag)
            return;

        if (!_isDragging)
        {
            if (Time.unscaledTime - _mousePressedTime < dragHoldDelay)
                return;

            _isDragging = true;

            // Начинаем с текущей позиции мыши.
            // Движение за время ожидания не вызывает скачка камеры.
            _previousMousePosition = mousePosition;
            return;
        }

        Plane dragPlane = new Plane(
            Vector3.forward,
            new Vector3(0f, 0f, transform.position.z)
        );

        if (TryGetWorldPoint(_previousMousePosition, dragPlane, out Vector3 previousPoint) &&
            TryGetWorldPoint(mousePosition, dragPlane, out Vector3 currentPoint))
        {
            Vector3 movement = previousPoint - currentPoint;

            if (!horizontal)
                movement.x = 0f;

            if (!vertical)
                movement.y = 0f;

            movement.z = 0f;

            transform.position = ClampPosition(
                transform.position + movement
            );
        }

        _previousMousePosition = mousePosition;
    }

    private bool TryGetWorldPoint(
        Vector2 screenPosition,
        Plane plane,
        out Vector3 worldPoint)
    {
        Ray ray = targetCamera.ScreenPointToRay(screenPosition);

        if (plane.Raycast(ray, out float distance))
        {
            worldPoint = ray.GetPoint(distance);
            return true;
        }

        worldPoint = default;
        return false;
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(
            position.x, minPosition.x, maxPosition.x
        );

        position.y = Mathf.Clamp(
            position.y, minPosition.y, maxPosition.y
        );

        return position;
    }

    private void OnDisable()
    {
        _isDragging = false;
        _canStartDrag = false;
    }

    private void OnValidate()
    {
        maxPosition.x = Mathf.Max(minPosition.x, maxPosition.x);
        maxPosition.y = Mathf.Max(minPosition.y, maxPosition.y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = boundsColor;

        float z = transform.position.z;

        Vector3 bottomLeft = new Vector3(
            minPosition.x, minPosition.y, z
        );

        Vector3 bottomRight = new Vector3(
            maxPosition.x, minPosition.y, z
        );

        Vector3 topRight = new Vector3(
            maxPosition.x, maxPosition.y, z
        );

        Vector3 topLeft = new Vector3(
            minPosition.x, maxPosition.y, z
        );

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}
