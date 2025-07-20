using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using FishNet.Object;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : NetworkBehaviour, BoatControls.IGameplayActions
{
    [Header("Movement Settings")]
    public float[] speedLevels = new float[] { 0f, 3f, 6f, 9f };
    public float accelerationRate = 2f;
    public float directionCorrection = 2f;

    [Header("Turning")]
    [SerializeField] private float turnTorque = 8000f;
    [SerializeField] private float turnDeadZone = 0.05f;
    [SerializeField] private float turnSmoothTime = 0.1f;

    [Header("UI")]
    public GameObject ShipHud;
    public TextMeshProUGUI speedText;
    public Image[] speedDots;
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.white;
    public Image anchorIcon;
    public float anchorSpeedThreshold = 0.3f;

    [Header("Audio")]
    public AudioSource windSource;
    public AudioClip windLoop;
    public GameObject AudioSources;
    public AudioSource uiAudioSource;
    public AudioClip bellSound;
    [Range(0.1f, 10f)] public float windVolumeChangeSpeed = 1f;

    // Intern
    private float turnInput = 0f;
    private float currentTurn = 0f;
    private float turnVelocity = 0f;
    private int currentSpeedLevel = 0;
    private float targetSpeed = 0f;
    private float currentSpeed = 0f;
    private bool windIsPlaying = false;
    private Rigidbody rb;
    private BoatControls controls;

    void Awake()
    {
        controls = new BoatControls();
        controls.Gameplay.SetCallbacks(this);
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable() => controls.Gameplay.Enable();
    void OnDisable() => controls.Gameplay.Disable();

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            ShipHud?.SetActive(false);
            AudioSources?.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        HandleTurning();
        HandleMovement();
        HandleWindSound();
        UpdateUI();
    }

    private void HandleTurning()
    {
        float targetTurn = Mathf.Abs(turnInput) > turnDeadZone ? turnInput : 0f;

        if (Mathf.Sign(targetTurn) != Mathf.Sign(currentTurn) && targetTurn != 0f)
        {
            turnVelocity = 0f;
            currentTurn = targetTurn;
        }
        else
        {
            currentTurn = Mathf.SmoothDamp(currentTurn, targetTurn, ref turnVelocity, turnSmoothTime);
        }

        rb.AddTorque(Vector3.up * currentTurn * turnTorque * Time.deltaTime, ForceMode.Force);
    }

    private void HandleMovement()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.fixedDeltaTime);
        Vector3 targetVelocity = transform.forward * currentSpeed;
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, accelerationRate * Time.fixedDeltaTime);

        // Richt bij om zijwaarts glijden te beperken
        Vector3 desiredDir = transform.forward * rb.linearVelocity.magnitude;
        rb.linearVelocity = Vector3.Slerp(rb.linearVelocity, desiredDir, Time.fixedDeltaTime * directionCorrection);
    }

    private void HandleWindSound()
    {
        float targetVolume = currentSpeedLevel / (float)(speedLevels.Length - 1);
        windSource.volume = Mathf.MoveTowards(windSource.volume, targetVolume, Time.deltaTime * windVolumeChangeSpeed);

        if (targetVolume > 0f && !windIsPlaying)
        {
            windSource.clip = windLoop;
            windSource.loop = true;
            windSource.Play();
            windIsPlaying = true;
        }
        else if (targetVolume == 0f && windIsPlaying)
        {
            windSource.Stop();
            windIsPlaying = false;
        }
    }

    private void UpdateUI()
    {
        if (speedText)
            speedText.text = $"{rb.linearVelocity.magnitude:F1}";

        for (int i = 0; i < speedDots.Length; i++)
        {
            speedDots[i].color = i < currentSpeedLevel ? activeColor : inactiveColor;
        }

        if (anchorIcon)
        {
            float speed = rb.linearVelocity.magnitude;
            float targetAlpha = speed < anchorSpeedThreshold ? 1f : 0f;
            Color iconColor = anchorIcon.color;
            iconColor.a = Mathf.MoveTowards(iconColor.a, targetAlpha, Time.fixedDeltaTime * 2f);
            anchorIcon.color = iconColor;
        }
    }

    public void OnTrust(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            int newLevel = Mathf.Min(currentSpeedLevel + 1, speedLevels.Length - 1);
            if (newLevel != currentSpeedLevel)
            {
                currentSpeedLevel = newLevel;
                targetSpeed = speedLevels[currentSpeedLevel];
                uiAudioSource.PlayOneShot(bellSound);
            }
        }
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            int newLevel = Mathf.Max(currentSpeedLevel - 1, 0);
            if (newLevel != currentSpeedLevel)
            {
                currentSpeedLevel = newLevel;
                targetSpeed = speedLevels[currentSpeedLevel];
                uiAudioSource.PlayOneShot(bellSound);
            }
        }
    }

    public void OnTurn(InputAction.CallbackContext context)
    {
        turnInput = context.ReadValue<float>();
    }

    public void OnLook(InputAction.CallbackContext context) { }
    public void OnAim(InputAction.CallbackContext context) { }
    public void OnFire(InputAction.CallbackContext context) { }
}
