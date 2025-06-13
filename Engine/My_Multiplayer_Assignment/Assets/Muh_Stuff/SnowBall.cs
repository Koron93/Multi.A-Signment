using UnityEngine;
using Unity.Netcode;

public class SnowBall : NetworkBehaviour
{
    [SerializeField] private GameObject Ball;
    [SerializeField] private Rigidbody rb;
    private float Speed = 15f;
    float timer = 2;
    private void Update()
    {
        if (!IsServer || Ball == null) return; // Ensure we only process if we're the server and the Lance is assigned
        // Move the object using Rigidbody with forward direction
        Vector3 movement = transform.forward * Speed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            // Rename object before returning it to the pool
            Ball.gameObject.name = "SnowBall";

            // Call the server-side despawn method, pass the NetworkObject ID instead of the GameObject
            //DespawnProjectileServerRpc(Ball.GetComponent<NetworkObject>().NetworkObjectId);
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
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the particle collided with a Player object
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get the NetworkObject of the collided object (the player)
            NetworkObject netObj = collision.gameObject.GetComponent<NetworkObject>();
            if (netObj == null) return; // If no NetworkObject, exit

            // Get the NetworkObject of the projectile (Lance)
            NetworkObject NetLance = Ball.GetComponent<NetworkObject>();
            if (NetLance == null) return; // If no NetworkObject on projectile, exit

            // Apply damage if the player is owned by another client, not the local client
            if (netObj.OwnerClientId != NetLance.OwnerClientId)
            {
                // Ensure we are on the server and apply the damage
                if (IsServer || IsOwner)
                {
                    // Get the PlayerHealth component from the player object
                    PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        // Call TakeDamageServerRpc to apply the damage
                        playerHealth.TakeDamageServerRpc(1f); // Apply 1 damage
                    }
                }
                else
                {
                    // If we are not the server, request the server to handle the damage
                    // This is where clients request damage to be applied via a ServerRpc
                    PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamageServerRpc(1f); // Request the server to apply damage
                    }
                }
            }
        }
    }
}
