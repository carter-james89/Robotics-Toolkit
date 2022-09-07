using QuadcopterUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static QuadcopterUtilities.IQuadcopter;

public interface IMotorThrustCalculator
{
    public struct MotorThrustValues
    {
        public float motorFL;
        public float motorFR;
        public float motorBR;
        public float motorBL;
    }
    public void SetAltitudeHold(float newHoldHeight);

    public void Initialize(float currentHeading);
    public MotorThrustValues Run(Vector3 currentPos, Vector3 currentEuler, IInputs.FlightControlValues inputs);
}

public class SimulatedLocalFlightController : MonoBehaviour, IFlightController
{
    private IMotorThrustCalculator _motorCalculator;

    private IQuadcopter _quadToControl;

    [SerializeField]
    private Motor flMotor;
    [SerializeField]
    private Motor frMotor;
    [SerializeField]
    private Motor blMotor;
    [SerializeField]
    private Motor brMotor;

    [SerializeField]
    private float _debugFLValue;
    [SerializeField]
    private float _debugFRValue;
    [SerializeField]
    private float _debugBLValue;
    [SerializeField]
    private float _debugBRValue;

    [SerializeField]
    private float _debugThrottleValue;


    private QuadcopterData _quadcopterData;
    private GroundStationData _groundStatationData;

    private Action<IQuadcopter.FlightStatus> _onFlightStatusChanged;

    private Rigidbody _rigidBody;

    private bool _isInitialized;
    public bool IsInitialized()
    {
        return _isInitialized;
    }
    public bool IsReadyToFly()
    {
        return true;
    }

    public Quaternion GetGyroRotation()
    {
        throw new NotImplementedException();
    }

    public IQuadcopter.QuadcopterData GetSensorData()
    {
        _quadcopterData = new QuadcopterData();
        _quadcopterData.gyroPitch = transform.localEulerAngles.x;
        _quadcopterData.gyroYaw = transform.localEulerAngles.y;
        _quadcopterData.gyroRoll = transform.localEulerAngles.z;

        _quadcopterData.posX = transform.localPosition.x;
        _quadcopterData.posY = transform.localPosition.y;
        _quadcopterData.posZ = transform.localPosition.z;

        //RaycastHit hit;
        //// Does the ray intersect any objects excluding the player layer
        //if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        //{
        //    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
        //}
        // _quadcopterData.height = hit.distance;
        _quadcopterData.height = transform.localPosition.y;

        return _quadcopterData;
    }



    public void Initialize(IQuadcopter quadToControl, Action<IQuadcopter.FlightStatus> onFlightStatusChanged)
    {
        Debug.Log("Initialize Local Fligth Controller");
        _quadToControl = quadToControl;
        _motorCalculator = GetComponent<IMotorThrustCalculator>();
        _rigidBody = quadToControl.GetGameObject().GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;
        _groundStatationData = new GroundStationData();
        _onFlightStatusChanged = onFlightStatusChanged;
        _isInitialized = true;
    }

    public bool IsSimulator()
    {
        return true;
    }

    public void Land()
    {
        _groundStatationData = new GroundStationData();
        frMotor.SetThrottle(0);
        flMotor.SetThrottle(0);

        brMotor.SetThrottle(0);
        blMotor.SetThrottle(0);
    }


    public void Run(IQuadcopter.FlightStatus flightStatus, IInputs.FlightControlValues craftInputs)
    {
        if(flightStatus == FlightStatus.Launching)
        {
            if(_quadcopterData.height > 1)
            {
                _onFlightStatusChanged.Invoke(FlightStatus.Flying);
            }
        }
        if(flightStatus != FlightStatus.PreLaunch)
        {
            var motorValues = _motorCalculator.Run(new Vector3(_quadcopterData.posX,_quadcopterData.posY,_quadcopterData.posZ), new Vector3(_quadcopterData.gyroPitch, _quadcopterData.gyroYaw, _quadcopterData.gyroRoll), craftInputs);

            _debugThrottleValue = craftInputs.throttle;
           // Debug.Log(motorValues.motorBL + " " + motorValues.motorFR);

            _groundStatationData.motorBRSpeed = motorValues.motorBR;
            _groundStatationData.motorBLSpeed = motorValues.motorBL;
            _groundStatationData.motorFRSpeed = motorValues.motorFR;
            _groundStatationData.motorFLSpeed = motorValues.motorFL;

            _debugBLValue = motorValues.motorBL;
            _debugBRValue = motorValues.motorBR;
            _debugFLValue = motorValues.motorFL;
            _debugFRValue = motorValues.motorFR;

            frMotor.SetThrottle((float)_groundStatationData.motorFRSpeed);
            flMotor.SetThrottle((float)_groundStatationData.motorFLSpeed);

            brMotor.SetThrottle((float)_groundStatationData.motorBRSpeed);
            blMotor.SetThrottle((float)_groundStatationData.motorBLSpeed);
        }
    }

    public void Takeoff()
    {
       // Debug.Log("Simulator TakeOff");
       // _motorCalculator = new MotorThrustCalculator();
        _motorCalculator.Initialize(_quadToControl.GetGameObject().transform.eulerAngles.y);
        _motorCalculator.SetAltitudeHold(1);
        _onFlightStatusChanged.Invoke(FlightStatus.Launching);
        _rigidBody.useGravity = true;
    }
}
