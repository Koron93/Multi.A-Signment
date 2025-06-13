using UnityEngine;
using Unity.Netcode;
using System.Runtime.CompilerServices;

public class RoomBehaviour : NetworkBehaviour
{
    public GameObject[] walls; // 0->up, 1->down, 2->right, 3->left
    public GameObject[] doors;

    public void Start()
    {
        if (IsClient)
        {
            ClientGettingRoomstatusServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
    // ServerRpc to change room status (called by clients)
    [ServerRpc(RequireOwnership = false)]
    public void UpdateRoomServerRPC(bool[] status)
    {
        // This will call the ClientRpc to update all clients with the new room status
        UpdateRoomClientRpc(status);
    }

    // ClientRpc to update room status on all clients
    [ClientRpc]
    public void UpdateRoomClientRpc(bool[] status, ClientRpcParams clientRpcParams = default)
    {
        for (int i = 0; i < status.Length; i++)
        {
            doors[i].SetActive(status[i]);  // Set door active based on status
            walls[i].SetActive(!status[i]); // Set wall inactive when the door is open
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientGettingRoomstatusServerRpc(ulong clientid)
    {
        bool[] currentStatus = GetCurrentRoomStatus(); // Fetch the current room status.

        UpdateRoomClientRpc(currentStatus, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientid } } });
    }


    // Method to get current room status (this can be your logic for fetching the state)
    private bool[] GetCurrentRoomStatus()
    {
        bool[] status = new bool[doors.Length];
        for (int i = 0; i < doors.Length; i++)
        {
            status[i] = doors[i].activeSelf; // Example, use your actual logic
        }
        return status;
    }
}