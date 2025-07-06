using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonController : NetworkBehaviour, BoatControls.IGameplayActions
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
            if (!IsOwner) return;
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
            FireCannonsServerRpc(true);  // true = left
            reloadUI.StartLeftReload();
        }
        else if (IsLookingRight() && reloadUI.IsRightReloaded())
        {
            FireCannonsServerRpc(false);  // false = right
            reloadUI.StartRightReload();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void FireCannonsServerRpc(bool isLeft)
    {
        Transform[] cannonPoints = isLeft ? leftCannons : rightCannons;
        FireCannons(cannonPoints);
    }

    void FireCannons(Transform[] cannonPoints)
    {
        foreach (var point in cannonPoints)
        {
            cannonAudioSource.PlayOneShot(cannonFireSound, 0.5f);

            var ball = Instantiate(cannonBallPrefab, point.position, point.rotation);
            var rb = ball.GetComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            ball.GetComponent<CannonBall>().shooter = this.gameObject;

            float maxAngle = 3f;
            Quaternion spreadRotation = Quaternion.Euler(
                Random.Range(-maxAngle, maxAngle),
                Random.Range(-maxAngle, maxAngle),
                0f
            );
            Vector3 spreadDirection = spreadRotation * point.forward;

            float randomizedForce = fireForce * Random.Range(0.9f, 1.1f);
            rb.AddForce(spreadDirection * randomizedForce, ForceMode.Impulse);

            ball.GetComponent<NetworkObject>().Spawn();

            Instantiate(smokeEffectPrefab, point.position, point.rotation);
            StartCoroutine(DestroyCannonballAfterTime(ball.GetComponent<NetworkObject>(), ballLifetime));
        }
    }

    IEnumerator DestroyCannonballAfterTime(NetworkObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null && obj.IsSpawned)
        {
            obj.Despawn();
        }
    }





    public void OnLook(InputAction.CallbackContext context) { }
    public void OnTrust(InputAction.CallbackContext context) { }
    public void OnBrake(InputAction.CallbackContext context) { }
    public void OnTurn(InputAction.CallbackContext context) { }
}
