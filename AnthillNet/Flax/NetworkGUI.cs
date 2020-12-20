using AnthillNet.Flax;
using FlaxEngine;
using FlaxEngine.GUI;

namespace MaterialsFeaturesTour
{
    public class NetworkGUI : Script
    {
        public UICanvas Canvas;
        public FontAsset Font;
        private UIControl IP, host, connect, stop;

        public override void OnStart()
        {
            if (!SpawnGUI())
                return;
            (host.Control as Button).Clicked += OnHost;
            (connect.Control as Button).Clicked += OnConnect;
            (stop.Control as Button).Clicked += OnStop;
        }

        #region Events
        private void OnHost()
        {
            NetworkManager.Instance.hostname = (IP.Control as TextBox).Text;
            NetworkManager.Instance.hostType = AnthillNet.HostType.Server;
            NetworkManager.Instance.Run();
        }
        private void OnConnect()
        {
            NetworkManager.Instance.hostname = (IP.Control as TextBox).Text;
            NetworkManager.Instance.hostType = AnthillNet.HostType.Client;
            NetworkManager.Instance.Run();
        }
        private void OnStop()
        {
            NetworkManager.Instance.Stop();
        }
        #endregion

        private bool SpawnGUI()
        {
            if (Canvas == null)
            {
                Debug.LogError("Canvas is not set in NetworkGUI!");
                return false;
            }
            IP = new UIControl
            {
                StaticFlags = StaticFlags.FullyStatic,
                Name = "IP",
                LocalPosition = new Vector3(10f, 10f, 0f),
                Parent = Canvas,
                Control = new TextBox
                {
                    Text = "localhost",
                    IsMultiline = false,
                    Size = new Vector2(140f, 20f),
                    Font = new FontReference(Font, 12),
                    BackgroundColor = Color.ParseHex("FFFFFF19"),
                    BorderColor = Color.ParseHex("00000000")
                }
            };
            host = new UIControl
            {
                StaticFlags = StaticFlags.FullyStatic,
                Name = "Host",
                LocalPosition = new Vector3(10f, 35f, 0f),
                Parent = Canvas,
                Control = new Button
                {
                    Text = "Host",
                    Size = new Vector2(65f, 20f),
                    Font = new FontReference(Font, 12),
                    BackgroundColor = Color.ParseHex("FFFFFF19"),
                    BorderColor = Color.ParseHex("00000000")
                }
            };
            connect = new UIControl
            {
                StaticFlags = StaticFlags.FullyStatic,
                Name = "Connect",
                LocalPosition = new Vector3(85f, 35f, 0f),
                Parent = Canvas,
                Control = new Button
                {
                    Text = "Host",
                    Size = new Vector2(65f, 20f),
                    Font = new FontReference(Font, 12),
                    BackgroundColor = Color.ParseHex("FFFFFF19"),
                    BorderColor = Color.ParseHex("00000000")
                }
            };
            stop = new UIControl
            {
                StaticFlags = StaticFlags.FullyStatic,
                Name = "Stop",
                LocalPosition = new Vector3(10f, 60f,0f),
                Parent = Canvas,
                Control = new Button
                {
                    Text = "Stop",
                    Size = new Vector2(140f, 20f),
                    Font = new FontReference(Font, 12),
                    BackgroundColor = Color.ParseHex("FFFFFF19"),
                    BorderColor = Color.ParseHex("00000000")
                }
            };
            return true
        }
    }
}
