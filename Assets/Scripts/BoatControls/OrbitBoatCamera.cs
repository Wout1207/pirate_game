using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitBoatCamera : MonoBehaviour, BoatControls.IGameplayActions
{
    public Transform target;
    public float defaultDistance = 15f;
    public float zoomedDistance = 10f;
    public float height = 0f;
    public float sensitivity = 100f;
    public float followSpeed = 5f;

    private float yaw = 0f;
    private float pitch = 20f;
    private Vector2 lookInput;
    private bool isAiming = false;

    private BoatControls controls;

    void Awake()
    {
        controls = new BoatControls();
        controls.Gameplay.SetCallbacks(this);
    }

    void OnEnable()
    {
        controls.Gameplay.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    void LateUpdate()
    {
        float lookX = lookInput.x * sensitivity * Time.deltaTime;
        float lookY = lookInput.y * sensitivity * Time.deltaTime;

        yaw += lookX;
        pitch -= lookY;
        pitch = Mathf.Clamp(pitch, -60f, 89f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        float currentDistance = isAiming ? zoomedDistance : defaultDistance;
        Vector3 offset = rotation * new Vector3(0f, height, -currentDistance);
        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
        transform.LookAt(target.position + Vector3.up * 2f);
    }

    public void OnTrust(InputAction.CallbackContext context) { }
    public void OnBrake(InputAction.CallbackContext context) { }
    public void OnTurn(InputAction.CallbackContext context) { }

    public void OnAim(InputAction.CallbackContext context)
    {
        SetAiming(context.ReadValueAsButton());
    }

    public void OnFire(InputAction.CallbackContext context) { }
}
