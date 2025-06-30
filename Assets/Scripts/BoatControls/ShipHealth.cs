using UnityEngine;
using UnityEngine.UI;

public class ShipHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isEnemy = false;

    [Header("Voor speler")]
    public Slider playerHealthSlider;  // enkel invullen op speelbaar schip

    void Start() 
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float amount)
{
    currentHealth -= amount;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

    if (isEnemy)
    {
        Debug.Log($"{gameObject.name} (enemy) heeft nog {currentHealth} / {maxHealth} HP.");
    }

    UpdateUI();

    if (currentHealth <= 0)
        Die();
}


    void UpdateUI()
    {
        if (playerHealthSlider != null)
            playerHealthSlider.value = currentHealth;
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} is gezonken!");
        // Je kunt hier een zinkanimatie starten, exploderen, of het schip vernietigen
        Destroy(gameObject);
    }
}
