using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCursor : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null || Mouse.current == null)
            return;

        Ray ray = targetCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        Plane plane = new Plane(
            Vector3.forward,
            new Vector3(0f, 0f, transform.position.z)
        );

        if (plane.Raycast(ray, out float distance))
            transform.position = ray.GetPoint(distance);
    }
}
