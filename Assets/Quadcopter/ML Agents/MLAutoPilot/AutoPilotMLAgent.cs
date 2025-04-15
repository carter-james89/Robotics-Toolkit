using QuadcopterUtilities;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AutoPilotMLAgent : Agent
{
    //private IAutoPilot _autoPilot;
    //private IQuadcopter _quadcopterToControl;

    private IInputs.FlightControlValues _flightControlValues;

    private bool _firstFlight = true;

    private bool _flying = false;
    [SerializeField]
    private MeshRenderer _groundRenderer;

    [SerializeField]
    private MLAutoPilot _autoPilot;
    [SerializeField]
    private Quadcopter _quadcopterToControl;

    private bool _quadcopterInitialized = false;

    private int _achievedWaypoints = 0;

    private Vector3 _bounds = new Vector3(1, 1, 1);



    [SerializeField]
    private float m_episodeRewards;
    private void Start()
    {
        InitializeQuadcopter();
    }

    private float _atTargetSeconds;
    private float _outOfYawRangeSeconds;

    public void InitializeQuadcopter()
    {
        if (!_quadcopterInitialized)
        {
            Debug.Log("Initialize ML Agent");
            //_autoPilot = autoPilot;
            //_quadcopterToControl = _autoPilot.GetQuadcopterToControl();
            _flightControlValues = new IInputs.FlightControlValues();
            _quadcopterToControl.Initialize(_quadcopterToControl.gameObject.GetComponent<IFlightController>(), GetComponent<PilotInputs>().GetInputValues);
            _autoPilot.Initialize(_quadcopterToControl);
            _quadcopterToControl.onTransformChanged += OnQuadcopterPositioned;
            _quadcopterInitialized = true;
        }

    }
    private void OnQuadcopterPositioned(Vector3 pos, Quaternion rot)
    {
        float dist = 0;
       // CheckForErrors(out dist);
        CalculateRewards(dist);
    }
    public IInputs.FlightControlValues GetFlightControlValues(IAutoPilot autoPilot)
    {
        // Debug.Log("get flight control values");
        RequestDecision();
        return _flightControlValues;// _quadcopterToControl.ConvertToHeadlessInputs(_flightControlValues);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //  Debug.Log("get heuristic");

        var descreteActions = actionsOut.DiscreteActions;
        // descreteActions[0] = ((int)Input.GetAxisRaw("Horizontal")) * 10; //FR
        //descreteActions[1] = (int)Input.GetAxisRaw("Vertical") * 10; //fl
        //descreteActions[2] = (int)Input.GetAxisRaw("Horizontal") * 10; //br
        //  descreteActions[3] = (int)Input.GetAxisRaw("Vertical") * 10; //bl

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {

        }

    }
    public override void OnEpisodeBegin()
    {
        InitializeQuadcopter();
        base.OnEpisodeBegin();

        if (!_firstFlight)
        {
            _autoPilot.DeactivateAutoPilot();
            _quadcopterToControl.Land();

            _flightControlValues.throttle = 0;
            _flightControlValues.pitch = 0;
            _flightControlValues.yaw = 0;
            _flightControlValues.roll = 0;

            _quadcopterToControl.GetGameObject().transform.localPosition = new Vector3(0, 0, 0);
            // _quadcopterToControl.GetGameObject().GetComponent<Rigidbody>().velocity = Vector3.zero;

        }
        _firstFlight = false;
        //if (!_endedEpisodeManually)
        //{
        //    _groundRenderer.material.color = Color.black;
        //}
        //_endedEpisodeManually = false;
        // //Debug.Log("Episode begin ");
        //OnEpisodeBeginEvent.Invoke();
        // flying = false;
        // _episodeSeconds = 0;
        _achievedWaypoints = 0;
        _atTargetSeconds = 0;
        _outOfYawRangeSeconds = 0;

        _flying = false;
        m_inRange = false;

        //_flightControlValues.throttle = 0;
        //_flightControlValues.pitch = 0;
        //_flightControlValues.yaw = 0;
        //_flightControlValues.roll = 0;

        //_quadcopterToControl.GetGameObject().transform.localPosition = new Vector3(0, 0, 0);
        //_quadcopterToControl.GetGameObject().GetComponent<Rigidbody>().velocity = Vector3.zero;

        _quadcopterToControl.transform.localPosition = new Vector3(0, .5f, 0);

        _quadcopterToControl.Takeoff();
        _autoPilot.ActivateAutoPilot();

        SetNewTarget(_autoPilot.GetGameObject().transform);


        // _autoPilot.GetGameObject().transform.localPosition = new Vector3(0, 3, 0);

        //_distReward = 0;
        // _angleReward = 0;

        //_onEpisodeBeginPosition = transform.localPosition;
    }
    private void EndTheEpisode(float reward)
    {
        ////Debug.Log("Angle Quit " + _currentAngle);
        //  _endedEpisodeManually = true;
        //  OnEpisodeEndEvent.Invoke();
        AddReward(reward);
        //_autoPilot.DeactivateAutoPilot();
        //_quadcopterToControl.Land();

        //_flightControlValues.throttle = 0;
        //_flightControlValues.pitch = 0;
        //_flightControlValues.yaw = 0;
        //_flightControlValues.roll = 0;

        //_quadcopterToControl.GetGameObject().transform.localPosition = new Vector3(0, 0, 0);
        //_quadcopterToControl.GetGameObject().GetComponent<Rigidbody>().velocity = Vector3.zero;

        // Debug.Log("end the episode");
        EndEpisode();
    }
    private bool m_inRange;
    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        // 1. Relative position: Clamp or normalize
        Vector3 relativePos = _autoPilot.GetGameObject().transform.InverseTransformPoint(
            _quadcopterToControl.GetGameObject().transform.position);
        sensor.AddObservation(Vector3.ClampMagnitude(relativePos, 1f));  // safer than raw

        // 2. Vertical velocity: Clamp to expected flight range (e.g., -10 to +10 m/s)
        float clampedVerticalVelocity = Mathf.Clamp(_quadcopterToControl.GetSensorData().VelocityVector.y, -10f, 10f) / 10f;
        sensor.AddObservation(clampedVerticalVelocity);

        // 3. Relative yaw angle: Signed and normalized [-1, 1]
        Vector3 forwardTarget = _autoPilot.GetGameObject().transform.forward;
        Vector3 forwardAgent = _quadcopterToControl.GetGameObject().transform.forward;
        float relativeYaw = Vector3.SignedAngle(forwardTarget, forwardAgent, Vector3.up); // -180 to 180
        sensor.AddObservation(relativeYaw / 180f); // normalize to [-1, 1]
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        _flightControlValues.yaw = Mathf.Clamp(actions.ContinuousActions[0], -1, 1);
        if (m_inRange)
        {
            //_flightControlValues.yaw = 0;
            _flightControlValues.throttle = Mathf.Clamp(actions.ContinuousActions[1], -1, 1);

            //_flightControlValues.yaw = Mathf.Clamp(actions.ContinuousActions[1], -1, 1);

            _flightControlValues.pitch = Mathf.Clamp(actions.ContinuousActions[2], -1, 1);

            _flightControlValues.roll = Mathf.Clamp(actions.ContinuousActions[3], -1, 1);
        }
        else
        {
            _flightControlValues.throttle = 0;

            //_flightControlValues.yaw = Mathf.Clamp(actions.ContinuousActions[1], -1, 1);

            _flightControlValues.pitch = 0;

            _flightControlValues.roll = 0;
        }
    }

    private bool m_atTarget = false;
    private void CalculateRewards(float dist)
    {
        float stepReward = 0f;

        dist = (_autoPilot.transform.position - _quadcopterToControl.transform.position).magnitude;
        // Normalize distance into a reward (closer = higher)
        float maxDistance = _bounds.magnitude; // You can also set this manually
       float proximityReward = Mathf.Clamp01(1f - (dist / maxDistance)); // 1 = close, 0 = far
        float proximityPenalty = Mathf.Clamp01(dist / maxDistance);
    
   

        // 1. Compute heading alignment (yaw)
        Vector3 targetForward = _autoPilot.GetGameObject().transform.forward;
        Vector3 agentForward = _quadcopterToControl.GetGameObject().transform.forward;
        float forwardError = Mathf.Abs(Vector3.SignedAngle(targetForward, agentForward, Vector3.up));

       // Debug.Log("Dist : " + dist);
        //Debug.Log("Forward Error : " + forwardError);
        // 2. Check out-of-bounds
        Vector3 quadPosition = _quadcopterToControl.GetGameObject().transform.localPosition;
        if (Mathf.Abs(quadPosition.x) > _bounds.x ||
            Mathf.Abs(quadPosition.y) > _bounds.y ||
            Mathf.Abs(quadPosition.z) > _bounds.z)
        {
            _groundRenderer.material.color = Color.red;
            EndTheEpisode(-5); // No bonus, just reset
            return;
        }

        // 4. Orientation penalty or alignment reward
        if (forwardError < 5f)
        {
            m_inRange = true;
            _outOfYawRangeSeconds = 0f;
            _groundRenderer.material.color = Color.yellow;
            stepReward += 0.01f; // Tune this value higher/lower as needed
        }
        else
        {
            _groundRenderer.material.color = Color.red;
            m_inRange = false;
            _outOfYawRangeSeconds += Time.fixedDeltaTime;
           // stepReward -= 0.1f;
        }

        // 5. Target proximity reward
        if (dist < 0.05f && forwardError < 5f)
        {
            _groundRenderer.material.color = Color.green;
            _atTargetSeconds += Time.fixedDeltaTime;
            m_atTarget = true;
             stepReward += proximityReward * 0.01f; // scale to small per-step reward
            // stepReward += 1f - (dist * 4f); // reward is higher the closer it gets
            // stepReward += 0.01f; // Tune this value higher/lower as needed
            //if (_atTargetSeconds > 3f)
            //{
            //  //  stepReward += 1f; // bonus for holding alignment and proximity
            //    _achievedWaypoints++;

            //    if (_achievedWaypoints >= 1)
            //    {
            //        EndTheEpisode(stepReward);
            //        return;
            //    }
            //    else
            //    {
            //        SetNewTarget(_autoPilot.transform);
            //        _atTargetSeconds = 0;
            //    }
            //}
        }
        else
        {
            // stepReward -= 0.02f; // or -0.015f
            stepReward -= proximityPenalty * 0.01f;
            _atTargetSeconds = 0;
            m_atTarget = false;
            if (forwardError < 10f)
            {
                _groundRenderer.material.color = Color.yellow;
            }
            else
            {
                _groundRenderer.material.color = Color.red;
            }
        }

        // 6. Reward for staying in flight (optional bonus)
        if (_quadcopterToControl.GetGameObject().transform.localPosition.y > 0.2f)
        {
            _flying = true;
          //  stepReward += 0.005f;
        }

        // 7. Final reward application
        AddReward(stepReward);

        m_episodeRewards = GetCumulativeReward();

        // Optional: handle max step timeout
        if (StepCount == MaxStep)
        {
            _groundRenderer.material.color = Color.gray;
            if (m_atTarget)
            {
                EndTheEpisode(5f);
            }
            else
            {
                EndTheEpisode(-5f);
            }
          
        }
    }



    private void SetNewTarget(Transform transformToSet)
    {
        var randomX = UnityEngine.Random.Range(-(_bounds.x - .5f), _bounds.x - .5f);
        var randomy = UnityEngine.Random.Range(1, _bounds.y - .5f);
        var randomZ = UnityEngine.Random.Range(-(_bounds.z - .5f), _bounds.z - .5f);

        _atTargetSeconds = 0;

        transformToSet.localPosition = new Vector3(randomX, randomy, randomZ);
        transformToSet.localEulerAngles += new Vector3(0, UnityEngine.Random.Range(-180, 180));
    }
}
