using ProcessCommunicationToolkit.SocketPortConnection;
using ProcessCommunicationToolkit_Csharp;
using QuadcopterUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static QuadcopterUtilities.IQuadcopter;

public class MotorThrustCalculator
{
    private PidController _elevationPid;
    private PidController _pitchPid;
    private PidController _rollPid;
    private PidController _yawPid;

    [SerializeField]
    private Transform _targetGimbal;

    /// <summary>
    /// How long has it been since the last Update, required for <see cref="PidController"/>
    /// </summary>
    /// <remarks>
    /// Exposed in Inspector solely for debuging
    /// </remarks>
    [SerializeField]
    private float _timeSinceLastUpdate;
    /// <summary>
    /// The time of the last update
    /// </summary>
    private float prevDeltaTime = 0;

    public struct MotorValues
    {
        public float motorFL;
        public float motorFR;
        public float motorBR;
        public float motorBL;
    }

    public float altitudeHoldPos { get; private set; }
    public void SetAltitudeHold(float newHoldHeight)
    {
        altitudeHoldPos = newHoldHeight;
    }

    public void Initialize(float currentHeading)
    {
        _yawHoldHeading = currentHeading;
        _elevationPid = new PidController(.03f, .04f, 0.04f, .8f, .1f);
        _elevationPid.SetPoint = 0;

        var translateP = .001f;
        var translateI = 0;
        var translateD = 0f;
        var translateLimit = .1f;

        _pitchPid = new PidController(translateP, translateI, translateD, translateLimit, -translateLimit);
        _pitchPid.SetPoint = 0;

        _rollPid = new PidController(translateP, translateI, translateD, translateLimit, -translateLimit);
        _rollPid.SetPoint = 0;

        _yawPid = new PidController(.03f, 0.05f, 0.008, .1f, -.1f);
        _yawPid.SetPoint = 0;

        prevDeltaTime = Time.time;
    }

    private float _yawHoldHeading;
    public MotorValues Run(Vector3 currentPos, Vector3 currentEuler, IInputs.FlightControlValues inputs)
    {
        _timeSinceLastUpdate = Time.time - prevDeltaTime;
        prevDeltaTime = Time.time;
        var deltaTime1 = (int)(_timeSinceLastUpdate * 1000);
        var deltaTime = new System.TimeSpan(0, 0, 0, 0, (deltaTime1));


        var throttleValue = calculateThrottle(inputs.throttle, currentPos.y, deltaTime);
        var eulerDif = currentEuler - calculateDesiredAngle(inputs);
        var pitchOffset = eulerDif.x;

        if (pitchOffset < -180)
            pitchOffset = 360 - System.Math.Abs(pitchOffset);
        else if (pitchOffset > 180)
            pitchOffset = -(360 - pitchOffset);

        _pitchPid.ProcessVariable = -pitchOffset;

        double trgtPitch = _pitchPid.ControlVariable(deltaTime);
        float pitchValue = (float)trgtPitch;

        //ROLL
        var rollOffset = eulerDif.z;

        if (rollOffset < -180)
            rollOffset = 360 - System.Math.Abs(rollOffset);
        else if (rollOffset > 180)
            rollOffset = -(360 - rollOffset);

        _rollPid.ProcessVariable = -rollOffset;

        double trgtRoll = _rollPid.ControlVariable(deltaTime);
        float rollValue = (float)trgtRoll;

        //yaw
        var yawOffset = eulerDif.y;
        if (yawOffset < -180)
            yawOffset = 360 - Math.Abs(yawOffset);
        else if (yawOffset > 180)
            yawOffset = -(360 - yawOffset);

        _yawPid.ProcessVariable = -yawOffset;

        var trgtyaw = _yawPid.ControlVariable(deltaTime);
        float yawValue = (float)trgtyaw;

        var motorValues = new MotorValues();
        motorValues.motorFR = throttleValue + pitchValue - rollValue + yawValue;
        motorValues.motorFL = throttleValue + pitchValue + rollValue - yawValue;
        motorValues.motorBR = throttleValue - pitchValue - rollValue - yawValue;
        motorValues.motorBL = throttleValue - pitchValue + rollValue + yawValue;

        return motorValues;
    }

