using UnityEngine;
using UnityEngine.InputSystem;

public class CannonController : MonoBehaviour, BoatControls.IGameplayActions
{
    public AudioSource cannonAudioSource;
    public AudioClip cannonFireSound;

    public Transform[] leftCannons;
    public Transform[] rightCannons;

    public GameObject cannonBallPrefab;
    public GameObject smokeEffectPrefab;
    public GameObject splashEffectPrefab;
    public CannonAimArc leftArc;
    public CannonAimArc rightArc;


    public float fireForce = 200f;
    public float ballLifetime = 5f;

    public Transform cameraTransform;
    private bool isAiming = false;

    private BoatControls controls;
    public CannonReloadUI reloadUI;

    void Awake()
    {
        controls = new BoatControls();
        controls.Gameplay.SetCallbacks(this);
    }

    void OnEnable() => controls.Gameplay.Enable();
    void OnDisable() => controls.Gameplay.Disable();

    void Update()
    {
        if (isAiming)
        {
            bool lookingLeft = IsLookingLeft();
            bool lookingRight = IsLookingRight();

            if (lookingLeft) leftArc.DrawArc(); else leftArc.HideArc();
            if (lookingRight) rightArc.DrawArc(); else rightArc.HideArc();
        }
        else
        {
            leftArc.HideArc();
            rightArc.HideArc();
        }
    }

    private bool IsLookingLeft()
    {
        Vector3 localForward = transform.InverseTransformDirection(cameraTransform.forward);
        return localForward.x < -0.1f;
    }

    private bool IsLookingRight()
    {
        Vector3 localForward = transform.InverseTransformDirection(cameraTransform.forward);
        return localForward.x > 0.1f;
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        isAiming = context.ReadValueAsButton();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (!context.performed || !isAiming) return;

        if (IsLookingLeft() && reloadUI.IsLeftReloaded())
        {
            FireCannons(leftCannons);
            reloadUI.StartLeftReload();
        }
        else if (IsLookingRight() && reloadUI.IsRightReloaded())
        {
            FireCannons(rightCannons);
            reloadUI.StartRightReload();
        }
    }

    void FireCannons(Transform[] cannonPoints)
{
    foreach (var point in cannonPoints)
    {
        cannonAudioSource.PlayOneShot(cannonFireSound, 0.5f);

        var ball = Instantiate(cannonBallPrefab, point.position, point.rotation);
        var rb = ball.GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Geef de afzender mee aan de kogel
        ball.GetComponent<CannonBall>().shooter = this.gameObject;

        // Kleine variatie in richting (max ±3 graden)
        float maxAngle = 3f;
        Quaternion spreadRotation = Quaternion.Euler(
            Random.Range(-maxAngle, maxAngle),
            Random.Range(-maxAngle, maxAngle),
            0f
        );
        Vector3 spreadDirection = spreadRotation * point.forward;

        // Willekeurige kracht (±10%)
        float randomizedForce = fireForce * Random.Range(0.9f, 1.1f);
        rb.AddForce(spreadDirection * randomizedForce, ForceMode.Impulse);

        Instantiate(smokeEffectPrefab, point.position, point.rotation);
        Destroy(ball, ballLifetime);
    }
}


    public void OnLook(InputAction.CallbackContext context) { }
    public void OnTrust(InputAction.CallbackContext context) { }
    public void OnBrake(InputAction.CallbackContext context) { }
    public void OnTurn(InputAction.CallbackContext context) { }
}
