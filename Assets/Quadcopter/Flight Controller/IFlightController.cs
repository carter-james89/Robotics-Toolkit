using QuadcopterUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static QuadcopterUtilities.IQuadcopter;

public interface IFlightController 
{
    public bool IsInitialized();

    public bool IsReadyToFly();
    
    public void Initialize(IQuadcopter quadToControl, Action<IQuadcopter.FlightStatus> onFlightStatusChanged);

    public Quaternion GetGyroRotation();

    public QuadcopterData GetSensorData();

    public void Run(IQuadcopter.FlightStatus flightStatus, IInputs.FlightControlValues craftInputs);

    public bool IsSimulator();

    public void Takeoff();
    public void Land();


}
