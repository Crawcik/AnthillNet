using FlaxEngine;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.CustomEditors;
using FlaxEngine.GUI;
using FlaxEditor;
using FlaxEditor.States;

namespace AnthillNet.Flax
{

    [CustomEditor(typeof(NetworkManager))]
    public class NetworkManagerEditor : GenericEditor
    {

        public override void Initialize(LayoutElementsContainer layout)
        {
            base.Initialize(layout);

            layout.Button(NetworkGUI.hostText, Color.DarkSlateGray).Button.Clicked += () => {
                NetworkManager.Instance.hostType = AnthillNet.HostType.Server;
                NetworkManager.Instance.Run();
            };
            layout.Button(NetworkGUI.connectText, Color.DarkSlateGray).Button.Clicked += () => {
                NetworkManager.Instance.hostType = AnthillNet.HostType.Client;
                NetworkManager.Instance.Run();
            };
            layout.Button(NetworkGUI.stopText, Color.DarkSlateGray).Button.Clicked += () => NetworkManager.Instance.Stop();
            var dispose = layout.Button(NetworkGUI.disposeTest, Color.DarkSlateGray);
            dispose.Button.Clicked += () => NetworkManager.Instance.Dispose();
            dispose.Button.TextColor = Color.IndianRed;
            Editor.Instance.StateMachine.StateChanged += OnStateChanged;
        }

        protected override void Deinitialize()
        {
            //this.RebuildLayout();
            base.Deinitialize();
        }

        private void OnStateChanged()
        {
            if (!Editor.Instance.StateMachine.IsPlayMode && NetworkManager.Instance != null)
                if (NetworkManager.Instance.Transport.Active)
                    NetworkManager.Instance.Dispose();
        }
    }
}
