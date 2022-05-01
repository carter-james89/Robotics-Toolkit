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
    private void Awake()
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
        CheckForErrors(out dist);
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
        //Debug.Log("collect observations");
        base.CollectObservations(sensor);
        //Debug.Log("Collect observations");

        // sensor.AddObservation(StepCount);
        //sensor.AddObservation(Vector3.Distance(_autoPilot.GetGameObject().transform.position, _quadcopterToControl.GetGameObject().transform.position));
        //sensor.AddObservation(transform.localPosition);
        //  sensor.AddObservation(_autoPilot.GetGameObject().transform.localPosition - _quadcopterToControl.GetGameObject().transform.localPosition);
        sensor.AddObservation(_autoPilot.GetGameObject().transform.InverseTransformPoint(_quadcopterToControl.GetGameObject().transform.position));
        sensor.AddObservation(_quadcopterToControl.GetSensorData().VelocityVector.y);
        //  sensor.AddObservation(rigidbody.angularVelocity);
        //sensor.AddObservation(_targetTransform.localEulerAngles - transform.localEulerAngles);
        sensor.AddObservation(_autoPilot.GetGameObject().transform.localEulerAngles.y - _quadcopterToControl.GetGameObject().transform.localEulerAngles.y);
        //  sensor.AddObservation(_atTargetSeconds);

    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Debug.Log("on action recieved");
        //_flightControlValues.pitch = ConvertInput(actions.DiscreteActions[0]) * .1f;
        //_flightControlValues.yaw = ConvertInput(actions.DiscreteActions[1]) * .1f;
        //_flightControlValues.roll = ConvertInput(actions.DiscreteActions[2]) * .1f;
        //_flightControlValues.throttle = ConvertInput(actions.DiscreteActions[3]) * .1f;

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



        //Debug.Log(actions.DiscreteActions[1]);

        //CalculateRewards1();

    }
    private float ConvertInput(float input)
    {
        //if(input >= 10)
        //{
        return input - 10;
        //}
        //else
        //{
        //    return -(input-10);
        //}
    }

    private void CalculateRewards(float dist)
    {
        if (_quadcopterToControl.GetGameObject().transform.localPosition.y < .1f)
        {
            _flying = true;
            // AddReward(-.1f);
        }
        //if(_flying && _quadcopterToControl.GetGameObject().transform.localPosition.y <= 0&& _flying)
        //{
        //    _groundRenderer.material.color = Color.blue;
        //    // EndTheEpisode(-5);
        //    AddReward(-1);
        //    return;
        //}
        //if(StepCount > 10 && !_flying)
        //{
        //    _groundRenderer.material.color = Color.black;
        //    AddReward(-1);
        //    return;
        //}
        if (_flying)
        {
            // AddReward(.01f);// _quadcopterToControl.GetGameObject().transform.localPosition.y);
            //AddReward(-dist);
        }
        // var forwardAngle = Vector3.Angle(_autoPilot.GetGameObject().transform.forward, _quadcopterToControl.GetGameObject().transform.forward);

        var forwardError = Mathf.Abs(_autoPilot.GetGameObject().transform.localEulerAngles.y - _quadcopterToControl.GetGameObject().transform.localEulerAngles.y);

        if (forwardError < -180)
            forwardError = 360 - System.Math.Abs(forwardError);
        else if (forwardError > 180)
            forwardError = -(360 - forwardError);

        //Debug.Log(forwardError);
        if (forwardError < 5)
        {
            _groundRenderer.material.color = Color.green;
            //  AddReward(.1f);
            m_inRange = true;
            _outOfYawRangeSeconds = 0;
        }
        else
        {
            _outOfYawRangeSeconds += Time.fixedDeltaTime;
            _groundRenderer.material.color = Color.red;
            AddReward(-.1f);
            m_inRange = false;
        }

        if (dist < .2 && forwardError < 10)
        {
            // Debug.Log("Frame : " + Time.frameCount + " : " + _atTargetSeconds);
            _atTargetSeconds += Time.fixedDeltaTime;
            //  AddReward(1 - dist);
            //  AddReward(.1f);
            //  Debug.Log("Getting reward");
            AddReward(1 - (dist * 4));
            if (_atTargetSeconds > 3)
            {
                AddReward(1);
                _achievedWaypoints++;
                //  _groundRenderer.material.color = Color.green;
                if (_achievedWaypoints == 1)//_waypointGoal)
                {
                    EndTheEpisode(0);
                }
                else
                {
                    SetNewTarget(_autoPilot.transform);

                }
            }
            // // EndTheEpisode(1);
        }
        else
        {
            _atTargetSeconds = 0;
        }

        if (dist < .3f && forwardError < 10)
        {
            // AddReward(1);
        }

        m_episodeRewards = GetCumulativeReward();
        //Debug.Log(StepCount);
        if (StepCount == MaxStep)
        {
            float exitReward = 0;
            if (_achievedWaypoints == 0)
            {
                // exitReward = -5;
            }
            _groundRenderer.material.color = Color.gray;
            // EndTheEpisode(exitReward);
        }
    }
    private void CheckForErrors(out float dist)
    {
        //varangle = Vector3.Angle(Vector3.up, transform.up);
        dist = Vector3.Distance(_autoPilot.GetGameObject().transform.position, _quadcopterToControl.GetGameObject().transform.position);

        //if (StepCount > 50 && rigidbody.velocity.magnitude < .01f)
        //{
        //    ////Debug.Log("TimeOut ");
        //    _groundRenderer.material.color = Color.green;
        //    EndTheEpisode(-1);
        //    return;
        //}

        //if (transform.localPosition.y > .1f)
        //{
        //    flying = true;
        //}
        //if (flying)
        //{
        //    if (transform.localPosition.y < .1f)
        //    {
        //        _groundRenderer.material.color = Color.blue;
        //        EndTheEpisode(-5);
        //    }
        //}

        //if (angle > _angleError)
        //{
        //    // //Debug.Log("flip out ");
        //    _groundRenderer.material.color = Color.yellow;
        //    EndTheEpisode(-5);
        //}
        var quadPosition = _quadcopterToControl.GetGameObject().transform.localPosition;
        if ((Mathf.Abs(quadPosition.x) > _bounds.x) ||
      (Mathf.Abs(quadPosition.y) > _bounds.y) ||
      (Mathf.Abs(quadPosition.z) > _bounds.z))
        {
            _groundRenderer.material.color = Color.red;
            EndTheEpisode(0);
            // AddReward(-1);
        }

    }

    private void Update()
    {
        //var forwardError = Mathf.Abs(_autoPilot.GetGameObject().transform.localEulerAngles.y - _quadcopterToControl.GetGameObject().transform.localEulerAngles.y);
        //Debug.Log(forwardError);
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
