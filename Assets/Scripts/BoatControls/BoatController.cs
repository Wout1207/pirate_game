using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour, BoatControls.IGameplayActions
{
    private float turnInput = 0f;
    private float currentTurn = 0f;
    private float turnVelocity = 0f;

    [Header("Beweging")]
    public float[] speedLevels = new float[] { 0f, 5f, 10f, 15f };
    private int currentSpeedLevel = 0;
    private float targetSpeed = 0f;
    private float currentSpeed = 0f;
    public float accelerationRate = 5f;
    public float directionCorrection = 2f;

    [Header("Draaien")]
    [SerializeField] private float turnTorque = 30000f;
    [SerializeField] private float turnDeadZone = 0.05f;
    [SerializeField] private float turnSmoothTime = 0.1f;

    [Header("UI")]
    public TextMeshProUGUI speedText;

    [Header("Snelheidsniveau UI")]
    public Image[] speedDots;
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.white;
    
    [Header("Anker UI")]
    public Image anchorIcon;
    public float anchorSpeedThreshold = 0.3f;

    [Header("Sound Effects")]
    public AudioSource windSource;
    public AudioClip windLoop;

    [Header("Windvolume")]
    [Range(0.1f, 10f)]
    public float windVolumeChangeSpeed = 1f;


    public AudioSource uiAudioSource;
    public AudioClip bellSound;

    private bool windIsPlaying = false;
    private Rigidbody rb;
    private BoatControls controls;

    void Awake()
    {
        controls = new BoatControls();
        controls.Gameplay.SetCallbacks(this);
    }

    void OnEnable() => controls.Gameplay.Enable();
    void OnDisable() => controls.Gameplay.Disable();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = 0.5f;
        rb.angularDamping = 2f;
    }

    void FixedUpdate()
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

        // Bouw de snelheid op richting target
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.fixedDeltaTime);
        rb.AddForce(transform.forward * currentSpeed, ForceMode.Force);

        // Richt bij (minder zijwaarts glijden)
        Vector3 desiredDir = transform.forward.normalized * rb.linearVelocity.magnitude;
        rb.linearVelocity = Vector3.Slerp(rb.linearVelocity, desiredDir, Time.fixedDeltaTime * directionCorrection);

        if (speedText != null)
            speedText.text = $"{rb.linearVelocity.magnitude:F1}";

        UpdateSpeedDots();
        HandleWindSound();
        // Toon ankericoon als de boot stilstaat
        if (anchorIcon != null)
        {
            bool isStopped = rb.linearVelocity.magnitude < anchorSpeedThreshold;
            Color iconColor = anchorIcon.color;
            float targetAlpha = rb.linearVelocity.magnitude < anchorSpeedThreshold ? 1f : 0f;
            iconColor.a = Mathf.MoveTowards(iconColor.a, targetAlpha, Time.fixedDeltaTime * 2f); // 2f = fadespeed
            anchorIcon.color = iconColor;
        }
    }

    private void HandleWindSound()
    {
        float speed = rb.linearVelocity.magnitude;
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

    private void UpdateSpeedDots()
    {
        for (int i = 0; i < speedDots.Length; i++)
        {
            speedDots[i].color = i < currentSpeedLevel ? activeColor : inactiveColor;
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

    public void OnLook(InputAction.CallbackContext context) { }
    public void OnAim(InputAction.CallbackContext context) { }
    public void OnFire(InputAction.CallbackContext context) { }
    public void OnTurn(InputAction.CallbackContext context)
    {
        turnInput = context.ReadValue<float>();
    }
}
