using ProcessCommunicationToolkit;
using ProcessCommunicationToolkit.SocketPortConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityControllerForTello;
using UnityEngine;

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
        client.ListenForServerData(client.SerializeJson(new QuadcopterData()).Length, OnDataRecievedFromServer);
    }

    private byte[] OnDataRecievedFromServer(byte[] arg)
    {
        var quadData = client.AttemptDeserialize(arg, typeof(QuadcopterData));
        return client.ConvertToBytes(client.SerializeJson(quadData));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        client.ShutDown();
        client.onConnectionEstablished += OnConnectedToServer;
    }

    //private void OnClientLogMessageRecieved(string obj)
    //{
    //    Debug.Log(obj);
    //}

    private void Update()
    {
        transform.rotation = Quaternion.Euler(new Vector3(gyroPitch,gyroYaw,gyroRoll));
    }
    public static void OnDataRecieved(byte[] data)
    {
       // client.
        //string someString = Encoding.ASCII.GetString(data);
        // Console.WriteLine(someString);
    }

    private  void OnReceivedCustomMessage(string obj)
    {
        if (obj.IndexOf("exit") > -1)
        {
            //ShutDown();
        }
        else
        {
            Debug.Log("got custom message " + obj);
            //arduinoConnection.Write(obj);
        }
    }
    private void OnReceivedValidObject(IUplinkData validPackage)
    {
        var quadData = validPackage as QuadcopterData;

        if(_rollOffset == 0)
        {
            _rollOffset = quadData.gyroRoll;
            _pitchOffset = quadData.gyroPitch;
            _yawOffset = quadData.gyroYaw;
        }
        gyroPitch = quadData.gyroPitch - _pitchOffset;
        gyroRoll = quadData.gyroRoll - _rollOffset;
        gyroYaw = quadData.gyroYaw - _yawOffset;

    
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
