using AnthillNet.Events;

using FlaxEngine;

namespace AnthillNet.Flax
{
    public class NetworkBehaviour : Script
    {
        public static Order Order => NetworkManager.Instance.Order;
        public static bool IsServer => NetworkManager.Instance.HostType == HostType.Server;
        public void SetupTick() => NetworkManager.Instance.Transport.OnTick += (sender) => OnTick();
        protected virtual void OnTick() { }
    }
}
