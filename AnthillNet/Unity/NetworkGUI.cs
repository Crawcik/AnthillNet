using UnityEngine;

namespace AnthillNet.Unity
{
    public class NetworkGUI : MonoBehaviour
    {
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 140, 100));
            GUILayout.BeginHorizontal();
            NetworkManager.Instance.hostname = GUILayout.TextField(NetworkManager.Instance.hostname);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Host"))
            {
                NetworkManager.Instance.hostType = AnthillNet.HostType.Server;
                NetworkManager.Instance.Run();
            }
            if (GUILayout.Button("Connect"))
            {
                NetworkManager.Instance.hostType = AnthillNet.HostType.Client;
                NetworkManager.Instance.Run();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Stop"))
            {
                NetworkManager.Instance.Stop();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

        }
    }
}
