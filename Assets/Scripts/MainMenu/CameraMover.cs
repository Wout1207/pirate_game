// CameraMover.cs
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotateSpeed = 3f;

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        if (target == null) return;

        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * rotateSpeed);
    }
}
