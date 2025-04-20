using System;
using TelloLib;
using TMPro;
using Toolkit.Utilities.Events;
using UnityEngine;
using UnityEngine.XR;

namespace FlightControllers.Quadcopters
{
    public class TelloFlightController : QuadcopterFlightController
    {
        /// <summary>
        /// Video feed to display the camera from the RemoteQuadcopter
        /// </summary>
        [SerializeField]
        private TelloVideoFeed _videoFeed;

        private Vector3 prevRecordedPos;

        [SerializeField] private Transform _positionOffsset;
        private Vector3 _manualOffset = Vector3.zero;


        /// The offset of the tracking values when tracking first achieved after liftoff
        /// </summary>
        /// <remarks>
        /// This is a weird bug either with the <see cref="RemoteQuadcopter"/> library or the RemoteQuadcopter itself
        /// When you take off the position of the RemoteQuadcopter is (0,0,0)
        /// Once it achieves its hover, a huge and random offset is applied to the position, which needs to be accounted for for all 
        /// future positioning data
        /// </remarks>
        private Vector3 _trackingFoundOffset = Vector3.zero;
        /// <summary>
        /// The current connection state with the RemoteQuadcopter, must be <see cref="Tello.ConnectionState.Connected"/> to control
        /// </summary>
        public Tello.ConnectionState connectionState;

        /// <summary>
        /// Is the RemoteQuadcopter tracking accurate this frame?
        /// </summary>
        /// <remarks>
        /// In poor lighting conditions or for no reason at all sometimes the position tracking of the RemoteQuadcopter is way off
        /// The deltaposition from the last valid frame is used to determine if there is an unreasonable jump
        /// </remarks>
        private bool validTrackingFrame;

        /// <summary>
        /// How many packkages have we recieved from the RemoteQuadcopter
        /// </summary>
        [SerializeField]
        private int _telloFrameCount = 0;
        /// <summary>
        /// The last frame recieved updated via <see cref="Tello_onUpdate(int)"/>
        /// </summary>
        private int _lastTelloUpdateFrame;


        public override bool IsReadyToFly()
        {
            if (_isInitialized && Tello.connectionState == Tello.ConnectionState.Connected)
            {
                return true;
            }
            return false;
        }

        public override Quaternion GetGyroRotation()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ConnectToTello();
        }

