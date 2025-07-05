using Unity.Netcode;
using UnityEngine;

public class CannonBall : NetworkBehaviour
{
    public float damage = 25f;
    public AudioClip splashSound;
    public GameObject splashEffect;
    public float waterHeight = 0f;
    public float splashThreshold = 0.1f;
    private bool hasSplashed = false;
    public float splashVolume = 1.0f;
    public GameObject shooter;

    void OnCollisionEnter(Collision collision)
    {
        if (hasSplashed) return;

        // Ignore collision with shooter
        if (collision.gameObject == shooter || collision.transform.root.gameObject == shooter)
            return;

        if (IsServer)
        {
            ShipHealth target = collision.gameObject.GetComponentInParent<ShipHealth>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
            Splash();
        }
    }

    void Update()
    {
        if (!IsServer || hasSplashed) return;

        if (transform.position.y <= waterHeight + splashThreshold)
        {
            Splash();
        }
    }

    void Splash()
    {
        hasSplashed = true;

        // Only the server spawns effects and despawns the object
        SpawnSplashEffectClientRpc(transform.position);
        NetworkObject.Despawn();
    }

    [ClientRpc]
    void SpawnSplashEffectClientRpc(Vector3 position)
    {
        Instantiate(splashEffect, position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(splashSound, position, splashVolume);
    }
}
