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
    public Slider playerHealthSlider;  // Only assigned on player-controlled ships

    private void Start()
    {
        currentHealth.OnValueChanged += OnHealthChanged;

        // Ensure correct UI on start
        UpdateUI();
    }

    private void OnDestroy()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    public void TakeDamage(float amount)
    {
        if (!IsServer) return;  // Only server modifies health

        currentHealth.Value = Mathf.Clamp(currentHealth.Value - amount, 0, maxHealth);

        if (isEnemy)
        {
            Debug.Log($"{gameObject.name} (enemy) heeft nog {currentHealth.Value} / {maxHealth} HP.");
        }

        // No need to call Die() here, OnHealthChanged handles it
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        UpdateUI();

        if (newValue <= 0 && IsServer)
        {
            Die();
        }
    }

    private void UpdateUI()
    {
        if (playerHealthSlider != null)
        {
            playerHealthSlider.value = currentHealth.Value;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} is gezonken!");

        if (IsServer)
        {
            Debug.Log("destroying ship");
            transform.parent.GetComponent<NetworkObject>().Despawn();
        }
    }
}
