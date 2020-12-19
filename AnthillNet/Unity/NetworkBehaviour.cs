using AnthillNet.Events;

using UnityEngine;

namespace AnthillNet.Unity
{
    public class NetworkBehaviour : MonoBehaviour
    {
        public static Order Order => NetworkManager.Instance.Order;
        public static bool IsServer => NetworkManager.Instance.HostType == HostType.Server;
        public void SetupTick() => NetworkManager.Instance.Transport.OnTick += (sender) => OnTick();
        protected virtual void OnTick() { }
    }
}
