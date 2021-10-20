using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightController : MonoBehaviour
{
    [SerializeField]
    private List<Motor> _motors;

    private PidController _elevationPid;
    private PidController _pitchPid;
    private PidController _rollPid;
    private PidController _yawPid;

    private PidController _positionPidX;
    private PidController _positionPidZ;

    [SerializeField]
    private Transform _targetGimbal;

    [SerializeField]
    private Motor flMotor;
    [SerializeField]
    private Motor frMotor;
    [SerializeField]
    private Motor blMotor;
    [SerializeField]
    private Motor brMotor;

    /// <summary>
    /// How long has it been since the last Update, required for <see cref="PidController"/>
    /// </summary>
    /// <remarks>
    /// Exposed in Inspector solely for debuging
    /// </remarks>
    [SerializeField]
    private float _timeSinceLastUpdate;
    /// <summary>
    /// The time of the last update
    /// </summary>
    private float prevDeltaTime = 0;

    private Transform _gimbleCraft;

    [SerializeField]
    private bool _stableize = true;

    // Start is called before the first frame update
    void Start()
    {
        _gimbleCraft = _targetGimbal;// new GameObject("Quad Gimbal").transform;
        _elevationPid = new PidController(.14f, .03f, 0.04f, .8f, .1f);
        _elevationPid.SetPoint = 0;

        var translateP = .001f;
        var translateI = 0;
        var translateD = 0f;
        var translateLimit = .1f;

        _pitchPid = new PidController(translateP, translateI, translateD, translateLimit, -translateLimit);
        _pitchPid.SetPoint = 0;

        _rollPid = new PidController(translateP, translateI, translateD, translateLimit, -translateLimit);
        _rollPid.SetPoint = 0;

        _yawPid = new PidController(.03f, 0.05f,0.008, .1f, -.1f);
        _yawPid.SetPoint = 0;

        _positionPidX = new PidController(7f, 1, 6.8, 30f, -30f);
        _positionPidX.SetPoint = 0;

        _positionPidZ = new PidController(7f, 1, 6.8, 30f, -30f);
        _positionPidZ.SetPoint = 0;
    }

    private void FixedUpdate()
    {


        _timeSinceLastUpdate = Time.time - prevDeltaTime;
        prevDeltaTime = Time.time;
        var deltaTime1 = (int)(_timeSinceLastUpdate * 1000);
        var deltaTime = new System.TimeSpan(0, 0, 0, 0, (deltaTime1));


        //Position X
        var offsetPos = transform.position - _targetGimbal.position;

        _positionPidX.ProcessVariable = offsetPos.x;
        var roll = _positionPidX.ControlVariable(deltaTime);

        float pitch = 0;
        if(offsetPos.x > 1)
        {
            pitch = 20;
        }
        else if (offsetPos.x < -1)
        {
            pitch = -20;
        }
        else
        {
        
            _positionPidZ.ProcessVariable = offsetPos.z;
             pitch = (float)_positionPidZ.ControlVariable(deltaTime);
        }
     


        var tempEuler = _gimbleCraft.eulerAngles;
        tempEuler.x = (float)pitch;
        tempEuler.z = -(float)roll;
        _gimbleCraft.eulerAngles = tempEuler;

        var offset = transform.position.y - _targetGimbal.transform.position.y;
        //var offset = .2f - transform.position.y;
        // Debug.Log(offset);
        _elevationPid.ProcessVariable = offset;
        // double trgtRoll = _elevationPid.ControlVariable(new System.TimeSpan(0, 0, 1));
        double trgtRoll = _elevationPid.ControlVariable(deltaTime);
        // Debug.Log(trgtRoll);
        // if (!float.IsNaN((float)trgtRoll))
        //      SetUniversalThrottle((float)trgtRoll);

        float throttleValue = 20;
        //if (offset > 1)
        //{
             throttleValue = (float)trgtRoll;
       // }
       

        // _gimbleCraft.transform.position = transform.position ;
        // _gimbleCraft.transform.rotation = transform.rotation;
        tempEuler = _gimbleCraft.transform.eulerAngles;
        // tempEuler.y = transform.eulerAngles.y;
        _gimbleCraft.transform.eulerAngles = tempEuler;
        transform.SetParent(_gimbleCraft);
        Vector3 eulerAngles = transform.localEulerAngles;
        transform.SetParent(null);

        var pitchOffset = eulerAngles.x;
        //var offset = .2f - transform.position.y;
        // Debug.Log(offset); 

        if (pitchOffset < -180)
            pitchOffset = 360 - System.Math.Abs(pitchOffset);
        else if (pitchOffset > 180)
            pitchOffset = -(360 - pitchOffset);

        _pitchPid.ProcessVariable = -pitchOffset;


        // double trgtRoll = _elevationPid.ControlVariable(new System.TimeSpan(0, 0, 1));
        double trgtPitch = _pitchPid.ControlVariable(deltaTime);
        // Debug.Log(trgtRoll);
        // if (!float.IsNaN((float)trgtRoll))
        float pitchValue = (float)trgtPitch;

        //Debug.Log("Pitch Error : " + pitchOffset + " PitchOffset : " + trgtPitch);

        //ROLL
        var rollOffset = eulerAngles.z;

        if (rollOffset < -180)
            rollOffset = 360 - System.Math.Abs(rollOffset);
        else if (rollOffset > 180)
            rollOffset = -(360 - rollOffset);

        _rollPid.ProcessVariable = -rollOffset;


        // double trgtRoll = _elevationPid.ControlVariable(new System.TimeSpan(0, 0, 1));
        trgtRoll = _rollPid.ControlVariable(deltaTime);
        // Debug.Log(trgtRoll);
        // if (!float.IsNaN((float)trgtRoll))
        float rollValue = (float)trgtRoll;

        // Debug.Log("Pitch Error : " + pitchOffset + " PitchOffset : " + trgtPitch);

        //yaw
        var yawOffset = eulerAngles.y;

        if (yawOffset < -180)
            yawOffset = 360 - System.Math.Abs(yawOffset);
        else if (yawOffset > 180)
            yawOffset = -(360 - yawOffset);

        _yawPid.ProcessVariable = -yawOffset;


        // double trgtyaw = _elevationPid.ControlVariable(new System.TimeSpan(0, 0, 1));
        var trgtyaw = _yawPid.ControlVariable(deltaTime);
        // Debug.Log(trgtyaw);
        // if (!float.IsNaN((float)trgtyaw))
        float yawValue = (float)trgtyaw;

        //pitchValue = 0;
        //rollValue = 0;
       // yawValue = 0;

        //if(transform.position.y < .5f)
        //{
        //    throttleValue = 1;
        //}

        if (_stableize)
        {
            frMotor.SetThrottle(throttleValue + pitchValue - rollValue + yawValue);
            flMotor.SetThrottle(throttleValue + pitchValue + rollValue - yawValue);

            brMotor.SetThrottle(throttleValue - pitchValue - rollValue - yawValue);
            blMotor.SetThrottle(throttleValue - pitchValue + rollValue + yawValue);
        }
        else
        {
            frMotor.SetThrottle(throttleValue);
            flMotor.SetThrottle(throttleValue);

            brMotor.SetThrottle(throttleValue);
            blMotor.SetThrottle(throttleValue);
        }

    }

    public void SetUniversalThrottle(float newThrottle)
    {
        Debug.Log("Set universal throttle : " + newThrottle);
        foreach (var motor in _motors)
        {
            motor.SetThrottle(newThrottle);
        }
    }
    // Update is called once per frame
    void Update()
    {



    }
}
