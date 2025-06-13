using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class NetworkTagSync : NetworkBehaviour
{
    // NetworkVariable to store the tag using FixedString
    public NetworkVariable<FixedString64Bytes> networkTag = new NetworkVariable<FixedString64Bytes>();

    private void Start()
    {
        // Set the initial tag for the object if we are the owner
        if (IsOwner)
        {
            // Initialize the tag with the object's tag (converted to FixedString)
            networkTag.Value = new FixedString64Bytes(gameObject.tag);
        }
    }

    private void Update()
    {
        // Only allow the owner to update the tag value
        if (IsOwner)
        {
            // If the object's tag has changed, update the NetworkVariable
            if (gameObject.tag != networkTag.Value.ToString())
            {
                networkTag.Value = new FixedString64Bytes(gameObject.tag);
            }
        }

        // Non-owners will update their tag to match the networked value
        if (!IsOwner)
        {
            // Ensure the tag matches the networked value
            gameObject.tag = networkTag.Value.ToString();
        }
    }
}