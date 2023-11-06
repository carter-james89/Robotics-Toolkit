using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
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
        public void EstablishConnection(IUDPConnectionEventListener.EventData eventData)
        {
            _ipAddress = eventData.IP;
            udpClient = new UdpClient(eventData.ListenPort);
            remoteEndPoint = new IPEndPoint(eventData.IP, eventData.ListenPort);
            byte[] sendBytes = Encoding.UTF8.GetBytes("Digital Twin Connection Established");
            udpClient.Send(sendBytes, sendBytes.Length, remoteEndPoint);
            //  sender.Close();
            //Debug.Log("Sent UDP response to " + ipAddress + ":" + port);

        }

        public override void PositionTransform()
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes("<1>");
            udpClient.Send(sendBytes, sendBytes.Length, remoteEndPoint);

            //// Listen synchronously (blocking call)
            //IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
            byte[] bytes = udpClient.Receive(ref remoteEndPoint); // This will block the thread until a message is received

            // Convert the bytes to a string and process the message
            string message = Encoding.UTF8.GetString(bytes);
            if (message.StartsWith("<") && message.EndsWith(">"))
            {
                // Extract the content inside the brackets
                string content = message.Trim('<', '>');
                // Process the message content here
                Debug.Log("Received: " + content);
            }
        }


        void OnDestroy()
        {
            udpClient?.Close();
        }
    }
}