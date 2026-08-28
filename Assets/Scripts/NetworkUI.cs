using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    private void OnGUI()
    {
        // Prevent NullReferenceException if NetworkManager is missing or not initialized
        if (NetworkManager.Singleton == null) return;

        // Create container layout for buttons
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        // Only show Host/Client buttons if not already connected
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Start Host"))
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUILayout.Button("Start Client"))
            {
                NetworkManager.Singleton.StartClient();
            }
        }

        GUILayout.EndArea();
    }
}