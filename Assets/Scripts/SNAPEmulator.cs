using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using System.Text;
using UnityEngine.Events;

namespace Assets.BeMoBI.Scripts
{
    public class SNAPEmulator : MonoBehaviour
    {

        [Range(1026, 65535)]
        public int PortToListenOn = 7897;

        public RemoteCommandRecievedEvent OnCommandRecieved;

        private Socket client;
        TcpListener tcpListener;

        private int HEADER_SIZE = 1024;

        void Awake()
        {
            tcpListener = new TcpListener(IPAddress.Any, PortToListenOn);
        }

        // Update is called once per frame
        void Update()
        {
            tcpListener.Start();

            if (tcpListener.Pending())
            {
                //Accept the pending client connection and return a TcpClient object initialized for communication.
                TcpClient tcpClient = tcpListener.AcceptTcpClient();

                int bytesRead = 0;
                SocketError err;
                byte[] receiveData = new byte[HEADER_SIZE];
                bytesRead = tcpClient.Client.Receive(receiveData, 0, HEADER_SIZE, SocketFlags.None, out err);

                if (bytesRead == 0)
                    return;

                var result = UTF8Encoding.UTF8.GetString(receiveData);

                if (result != null && result != String.Empty)
                    Route(result);
            }

        }
        
        private void Route(string result)
        {
            if (OnCommandRecieved != null && OnCommandRecieved.GetPersistentEventCount() > 0)
                OnCommandRecieved.Invoke(result);
        }
    }

    [Serializable]
    public class RemoteCommandRecievedEvent : UnityEvent<String> { }
}
