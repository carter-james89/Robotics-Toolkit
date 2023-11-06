using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class MdnsListener : MonoBehaviour
{
    private Thread mdnsThread;
    private UdpClient client;
    private bool isListening;

    private const int MdnsPort = 5353;
    private readonly IPAddress MdnsMulticastAddressV4 = IPAddress.Parse("224.0.0.251");
    // For IPv6, use: private readonly IPAddress MdnsMulticastAddressV6 = IPAddress.Parse("ff02::fb");

    void Start()
    {
        StartListening();
    }

    private void StartListening()
    {
        isListening = true;
        mdnsThread = new Thread(ListenForMdns)
        {
            IsBackground = true
        };
        mdnsThread.Start();
    }

    private void ListenForMdns()
    {
        using (client = new UdpClient())
        {
            client.ExclusiveAddressUse = false;
            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, MdnsPort);

            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.ExclusiveAddressUse = false;

            client.Client.Bind(localEp);

            IPAddress multicastAddress = MdnsMulticastAddressV4;
            client.JoinMulticastGroup(multicastAddress);

            Debug.Log("Listening for mDNS packets on " + multicastAddress);

            try
            {
                while (isListening)
                {
                    IPEndPoint remoteEp = new IPEndPoint(IPAddress.Any, MdnsPort);
                    byte[] data = client.Receive(ref remoteEp);

                    // Convert the byte array to a hex string for logging
                    var hex = BitConverter.ToString(data);
                    Debug.Log($"Received mDNS packet from {remoteEp.Address}: {hex}");

                    // Here you would need to implement or call a proper mDNS/DNS-SD packet parser
                    // to interpret the binary data and extract meaningful information.
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving mDNS packet: " + e.Message);
            }
        }
    }


    void OnDestroy()
    {
        StopListening();
    }

    private void StopListening()
    {
        if (isListening)
        {
            isListening = false;
            client?.DropMulticastGroup(MdnsMulticastAddressV4);
            client?.Close();
            mdnsThread?.Join();
        }
    }
}
