using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    public class QuadcopterGroundControl : MonoBehaviour
    {
        private enum GroundControlMode
        {
            Tello,
            TelloSim,
            Picopter,
            PicopterSim,
            Ardupilot,
            ArdupilotSim
        }
        [SerializeField] private GroundControlMode _controlMode;
        [SerializeField] private Quadcopter _tello;
        [SerializeField] private Quadcopter _picopter;
        [SerializeField] private Quadcopter _ardupilot;
        public Quadcopter activeQuad { get; private set; }

        [SerializeField] private SimulatedLocalFlightController _simulatedLocalFlightController;
        [SerializeField] private TelloFlightController _telloFlightController;
        [SerializeField] private PiCopterFlightController _picopterFlightController;
        [SerializeField] private PiCopterFlightController _ardupilotFlightController;
        private IFlightController _flightController;


        [SerializeField] private WaypointMission _waypointMission;

        /// <summary>
        /// The autopilot to provide to the <see cref="quadcopter/>
        /// </summary>
        [SerializeField] private WaypointAutoPilot _waypointPilot;

        /// <summary>
        /// The source of the Pilot inputs for this program
        /// </summary>
        [SerializeField] private PilotInputs _pilotInupts;
        // Start is called before the first frame update
        void Start()
        {
            switch (_controlMode)
            {
                case GroundControlMode.Tello:
                    activeQuad = _tello;
                    _flightController = activeQuad.gameObject.GetComponent<TelloFlightController>();
                    break;
                case GroundControlMode.TelloSim:
                    activeQuad = _tello;
                    _flightController = activeQuad.gameObject.GetComponent<SimulatedOnboardFlightController>();
                    break;
                case GroundControlMode.Picopter:
                    activeQuad = _picopter;
                    _flightController = activeQuad.gameObject.GetComponent<PiCopterFlightController>();
                    break;
                case GroundControlMode.PicopterSim:
                    activeQuad = _picopter;
                    _flightController = activeQuad.gameObject.GetComponent<SimulatedLocalFlightController>();
                    break;
                case GroundControlMode.Ardupilot:
                    activeQuad = _ardupilot;
                    _flightController = activeQuad.gameObject.GetComponent<ArduPilotFlightController>();
                    break;
                default:
                    break;
            }

            activeQuad.Initialize(_flightController, _pilotInupts);
            _waypointPilot.Initialize(activeQuad);

        }

        // Update is called once per frame
        void Update()
        {
            var pilotInputs = _pilotInupts.GetInputValues();
           // Debug.Log(pilotInputs)
            if (pilotInputs.toggleAutoPilot)
            {
                _waypointPilot.ToggleAutoPilot();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_waypointMission && _waypointMission.gameObject.activeInHierarchy)
                {
                    _waypointPilot.BeginMission(_waypointMission);
                }
            }
        }
    }
}
