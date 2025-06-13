using UnityEngine;
using Unity.Netcode;

public class ClientPlayerScript : NetworkBehaviour
{
    [SerializeField] private PlayerMovementScript m_PlayerMovement;
    [SerializeField] private Camera m_Camera;
    [SerializeField] private CharacterController m_CharacterController;
    [SerializeField] private AudioListener m_AudioListener;

    private void Start()
    {
        // Ensure components are disabled initially for all clients.
        if (m_PlayerMovement != null) m_PlayerMovement.enabled = false;
        if (m_Camera != null) m_Camera.enabled = false;
        //if (m_CharacterController != null) m_CharacterController.enabled = false;
        if (m_AudioListener != null) m_AudioListener.enabled = false;
        OnNetworkSpawn();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Ensure the script only runs for the client.
        enabled = NetworkManager.Singleton.IsClient;

        // If the current object is not owned by this client, disable components
        if (!IsOwner)
        {
            if (m_PlayerMovement != null) m_PlayerMovement.enabled = false;
            //if (m_CharacterController != null) m_CharacterController.enabled = false;
            if (m_Camera != null) m_Camera.enabled = false;
            if (m_AudioListener != null) m_AudioListener.enabled = false;
            return;
        }

        // If the client is the owner, enable the components
        if (m_PlayerMovement != null) m_PlayerMovement.enabled = true;
        //if (m_CharacterController != null) m_CharacterController.enabled = true;
        if (m_Camera != null) m_Camera.enabled = true;
        if (m_AudioListener != null) m_AudioListener.enabled = true;
    }
}