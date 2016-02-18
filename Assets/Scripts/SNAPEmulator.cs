using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using System.Text;
using UnityEngine.Events;
using NLog;
namespace Assets.BeMoBI.Scripts
{
    public class SNAPEmulator : MonoBehaviour
    {
        NLog.Logger log = NLog.LogManager.GetLogger("App");

        [Range(1026, 65535)]
        public int PortToListenOn = 7897;

        public RemoteCommandRecievedEvent OnCommandRecieved;

        private Socket client;
        TcpListener tcpListener;

        private int HEADER_SIZE = 1024;

        void Awake()
        {
            log.Info(string.Format("SNAP Emulation online - listen to Port: {0}!", PortToListenOn));
            SetupTcpListener();
        }

        // Update is called once per frame
        void Update()
        {
            if(tcpListener == null)
            {
                SetupTcpListener();
            }

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

        private void SetupTcpListener()
        {
            tcpListener = new TcpListener(IPAddress.Any, PortToListenOn);
        }

        private void Route(string result)
        {
            log.Info(string.Format("SNAPEmulator recieved: {0}", result));

            if (OnCommandRecieved != null && OnCommandRecieved.GetPersistentEventCount() > 0)
                OnCommandRecieved.Invoke(result);
        }
    }

    [Serializable]
    public class RemoteCommandRecievedEvent : UnityEvent<String> { }
}
