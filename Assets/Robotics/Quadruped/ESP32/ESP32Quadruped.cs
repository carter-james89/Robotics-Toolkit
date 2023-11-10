
using UnityEngine;
using System.Net.Sockets;
using Toolkit.NetworkUtilites;
using System.Net;
using System.Text;
using System;


namespace Toolkit.Robotics.Quadruped
{
    public class ESP32Quadruped : Quadruped
    {
        UdpClient udpClient;
        IPEndPoint remoteEndPoint;
        IPAddress _ipAddress;
        private bool _connected = false;
        private System.Diagnostics.Stopwatch connectionStopwatch = new System.Diagnostics.Stopwatch();
        public void EstablishConnection(IUDPConnectionEventListener.EventData eventData)
        {
            UnityEngine.Debug.Log("Attempt to establish connection");
            _ipAddress = IPAddress.Parse(eventData.ConnectionData.IP);
            udpClient = new UdpClient(eventData.ConnectionData.Port);
            remoteEndPoint = new IPEndPoint(_ipAddress, eventData.ConnectionData.Port);
            SendUDPMessageAndWaitForResponse(0, new byte[0]);
            connectionStopwatch.Start();
            _connected = true;
            UnityEngine.Debug.Log("CONNECTED");
            //onConnectionTime = Time.timeSinceLevelLoad;
        }

        private void Update()
        {
            if (_connected)
            {
                RunSpeedTest();
                //PositionTransform();
            }
        }

        private byte[] SendUDPMessageAndWaitForResponse(int header, byte[] message, int timeout = 10000)
        {
            // Combine the header and the message into one byte array
            byte[] headerBytes = BitConverter.GetBytes(header);
            byte[] packet = new byte[headerBytes.Length + message.Length];
            Buffer.BlockCopy(headerBytes, 0, packet, 0, headerBytes.Length);
            Buffer.BlockCopy(message, 0, packet, headerBytes.Length, message.Length);
           // Debug.Log("Send message with header " + header.ToString());
            // Send the packet
            udpClient.Send(packet, packet.Length, remoteEndPoint);

            // Set the timeout duration in milliseconds
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
                       // Debug.Log("Response header was correct");
                        // Create a new array to hold the remaining bytes after the header
                        byte[] remainingBytes = new byte[bytes.Length - sizeof(int)];
                        Buffer.BlockCopy(bytes, sizeof(int), remainingBytes, 0, remainingBytes.Length);

                       // Debug.Log("message length " + remainingBytes.Length);

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
                    Debug.LogWarning("Timeout, no response received.");
                }
                else
                {
                    Debug.LogError("SocketException occurred: " + ex.Message);
                }
            }

            return null; // Return null if no response was received or if headers do not match
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


        public override void PositionTransform()
        {
            // Example usage of the new SendUDPMessageAndWaitForResponse method
            int header = 2; // Example header
            byte[] message = new byte[0]; // No additional message content
            byte[] response = SendUDPMessageAndWaitForResponse(header, message);

       
        }



        void OnDestroy()
        {
            udpClient?.Close();
        }
    }
}