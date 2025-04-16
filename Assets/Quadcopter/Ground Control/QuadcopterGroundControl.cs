using System;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    public class QuadcopterGroundControl : MonoBehaviour
    {
        public enum GroundControlMode
        {
            Tello,
            TelloSim,
            TelloSimBoth,
            Picopter,
            PicopterSim,
            Ardupilot,
            ArdupilotSim
        }
        [SerializeField] private GroundControlMode _controlMode;
        public GroundControlMode GetGroundControlMode()
        {
            return _controlMode;
        }
        public void SetGroundControlMode(GroundControlMode mode)
        {
            _controlMode = mode;
            Initialize();
        }
        [SerializeField] private Quadcopter _tello;
        [SerializeField] private Quadcopter _telloSim;

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

        private SimulatedOnboardFlightController _simulatedOnboardFlightController;
        private TelloFlightController _telloFlightController;
        // [SerializeField] private PiCopterFlightController _picopterFlightController;
        // [SerializeField] private PiCopterFlightController _ardupilotFlightController;
        // private IFlightController _flightController;


        [SerializeField] private WaypointMission _waypointMission;

        /// <summary>
        /// The autopilot to provide to the <see cref="quadcopter/>
        /// </summary>
        [SerializeField] private WaypointAutoPilot _telloWaypointPilot;
        /// <summary>
        /// The autopilot to provide to the <see cref="quadcopter/>
        /// </summary>
        [SerializeField] private WaypointAutoPilot _telloSimWaypointPilot;

        /// <summary>
        /// The source of the Pilot inputs for this program
        /// </summary>
        [SerializeField] private PilotInputs _pilotInupts;
        // Start is called before the first frame update
        void Start()
        {
            _telloFlightController = _tello.gameObject.GetComponent<TelloFlightController>();
            _simulatedOnboardFlightController = _telloSim.gameObject.GetComponent<SimulatedOnboardFlightController>();

            if(_telloFlightController == null)
            {
                Debug.LogError("Tello Flight Controller is null__");
                return;
            }
            if(_simulatedOnboardFlightController == null)
            {
                Debug.LogError("Simulated Onboard Flight Controller is null__");
                return;
            }

            Debug.Log(_telloFlightController.name);

            Initialize();
        }
        private void Initialize()
        {
            switch (_controlMode)
            {
                case GroundControlMode.Tello:
                    _tello.Initialize(_telloFlightController, _pilotInupts);
                    _telloWaypointPilot.Initialize(_tello);
                    break;
                case GroundControlMode.TelloSim:
                    _telloSim.Initialize(_simulatedOnboardFlightController, _pilotInupts);
                    _telloSimWaypointPilot.Initialize(_telloSim);
                    break;
                case GroundControlMode.TelloSimBoth:
                    _tello.Initialize(_telloFlightController, _pilotInupts);
                    _telloSim.Initialize(_simulatedOnboardFlightController, _pilotInupts);

                    _telloWaypointPilot.Initialize(_tello);
                    _telloSimWaypointPilot.Initialize(_telloSim);
                    break;
                case GroundControlMode.Picopter:
                    break;
                case GroundControlMode.PicopterSim:
                    break;
                case GroundControlMode.Ardupilot:
                    break;
                case GroundControlMode.ArdupilotSim:
                    break;
                default:
                    break;
            }
        }

        public void TakeOff()
        {
            bool success = false;
            switch (_controlMode)
            {
                case GroundControlMode.Tello:
                    success = _tello.AttemptTakeoff();
                    break;
                case GroundControlMode.TelloSim:
                    success = _telloSim.AttemptTakeoff();
                    break;
                case GroundControlMode.TelloSimBoth:
                    success = _tello.AttemptTakeoff() && _telloSim.AttemptTakeoff();
                    break;
                case GroundControlMode.Picopter:
                    break;
                case GroundControlMode.PicopterSim:
                    break;
                case GroundControlMode.Ardupilot:
                    break;
                case GroundControlMode.ArdupilotSim:
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
                case GroundControlMode.Tello:
                    _tello.AttemptLand();
                    break;
                case GroundControlMode.TelloSim:
                    _telloSim.AttemptLand();
                    break;
                case GroundControlMode.TelloSimBoth:
                    _tello.AttemptLand();
                    _telloSim.AttemptLand();
                    break;
                case GroundControlMode.Picopter:
                    break;
                case GroundControlMode.PicopterSim:
                    break;
                case GroundControlMode.Ardupilot:
                    break;
                case GroundControlMode.ArdupilotSim:
                    break;
                default:
                    break;
            }
            _isFlying = false;
        }
        public void ToggleAutoPilot()
        {
            Debug.Log("toggle auto pilot");
            if (!IsFlying())
            {
                Debug.LogError("not flying");   
                return;
            }
            switch (_controlMode)
            {
                case GroundControlMode.Tello:
                    _telloWaypointPilot.ToggleAutoPilot();
                  
                    break;
                case GroundControlMode.TelloSim:
                    _telloSimWaypointPilot.ToggleAutoPilot();
                    break;
                case GroundControlMode.TelloSimBoth:
                    _telloWaypointPilot.ToggleAutoPilot();
                    _telloSimWaypointPilot.ToggleAutoPilot();
                    break;
                case GroundControlMode.Picopter:
                    break;
                case GroundControlMode.PicopterSim:
                    break;
                case GroundControlMode.Ardupilot:
                    break;
                case GroundControlMode.ArdupilotSim:
                    break;
                default:
                    break;
            }
            _autoPilotActive = !_autoPilotActive;
        }
        internal void BeginWaypointMission()
        {
            if (!IsFlying())
            {
                return;
            }
            switch (_controlMode)
            {
                case GroundControlMode.Tello:
                    _telloWaypointPilot.BeginMission(_waypointMission);
                    break;
                case GroundControlMode.TelloSim:
                    _telloSimWaypointPilot.BeginMission(_waypointMission);
                    break;
                case GroundControlMode.TelloSimBoth:
                    _telloWaypointPilot.BeginMission(_waypointMission);
                    _telloSimWaypointPilot.BeginMission(_waypointMission);
                    break;
                case GroundControlMode.Picopter:
                    break;
                case GroundControlMode.PicopterSim:
                    break;
                case GroundControlMode.Ardupilot:
                    break;
                case GroundControlMode.ArdupilotSim:
                    break;
                default:
                    break;
            }
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


    }
}
