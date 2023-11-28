using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Toolkit.Utilities.Events;

[Serializable]
public class ConnectionData
{
    public string BoardType;
    public string Name;
    public string IP;
    public int Port;

    public ConnectionData() { }
}

namespace Toolkit.NetworkUtilites
{
    public interface IUDPConnectionEventListener
    {
        public class EventData
        {
            public Type EventType;
            public IPAddress BroadcastIP;
            public int BroadcastPort;
            public ConnectionData ConnectionData;

            public EventData(Type eventType, IPAddress broadcastIP, int broadcastPort, ConnectionData connectionData)
            {
                EventType = eventType;
                BroadcastIP = broadcastIP;
                BroadcastPort = broadcastPort;
                ConnectionData = connectionData;
            }
        }
        public enum Type
        {
            OnBroadcastReceived,
            OnRobotDisconnected,
        }
        void OnConnectionEventOccured(EventData eventData);
    }

    public class UDPConnectionListener
    {
        private static UDPConnectionListener _instance;
        private UdpClient listener;
        private bool isListening;
        private const int listenPort = 5501; // The port number to listen on
        private InterfaceEventManager<IUDPConnectionEventListener> _listenerManager = new InterfaceEventManager<IUDPConnectionEventListener>("UDP Listener");

        public static UDPConnectionListener Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UDPConnectionListener();
                }
                return _instance;
            }
        }

        private UDPConnectionListener()
        {
            isListening = true;
            listener = new UdpClient(listenPort);
            listener.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            Debug.Log("Listening for UDP messages on port: " + listenPort);
        }


        private void ReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
            byte[] receivedBytes = listener.EndReceive(ar, ref remoteEndPoint);

            // Convert the byte array to a string
            string receivedString = Encoding.UTF8.GetString(receivedBytes);
            Debug.Log("Received UDP Broadcast from " + remoteEndPoint.Address + " : " + receivedString);

            // Deserialize the JSON string into the ConnectionData class
            ConnectionData data = JsonUtility.FromJson<ConnectionData>(receivedString);

            NotifyListeners(new IUDPConnectionEventListener.EventData(IUDPConnectionEventListener.Type.OnBroadcastReceived, remoteEndPoint.Address, listenPort, data));

            // Continue listening for UDP messages
            if (isListening)
            {
                listener.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            }
        }


        public void Shutdown()
        {
            isListening = false;
            listener.Close();
        }

        public void SubscribeToConnectionEvents(IUDPConnectionEventListener newListener)
        {
            _listenerManager.AddListener(newListener);
        }
        public void UnsubscribeFromConnectionEvents(IUDPConnectionEventListener listener)
        {
            _listenerManager.RemoveListener(listener);
        }
        private void NotifyListeners(IUDPConnectionEventListener.EventData eventData)
        {
            foreach (var listener in _listenerManager.GetListeners())
            {
                listener.OnConnectionEventOccured(eventData);
            }
        }
    }
}
