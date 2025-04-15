using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Events;

namespace FlightControllers.Quadcopters
{
    public class MLAgentMotorThrustCalculator : Agent, IMotorThrustCalculator
    {
        public UnityEvent OnEpisodeBeginEvent = new UnityEvent();
        public UnityEvent OnEpisodeEndEvent = new UnityEvent();

        [SerializeField]
        private Transform _targetTransform;

        [SerializeField]
        private bool _useTrainer;
        public bool UseTrainer()
        {
            return _useTrainer;
        }
        private IMotorThrustCalculator _thrustCalculatorTrainer;

        [SerializeField]
        private MeshRenderer _groundRenderer;

        private Vector3 bounds;

        private bool _endedEpisodeManually = false;

        private float _targetLoiterHeight = 1;

        private Vector3 _onEpisodeBeginPosition;

        [SerializeField]
        private TMPro.TextMeshPro _debugText;
        [SerializeField]
        private TMPro.TextMeshPro _fixedTimeText;

        [SerializeField]
        private TMPro.TextMeshPro _distRewardText;
        [SerializeField]
        private TMPro.TextMeshPro _angleRewardText;

        private float _distReward;
        private float _angleReward;

        [SerializeField]
        private bool _disableRenderers;

        private int _achievedWaypoints = 0;
        private int _waypointGoal = 2;

        private Rigidbody rigidbody;
        private void Awake()
        {
            var groundSize = transform.InverseTransformVector(_groundRenderer.bounds.size);
            bounds = new Vector3(groundSize.x, groundSize.x, groundSize.z);
            rigidbody = GetComponent<Rigidbody>();

            if (_useTrainer)
            {
                _thrustCalculatorTrainer = new MotorThrustCalculator();
                //_thrustCalculatorTrainer.Initialize(0);
            }

            if (_disableRenderers)
            {
                foreach (var item in GetComponentsInChildren<MeshRenderer>())
                {
                    item.enabled = false;
                }
            }
        }


        #region IMotorThrustCalculator
        public void Initialize(float currentHeading)
        {
            if (_thrustCalculatorTrainer != null)
            {
                _thrustCalculatorTrainer.Initialize(currentHeading);
                _trainerValues = new IMotorThrustCalculator.MotorThrustValues();
            }
            _atTargetSeconds = 0;
        }
        IMotorThrustCalculator.MotorThrustValues _trainerValues;
        public IMotorThrustCalculator.MotorThrustValues Run(Vector3 currentPos, Vector3 currentEuler, IInputs.FlightControlValues inputs)
        {
            if (_thrustCalculatorTrainer != null)
            {
                _trainerValues = _thrustCalculatorTrainer.Run(currentPos, currentEuler, inputs);
                // Debug.Log(_trainerValues.motorBL);
                RequestDecision();
                return _trainerValues;
            }
            else
            {
                //Debug.Log("Run Motor Thrust calculator");
                //Debug.Log("Request Decision");
                RequestDecision();

                return _motorThrustValues;
            }

        }

        public void SetAltitudeHold(float newHoldHeight)
        {
            _targetLoiterHeight = newHoldHeight;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            //Debug.Log("collect observations");
            base.CollectObservations(sensor);

            //sensor.AddObservation(_targetTransform.InverseTransformPoint(transform.position).y);
            //sensor.AddObservation(transform.localEulerAngles.x);
            //sensor.AddObservation(transform.localEulerAngles.z
            //sensor.AddObservation(_targetTransform.transform.localEulerAngles.x);
            //sensor.AddObservation(_targetTransform.transform.localEulerAngles.z);

            //sensor.AddObservation(transform.localPosition);
            sensor.AddObservation(_targetTransform.localPosition - transform.localPosition);
            // sensor.AddObservation(rigidbody.velocity);
            // sensor.AddObservation(rigidbody)
            //  sensor.AddObservation(rigidbody.angularVelocity);
            //sensor.AddObservation(_targetTransform.localEulerAngles - transform.localEulerAngles);
            sensor.AddObservation(_targetTransform.localEulerAngles - transform.localEulerAngles);
            //  sensor.AddObservation(_atTargetSeconds);
        }
        private IMotorThrustCalculator.MotorThrustValues _motorThrustValues;
        public override void OnActionReceived(ActionBuffers actions)
        {
            //Debug.Log("on action recieved");
            _motorThrustValues.motorFR = actions.DiscreteActions[0] * .1f;
            _motorThrustValues.motorFL = actions.DiscreteActions[1] * .1f;
            _motorThrustValues.motorBR = actions.DiscreteActions[2] * .1f;
            _motorThrustValues.motorBL = actions.DiscreteActions[3] * .1f;

            //CalculateRewards1();
            CalculateSimpleRewards();
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();

            if (!_endedEpisodeManually)
            {
                _groundRenderer.material.color = Color.black;
            }
            _endedEpisodeManually = false;
            // //Debug.Log("Episode begin ");
            OnEpisodeBeginEvent.Invoke();
            flying = false;
            _episodeSeconds = 0;
            _achievedWaypoints = 0;

            _motorThrustValues = new IMotorThrustCalculator.MotorThrustValues();
            _motorThrustValues.motorFR = 0;
            _motorThrustValues.motorFL = 0;
            _motorThrustValues.motorBR = 0;
            _motorThrustValues.motorBL = 0;

            _targetTransform.localPosition = new Vector3(0, 3, 0);
            SetNewTarget();
            transform.localPosition = new Vector3(0, 0, 0);

            _distReward = 0;
            _angleReward = 0;

            _onEpisodeBeginPosition = transform.localPosition;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            //  Debug.Log("get heuristic");

            var descreteActions = actionsOut.DiscreteActions;
            //descreteActions[0] = ((int)Input.GetAxisRaw("Horizontal")) * 10; //FR
            //descreteActions[1] = (int)Input.GetAxisRaw("Vertical") * 10; //fl
            //descreteActions[2] = (int)Input.GetAxisRaw("Horizontal") * 10; //br
            //descreteActions[3] = (int)Input.GetAxisRaw("Vertical") * 10; //bl

            descreteActions[0] = (int)_trainerValues.motorFR * 10;
            descreteActions[1] = (int)_trainerValues.motorFL * 10;
            descreteActions[2] = (int)_trainerValues.motorBR * 10;
            descreteActions[3] = (int)_trainerValues.motorBL * 10;
        }

