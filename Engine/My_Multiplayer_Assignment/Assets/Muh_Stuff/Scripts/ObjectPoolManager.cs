using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class ObjectPoolManager : MonoBehaviour
{
    public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();

    // Method to spawn a networked object from the pool
    [ServerRpc(RequireOwnership = false)]  // Make sure clients can request spawning even if they don't own the object
    public static GameObject SpawnObjectServerRpc(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation, ulong clientId)
    {
        // Find the pool based on the object's name
        PooledObjectInfo pool = ObjectPools.Find(n => n.LookupString == objectToSpawn.name);

        // If no pool exists for the object, create a new pool
        if (pool == null)
        {
            pool = new PooledObjectInfo() { LookupString = objectToSpawn.name };
            ObjectPools.Add(pool);
        }

        // Check if there are inactive objects in the pool
        GameObject spawnableObject = pool.InactiveObjects.FirstOrDefault();

        if (spawnableObject == null)
        {
            // If no inactive objects exist, instantiate a new one
            spawnableObject = Instantiate(objectToSpawn, spawnPosition, spawnRotation);

            // Ensure it has a NetworkObject and spawn it across the network
            NetworkObject networkObject = spawnableObject.GetComponent<NetworkObject>();
            if (networkObject != null && !networkObject.IsSpawned)
            {
                networkObject.Spawn(); // This will spawn the object across the network
            }

            // Assign ownership to the client who is requesting the spawn
            if (networkObject != null && !networkObject.IsOwner)
            {
                networkObject.ChangeOwnership(clientId); // Assign ownership to the requesting client
            }
        }
        else
        {
            // Reuse an object from the pool
            spawnableObject.transform.position = spawnPosition;
            spawnableObject.transform.rotation = spawnRotation;
            pool.InactiveObjects.Remove(spawnableObject);
            spawnableObject.SetActive(true);

            // Ensure the NetworkObject is properly spawned
            NetworkObject networkObject = spawnableObject.GetComponent<NetworkObject>();
            if (networkObject != null && !networkObject.IsSpawned)
            {
                networkObject.Spawn(); // Ensure it's properly spawned on the network
            }

            // Reassign ownership to the client who is reusing the object
            if (networkObject != null && !networkObject.IsOwner)
            {
                networkObject.ChangeOwnership(clientId); // Reassign ownership to the current client
            }
        }

        return spawnableObject;
    }

    // Method to return a networked object to the pool
    [ServerRpc]
    public static void ReturnToPool(GameObject obj)
    {
        string goName = obj.name;
        PooledObjectInfo pool = ObjectPools.Find(n => n.LookupString == goName);

        if (pool == null)
        {
            Debug.LogWarning("Trying to pool an unpooled item.");
        }
        else
        {
            obj.SetActive(false); // Deactivate the object
            NetworkObject networkObject = obj.GetComponent<NetworkObject>();

            if (networkObject != null && networkObject.IsSpawned)
            {
                networkObject.Despawn(); // Ensure the object is despawned on the network
            }

            pool.InactiveObjects.Add(obj); // Return to the pool
        }
    }

    private static bool IsOwner(NetworkObject networkObject)
    {
        return networkObject != null && networkObject.IsOwner;
    }
}

public class PooledObjectInfo
{
    public string LookupString;  // Name of the object used for lookup
    public List<GameObject> InactiveObjects = new List<GameObject>();  // Pool of inactive objects
}
