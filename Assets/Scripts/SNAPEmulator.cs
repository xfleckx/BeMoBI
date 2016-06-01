using UnityEngine; 
using UnityEngine.Events;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System; 
using System.Text;
using System.Linq;
using NLog;
namespace Assets.BeMoBI.Scripts
{
    public class SNAPEmulator : MonoBehaviour
    {
        static readonly char[] commandSeparator = new char[] { '\n' };

        NLog.Logger appLog = NLog.LogManager.GetLogger("App");

        [Range(1026, 65535)]
        public int PortToListenOn = 7897;

        public RemoteCommandRecievedEvent OnCommandRecieved;

        private Socket client;
        TcpListener tcpListener;

        private int BufferSizeForIncomingData = 256;

        void Awake()
        {
            var appInit = FindObjectOfType<AppInit>();

            appInit.OnAppConfigLoaded += (c) =>
            {
                PortToListenOn = c.SNAP_Emulation_Port;
                Initialize();
            };

        }

        private void Initialize()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, PortToListenOn);
            }
            catch (Exception)
            {
                var message = string.Format("SNAP emulation (Remote Control) could be initialized on TCP Port: {0}", PortToListenOn);
                appLog.Fatal(message);
            }

            appLog.Info(string.Format("SNAP Emulation online - listen to Port: {0}!", PortToListenOn));
        }

        void Update()
        {
            if(tcpListener == null)
            {
                Initialize();
            }

            tcpListener.Start();

            if (tcpListener.Pending())
            {
                //Accept the pending client connection and return a TcpClient object initialized for communication.
                TcpClient tcpClient = tcpListener.AcceptTcpClient();

                int bytesRead = 0;
                SocketError err;
                byte[] receiveData = new byte[BufferSizeForIncomingData];
                bytesRead = tcpClient.Client.Receive(receiveData, 0, BufferSizeForIncomingData, SocketFlags.None, out err);

                if (bytesRead == 0)
                    return;

                // we expecting values from python (LabRecorder) which default encoding is ascii
                var incomingString = Encoding.ASCII.GetString(receiveData);
                
                var stringWithOutZeroBytes = incomingString.RemoveZeroBytes();

                if (stringWithOutZeroBytes != String.Empty)
                    RouteToSubscriber(stringWithOutZeroBytes);
            }
        } 

        private void RouteToSubscriber(string result)
        {
            appLog.Info(string.Format("SNAPEmulator recieved: {0}", result));

            var splittedStrings = result.Split(commandSeparator);

            var tempList = new List<string>(splittedStrings);
            // just to avoid unnecessary function calls for empty strings in the array after splitting
            var allNoneEmptyCommands = tempList.Where((s) => !string.IsNullOrEmpty(s));
            
            foreach (var commandText in allNoneEmptyCommands)
            {
                if (OnCommandRecieved != null && OnCommandRecieved.GetPersistentEventCount() > 0)
                    OnCommandRecieved.Invoke(commandText);
            }
        }
    }

    [Serializable]
    public class RemoteCommandRecievedEvent : UnityEvent<String> { }
}
