using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerShoot : NetworkBehaviour
{
    [SerializeField] private GameObject projectilePrefab; // Reference to the networked projectile prefab
    [SerializeField] private Transform firePoint; // The point where the projectile spawns
    void OnClientConnect()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Client connected with ClientId: " + NetworkManager.Singleton.LocalClientId);
        }
    }
    public override void OnNetworkSpawn()
    {
        // Ensure this script only runs for the owning client
        if (!IsOwner)
        {
            enabled = false; // Disable the shooting script on clients that don't own the object
        }
    }

    public void ShootUpdate()
    {
        if (IsOwner && Input.GetButtonDown("Fire1"))
        {
            // Only the owner (local client) can shoot
            ShootProjectileServerRpc(NetworkManager.Singleton.LocalClientId);
            print("got this far");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    // Function to handle the shooting logic
    void ShootProjectileServerRpc(ulong clientid)
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // Call a ServerRpc to spawn the projectile on the server
            print("Now im here");
            SpawnProjectile(firePoint.position, firePoint.rotation, clientid);
        }
    }

    void SpawnProjectile(Vector3 position, Quaternion rotation, ulong clientid)
    {
        print("Server has received the request to spawn projectile");

        // Call the object pool to spawn the projectile on the server
        GameObject projectile = ObjectPoolManager.SpawnObjectServerRpc(projectilePrefab, position, rotation, clientid);

        // Ensure the projectile has a NetworkObject component and spawn it across the network
        NetworkObject networkObject = projectile.GetComponent<NetworkObject>();
        if (networkObject != null && !networkObject.IsSpawned)
        {
            //networkObject.Spawn(); // This spawns the object across the network
        }
    }
}