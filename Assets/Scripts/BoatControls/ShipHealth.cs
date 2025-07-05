using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ShipHealth : NetworkBehaviour
{
    public float maxHealth = 100f;

    // Synced health across the network
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public bool isEnemy = false;

    [Header("Voor speler")]
    public Slider playerHealthSlider;

    void Start()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        UpdateUI();
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public void TakeDamage(float amount)
    {
        if (!IsServer) return;  // Only the server can apply damage

        currentHealth.Value -= amount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        if (isEnemy)
        {
            Debug.Log($"{gameObject.name} (enemy) heeft nog {currentHealth.Value} / {maxHealth} HP.");
        }

        if (currentHealth.Value <= 0)
            Die();
    }

    void OnHealthChanged(float oldValue, float newValue)
    {
        UpdateUI();

        if (newValue <= 0 && IsServer)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        if (playerHealthSlider != null)
            playerHealthSlider.value = currentHealth.Value;
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} is gezonken!");

        if (IsServer)
        {
            if (TryGetComponent<NetworkObject>(out var netObj))
            {
                netObj.Despawn();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