        /// <summary>
        /// Attempt to connect to the RemoteQuadcopter via <see cref="Tello"/> Library
        /// Must be connected to quadcopter via wifi
        /// </summary>
        public void ConnectToTello()
        {
            Debug.Log("Connecting to Tello");
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
        [SerializeField] private TextMeshProUGUI _connectionStatusText;
        [SerializeField] private TextMeshProUGUI _batteryStatusText;
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private TextMeshProUGUI _posUncertText;
        /// <summary>
        /// Called from <see cref="Tello.onConnection"/> when the state of the connection with the RemoteQuadcopter is changed
        /// </summary>
        private void Tello_onStateChanged(Tello.ConnectionState newState)
        {
            Debug.Log("Tello State Updated : " + newState);
            if (newState == Tello.ConnectionState.Connected)
            {
                _connectionStatusText.text = newState.ToString();
                // Debug.Log("Connected to RemoteQuadcopter, please wait for camera feed " + RemoteQuadcopter.state.);
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
        /// Called from <see cref="Tello.onUpdate"/> when an update a package is recieved from the RemoteQuadcopter
        /// </summary>
        ///<remarks>
        ///<see cref="Tello_onUpdate(int)"/> happens on its own thread, and to interact with unity/inputs we need to use <see cref="Update"/>
        ///This simply records that an update has been recieved from the RemoteQuadcopter, and will be handled in the next <see cref="Update"/>
        /// </remarks>
        private void Tello_onUpdate(int cmdID)
        {
            _telloFrameCount++;
            _lastTelloUpdateFrame = Time.frameCount;
        }

        public void ManuallyCallibrate()
        {
            //var rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            //// If the device is valid, check for the primary button press
            //if (rightHandDevice.isValid)
            //{
            //    bool primaryButtonPressed;
            //    if (rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out primaryButtonPressed) && primaryButtonPressed)
            //    {
            _manualOffset = transform.position - _positionOffsset.position;
            //    }
            //}
        }

        public override QuadcopterData GetSensorData()
        {
            SyncDataWithTello();
            return _quadcopterData;
        }
        /// <summary>
        /// Store all the information from the RemoteQuadcopter package locally
        /// </summary>
        /// <remmarks>
        /// Not all values are guaranteed to work or be accurate, they come from RemoteQuadcopter and <see cref="Tello"/> library
        /// </remmarks>
        public void SyncDataWithTello()
        {
            connectionState = Tello.connectionState;

            var state = Tello.state;
            posX = Tello.state.posY - _trackingFoundOffset.x;
            posY = -Tello.state.posZ - _trackingFoundOffset.y;
            posZ = Tello.state.posX - _trackingFoundOffset.z;
            //posX = RemoteQuadcopter.state.posY - _manualOffset.x;
            //posY = -RemoteQuadcopter.state.posZ - _manualOffset.y;
            //posZ = RemoteQuadcopter.state.posX - _manualOffset.z;


            _quadcopterData.posX = posX;
            _quadcopterData.posY = posY;
            _quadcopterData.posZ = posZ;

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

            yaw = yaw * (180 / Mathf.PI);
            pitch = (pitch * (180 / Mathf.PI));
            roll = roll * (180 / Mathf.PI);

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
            height = state.height * .1f;
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

            _quadcopterData.height = height;
            _quadcopterData.gyroPitch = pitch;
            _quadcopterData.gyroRoll = roll;
            _quadcopterData.gyroYaw = yaw;

            _batteryStatusText.text = batteryPercent + "%";
            _speedText.text = flyspeed.ToString();
            _posUncertText.text = posUncertainty.ToString();


            VallidateTrackingInfo(new Vector3(posX, posY, posZ));
        }

        public override void Run(FlightStatus flightStatus, IInputSource.FlightControlValues craftInputs)
        {
            if (_lastTelloUpdateFrame != Time.frameCount)
            {
                // Debug.Log(flightStatus);
                switch (flightStatus)
                {
                    case FlightStatus.Launching:
                        CheckForLaunchComplete();
                        break;
                    case FlightStatus.Flying:
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
        /// Set the position of the virtual RemoteQuadcopter in the Unity environment
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
        /// Launch the RemoteQuadcopter via its auto liftoff feature
        /// </summary>
        public override bool AttemptTakeoff()
        {

            if (connectionState == Tello.ConnectionState.Connected)
            {
                Debug.Log("Tello takeoff");
                Tello.takeOff();
                NotifyListeners(FlightControllerEventType.OnTakeOffBegin);
                return true;
            }
            else
            {
                Debug.LogWarning("Not connected to tello prior to takeoff command : " + connectionState);
                return false;
            }

        }
        /// <summary>
        /// AttemptLand the RemoteQuadcopter via its auto land feature
        /// </summary>
        public override bool AttemptLand()
        {
            Tello.land();
            NotifyListeners(FlightControllerEventType.OnLandBegin);
            return true;
        }
        /// <summary>
        /// Check to see if the RemoteQuadcopter has finished its auto takeoff
        /// </summary>
        /// <remarks>
        /// This is a weird but either with the <see cref="Tello"/> library or the RemoteQuadcopter itself
        /// When you take off the position of the RemoteQuadcopter is (0,0,0)
        /// Once it achieves its hover hover, a huge and random offset is applied to the position, which needs to be accounted for
        /// Also difficult to determin when this happens. <see cref="flymode"/> used to work but as of 3.0 it isnt realiable unless you also check for <see cref="flying"/>
        /// And even that isnt great as there is a long delay
        /// </remarks>
        public void CheckForLaunchComplete()
        {
            var deltaRawPosition = _prevRawPosition - rawPosition;
            _prevRawPosition = rawPosition;

            // if (flymode == 11)// || deltaRawPosition.magnitude > 1)
            if (flymode == 6 && flying)
            {
                var state = Tello.state;
                posX = Tello.state.posY;
                posY = -Tello.state.posZ;
                posZ = Tello.state.posX;

                _trackingFoundOffset = new Vector3(posX, posY, posZ);
                Debug.Log("launch complete");
                // _trackingFoundOffset = new Vector3(posX, posY, posZ);
                Debug.Log("Set Launch offset to : " + _trackingFoundOffset);

                NotifyListeners(FlightControllerEventType.OnTakeOffEnd);
            }
        }



        public override bool IsSimulator()
        {
            return false;
        }

        private void OnDestroy()
        {
            if (_isInitialized)
            {
                Debug.Log("Stop Tello Connection");
                if (connectionState == Tello.ConnectionState.Connecting)
                {
                    //RemoteQuadcopter.stopConnecting();
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

        //RemoteQuadcopter api, public so they can be seen in inspector
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
        public float height;
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

}