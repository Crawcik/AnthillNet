using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace AnthillNet.API
{
    public class Connection
    {
        public int MaxMessagesCount = 200;

        public Socket socket { private set; get; }
        private int maxBufferSize;
        public List<Message> Messages = new List<Message>();
        public Connection(Socket socket, int maxBufferSize = 2*1024)
        {
            this.maxBufferSize = maxBufferSize;
            this.socket = socket;
            socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, WaitForMessage, null);
        }

        private void WaitForMessage(IAsyncResult ar)
        {
            try
            {
                byte[] buffer = new byte[maxBufferSize];
                socket.EndReceive(ar);
                socket.Receive(buffer, buffer.Length, 0);
                Messages.Add(Message.Deserialize(buffer));
                socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, WaitForMessage, null);
            }
            catch
            {
 
            }
        }
        //Delegates
    }
}
