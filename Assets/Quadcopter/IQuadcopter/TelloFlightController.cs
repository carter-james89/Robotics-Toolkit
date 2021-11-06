using QuadcopterUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TelloLib;
using UnityEngine;

public class TelloFlightController : MonoBehaviour, IFlightController
{
    /// <summary>
    /// Video feed to display the camera from the Tello
    /// </summary>
    [SerializeField]
    private TelloVideoFeed _videoFeed;

    private Vector3 prevRecordedPos;

    /// <summary>
    /// The current connection state with the Tello, must be <see cref="Tello.ConnectionState.Connected"/> to control
    /// </summary>
    public Tello.ConnectionState connectionState;

    /// <summary>
    /// Is the Tello tracking accurate this frame?
    /// </summary>
    /// <remarks>
    /// In poor lighting conditions or for no reason at all sometimes the position tracking of the Tello is way off
    /// The deltaposition from the last valid frame is used to determine if there is an unreasonable jump
    /// </remarks>
    private bool validTrackingFrame;


    /// <summary>
    /// How many packkages have we recieved from the Tello
    /// </summary>
    [SerializeField]
    private int _telloFrameCount = 0;
    /// <summary>
    /// The last frame recieved updated via <see cref="Tello_onUpdate(int)"/>
    /// </summary>
    private int _lastTelloUpdateFrame;

    private Action<IQuadcopter.FlightStatus> _onFlightStatusChanged;

    private bool _isInitialized;
    public bool IsInitialized()
    {
        return _isInitialized;
    }

    public Quaternion GetGyroRotation()
    {
        throw new System.NotImplementedException();
    }

    public void Initialize(IQuadcopter quadToControl, Action<IQuadcopter.FlightStatus> onFlightStatusChanged)
    {
        ConnectToTello();
        _onFlightStatusChanged = onFlightStatusChanged;
    }

    /// <summary>
    /// Attempt to connect to the Tello via <see cref="Tello"/> Library
    /// Must be connected to quadcopter via wifi
    /// </summary>
    public void ConnectToTello()
    {
        Tello.onConnection += Tello_onStateChanged;
        Tello.onUpdate += Tello_onUpdate;
        if (_videoFeed)
        {
            _videoFeed.InitializeFeed();
        }
        else
        {
            Debug.LogWarning("No TelloVideoFeed supplied in inspector, will not display video feed from Tello");
        }
        Tello.startConnecting();
    }

    /// <summary>
    /// Called from <see cref="Tello.onConnection"/> when the state of the connection with the Tello is changed
    /// </summary>
    private void Tello_onStateChanged(Tello.ConnectionState newState)
    {
        Debug.Log("Tello State Updated : " + newState);
        if (newState == Tello.ConnectionState.Connected)
        {
            // Debug.Log("Connected to Tello, please wait for camera feed " + Tello.state.);
            Tello.setPicVidMode(1); // 0: picture, 1: video
            Tello.setVideoBitRate((int)TelloVideoFeed.VideoBitRate.VideoBitRateAuto);
            Tello.requestIframe();
        }
        else if (newState == Tello.ConnectionState.Disconnected)
        {
            Debug.Log("Disconnected from Tello");
        }
    }
    /// <summary>
    /// Called from <see cref="Tello.onUpdate"/> when an update a package is recieved from the Tello
    /// </summary>
    ///<remarks>
    ///<see cref="Tello_onUpdate(int)"/> happens on its own thread, and to interact with unity/inputs we need to use <see cref="Update"/>
    ///This simply records that an update has been recieved from the Tello, and will be handled in the next <see cref="Update"/>
    /// </remarks>
    private void Tello_onUpdate(int cmdID)
    {
        _telloFrameCount++;
        _lastTelloUpdateFrame = Time.frameCount;
    }

    public IQuadcopter.QuadcopterData GetSensorData()
    {
        SyncDataWithTello();
        return new IQuadcopter.QuadcopterData();
    }
    /// <summary>
    /// Store all the information from the Tello package locally
    /// </summary>
    /// <remmarks>
    /// Not all values are guaranteed to work or be accurate, they come from Tello and <see cref="Tello"/> library
    /// </remmarks>
    public void SyncDataWithTello()
    {
        connectionState = Tello.connectionState;

        var state = Tello.state;
        posX = Tello.state.posY;
        posY = -Tello.state.posZ;
        posZ = Tello.state.posX;

        verticalSpeed = state.verticalSpeed;

        velY = state.velY;

        rawPosition = new Vector3(posX, posY, posZ);

        quatW = state.quatW;
        quatX = state.quatW;
        quatY = state.quatW;
        quatZ = state.quatW;

        var eulerInfo = state.toEuler();

        pitch = (float)eulerInfo[0];
        roll = (float)eulerInfo[1];
        yaw = (float)eulerInfo[2];

        toEuler = new Vector3(pitch, roll, yaw);

        posUncertainty = state.posUncertainty;
        batteryLow = state.batteryLow;
        batteryPercent = state.batteryPercentage;
        cameraState = state.cameraState;
        downVisualState = state.downVisualState;
        telloBatteryLeft = state.droneBatteryLeft;
        telloFlyTimeLeft = state.droneFlyTimeLeft;
        flymode = state.flyMode;
        flyspeed = state.flySpeed;
        flyTime = state.flyTime;
        gravityState = state.gravityState;
        height = state.height;
        imuCalibrationState = state.imuCalibrationState;
        imuState = state.imuState;
        lightStrength = state.lightStrength;
        onGround = state.onGround;
        powerState = state.powerState;
        pressureState = state.pressureState;
        temperatureHeight = state.temperatureHeight;
        wifiDisturb = state.wifiDisturb;
        wifiStrength = state.wifiStrength;
        windState = state.windState;
        flying = state.flying;

        hover = state.droneHover;

        VallidateTrackingInfo(new Vector3(posX,posY,posZ));
    }

