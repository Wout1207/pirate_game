using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 10f, -15f);
    public float followSpeed = 5f;
    public float rotationSpeed = 3f;

    private float yaw = 0f;

    void Update()
    {
        // Muisbesturing
        if (Input.GetMouseButton(1)) // Rechtermuisknop ingedrukt
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
        }

        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
        transform.LookAt(target);
    }
}
