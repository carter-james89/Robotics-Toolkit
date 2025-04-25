using System;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    public class QuadcopterGroundControl : MonoBehaviour
    {
        public enum GroundControlMode
        {
            RemoteQuadcopter,
            SiimulatedQuadcopter,
            Both,
        }
        [SerializeField] private GroundControlMode _controlMode;

        public enum AutoPilotMode
        {
            PID,
            MLAgent,
        }
        [SerializeField] private AutoPilotMode _autoPilotMode;
        public GroundControlMode GetGroundControlMode()
        {
            return _controlMode;
        }
        public void SetGroundControlMode(GroundControlMode mode)
        {
            _controlMode = mode;
            Initialize();
        }
        [SerializeField] private Quadcopter _remoteQuadcopter;
        [SerializeField] private Quadcopter _simulatedQuadcopter;

        private bool _isFlying = false;
        public bool IsFlying()
        {
            return _isFlying;
        }
        private bool _autoPilotActive = false;
        public bool IsAutoPilotActive()
        {
            return _autoPilotActive;
        }


        // [SerializeField] private Quadcopter _picopter;
        //  [SerializeField] private Quadcopter _ardupilot;
        // public Quadcopter activeQuad { get; private set; }

        // private SimulatedOnboardFlightController _simulatedOnboardFlightController;
        // private TelloFlightController _telloFlightController;
        // [SerializeField] private PiCopterFlightController _picopterFlightController;
        // [SerializeField] private PiCopterFlightController _ardupilotFlightController;
        // private IFlightController _flightController;

        private IQuadcopter _quadcopter;
        private IAutoPilot _autoPilot;
        [SerializeField] private WaypointAutoPilotController _waypointMissionController;


        [SerializeField] private WaypointMission _waypointMission;

        /// <summary>
        /// The autopilot to provide to the <see cref="quadcopter/>
        /// </summary>
       // [SerializeField] private WaypointAutoPilotController _telloWaypointPilotController;
       // [SerializeField] private WaypointAutoPilotController _telloSimPilotController;
        /// <summary>
        /// The source of the Pilot inputs for this program
        /// </summary>
        [SerializeField] private PilotInputs _pilotInupts;
        // Start is called before the first frame update
        void Start()
        {
            //_telloFlightController = _remoteQuadcopter.gameObject.GetComponent<TelloFlightController>();
            //_simulatedOnboardFlightController = _simulatedQuadcopter.gameObject.GetComponent<SimulatedOnboardFlightController>();

            //if(_telloFlightController == null)
            //{
            //    Debug.LogError("Tello Flight Controller is null__");
            //    return;
            //}
            //if(_simulatedOnboardFlightController == null)
            //{
            //    Debug.LogError("Simulated Onboard Flight Controller is null__");
            //    return;
            //}

        

            Initialize();
        }
        private void Initialize()
        {
             _autoPilot = null;
            Debug.Log("Initialize ground control in mode : " + _controlMode);
            switch (_controlMode)
            {
                case GroundControlMode.RemoteQuadcopter:
                    _quadcopter = _remoteQuadcopter;
                    foreach (var flightController in _remoteQuadcopter.GetComponents<IFlightController>())
                    {
                        if (!flightController.IsSimulator())
                        {
                            _remoteQuadcopter.Initialize(flightController, _pilotInupts);
                           // _autoPilot = GetAutoPilot(_remoteQuadcopter);
                           // _waypointMissionController.Initialize(_autoPilot, _remoteQuadcopter);//has to happen first
                           // _autoPilot.Initialize(_remoteQuadcopter);
                        }
                    }
                    break;
                case GroundControlMode.SiimulatedQuadcopter:
                    _quadcopter = _simulatedQuadcopter;
                    foreach (var flightController in _simulatedQuadcopter.GetComponents<IFlightController>())
                    {
                        if (flightController.IsSimulator())
                        {
                            _simulatedQuadcopter.Initialize(flightController, _pilotInupts);
                          //  _autoPilot = GetAutoPilot(_simulatedQuadcopter);
                          //  _waypointMissionController.Initialize(_autoPilot, _simulatedQuadcopter);//has to happen first
                          //  _autoPilot.Initialize(_simulatedQuadcopter);
                        }
                    }
                    break;
                case GroundControlMode.Both://not working yet
                    foreach (var flightController in _simulatedQuadcopter.GetComponents<IFlightController>())
                    {
                        if (!flightController.IsSimulator())
                        {
                            _remoteQuadcopter.Initialize(flightController, _pilotInupts);
                          //  _autoPilot = GetAutoPilot(_remoteQuadcopter);
                          //  _waypointMissionController.Initialize(_autoPilot, _remoteQuadcopter);//has to happen first
                          //  _autoPilot.Initialize(_remoteQuadcopter);
                        }
                        else
                        {
                            _simulatedQuadcopter.Initialize(flightController, _pilotInupts);
                          //  _autoPilot = GetAutoPilot(_simulatedQuadcopter);
                          //  _waypointMissionController.Initialize(_autoPilot, _simulatedQuadcopter);//has to happen first
                          //  _autoPilot.Initialize(_simulatedQuadcopter);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    
        private IAutoPilot GetAutoPilot(Quadcopter quad)
        {
            switch (_autoPilotMode)
            {
                case AutoPilotMode.PID:
                    return _waypointMissionController.GetComponent<PIDAutoPilot>();
                case AutoPilotMode.MLAgent:
                    return _waypointMissionController.GetComponent<MLAutoPilot>();
                default:
                    break;
            }
            return null;
        }

        public void TakeOff()
        {
            bool success = false;
            switch (_controlMode)
            {
                case GroundControlMode.RemoteQuadcopter:
                    success = _remoteQuadcopter.AttemptTakeoff();
                    break;
                case GroundControlMode.SiimulatedQuadcopter:
                    success = _simulatedQuadcopter.AttemptTakeoff();
                    break;
                case GroundControlMode.Both:
                    success = _remoteQuadcopter.AttemptTakeoff() && _simulatedQuadcopter.AttemptTakeoff();
                    break;
                default:
                    break;
            }
            if (success)
            {
                Debug.Log("Takeoff success");   
                _isFlying = true;
            }
            else
            {
                Debug.LogError("Takeoff failed");   
            }
        }
        public void Land()
        {
            if (!IsFlying())
            {
                return;
            }
            switch (_controlMode)
            {
                case GroundControlMode.RemoteQuadcopter:
                    _remoteQuadcopter.AttemptLand();
                    break;
                case GroundControlMode.SiimulatedQuadcopter:
                    _simulatedQuadcopter.AttemptLand();
                    break;
                case GroundControlMode.Both:
                    _remoteQuadcopter.AttemptLand();
                    _simulatedQuadcopter.AttemptLand();
                    break;
                default:
                    break;
            }
            _isFlying = false;
        }
        private bool _autoPilotInitialized = false;

        public void ToggleAutoPilot()
        {
            Debug.Log("toggle auto pilot");
            if (!IsFlying())
            {
                Debug.LogError("not flying");   
                return;
            }

            if(!_autoPilotInitialized)
            {
                _autoPilot = GetAutoPilot(_quadcopter as Quadcopter);
                _waypointMissionController.Initialize(_autoPilot, _quadcopter as Quadcopter);//has to happen first
                _autoPilot.Initialize(_quadcopter);
                _autoPilotInitialized = true;
            }






            _autoPilot.ToggleAutoPilot();
            //switch (_controlMode)
            //{
            //    case GroundControlMode.RemoteQuadcopter:
            //        _telloWaypointPilotController.ToggleAutoPilot();

            //        break;
            //    case GroundControlMode.SiimulatedQuadcopter:
            //        _telloSimWaypointPilot.ToggleAutoPilot();
            //        break;
            //    case GroundControlMode.Both:
            //        _telloWaypointPilotController.ToggleAutoPilot();
            //        _telloSimWaypointPilot.ToggleAutoPilot();
            //        break;
            //    case GroundControlMode.Picopter:
            //        break;
            //    case GroundControlMode.PicopterSim:
            //        break;
            //    case GroundControlMode.Ardupilot:
            //        break;
            //    case GroundControlMode.ArdupilotSim:
            //        break;
            //    default:
            //        break;
            //}
            _autoPilotActive = !_autoPilotActive;
        }
        internal void BeginWaypointMission()
        {
            if (!IsFlying())
            {
                return;
            }
            _waypointMissionController.BeginMission(_waypointMission);
            //switch (_controlMode)
            //{
            //    case GroundControlMode.RemoteQuadcopter:
            //        _telloWaypointPilotController.BeginMission(_waypointMission);
            //        break;
            //    case GroundControlMode.SiimulatedQuadcopter:
            //        _telloSimWaypointPilot.BeginMission(_waypointMission);
            //        break;
            //    case GroundControlMode.Both:
            //        _telloWaypointPilotController.BeginMission(_waypointMission);
            //        _telloSimWaypointPilot.BeginMission(_waypointMission);
            //        break;
            //    case GroundControlMode.Picopter:
            //        break;
            //    case GroundControlMode.PicopterSim:
            //        break;
            //    case GroundControlMode.Ardupilot:
            //        break;
            //    case GroundControlMode.ArdupilotSim:
            //        break;
            //    default:
            //        break;
            //}
        }

        // Update is called once per frame
        void Update()
        {
            var pilotInputs = _pilotInupts.GetInputValues();

            if(pilotInputs.takeOff)
            {
                TakeOff();
            }
            if (pilotInputs.land)
            {
                Land();
            }
            // Debug.Log(pilotInputs)
            if (pilotInputs.toggleAutoPilot)
            {
                ToggleAutoPilot();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_waypointMission && _waypointMission.gameObject.activeInHierarchy)
                {
                    BeginWaypointMission();
                }
            }
        }

        public AutoPilotMode GetAutoPilotMode()
        {
            return _autoPilotMode;
        }

        public void SetAutoPilotMode(AutoPilotMode mode)
        {
            _autoPilotMode = mode;
            Initialize(); // reinitialize to apply new autopilot setting
        }
    }
}