        #endregion

        public float GetCurrentAngle()
        {
            return _currentAngle;
        }
        [SerializeField] private float _angleError = 50;
        private float _currentAngle = 0;

        private float _episodeSeconds = 0;
        private float _atTargetSeconds = 0;

        bool flying = true;

        private void CalculateSimpleRewards()
        {
            float angle = 0;
            float dist = 0;
            CheckForErrors(out dist, out angle);

            if (flying)
            {
                //AddReward(.01f);
            }
            // AddReward(-1f / MaxStep);
            //  Debug.Log(dist);
            var forwardAngle = Vector3.Angle(transform.forward, _targetTransform.forward);
            if (dist < .2 && forwardAngle < 10)
            {
                _atTargetSeconds += Time.fixedDeltaTime;
                AddReward(.1f);
                if (_atTargetSeconds > 5)
                {
                    AddReward(5);
                    _achievedWaypoints++;
                    _groundRenderer.material.color = Color.green;
                    if (_achievedWaypoints < _waypointGoal)
                    {
                        SetNewTarget();
                    }
                    else
                    {
                        EndTheEpisode(0);
                    }


                }
                // EndTheEpisode(1);
            }
            else
            {
                _atTargetSeconds = 0;
            }

            if (StepCount == MaxStep)
            {
                EndTheEpisode(0);
            }
        }

        private void CheckForErrors(out float dist, out float angle)
        {
            angle = Vector3.Angle(Vector3.up, transform.up);
            dist = Vector3.Distance(transform.position, _targetTransform.position);

            //if (StepCount > 50 && rigidbody.velocity.magnitude < .01f)
            //{
            //    ////Debug.Log("TimeOut ");
            //    _groundRenderer.material.color = Color.green;
            //    EndTheEpisode(-1);
            //    return;
            //}

            if (transform.localPosition.y > .1f)
            {
                flying = true;
            }
            if (flying)
            {
                if (transform.localPosition.y < .1f)
                {
                    _groundRenderer.material.color = Color.blue;
                    EndTheEpisode(-5);
                }
            }

            if (angle > _angleError)
            {
                // //Debug.Log("flip out ");
                _groundRenderer.material.color = Color.yellow;
                EndTheEpisode(-5);
            }
            if ((Mathf.Abs(transform.localPosition.x) > 2) ||
          (Mathf.Abs(transform.localPosition.y) > 5) ||
          (Mathf.Abs(transform.localPosition.z) > 2))
            {
                _groundRenderer.material.color = Color.red;
                EndTheEpisode(-5);

            }

        }



        private void SetNewTarget()
        {
            var randomX = UnityEngine.Random.Range(-1.5f, 1.5f);
            var randomy = UnityEngine.Random.Range(1, 3.5f);
            var randomZ = UnityEngine.Random.Range(-1.5f, 1.5f);

            _atTargetSeconds = 0;

            _targetTransform.localPosition = new Vector3(randomX, randomy, randomZ);
            _targetTransform.localEulerAngles += new Vector3(0, UnityEngine.Random.Range(-180, 180));
        }


        private void EndTheEpisode(float reward)
        {
            ////Debug.Log("Angle Quit " + _currentAngle);
            _endedEpisodeManually = true;
            OnEpisodeEndEvent.Invoke();
            AddReward(reward);
            EndEpisode();
        }
    }


}