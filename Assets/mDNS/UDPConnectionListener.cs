using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Collections.Generic;
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
            OnRobotConnected,
            OnRobotDisconnected,
        }
        public void OnConnectionEventOccured(EventData eventData);
      
    }
    public class UDPConnectionListener : MonoBehaviour
    {
        private UdpClient listener;
        private bool isListening;
        private const int listenPort = 5500; // The port number to listen on

        private InterfaceEventManager<IUDPConnectionEventListener> _listenerManager = new InterfaceEventManager<IUDPConnectionEventListener>("UDP Listener");

        void Start()
        {
            isListening = true;
            listener = new UdpClient(listenPort);
            listener.BeginReceive(new AsyncCallback(ReceiveCallback), null);
           // listener.re
            Debug.Log("Listening for UDP messages on port: " + listenPort);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Debug.Log("Received UDP message from)");// " + remoteEndPoint.Address + " : " + receivedString);
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
            byte[] receivedBytes = listener.EndReceive(ar, ref remoteEndPoint);

            
            // Convert the byte array to a string
            string receivedString = Encoding.UTF8.GetString(receivedBytes);
            Debug.Log("Received UDP message from " + remoteEndPoint.Address + " : " + receivedString);

            // Deserialize the JSON string into the ConnectionData class
            ConnectionData data = JsonUtility.FromJson<ConnectionData>(receivedString);

            Debug.Log(data.BoardType);
            Debug.Log(data.IP);
            Debug.Log(data.Port);
            // Check if the message contains "ESP32"
            if (data.BoardType.Contains("esp32"))
            {
                // Send a response to the sender's local UDP port
              //  SendUDPResponse(data.ip, data.port, "Received your message, ESP32!");
                OnConnectedToQuadruped(remoteEndPoint.Address, data);
            }

            // Continue listening for UDP messages
            if (isListening)
            {
                listener.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            }
        } 

        private void OnConnectedToQuadruped(IPAddress ip, ConnectionData connectionData)
        {
            //  var quadrupedData = new UDPConnectionData(name, ip, port);
           // Debug.Log("Connected to quad at ip " + ip.ToString());
          NotifyListeners(new IUDPConnectionEventListener.EventData(IUDPConnectionEventListener.Type.OnRobotConnected,ip,listenPort,connectionData));
        }

        private void SendUDPResponse(string ipAddress, int port, string message)
        {
            UdpClient sender = new UdpClient();
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);
            sender.Send(sendBytes, sendBytes.Length, remoteEndPoint);
            sender.Close();
            Debug.Log("Sent UDP response to " + ipAddress + ":" + port);
        }

        private void OnDisable()
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
