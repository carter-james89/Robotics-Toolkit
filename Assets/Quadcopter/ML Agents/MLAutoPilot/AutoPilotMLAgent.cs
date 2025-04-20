using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Events;

namespace FlightControllers.Quadcopters
{
    public class AutoPilotMLAgent : Agent
    {
        private IInputSource.FlightControlValues _flightControlValues;
        private bool _firstFlight = true;
        private bool _flying = false;
        private bool _quadcopterInitialized = false;
        private bool _inYawRange = false;
        private bool m_atTarget = false;


        [SerializeField] private bool _isTraining = false;
        public bool IsTraining() { return _isTraining; }

        [SerializeField] private MeshRenderer _groundRenderer;
        [SerializeField] private MLAutoPilot _autoPilot;
        [SerializeField] private Quadcopter _quadcopterToControl;

        private int _achievedWaypoints = 0;
        private Vector3 _bounds = new Vector3(2, 1, 2);
        private float _atTargetSeconds;
        private float _outOfYawRangeSeconds;
        [SerializeField] private float m_episodeRewards;

        public UnityEvent OnEpisodeBeginEvent;
        public UnityEvent OnEpisodeCompleteEvent;

        public void Initialize(IAutoPilot autoPilot)
        {
            Debug.Log("Initialize ML Agent");
            base.Initialize();
            _autoPilot = autoPilot as MLAutoPilot;
            _quadcopterToControl = _autoPilot.GetQuadcopterToControl() as Quadcopter;

            _quadcopterToControl.onTransformChanged += OnQuadcopterPositioned;
            _quadcopterInitialized = true;


            if (!_isTraining)
            {
                MaxStep = 0;
                //  _autoPilotAgent.Initialize(this);
            }
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
            base.OnEpisodeBegin();

            if (!_firstFlight)
            {
                EndTheEpisode(m_atTarget ? 10f : -10f);
            }
        

            _firstFlight = false;
            _achievedWaypoints = 0;
            _atTargetSeconds = 0;
            _outOfYawRangeSeconds = 0;
            _flying = false;
            _inYawRange = false;

            OnEpisodeBeginEvent?.Invoke();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            if (_autoPilot == null)
            {
                // Debug.LogError("AutoPilot is null");
                return;
            }
            Vector3 relativePos = _autoPilot.transform.InverseTransformPoint(_quadcopterToControl.transform.position);
            sensor.AddObservation(Vector3.ClampMagnitude(relativePos, 1f));

            Vector3 vel = _quadcopterToControl.GetSensorData().AngularVelocityVector;
            sensor.AddObservation(Vector3.ClampMagnitude(vel / 10f, 1f).y);

            sensor.AddObservation(Vector3.ClampMagnitude(_quadcopterToControl.GetSensorData().LinearVelocityVector / 10f, 1f));

            float relativeYaw = Vector3.SignedAngle(_autoPilot.transform.forward, _quadcopterToControl.transform.forward, Vector3.up);
            sensor.AddObservation(relativeYaw / 180f);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            _flightControlValues.yaw = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);

            //   Debug.Log("ML Action Receieved);");

            if (_inYawRange)
            {
                //   Debug.Log("apply values");
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
         //   Debug.Log("Calculate Rewards");
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

            if (_isTraining)
            {
                Vector3 quadLocal = _quadcopterToControl.transform.localPosition;
                if (Mathf.Abs(quadLocal.x) > _bounds.x || Mathf.Abs(quadLocal.y) > _bounds.y || Mathf.Abs(quadLocal.z) > _bounds.z)
                {
                    _groundRenderer.material.color = Color.red;
                    EndTheEpisode(-5f);
                    return;
                }
            }

            if (forwardError < 5f)
            {
                _inYawRange = true;
                _outOfYawRangeSeconds = 0f;
                _groundRenderer.material.color = Color.yellow;
                stepReward += 0.01f;
            }
            else
            {
                _inYawRange = false;
                _outOfYawRangeSeconds += Time.fixedDeltaTime;
            }



            if (dist < 0.03f && forwardError < 5f)
            {
                m_atTarget = true;
                _groundRenderer.material.color = Color.green;
                // stepReward += proximityReward * 0.01f;

                // Reward for being stable (velocity near zero)
                float speed = _quadcopterToControl.GetSensorData().LinearVelocityVector.magnitude;
                float stabilityReward = Mathf.Clamp01(1f - speed / 0.5f); // full reward if speed ~0, fades out at 0.5 m/s
                stepReward += stabilityReward * 0.01f;
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

            //if (StepCount == MaxStep )
            //{
            //    _groundRenderer.material.color = Color.gray;
            //    EndTheEpisode(m_atTarget ? 10f : -10f);
            //}
        }

        

        private void EndTheEpisode(float reward)
        {
            Debug.Log("End Episode");
            //AddReward(reward);
            //EndEpisode();
            _flightControlValues = new IInputSource.FlightControlValues();
            OnEpisodeCompleteEvent?.Invoke();
        }

        public void SetNewTarget(Transform transformToSet)
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
