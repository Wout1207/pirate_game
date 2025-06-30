using UnityEngine;
using UnityEngine.UI;

public class CannonReloadUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image leftReloadImage;
    public Image rightReloadImage;

    [Header("Instellingen")]
    public float reloadDuration = 3f;

    [Header("Sound Effects")]
    public AudioSource reloadAudioSource;
    public AudioClip reloadDoneSound;

    private float leftTimer = 0f;
    private float rightTimer = 0f;

    void Update()
    {
        if (leftTimer > 0f)
        {
            leftTimer -= Time.deltaTime;
            leftReloadImage.fillAmount = 1f - (leftTimer / reloadDuration);
            if (leftTimer <= 0f)
                reloadAudioSource.PlayOneShot(reloadDoneSound, 0.5f);
        }

        if (rightTimer > 0f)
        {
            rightTimer -= Time.deltaTime;
            rightReloadImage.fillAmount = 1f - (rightTimer / reloadDuration);
            if (rightTimer <= 0f)
                reloadAudioSource.PlayOneShot(reloadDoneSound);
        }
    }

    public void StartLeftReload()
    {
        leftTimer = reloadDuration;
        leftReloadImage.fillAmount = 0f;
    }

    public void StartRightReload()
    {
        rightTimer = reloadDuration;
        rightReloadImage.fillAmount = 0f;
    }

    public bool IsLeftReloaded() => leftTimer <= 0f;
    public bool IsRightReloaded() => rightTimer <= 0f;
}
