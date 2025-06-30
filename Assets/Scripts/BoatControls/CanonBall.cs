using UnityEngine;

public class CannonBall : MonoBehaviour
{
    public float damage = 25f;
    public AudioClip splashSound;
    public GameObject splashEffect;
    public float waterHeight = 0f;         // Pas dit aan op je waterspiegel
    public float splashThreshold = 0.1f;   // Hoe dicht de bal bij het water moet zijn
    private bool hasSplashed = false;      // voorkomt dubbele splash

    public float splashVolume = 1.0f;      // NIEUW: volume regelbaar in de inspector
    public GameObject shooter; // toe te voegen bovenaan

    void OnCollisionEnter(Collision collision)
    {
        if (hasSplashed) return;

        // Negeer botsing met afzender zelf
        if (collision.gameObject == shooter || collision.transform.root.gameObject == shooter)
            return;

        ShipHealth target = collision.gameObject.GetComponentInParent<ShipHealth>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }

        Splash();
    }

    void Update()
    {
        if (!hasSplashed && transform.position.y <= waterHeight + splashThreshold)
        {
            Splash();
        }
    }

    void Splash()
    {
        hasSplashed = true;
        Instantiate(splashEffect, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(splashSound, transform.position, splashVolume);  // Volume toegevoegd
        Destroy(gameObject);
    }
}
