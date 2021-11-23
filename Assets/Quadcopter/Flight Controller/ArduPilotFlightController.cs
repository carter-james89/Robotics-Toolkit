using ProcessCommunicationToolkit.UDP;
using QuadcopterUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ArduPilotFlightController : MonoBehaviour, IFlightController
{
    public Quaternion GetGyroRotation()
    {
        throw new NotImplementedException();
    }

    public IQuadcopter.QuadcopterData GetSensorData()
    {
        return new IQuadcopter.QuadcopterData();
    }
    private UDPClient client;
    public void Initialize(IQuadcopter quadToControl, Action<IQuadcopter.FlightStatus> onFlightStatusChanged)
    {
        client = new UDPClient();
        client.Connect("192.168.10.1", 8889);
    }

    public bool IsInitialized()
    {
        return true;
    }

    public bool IsReadyToFly()
    {
        return true;
    }

    public bool IsSimulator()
    {
        throw new NotImplementedException();
    }

    public void Land()
    {
        throw new NotImplementedException();
    }

    public void Run(IQuadcopter.FlightStatus flightStatus, IInputs.FlightControlValues craftInputs)
    {
       
    }

    public void Takeoff()
    {
        throw new NotImplementedException();
    }

    private void OnDestroy()
    {
        client.Shutdown();
    }
}