    private Vector3 calculateDesiredAngle(IInputs.FlightControlValues inputs)
    {
        float pitchAngle = 0;
        if (inputs.pitch > 0)
        {
            pitchAngle = Mathf.Lerp(0, 15, inputs.pitch);
        }
        else
        {
            pitchAngle = Mathf.Lerp(0, -15, -inputs.pitch);
        }
        float rollAngle = 0;
        if (inputs.roll > 0)
        {
            rollAngle = Mathf.Lerp(0, 15, inputs.roll);
        }
        else
        {
            rollAngle = Mathf.Lerp(0, -15, -inputs.roll);
        }

        float yawAngle = 0;
        if (inputs.yaw > 0)
        {
            yawAngle = Mathf.Lerp(0, 15, inputs.yaw);
            _yawHoldHeading = _yawHoldHeading - yawAngle;
        }
        else if (inputs.yaw < 0)
        {
            yawAngle = Mathf.Lerp(0, 15, -inputs.yaw);
            _yawHoldHeading = _yawHoldHeading + yawAngle;
        }
        return new Vector3(pitchAngle, _yawHoldHeading, -rollAngle);
    }

    private float calculateThrottle(float throttle, float currentHeight, TimeSpan deltaTime)
    {
        float throttleValue = 0;
        if (Math.Abs(throttle) > 0)
        {
            altitudeHoldPos = currentHeight;
            throttleValue = throttle;
        }
        else
        {
            var heightOffset = currentHeight - altitudeHoldPos;
            _elevationPid.ProcessVariable = heightOffset;
            double trgtThrottle = _elevationPid.ControlVariable(deltaTime);
            throttleValue = (float)trgtThrottle;
        }
        return throttleValue;
    }

    private void calculatePitch(float desiredPitch, TimeSpan deltaTime)
    {

    }
}

public class PiCopterFlightController : MonoBehaviour, IFlightController
{
    public float gyroYaw;
    public float gyroRoll;
    public float gyroPitch;

    private static ClientUplink client;
    private static ServerUplink server;

    private Action<IQuadcopter.FlightStatus> _onFlightStatusChanged;

    [SerializeField]
    private bool _sendToClient = true;

    private float _pitchOffset;
    private float _rollOffset;
    private float _yawOffset;

    private QuadcopterData _quadcopterData;
    private GroundStationData _groundStatationData;

    private MotorThrustCalculator _motorCalculator;

    private IQuadcopter _quadToControl;

    private bool _isInitialized;
    private bool _serverConnected;
    private bool _clientConnected;
    public bool IsInitialized()
    {
        return _isInitialized;
    }
    public bool IsReadyToFly()
    {
        if ((!_startServer || server.uplinkStatus == SocketUplink.Status.Connected) && (!_startClient || client.uplinkStatus == SocketUplink.Status.Connected))
        {
            return true;
        }
        //Debug.Log(server.uplinkStatus);
        //Debug.Log(_clientConnected);
        return false;
    }


    private const string ipAddress = "192.168.86.41";// "192.168.86.50";
    private const string serverIPAddress = "192.168.86.27";

    private bool _startServer = true;
    private bool _startClient = true;

    public Quaternion GetGyroRotation()
    {
        throw new System.NotImplementedException();
    }

