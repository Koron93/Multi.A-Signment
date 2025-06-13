using UnityEngine;
using Unity.Netcode;

public class IceLanceScript : NetworkBehaviour
{
    [SerializeField] private ParticleSystem particleSystem; // The particle system on the lance
    [SerializeField] private GameObject Lance;  // Make sure this is assigned in the inspector or instantiated beforehand
    float timer = 2;

    private void Update()
    {
        if (!IsServer || Lance == null) return; // Ensure we only process if we're the server and the Lance is assigned

        timer -= Time.deltaTime;
        if (timer < 0)
        {
            // Rename object before returning it to the pool
            Lance.gameObject.name = "IceLance";

            // Call the server-side despawn method, pass the NetworkObject ID instead of the GameObject
            DespawnProjectileServerRpc(Lance.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    // ServerRpc now receives the NetworkObjectId
    [ServerRpc(RequireOwnership = false)]
    void DespawnProjectileServerRpc(ulong networkObjectId)
    {
        if (IsServer) // Ensure this is only executed on the server
        {
            // Find the NetworkObject using the received NetworkObjectId
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
            {
                // Return the object to the pool, assuming you have an object pooling system in place
                // Ensure pooling is implemented correctly
                ObjectPoolManager.ReturnToPool(networkObject.gameObject);
            }
            else
            {
                Debug.LogError("NetworkObject with the given ID not found.");
            }
        }
    }

    // This method will be called when a particle from a particle system collides with a collider
    private void OnParticleCollision(GameObject collision)
    {
        // Check if the particle collided with a "Player" object
        if (collision.CompareTag("Player"))
        {
            print("Hi");

            // Get the NetworkObject of the collided object (the player)
            NetworkObject netObj = collision.GetComponent<NetworkObject>();
            if (netObj == null) return; // If no NetworkObject, exit

            // Get the NetworkObject of the projectile (Lance)
            NetworkObject netLance = Lance.GetComponent<NetworkObject>();
            if (netLance == null) return; // If no NetworkObject on projectile, exit

            // Log the NetworkObjectId of both the player and lance
            print($"Player's NetworkObjectId: {netObj.NetworkObjectId}");
            print($"Lance's NetworkObjectId: {netLance.NetworkObjectId}");
            print($"Shooting Player's NetworkObjectId: {NetworkObject.NetworkObjectId}");

            // Apply damage only if the player is a different object (with a different NetworkObjectId)
            if (netObj.NetworkObjectId != netLance.NetworkObjectId)
            {
                print("Catch me if you can");

                // Only the server should handle damage application
                if (IsServer)
                {
                    print("Applying damage...");

                    // Get the PlayerHealth component from the collided player
                    PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        // Apply damage using the ServerRpc
                        playerHealth.TakeDamageServerRpc(1f); // Apply 1 damage
                    }
                }
                else
                {
                    // If we are not the server, we ask the server to handle the damage
                    PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamageServerRpc(1f); // Request server to apply damage
                    }
                }
            }
        }
    }
}