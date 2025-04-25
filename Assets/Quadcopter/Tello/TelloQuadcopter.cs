using System;
using TelloLib;
using UnityEngine;


namespace  FlightControllers.Quadcopters
{
    /// <summary>
    /// Quadcopter class to control the real world DJI RemoteQuadcopter
    /// Uses the <see cref="Tello"/> library to send and recieve data from the craft
    /// </summary>
    public class TelloQuadcopter : Quadcopter
    {
        /// <summary>
        /// Video feed to display the camera from the RemoteQuadcopter
        /// </summary>
        [SerializeField]private TelloVideoFeed _videoFeed;

        /// <summary>
        /// Is the RemoteQuadcopter tracking accurate this frame?
        /// </summary>
        /// <remarks>
        /// In poor lighting conditions or for no reason at all sometimes the position tracking of the RemoteQuadcopter is way off
        /// The deltaposition from the last valid frame is used to determine if there is an unreasonable jump
        /// </remarks>
        private bool validTrackingFrame;

        /// <summary>
        /// The current connection state with the RemoteQuadcopter, must be <see cref="Tello.ConnectionState.Connected"/> to control
        /// </summary>
        public Tello.ConnectionState connectionState;

        /// <summary>
        /// The offset of the tracking values when tracking first achieved after liftoff
        /// </summary>
        /// <remarks>
        /// This is a weird bug either with the <see cref="Tello"/> library or the RemoteQuadcopter itself
        /// When you take off the position of the RemoteQuadcopter is (0,0,0)
        /// Once it achieves its hover, a huge and random offset is applied to the position, which needs to be accounted for for all 
        /// future positioning data
        /// </remarks>
        private Vector3 _trackingFoundOffset = Vector3.zero;
        /// <summary>
        /// The offset of elevation when tracking first achieved
        /// </summary>
        private float _elevationOffset;

        /// <summary>
        /// Static position of the takeoff position, set at takeoff
        /// </summary>
        [SerializeField]
        private Transform _takeOffGround;
        /// <summary>
        /// Quad representation of where the RemoteQuadcopter sensor thinks the ground is, set continuously
        /// </summary>
        [SerializeField]
        private Transform _sensorGround;

        private bool validTrackedFrame;

        /// <summary>
        /// Initialize the autopilot, and provide the depenencies it needs. 
        /// </summary>
        /// <param name="pilotInputs">Where to find the inputs from the pilot</param>
        /// <param name="autoPilot">The autopilot module used, activated via <see cref="ActivateAutoPilot"/></param>
        //public override void Initialize(Func<IInputSource.FlightControlValues> defaultInputSource)
        //{
        //    base.Initialize(defaultInputSource);
     
        //}




        private void Update()
        {
            //if (_lastTelloUpdateFrame != Time.frameCount)
            //{
            //    SyncDataWithTello();
            //    SetSensorGround();

            //    switch (_flightStatus)
            //    {
            //        case FlightStatus.Launching:
            //            CheckForLaunchComplete();
            //            break;
            //        case FlightStatus.Flying:
            //            try
            //            {
            //                validTrackedFrame = SetVirtualTelloPosition();
            //            }
            //            catch (Exception e)
            //            {
            //                Debug.Log(e + " : Emergency Abort");
            //                abort?.Invoke();                          
            //            }
            //            break;
            //    }
            //    ProcessInputs(); //need to run in update to get Input Values for this frame
            //    SendTelloInputs();
            //}
        }
        /// <summary>
        /// Sent the Inputs from either the <see cref="PilotInputs"/> or <see cref="IAutoPilot"/> to the RemoteQuadcopter
        /// </summary>
        public void SendTelloInputs()
        {
            if (_flightStatus != FlightStatus.PreLaunch)
            {
                Tello.controllerState.setAxis(currentInputs.yaw, currentInputs.throttle, currentInputs.roll, currentInputs.pitch);
            }
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

            if (flymode == 6 && flying)// || deltaRawPosition.magnitude > 1)
            {
                Debug.Log("launch complete");
                _trackingFoundOffset = new Vector3(posX, posY, posZ);

                SetSensorGround();
                if (_takeOffGround)
                {
                   // _takeOffGround.transform.position = transform.position - new Vector3(0, height * .1f, 0);
                }
                SetVirtualTelloPosition();

                _elevationOffset = height * .1f;
                SetHomePoint(new Vector3(0, height * .1f, 0));
                _flightStatus = FlightStatus.Flying;
            }
        }
        /// <summary>
        /// Set the <see cref="sensorGround"/> position to viusalize where the RemoteQuadcopter thinks the ground is
        /// </summary>
        public void SetSensorGround()
        {
            if (_sensorGround)
            {
                _sensorGround.position = transform.position - new Vector3(0, height * .1f, 0);
            }
        }

