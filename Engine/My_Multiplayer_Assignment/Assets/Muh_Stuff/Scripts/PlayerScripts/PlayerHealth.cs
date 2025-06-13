using UnityEngine;
using Unity.Netcode;
using System;
using System.Diagnostics;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private Healthbar healthBar;  // Reference to the health bar UI

    public event Action<PlayerHealth> PlayerDeath;

    protected void OnPlayerDeath(PlayerHealth playerHealth)
    {
        PlayerDeath?.Invoke(playerHealth);
    }

    // Network variable to keep track of health across the network
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(10f);

    // Property to get and set health
    public float CurrentHealth
    {
        get => currentHealth.Value;
        set
        {
            if (IsServer) // Make sure only the server can modify health
            {
                currentHealth.Value = Mathf.Clamp(value, 0f, maxHealth);
            }
        }
    }

    // ServerRpc function to apply damage from the client
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        if (IsServer)
        {
            var targetClientId = OwnerClientId; // Send only to owner of this object
            var rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { targetClientId }
                }
            };

            UnityEngine.Debug.Log("Damage applied to player: " + damage);
            CurrentHealth -= damage;  // Apply the damage

            // Update health bar on all clients (including the server)
            TakeDamageClientRpc(CurrentHealth, rpcParams);
        
            if (CurrentHealth <= 0)
            {

                OnDeathClientRpc();
                OnPlayerDeath(this);
            }
        }
    }

    [ClientRpc]
    public void TakeDamageClientRpc(float NewHealth, ClientRpcParams rpcParams = default)
    {
        if (healthBar == null)
        {
            GameObject healthBarObject = GameObject.Find("Health");
            if (healthBarObject != null)
            {
                healthBar = healthBarObject.GetComponent<Healthbar>();
            }
        }

        if (healthBar != null)
        {
            float healthPercentage = NewHealth / maxHealth;
            UnityEngine.Debug.Log(healthPercentage);
            healthBar.UpdateHealthBar(healthPercentage);
        }
    }

    // ClientRpc to notify clients of death
    [ClientRpc]
    private void OnDeathClientRpc()
    {
        if (IsClient)
        {
            print("Haha you're dead");

            // Handle player death (e.g., stop movement, show death UI, etc.)
            // You can also trigger player death animations here


        }
    }

    private void OnClientConnected(ulong clientid)
    {
        if (IsClient)
        {
            // Find the Healthbar if it's not already assigned
            if (healthBar == null)
            {
                GameObject healthBarObject = GameObject.Find("HealthBArObject"); // Adjust as necessary
                if (healthBarObject != null)
                {
                    healthBar = healthBarObject.GetComponent<Healthbar>();
                }
            }

            // Initial health bar update (if applicable, based on the initial health value)
            if (healthBar != null)
            {
                healthBar.UpdateHealthBar(currentHealth.Value / maxHealth);
            }
        }
    }
}