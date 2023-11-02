using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPCommunicationManager 
{
    protected IPHostEntry ipHostInfo;
    protected IPAddress ipAddress;
    protected IPEndPoint localEndPoint;
    public Action<string> uplinkMessage;
    public int port { get; protected set; }
    protected Socket dataSocket;

    private UdpClient _udpClient;
    public UDPCommunicationManager(int port, string ipAddress)
    {
        //  ipAddress = "192.168.86.50"
        this.port = port;
        // Establish the remote endpoint for the socket.  
        // This example uses port 11000 on the local computer.  
       // ipHostInfo = Dns.GetHostEntry(ipAddress);
        this.ipAddress = IPAddress.Parse(ipAddress);  //ipHostInfo.AddressList[0];

        localEndPoint = new IPEndPoint(this.ipAddress, port);
        Debug.Log("Send Connection Request");
        try
        {
            _udpClient = new UdpClient();
            _udpClient.Connect(localEndPoint);
             byte[] buffer = Encoding.ASCII.GetBytes("<1>");
            _udpClient.Send(buffer, buffer.Length);

            // Listen for a response
           var _receiveBuffer = _udpClient.Receive(ref localEndPoint);
            string receivedMessage = Encoding.ASCII.GetString(_receiveBuffer);
            Debug.Log("Received: " + receivedMessage);

        }
        catch (Exception e) { Debug.Log(e.Message); }

      
    }
}
