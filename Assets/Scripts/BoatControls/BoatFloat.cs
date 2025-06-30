using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatFloat : MonoBehaviour
{
    [Header("Instellingen")]
    public Transform[] floatPoints; // Plaats 3–4 float points onder je schip
    [SerializeField] private float waterLevel = 0f;
    [SerializeField] private float buoyancy = 600f;
    [SerializeField] private float maxDepth = 0.5f;
    [SerializeField] private float waterDrag = 1f;
    [SerializeField] private float angularDrag = 3f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Verlaag het zwaartepunt voor stabiliteit
        rb.centerOfMass = new Vector3(0, -1f, 0);
    }

    void FixedUpdate()
{
    rb.linearDamping = waterDrag;
    rb.angularDamping = angularDrag;

    foreach (var point in floatPoints)
    {
        if (point == null) continue;

        Vector3 wp = point.position;
        float waveHeight = waterLevel + Mathf.Sin(Time.time * 0.2f + wp.x * 0.1f + wp.z * 0.1f) * 0.1f;

        if (wp.y < waveHeight)
        {
            float displacement = waveHeight - wp.y;
            float depthPercent = Mathf.Clamp01(displacement / maxDepth);

            // demping op basis van verticale snelheid
            Vector3 velocity = rb.GetPointVelocity(wp);
            float damping = Mathf.Clamp01(1f - velocity.y * 0.1f);

            Vector3 force = Vector3.up * depthPercent * buoyancy * damping;
            rb.AddForceAtPosition(force, wp);

            Debug.DrawRay(wp, force * 0.001f, Color.cyan); // visueel
        }
    }
}


    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (floatPoints != null)
        {
            foreach (var point in floatPoints)
            {
                if (point != null)
                    Gizmos.DrawSphere(point.position, 0.1f);
            }
        }
    }
}
