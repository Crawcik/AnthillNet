using AnthillNet.Events;

using FlaxEngine;

namespace AnthillNet.Flax
{
    public class NetworkBehaviour : Script
    {
        #region Properties
        public Order Order => NetworkManager.Instance.Order;
        public bool IsServer => NetworkManager.Instance.Type == HostType.Server;
        public bool IsClient => NetworkManager.Instance.Type == HostType.Server;
        #endregion

        #region Overrides
        public override void OnEnable()
        {
            NetworkManager.Instance.OnTick += () => OnTick();
            base.OnEnable();
        }
        public override void OnDisable()
        {
            NetworkManager.Instance.OnTick -= () => OnTick();
            base.OnDisable();
        }
        #endregion

        public virtual void OnTick() { }
    }
}
