using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleBuoyancy : MonoBehaviour
{
    [Header("Drijfinstellingen")]
    public float waterLevel = 0f;
    public float buoyancyStrength = 10f;
    public float damping = 0.5f;

    [Header("Golfbeweging (optioneel)")]
    public float waveAmplitude = 0.2f;
    public float waveFrequency = 0.5f;

    [Header("Kanteling (optioneel)")]
    public float rollAmount = 1.5f;
    public float pitchAmount = 1f;
    public float tiltSpeed = 0.3f;

    [Header("Random beweging (optioneel)")]
    public float randomForceStrength = 0.2f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        // Simuleer golven
        float waveMotion = Mathf.Sin(Time.time * waveFrequency + transform.position.x + transform.position.z) * waveAmplitude;
        float targetY = waterLevel + waveMotion;

        float depth = targetY - transform.position.y;

        if (depth > 0f)
        {
            float buoyancyForce = depth * buoyancyStrength;

            float verticalVelocity = rb.linearVelocity.y;
            float dampingForce = -verticalVelocity * damping;

            Vector3 totalForce = new Vector3(0f, buoyancyForce + dampingForce, 0f);
            rb.AddForce(totalForce, ForceMode.Acceleration);

            Debug.DrawRay(transform.position, totalForce * 0.1f, Color.cyan);
        }

        // Kleine willekeurige duwtjes om beweging te behouden
        float fx = (Mathf.PerlinNoise(Time.time, 0f) - 0.5f) * randomForceStrength;
        float fz = (Mathf.PerlinNoise(0f, Time.time) - 0.5f) * randomForceStrength;
        rb.AddForce(new Vector3(fx, 0f, fz), ForceMode.Acceleration);
    }

    void Update()
    {
        // Optionele kanteling (roll en pitch)
        float roll = Mathf.Sin(Time.time * tiltSpeed) * rollAmount;
        float pitch = Mathf.Sin(Time.time * tiltSpeed * 1.2f) * pitchAmount;

        Quaternion targetRotation = Quaternion.Euler(pitch, transform.rotation.eulerAngles.y, roll);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);
    }
}
