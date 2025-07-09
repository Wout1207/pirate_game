using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotateSpeed = 3f;

    [Header("Mouse Follow Settings")]
    public float maxOffset = 0.5f;     // How far the camera can shift from center
    public float followSpeed = 2f;     // How quickly the camera follows the mouse

    private Transform target;
    private Vector3 mouseOffset = Vector3.zero;

    void Update()
    {
        if (target == null) return;

        // Follow target position and rotation
        Vector3 basePosition = target.position;
        Quaternion baseRotation = target.rotation;

        // Calculate mouse position relative to screen center (-1 to 1)
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 mousePos = Input.mousePosition;
        Vector2 normalized = (mousePos - screenCenter) / screenCenter;
        normalized = Vector2.ClampMagnitude(normalized, 1f);

        // Map to maxOffset, but apply in local space (camera-relative)
        Vector3 targetOffset =
            (transform.right * normalized.x + transform.up * normalized.y) * maxOffset;

        // Smooth offset movement
        mouseOffset = Vector3.Lerp(mouseOffset, targetOffset, Time.deltaTime * followSpeed);

        // Apply offset to position
        transform.position = Vector3.Lerp(transform.position, basePosition + mouseOffset, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, baseRotation, Time.deltaTime * rotateSpeed);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
