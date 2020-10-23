using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace AnthillNet.Core
{
    public sealed class Server : Host
    {
        private Dictionary<EndPoint, Connection> Dictionary;
        public Server() => this.Logging.LogName = "Server";

        private EndPoint lastEndPoint;


        public override void Start(string hostname, ushort port)
        {
            this.Dictionary = new Dictionary<EndPoint, Connection>();
            HostSocket.Bind(new IPEndPoint(IPAddress.Parse(hostname), port));
            if (Protocol == ProtocolType.TCP)
            {
                HostSocket.Listen(100);
                this.HostSocket.BeginAccept(WaitForConnection, null);
            }
            else if(Protocol == ProtocolType.UDP)
            {
                lastEndPoint = new IPEndPoint(IPAddress.Any, port);
                NetworkStack stack = new NetworkStack() { buffer = new byte[MaxMessageSize] };
                HostSocket.BeginReceiveFrom(stack.buffer, 0, stack.buffer.Length, 0, ref lastEndPoint, WaitForMessageFrom, stack);
            }
            Logging.Log($"Starting listening on port {port} with {Enum.GetName(typeof(ProtocolType), Protocol)} protocol", LogType.Debug);
            base.OnStop += OnStopped;
            base.Start(hostname, port);
        }

        private void OnStopped(object sender)
        {
            Logging.Log($"Stopped.", LogType.Info);
            this.Dictionary.Clear();
            base.OnStop -= OnStopped;
            this.HostSocket.Dispose();
        }

        public override void Stop()
        {
            if (!this.Active) return;
            Logging.Log($"Stopping...", LogType.Debug);
            this.HostSocket.Close();
            base.Stop();
        }
        public override void ForceStop()
        {
            if (!this.Active) return;
            Logging.Log($"Force stopping...", LogType.Debug);
            this.HostSocket.Close();
            base.ForceStop();
        }

        public override void Tick()
        {
            foreach (Connection connection in this.Dictionary.Values)
                if (connection.MessagesCount > 0)
                {
                    base.IncomingMessagesInvoke(connection);
                    connection.ClearMessages();
                }
            base.Tick();
        }

        private void WaitForConnection(IAsyncResult ar)
        {
            try
            {
                Socket client = this.HostSocket.EndAccept(ar);
                Connection connection = new Connection(client);
                this.Dictionary.Add(client.RemoteEndPoint, connection);
                NetworkStack stack = new NetworkStack() { socket = client, buffer = new byte[MaxMessageSize] };
                client.BeginReceive(stack.buffer, 0, MaxMessageSize, 0, WaitForMessage, stack);
                this.Logging.Log($"Client {client.RemoteEndPoint} connected", LogType.Info);
                base.Connect(connection);
                this.HostSocket.BeginAccept(WaitForConnection, null);
            }
            catch (Exception e)
            {
                if (e is SocketException)
                    base.InternalHostErrorInvoke(e);
            }
        }

        private void WaitForMessage(IAsyncResult ar)
        {
            NetworkStack stack = (NetworkStack)ar.AsyncState;
            try
            {
                stack.socket.EndReceive(ar);
                this.Dictionary[stack.socket.RemoteEndPoint].Add(Message.Deserialize(stack.buffer));
                stack.socket.BeginReceive(stack.buffer, 0, MaxMessageSize, 0, WaitForMessage, stack);
            }
            catch (SocketException)
            {
                this.Logging.Log($"Client {stack.socket.RemoteEndPoint} disconnect from server", LogType.Info);
                this.Dictionary.Remove(stack.socket.RemoteEndPoint);
                stack.socket.Shutdown(SocketShutdown.Both);
                stack.socket.Close();
            }
            catch (ObjectDisposedException e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }

        private void WaitForMessageFrom(IAsyncResult ar)
        {
            NetworkStack stack = (NetworkStack)ar.AsyncState;
            try
            {
                HostSocket.EndReceiveFrom(ar, ref lastEndPoint);
                if (!this.Dictionary.ContainsKey(lastEndPoint))
                    this.Dictionary.Add(lastEndPoint, new Connection(lastEndPoint as IPEndPoint));
                this.Dictionary[lastEndPoint].Add(Message.Deserialize(stack.buffer));
                HostSocket.BeginReceiveFrom(stack.buffer, 0, MaxMessageSize, 0, ref lastEndPoint, WaitForMessageFrom, stack);
            }
            catch (SocketException)
            {
                HostSocket.Close();
            }
            catch(ObjectDisposedException e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }

        public override void Send(Message message, IPEndPoint IPAddress)
        {
            byte[] buf = message.Serialize();
            if (this.MaxMessageSize > buf.Length)
                SendOperation(buf, IPAddress);
            else
                base.InternalHostErrorInvoke(new Exception("Message data is too big!"));

        }

        private void SendOperation(byte[] buf, IPEndPoint IPAddress)
        {
            try
            {
                if (Protocol == ProtocolType.TCP)
                {
                    Socket socket = this.Dictionary[IPAddress].Socket;
                    socket.BeginSend(buf, 0, buf.Length, 0, (IAsyncResult ar) => socket.EndSend(ar), null);
                }
                else if (Protocol == ProtocolType.UDP)
                {
                    HostSocket.BeginSendTo(buf, 0, buf.Length, 0, IPAddress, (IAsyncResult ar) => HostSocket.EndSend(ar), null);
                }
            }
            catch (Exception e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }

        public void SendToAll(Message message)
        {
            byte[] buf = message.Serialize();
            if (this.MaxMessageSize > buf.Length)
                foreach (Connection ip in Dictionary.Values)
                    SendOperation(buf, ip.EndPoint);
            else
                InternalHostErrorInvoke(new Exception("Message data is too big!"));
        }

        public override void Dispose()
        {
            this.Logging.Log($"Disposing", LogType.Debug);
            this.ForceStop();
            base.Dispose();
        }
    }
}
