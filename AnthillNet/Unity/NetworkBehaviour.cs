using AnthillNet.Events;

using UnityEngine;

namespace AnthillNet.Unity
{
    public class NetworkBehaviour : MonoBehaviour
    {
        public Order Order => NetworkManager.Instance.Order;
        public EventManager Event => NetworkManager.Instance.EventManager;
        public bool IsServer => NetworkManager.Instance.HostType == HostType.Server;

        public void SetupTick() => NetworkManager.Instance.Transport.OnTick += Transport_OnTick;
        protected virtual void OnTick() { }

        private void Transport_OnTick(object sender) => OnTick();
    }
}
