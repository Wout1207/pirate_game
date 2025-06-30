using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CannonAimArc : MonoBehaviour
{
    public float waterHeight = 0f;  // Stel dit in op de Y-hoogte van het wateroppervlak
    public GameObject impactMarkerPrefab; // de prefab van het cirkeltje
    private GameObject currentImpactMarker;

    public Transform cannonMuzzle;
    public float fireForce = 200f;
    public int resolution = 30;
    public float timeStep = 0.1f;
    public float maxTime = 3f;
    public LayerMask collisionLayers;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void DrawArc()
{
    Vector3[] points = new Vector3[resolution];
    Vector3 startPos = cannonMuzzle.position;
    Vector3 startVel = cannonMuzzle.forward * fireForce / cannonMuzzle.GetComponentInParent<Rigidbody>().mass;

    points[0] = startPos;
    for (int i = 1; i < resolution; i++)
    {
        float t = i * timeStep;
        Vector3 pos = startPos + startVel * t + 0.5f * Physics.gravity * t * t;

        points[i] = pos;

        if (pos.y <= waterHeight)
        {
            line.positionCount = i + 1;
            line.SetPositions(points);

            // Instantieer marker precies op wateroppervlak
            if (impactMarkerPrefab != null)
            {
                if (currentImpactMarker != null)
                    Destroy(currentImpactMarker);

                Vector3 markerPos = new Vector3(pos.x, waterHeight + 0.01f, pos.z);
                currentImpactMarker = Instantiate(impactMarkerPrefab, markerPos, Quaternion.identity);
            }

            return;
        }
    }

    // Als nooit water geraakt wordt (vb. hoge boog): volledige lijn tekenen en marker verbergen
    line.positionCount = resolution;
    line.SetPositions(points);

    if (currentImpactMarker != null)
    {
        Destroy(currentImpactMarker);
        currentImpactMarker = null;
    }
}


    public void HideArc()
    {
        line.positionCount = 0;
    }
}
