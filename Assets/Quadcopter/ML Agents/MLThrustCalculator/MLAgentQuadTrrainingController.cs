using UnityEngine;

namespace FlightControllers.Quadcopters
{
    public class MLAgentQuadTrrainingController : MonoBehaviour
    {
        /// <summary>
        /// The source of the Pilot inputs for this program
        /// </summary>
        [SerializeField] private PilotInputs _pilotInupts;

        private IQuadcopter _quadcopter;
        private IFlightController _simulatedOnBoardFlightController;
        private MLAgentMotorThrustCalculator _motorThrustCalculator;

        [SerializeField]
        private TMPro.TextMeshPro _angleText;

        /// <summary>
        /// The autopilot to provide to the <see cref="quadcopter/>
        /// </summary>
        [SerializeField] private WaypointAutoPilot _waypointPilot;
        [SerializeField] private WaypointMission _waypointMission;

        private void Awake()
        {
            _quadcopter = GetComponentInChildren<IQuadcopter>();
            _simulatedOnBoardFlightController = GetComponentInChildren<IFlightController>();
            _motorThrustCalculator = GetComponentInChildren<MLAgentMotorThrustCalculator>();

            _motorThrustCalculator.OnEpisodeBeginEvent.AddListener(OnEpisodeBegin);
            _motorThrustCalculator.OnEpisodeEndEvent.AddListener(OnEpisodeEnd);

            _quadcopter.Initialize(_simulatedOnBoardFlightController, _pilotInupts);


        }

        private void Start()
        {
            _waypointPilot.Initialize(_quadcopter);
            // _waypointPilot.ToggleAutoPilot();
        }

        private void OnEpisodeEnd()
        {
            if (_motorThrustCalculator.UseTrainer())
            {
                _waypointPilot.DeactivateAutoPilot();
            }

            //_waypointPilot.EndMission();
            _angleText.text = _motorThrustCalculator.GetCurrentAngle().ToString();
            _quadcopter.AttemptLand();
        }

        private void OnEpisodeBegin()
        {
            // Debug.Log("Episode Begin at Training Controller");
            _quadcopter.GetGameObject().transform.localPosition = Vector3.zero;
            _quadcopter.GetGameObject().transform.localEulerAngles = Vector3.zero;
            _quadcopter.GetGameObject().GetComponent<Rigidbody>().velocity = Vector3.zero;
            _quadcopter.GetGameObject().GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            _quadcopter.AttemptTakeoff();

            if (_motorThrustCalculator.UseTrainer())
            {
                _waypointPilot.ActivateAutoPilot();
                _waypointPilot.BeginMission(_waypointMission);
            }


        }



        // Update is called once per frame
        void Update()
        {
            var pilotInputs = _pilotInupts.GetInputValues();
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