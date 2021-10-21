using ProcessCommunicationToolkit;
using ProcessCommunicationToolkit.SocketPortConnection;
using System;
using UnityControllerForTello;
using UnityEngine;
using ProcessCommunicationToolkit_Csharp;

public class QuadcopterData : IUplinkData
{
    public double throttle;
    public double yaw;
    public double pitch;
    public double roll;

    public float gyroYaw;
    public float gyroRoll;
    public float gyroPitch;
}
public class PiQuadcopter : Quadcopter
{
    public float gyroYaw;
    public float gyroRoll;
    public float gyroPitch;

    private static ClientUplink client;

    [SerializeField]
    private bool _sendToClient = true;

    private float _pitchOffset;
    private float _rollOffset;
    private float _yawOffset;

    public override void Initialize(Func<IInputs.FlightControlValues> defaultInputSource)
    {
        base.Initialize(defaultInputSource);



      
    }

    private void Awake()
    {
        client = new ClientUplink(11000, "192.168.86.50");
         client.uplinkMessage += x => Debug.Log(x);
       // client.uplinkMessage += OnClientLogMessageRecieved;
        client.EstablishConnection();

        client.onConnectionEstablished += OnConnectedToServer;
    }

    private void OnConnectedToServer(string obj)
    {
        client.ListenForServerData(CommunicationUtilities.TypeToByte(new QuadcopterData()).Length, OnDataRecievedFromServer);
    }

    private byte[] OnDataRecievedFromServer(byte[] arg)
    {
        QuadcopterData dataFromQuad = (QuadcopterData)CommunicationUtilities.ByteToType<QuadcopterData>(arg);
        //run calculations and return
        if (_rollOffset == 0)
        {
            _rollOffset = dataFromQuad.gyroRoll;
            _pitchOffset = dataFromQuad.gyroPitch;
            _yawOffset = dataFromQuad.gyroYaw;
        }
        gyroPitch = dataFromQuad.gyroPitch - _pitchOffset;
        gyroRoll = dataFromQuad.gyroRoll - _rollOffset;
        gyroYaw = dataFromQuad.gyroYaw - _yawOffset;
        return CommunicationUtilities.TypeToByte(dataFromQuad);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        client.ShutDown();
        client.onConnectionEstablished += OnConnectedToServer;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(new Vector3(gyroPitch,gyroYaw,gyroRoll));
    }

    public override bool IsSimulator()
    {
        return false;
    }

    public override bool IsTracking()
    {
        return true;
    }

    public override void Land()
    {
     
    }

    public override void TakeOff()
    {
       
    }
}
