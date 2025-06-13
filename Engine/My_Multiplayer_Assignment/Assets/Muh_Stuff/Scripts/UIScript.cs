using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class UIScript : MonoBehaviour
{
    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
    }
    public void Host()
    {
        NetworkManager.Singleton.StartHost();
    }
    
    public void Client()
    {
        NetworkManager.Singleton.StartClient();
    }
}
