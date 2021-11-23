using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboticServo : MonoBehaviour
{
    [SerializeField]
    private float _servoAngle;

    private float _servoForce = 500;

    private float _servoVelocityMax = 10;//500;
    public void SetAngle(float newAngle)
    {
        // setPoint = newAngle;
        setPoint = newAngle + _endPointOffset;
    }

    public Matrix4x4 _base = new Matrix4x4();
    private Vector3 _basePosOffset;
    private Quaternion _baseRotOffset;

    [SerializeField]
    private Transform _endPointPosition;
    private float _endPointOffset;

    [SerializeField]
    private HingeJoint _servoJoint;

    private Rigidbody _rigidbody;

    private PidController _velocityPID;
    private PidController _forcePID;


    [SerializeField]
    private float _angleDif;

    bool run = false;
    public Transform debugBase;


    public bool runMotor;
    // Start is called before the first frame update
    void Start()
    {
        //Time.timeScale = .06f;
        // _base.SetTRS(transform.position, transform.rotation, Vector3.one);

        _basePosOffset = transform.parent.InverseTransformPoint(transform.position);
        _baseRotOffset = Quaternion.Inverse(transform.parent.rotation) * transform.rotation;

        if (_endPointPosition)
        {
            var offset = transform.InverseTransformPoint(_endPointPosition.position);
            var d1 = Math.Atan(offset.y / offset.z);
            _endPointOffset = (float)(d1 * (180 / Math.PI));
        }

        _servoJoint = GetComponent<HingeJoint>();
        _rigidbody = GetComponent<Rigidbody>();
        _velocityPID = new PidController(1, 1, .1f, _servoVelocityMax, -_servoVelocityMax);
        _forcePID = new PidController(0, 50, 0, 1000, 100);
    }

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

    public float setPoint { get; private set; } = 0;

    public float trgtVelocity;
    public float trgtForce;

    [SerializeField]
    private Vector3 _targetOffset;

    private void FixedUpdate()
    {
        UpdateBase();
        if (runMotor && run)
        {
            setServoVelocity();
        }
        else
        {
           // motor.force = trgtForce;
           // motor.targetVelocity = trgtVelocity;
        }
    }

    public void UpdateBase()
    {
        var basePos = transform.parent.TransformPoint(_basePosOffset);
        var baseRot = transform.parent.rotation * _baseRotOffset;

        if (debugBase)
        {
            debugBase.transform.position = basePos;
            debugBase.transform.rotation = baseRot;
        }
        _base.SetTRS(transform.position, baseRot, Vector3.one);      
        //s_targetOffset = transform.InverseTransformPoint(_endPointTarget.position);
    }

    public float CalculateIKPosition(Vector3 point, Vector3 targetPoint, RoboticServo childServo = null)
    {
        _targetOffset = _base.inverse.MultiplyPoint(targetPoint);
        if (childServo)
        {
            // var targetOffset = _base.inverse.MultiplyPoint(targetPoint);
            var childTargetOffset = childServo._base.inverse.MultiplyPoint(targetPoint);

            var d1 = Math.Atan(_targetOffset.y / _targetOffset.z);
            d1 *= (180 / Math.PI);
            var targetDistC = Vector3.Distance(targetPoint, transform.position);
            var childTargetDistB = Vector3.Distance(point, childServo.transform.position);
            var distToChildServoA = Vector3.Distance(transform.position, childServo.transform.position);
            var d2 = LawOfCosines(distToChildServoA, childTargetDistB, targetDistC);
            var hipElvAngle = d2 + d1;
            // hipElvAngle += hipElvAngleOffset;
            if (!Double.IsNaN(hipElvAngle))
            {
                SetAngle(-(float)hipElvAngle);
            }
            else
            {
                CalculateSingleIK(point, targetPoint);
            }
        }
        else
        {
            CalculateSingleIK(point,targetPoint);
        }
        return setPoint;
    }
    public float LawOfCosines(float a, float b, float c)
    {
        var topEqu = (Math.Pow(c, 2) + Math.Pow(a, 2) - Math.Pow(b, 2));
        var bottomEqu = 2 * a * c;
        var angle = topEqu / bottomEqu;
        angle = (float)Math.Acos(angle);
        angle = (float)(angle * 180 / Math.PI);
        return (float)angle;
    }

    public void CalculateSingleIK(Vector3 point, Vector3 targetPoint)
    {
        double jointTwoAngle = 0;
        _targetOffset = _base.inverse.MultiplyPoint(targetPoint);
        if (_targetOffset.z > 0)
        {
            jointTwoAngle = Math.Atan(_targetOffset.y / _targetOffset.z);
            jointTwoAngle *= (180 / Math.PI);
            //jointTwoAngle -= 90;
        }
        else
        {
            jointTwoAngle = Math.Atan(_targetOffset.z / _targetOffset.y);
            jointTwoAngle *= -(180 / Math.PI);
            if (_targetOffset.y > 0)
            {
                jointTwoAngle += 90;
            }
            else
            {
                jointTwoAngle -= 90;
            }
            //jointTwoAngle = 0;
        }

        //kneeAngle -= kneeAngleOffset;
        //  if (!Double.IsNaN(jointTwoAngle))
        //  {
        // setPoint = -(float)jointTwoAngle;
        SetAngle(-(float)jointTwoAngle);
        // }
        //setServoVelocity();
    }

    private void setServoVelocity()
    {
        _servoAngle = _servoJoint.angle;
        var motor = _servoJoint.motor;
        motor.force = _servoForce;

        // motor.

        _timeSinceLastUpdate = Time.time - prevDeltaTime;
        prevDeltaTime = Time.time;
        var deltaTime1 = (int)(_timeSinceLastUpdate * 1000);
        var deltaTime = new System.TimeSpan(0, 0, 0, 0, (deltaTime1));

        _angleDif = _servoAngle - setPoint;

        _velocityPID.ProcessVariable = _angleDif;
        trgtVelocity = (float)_velocityPID.ControlVariable(deltaTime);

        _forcePID.ProcessVariable = _angleDif;
        trgtForce = (float)_forcePID.ControlVariable(deltaTime);

        motor.force = trgtForce;
        motor.targetVelocity = trgtVelocity;

        //motor.

        _servoJoint.motor = motor;


        // motor.targetVelocity = trgtVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        //_base.SetTRS(transform.position, transform.rotation, Vector3.one);

        // _targetOffset = _base.inverse.MultiplyPoint( _endPointTarget.position);
        // var motor = _servoJoint.motor;
        // //   setServoVelocity();

        UpdateBase();


        // // _servo.transform.position = transform.position;
        // _servoAngle = _servoJoint.angle;
        // //var dir = transform.position - _target.position;

        //// motor.force = 10000000000000;
        // //motor.targetVelocity = 0;
        // //transform.rotation = Quaternion.LookRotation(dir);
        // // _target.GetComponent<Rigidbody>().useGravity = true;

        if (Input.GetKey(KeyCode.UpArrow) && runMotor)
        {
            // _target.GetComponent<Rigidbody>().useGravity = false;
            // _rigidbody.AddTorque(transform.right * 100000);
            // _servoJoint.
            // Debug.Log("motor");
            run = true;
            prevDeltaTime = Time.time;
            //motor.targetVelocity = trgtVelocity;


            // _target.GetComponent<HingeJoint>().motor = motor;
        }

        // else if (Input.GetKey(KeyCode.DownArrow) && runMotor)
        // {
        //     motor.targetVelocity = -100;
        //     // _rigidbody.AddTorque(-transform.right * 100000);
        //     // _target.GetComponent<Rigidbody>().useGravity = false;
        //     // _target.GetComponent<Rigidbody>().AddTorque(-_servo.right);
        //     // _servo.GetComponent<Rigidbody>().AddTorque(_servo.right * .2f);
        //     //_target.GetComponent<Rigidbody>().AddTorque(transform.right *5000,ForceMode.VelocityChange );
        //     // _target.GetComponent<Rigidbody>().AddForce(transform.forward *1, ForceMode.Force);
        //     //_target.GetComponent<Rigidbody>().AddRelativeTorque(-transform.right * 100000);
        // }

        //// _servoJoint.motor = motor;

        // //    _servo.LookAt(_target);

        // //   var tempEuler = _servo.localEulerAngles;
        // // tempEuler.y = 0;
        // // tempEuler.z = 0;
        // //_servo.localEulerAngles = tempEuler;

    }
}
