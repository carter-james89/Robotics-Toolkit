using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    public class AutoPilotMLAgent : Agent
    {
        private IInputSource.FlightControlValues _flightControlValues;
        private bool _firstFlight = true;
        private bool _flying = false;
        private bool _quadcopterInitialized = false;
        private bool m_inRange = false;
        private bool m_atTarget = false;

        [SerializeField] private MeshRenderer _groundRenderer;
        [SerializeField] private MLAutoPilot _autoPilot;
        [SerializeField] private Quadcopter _quadcopterToControl;

        private int _achievedWaypoints = 0;
        private Vector3 _bounds = new Vector3(1, 1, 1);
        private float _atTargetSeconds;
        private float _outOfYawRangeSeconds;
        [SerializeField] private float m_episodeRewards;

        private void Start() => InitializeQuadcopter();

        public void InitializeQuadcopter()
        {
            if (_quadcopterInitialized) return;

            Debug.Log("Initialize ML Agent");
            _flightControlValues = new IInputSource.FlightControlValues();
            _quadcopterToControl.Initialize(_quadcopterToControl.GetComponent<IFlightController>(), GetComponent<PilotInputs>());
            _autoPilot.Initialize(_quadcopterToControl);
            _quadcopterToControl.onTransformChanged += OnQuadcopterPositioned;
            _quadcopterInitialized = true;
        }

        private void OnQuadcopterPositioned(Vector3 pos, Quaternion rot)
        {
            float dist = 0;
            CalculateRewards(dist);
        }

        public IInputSource.FlightControlValues GetFlightControlValues(IAutoPilot autoPilot)
        {
            RequestDecision();
            return _flightControlValues;
        }

        public override void OnEpisodeBegin()
        {
            InitializeQuadcopter();
            base.OnEpisodeBegin();

            if (!_firstFlight)
            {
                _autoPilot.DeactivateAutoPilot();
                _quadcopterToControl.AttemptLand();

                _flightControlValues = new IInputSource.FlightControlValues();
                _quadcopterToControl.transform.localPosition = Vector3.zero;
            }

            _firstFlight = false;
            _achievedWaypoints = 0;
            _atTargetSeconds = 0;
            _outOfYawRangeSeconds = 0;
            _flying = false;
            m_inRange = false;

            _quadcopterToControl.transform.localPosition = new Vector3(0, 0.5f, 0);
            _quadcopterToControl.AttemptTakeoff();
            _autoPilot.ActivateAutoPilot();
            SetNewTarget(_autoPilot.transform);
        }

        public override void Heuristic(in ActionBuffers actionsOut) { }

        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 relativePos = _autoPilot.transform.InverseTransformPoint(_quadcopterToControl.transform.position);
            sensor.AddObservation(Vector3.ClampMagnitude(relativePos, 1f));

            Vector3 vel = _quadcopterToControl.GetSensorData().VelocityVector;
            sensor.AddObservation(Vector3.ClampMagnitude(vel / 10f, 1f).y);

            float relativeYaw = Vector3.SignedAngle(_autoPilot.transform.forward, _quadcopterToControl.transform.forward, Vector3.up);
            sensor.AddObservation(relativeYaw / 180f);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            _flightControlValues.yaw = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);

            if (m_inRange)
            {
                _flightControlValues.throttle = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
                _flightControlValues.pitch = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
                _flightControlValues.roll = Mathf.Clamp(actions.ContinuousActions[3], -1f, 1f);
            }
            else
            {
                _flightControlValues.throttle = 0;
                _flightControlValues.pitch = 0;
                _flightControlValues.roll = 0;
            }
        }

        private void CalculateRewards(float dist)
        {
            float stepReward = 0f;

            Vector3 posAgent = _quadcopterToControl.transform.position;
            Vector3 posTarget = _autoPilot.transform.position;
            dist = Vector3.Distance(posTarget, posAgent);
            float maxDistance = _bounds.magnitude;
            float proximityReward = Mathf.Clamp01(1f - (dist / maxDistance));
            float proximityPenalty = Mathf.Clamp01(dist / maxDistance);

            Vector3 forwardAgent = _quadcopterToControl.transform.forward;
            Vector3 forwardTarget = _autoPilot.transform.forward;
            float forwardError = Mathf.Abs(Vector3.SignedAngle(forwardTarget, forwardAgent, Vector3.up));

            Vector3 quadLocal = _quadcopterToControl.transform.localPosition;
            if (Mathf.Abs(quadLocal.x) > _bounds.x || Mathf.Abs(quadLocal.y) > _bounds.y || Mathf.Abs(quadLocal.z) > _bounds.z)
            {
                _groundRenderer.material.color = Color.red;
                EndTheEpisode(-5f);
                return;
            }

            if (forwardError < 5f)
            {
                m_inRange = true;
                _outOfYawRangeSeconds = 0f;
                _groundRenderer.material.color = Color.yellow;
                stepReward += 0.01f;
            }
            else
            {
                m_inRange = false;
                _outOfYawRangeSeconds += Time.fixedDeltaTime;
            }

            if (dist < 0.03f && forwardError < 5f)
            {
                m_atTarget = true;
                _groundRenderer.material.color = Color.green;
                stepReward += proximityReward * 0.01f;
            }
            else
            {
                m_atTarget = false;
                _atTargetSeconds = 0;
                _groundRenderer.material.color = forwardError < 5f ? Color.yellow : Color.red;
                stepReward -= proximityPenalty * 0.01f;
            }

            if (_quadcopterToControl.transform.localPosition.y > 0.2f)
                _flying = true;

            AddReward(stepReward);
            m_episodeRewards = GetCumulativeReward();

            if (StepCount == MaxStep)
            {
                _groundRenderer.material.color = Color.gray;
                EndTheEpisode(m_atTarget ? 10f : -10f);
            }
        }

        private void EndTheEpisode(float reward)
        {
            AddReward(reward);
            EndEpisode();
        }

        private void SetNewTarget(Transform transformToSet)
        {
            float x = Random.Range(-(_bounds.x - 0.5f), _bounds.x - 0.5f);
            float y = Random.Range(1f, _bounds.y - 0.5f);
            float z = Random.Range(-(_bounds.z - 0.5f), _bounds.z - 0.5f);

            _atTargetSeconds = 0;
            transformToSet.localPosition = new Vector3(x, y, z);
            transformToSet.localEulerAngles += new Vector3(0, Random.Range(-180, 180));
        }
    }
}