    public void Run(IQuadcopter.FlightStatus flightStatus, IInputs.FlightControlValues craftInputs)
    {
        if (_lastTelloUpdateFrame != Time.frameCount)
        {
            switch (flightStatus)
            {
                case IQuadcopter.FlightStatus.Launching:
                    CheckForLaunchComplete();
                    break;
                case IQuadcopter.FlightStatus.Flying:
                    try
                    {
                        //validTrackedFrame = SetVirtualTelloPosition();
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e + " : Emergency Abort");
                       // abort?.Invoke();
                    }
                    break;
            }
        }
        Tello.controllerState.setAxis(craftInputs.yaw, craftInputs.throttle, craftInputs.roll, craftInputs.pitch);
    }
    /// <summary>
    /// Set the position of the virtual Tello in the Unity environment
    /// </summary>
    /// <returns>Is this an accurate frame, <see cref="validTrackingFrame"/> </returns>
    public bool VallidateTrackingInfo(Vector3 pos)
    {
        validTrackingFrame = true;

        Vector3 dif = prevRecordedPos - pos;
        var xDif = dif.x;
        var yDif = dif.y;
        var zDif = dif.z;

        //valid tello frame
        if (Mathf.Abs(xDif) < 2 & Mathf.Abs(yDif) < 2 & Mathf.Abs(zDif) < 2)
        {

        }
        else
        {
            // Debug.Log("Tracking lost " + _telloFrameCount);
            validTrackingFrame = false;
        }
        prevRecordedPos = pos;
        return validTrackingFrame;
    }

    /// <summary>
    /// Launch the Tello via its auto liftoff feature
    /// </summary>
    public void Takeoff()
    {
        if (connectionState == Tello.ConnectionState.Connected)
        {
            Tello.takeOff();
            _onFlightStatusChanged.Invoke(IQuadcopter.FlightStatus.Launching);
        }
        else
        {
            Debug.LogWarning("Not connected to tello prior to takeoff command : " + connectionState);
        }
      
    }
    /// <summary>
    /// Land the Tello via its auto land feature
    /// </summary>
    public void Land()
    {
        Tello.land();
        _onFlightStatusChanged.Invoke(IQuadcopter.FlightStatus.Landing);
    }
    /// <summary>
    /// Check to see if the Tello has finished its auto takeoff
    /// </summary>
    /// <remarks>
    /// This is a weird but either with the <see cref="Tello"/> library or the Tello itself
    /// When you take off the position of the Tello is (0,0,0)
    /// Once it achieves its hover hover, a huge and random offset is applied to the position, which needs to be accounted for
    /// Also difficult to determin when this happens. <see cref="flymode"/> used to work but as of 3.0 it isnt realiable unless you also check for <see cref="flying"/>
    /// And even that isnt great as there is a long delay
    /// </remarks>
    public void CheckForLaunchComplete()
    {
        var deltaRawPosition = _prevRawPosition - rawPosition;
        _prevRawPosition = rawPosition;

        if (flymode == 6 && flying)// || deltaRawPosition.magnitude > 1)
        {
            Debug.Log("launch complete");
            _onFlightStatusChanged.Invoke(IQuadcopter.FlightStatus.Flying);
}
    }

    public bool IsSimulator()
    {
        return false;
    }

    private void OnDestroy()
    {
        if (_isInitialized)
        {
            if (connectionState == Tello.ConnectionState.Connecting)
            {
                //Tello.stopConnecting();
            }
            Tello.onConnection -= Tello_onStateChanged;
            Tello.onUpdate -= Tello_onUpdate;
            Tello.stopConnecting();
        }
   
    }
    


    public Vector3 rawPosition;
    private Vector3 _prevRawPosition;

    public int verticalSpeed { get; private set; }
    public float velY { get; private set; }

    //Tello api, public so they can be seen in inspector
    public bool flying;
    public bool hover;
    public float posUncertainty;
    public bool batteryLow;
    public int batteryPercent;
    public int cameraState;
    public bool downVisualState;
    public int telloBatteryLeft;
    public int telloFlyTimeLeft;
    public int flymode;
    public int flyspeed;
    public int flyTime;
    public bool gravityState;
    public int height;
    public int imuCalibrationState;
    public bool imuState;
    public int lightStrength;
    public bool onGround = true;
    public bool powerState;
    public bool pressureState;
    public int temperatureHeight;
    public int wifiDisturb;
    public int wifiStrength;
    public bool windState;
    public float posX = 0, posY, posZ;
    public float quatW;
    public float quatX;
    public float quatY;
    public float quatZ;
    public float yaw, pitch, roll;
    public Vector3 toEuler;
}
