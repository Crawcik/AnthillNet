using AnthillNet.Events;

using FlaxEngine;

namespace AnthillNet.Flax
{
    public class NetworkBehaviour : Script
    {
        public Order Order => NetworkManager.Instance.Order;
        public bool IsServer => NetworkManager.Instance.Type == HostType.Server;
        public void SetupTick() => NetworkManager.Instance.Transport.OnTick += (sender) => OnTick();
        protected virtual void OnTick() { }
    }
}
