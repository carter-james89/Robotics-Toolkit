
using UnityEngine;
using System.Net.Sockets;
using Toolkit.NetworkUtilites;
using System.Net;
using System.Text;
using System;


namespace Toolkit.Robotics.Quadruped
{
    [RequireComponent(typeof(UDPCommunicationListener))]
    public class ESP32Quadruped : Quadruped, IUDPConnectionEventListener
    {
        UdpClient udpClient;
        IPEndPoint remoteEndPoint;
        IPAddress _ipAddress;
        private bool _connected = false;
        private System.Diagnostics.Stopwatch connectionStopwatch = new System.Diagnostics.Stopwatch();

        [SerializeField]
        private string _name = "bittle";
        protected override void Start()
        {
            if (SimulationMode())
            {
                base.Start();
                return;
            }
            UDPConnectionListener.Instance.SubscribeToConnectionEvents(this);
        }
        public void OnConnectionEventOccured(IUDPConnectionEventListener.EventData eventData)
        {
            Debug.Log("Check broadcast for correct esp32 info");
            if (eventData.ConnectionData.BoardType.ToLower().Contains("esp32"))
            {
                if (eventData.ConnectionData.Name.ToLower().Contains(_name))
                {
                    EstablishConnection(eventData);
                }
                else
                {
                    Debug.Log("Not a bittle");
                }
            }
            else
            {
                Debug.Log("Not a ESP32");
            }
        }

        public void EstablishConnection(IUDPConnectionEventListener.EventData eventData)
        {
            Debug.Log("Attempt to establish connection");
            _ipAddress = IPAddress.Parse(eventData.ConnectionData.IP);
            udpClient = new UdpClient(eventData.ConnectionData.Port);
            remoteEndPoint = new IPEndPoint(_ipAddress, eventData.ConnectionData.Port);
            SendUDPMessageAndWaitForResponse(0, new byte[0]);
            connectionStopwatch.Start();
            _connected = true;
            UnityEngine.Debug.Log("CONNECTED");
            UDPConnectionListener.Instance.UnsubscribeFromConnectionEvents(this);
            //Bootup();
        }

        protected override void Update()
        {
            if(_connected && !_isRunning)
            {
                Bootup();//needs to be done on this thread pretty sure
            }
            base.Update();
        }

        private byte[] SendUDPMessageAndWaitForResponse(int header, byte[] message, int timeout = 10000)
        {
            if (SimulationMode())
            {
                Debug.LogWarning("Should not be sending udp messages in simulation mode");
                return null;
            }
            // Combine the header and the message into one byte array
            byte[] headerBytes = BitConverter.GetBytes(header);
            byte[] packet = new byte[headerBytes.Length + message.Length];
            Buffer.BlockCopy(headerBytes, 0, packet, 0, headerBytes.Length);
            Buffer.BlockCopy(message, 0, packet, headerBytes.Length, message.Length);
            udpClient.Send(packet, packet.Length, remoteEndPoint);
            udpClient.Client.ReceiveTimeout = timeout;

            try
            {
                // This will block the thread until a message is received or timeout occurs
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = udpClient.Receive(ref remoteIpEndPoint);

                // Check if the received bytes array is long enough to contain a header
                if (bytes.Length >= sizeof(int))
                {
                    // Extract the header from the received bytes
                    int receivedHeader = BitConverter.ToInt32(bytes, 0);

                    // Confirm that the received header matches the sent header
                    if (receivedHeader == header)
                    {
                        // Create a new array to hold the remaining bytes after the header
                        byte[] remainingBytes = new byte[bytes.Length - sizeof(int)];
                        Buffer.BlockCopy(bytes, sizeof(int), remainingBytes, 0, remainingBytes.Length);
                        return remainingBytes; // Return the remaining bytes
                    }
                    else
                    {
                        Debug.LogWarning("Received header does not match the sent header.");
                    }
                }
                else
                {
                    Debug.LogWarning("Received bytes do not contain a complete header.");
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    OnTimeOut();
                }
                else
                {
                    OnConnectionError("SocketException occurred: " + ex.Message);
                }
            }
            return null; // Return null if no response was received or if headers do not match
        }

        private void OnTimeOut()
        {
            Debug.LogWarning("Timeout, no response received.");
            OnConnectionShutdown();
        }
        private void OnConnectionError(string error)
        {
            Debug.LogWarning(error);
            OnConnectionShutdown();
        }
        private void OnConnectionShutdown()
        {
            UDPConnectionListener.Instance.SubscribeToConnectionEvents(this);
        }

        public void RunSpeedTest()
        {
            int header = 1; // Example header
            byte[] message = new byte[0]; // No additional message content
            byte[] response = SendUDPMessageAndWaitForResponse(header, message);

            if (response != null)// && response.Length >= sizeof(long))
            {
                // Deserialize the response to get the milliseconds since connection from the ESP32
                long esp32TimeSinceConnection = BitConverter.ToInt32(response, 0);

                // Get the time since connection in milliseconds from the Stopwatch
                long unityTimeSinceConnection = connectionStopwatch.ElapsedMilliseconds;

                // Compare the two times
                long timeDifference = unityTimeSinceConnection - esp32TimeSinceConnection;

               // Debug.Log($"ESP32 time since connection: {esp32TimeSinceConnection} ms");
               // Debug.Log($"Unity time since connection: {unityTimeSinceConnection} ms");
                Debug.Log($"Time difference: {timeDifference} ms");
            }
            else
            {
                Debug.LogWarning("Invalid or no response received.");
            }
        }

        protected override void PositionTransform()
        {
            if (SimulationMode())
            {
                return;
            }
            RunSpeedTest();
            return;
            // Example usage of the new SendUDPMessageAndWaitForResponse method
            int header = 2; // Example header
            byte[] message = new byte[0]; // No additional message content
            byte[] response = SendUDPMessageAndWaitForResponse(header, message);

       
        }

        void OnDestroy()
        {
            udpClient?.Close();
            UDPConnectionListener.Instance.Shutdown();
        }
    }
}