    public void Initialize(IQuadcopter quadToControl, Action<IQuadcopter.FlightStatus> onFlightStatusChanged)
    {
        _quadToControl = quadToControl;
        _onFlightStatusChanged = onFlightStatusChanged;

        _quadcopterData = new QuadcopterData();
        _groundStatationData = new GroundStationData();

        if (_startServer)
        {
            server = new ServerUplink(11001, serverIPAddress);
            server.uplinkMessage += x => Debug.Log("Server Log : " + x);
            server.EstablishConnection();
        }
        if (_startClient)
        {
            client = new ClientUplink(11000, ipAddress);
            client.uplinkMessage += x => Debug.Log("Client Log : " + x);
            // client.uplinkMessage += OnClientLogMessageRecieved;
            client.onConnectionEstablished += OnConnectedToServer;
            client.EstablishConnection();
        }
        _motorCalculator = new MotorThrustCalculator();
        _motorCalculator.Initialize(0);

        _isInitialized = true;
    }

    private void OnConnectedToServer(string obj)
    {
        _serverConnected = true;
        client.ListenForServerData(CommunicationUtilities.TypeToByte(new QuadcopterData()).Length, OnDataRecievedFromServer);
    }

    [SerializeField]
    private float _rawYaw;
    [SerializeField]
    private float _rawPitch;
    [SerializeField]
    private float _rawRoll;
    private byte[] OnDataRecievedFromServer(byte[] arg)
    {
        _quadcopterData = (QuadcopterData)CommunicationUtilities.ByteToType<QuadcopterData>(arg);
        //run calculations and return
        if (_rollOffset == 0)
        {
            _rollOffset = _quadcopterData.gyroRoll;
            _pitchOffset = _quadcopterData.gyroPitch;
            _yawOffset = _quadcopterData.gyroYaw;
        }

        _rawYaw = _quadcopterData.gyroYaw;
        _rawPitch = _quadcopterData.gyroPitch;
        _rawRoll = _quadcopterData.gyroRoll;

        gyroPitch = _quadcopterData.gyroPitch - _pitchOffset;
        gyroRoll = _quadcopterData.gyroRoll - _rollOffset;
        gyroYaw = _quadcopterData.gyroYaw - _yawOffset;

        return new byte[1];
    }

    public QuadcopterData GetSensorData()
    {
        return _quadcopterData;
    }

    public bool IsSimulator()
    {
        return false;
    }

    public void Run(FlightStatus flightStatus, IInputs.FlightControlValues desiredInputs)
    {
        if (Time.frameCount > 500)
        {
            _groundStatationData.yaw = desiredInputs.yaw;
            _groundStatationData.pitch = desiredInputs.pitch;
            _groundStatationData.roll = desiredInputs.roll;
            _groundStatationData.throttle = desiredInputs.throttle;


            var motorValues = _motorCalculator.Run(new Vector3(_quadcopterData.posX, _quadcopterData.posY, _quadcopterData.posZ), new Vector3(_quadcopterData.gyroPitch, _quadcopterData.gyroYaw, _quadcopterData.gyroRoll), desiredInputs);

            _groundStatationData.motorBRSpeed = Math.Round(motorValues.motorBR);
            _groundStatationData.motorBLSpeed = Math.Round(motorValues.motorBL);
            _groundStatationData.motorFRSpeed = Math.Round(motorValues.motorFR);
            _groundStatationData.motorFLSpeed = Math.Round(motorValues.motorFL);

            byte[] json = CommunicationUtilities.TypeToByte(_groundStatationData);
            if (_startServer)
            {
                //byte[] responce = server.SendMessage(json, json.Length);
                byte[] responce = server.SendMessageWithHeader(json, json.Length);
            }                
        }
    }

    public void Takeoff()
    {

        _motorCalculator.Initialize(_quadToControl.GetGameObject().transform.eulerAngles.y);
        _motorCalculator.SetAltitudeHold(1);
        _onFlightStatusChanged.Invoke(FlightStatus.Launching);
    }

    public void Land()
    {
        throw new System.NotImplementedException();
    }

    private void OnDestroy()
    {
        if (_isInitialized)
        {
            if (client != null)
            {
                client.onConnectionEstablished -= OnConnectedToServer;
                client.ShutDown();
            }
            if (server != null)
            {
                server.ShutDown();
            }

        }
    }
}