        /// <summary>
        /// Get the current position of the RemoteQuadcopter, taking into account the <see cref="_trackingFoundOffset"/> described in <see cref="CheckForLaunchComplete"/>
        /// </summary>
        /// <returns>The Global Postion of the RemoteQuadcopter</returns>
        public Vector3 GetCurrentPos()
        {
           // return new Vector3(posX - _trackingErrorOffset.x, posY - _trackingErrorOffset.y + _elevationOffset, posZ - _trackingErrorOffset.z);
            return new Vector3(posX - _trackingFoundOffset.x, height * .1f, posZ - _trackingFoundOffset.z);
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
        }

        public Vector3 adjustedPosition;
        public Vector3 rawPosition;
        private Vector3 _prevRawPosition;

        public int verticalSpeed { get; private set; }
        public float velY { get; private set; }

        /// <summary>
        /// Set the position of the virtual RemoteQuadcopter in the Unity environment
        /// </summary>
        /// <returns>Is this an accurate frame, <see cref="validTrackingFrame"/> </returns>
        public bool SetVirtualTelloPosition()
        {
            validTrackingFrame = true;
            adjustedPosition = GetCurrentPos();
            Vector3 dif = adjustedPosition - transform.position;
            var xDif = dif.x;
            var yDif = dif.y;
            var zDif = dif.z;

            //valid tello frame
            if (Mathf.Abs(xDif) < 2 & Mathf.Abs(yDif) < 2 & Mathf.Abs(zDif) < 2)
            {
                yaw = yaw * (180 / Mathf.PI);
                pitch = (pitch * (180 / Mathf.PI));
                roll = roll * (180 / Mathf.PI);
                //SetVirtualPosition(new Vector2(adjustedPosition.x, adjustedPosition.z), height * .1f, Quaternion.Euler(new Vector3(-pitch, yaw, roll)),vertSpeed);
              //  transform.position = adjustedPosition;
               // transform.position += new Vector3(0, _elevationOffset, 0);
                //yaw = yaw * (180 / Mathf.PI);
                //pitch = (pitch * (180 / Mathf.PI));
                //roll = roll * (180 / Mathf.PI);
                //transform.localEulerAngles = new Vector3(-pitch, yaw, roll);

                OnTransformUpdated();
            }
            else
            {
               // Debug.Log("Tracking lost " + _telloFrameCount);
                validTrackingFrame = false;
            }
            return validTrackingFrame;
        }

        /// <summary>
        /// Launch the RemoteQuadcopter via its auto liftoff feature
        /// </summary>
        public override bool AttemptTakeoff()
        {
            if (connectionState == Tello.ConnectionState.Connected)
            {
                Debug.Log("TakeOff");
                Tello.takeOff();
                _flightStatus = FlightStatus.Launching;
                _prevRawPosition = rawPosition;
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
            if (connectionState == Tello.ConnectionState.Connected)
            {
                Debug.Log("Land");
                Tello.land();
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                _flightStatus = FlightStatus.Landing;
                return true;
            }
            else
            {
                Debug.LogWarning("Not connected to Tello at time of land command : " + connectionState);
                return false;
            }
        }

 

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(connectionState == Tello.ConnectionState.Connecting)
            {
                //RemoteQuadcopter.stopConnecting();
            }
          //  RemoteQuadcopter.onConnection -= Tello_onStateChanged;
           // RemoteQuadcopter.onUpdate -= Tello_onUpdate;
            //if(connectionState == RemoteQuadcopter.ConnectionState.Connected)
            //{
            //   RemoteQuadcopter.sto
            //}
            Tello.stopConnecting();
        }



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
}
