using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboticServo : MonoBehaviour
{
    public float deltaAngle;
    public float prevAngle = 0;
    [SerializeField]
    private float _servoAngle
    {
        get
        {
            return -_servoJoint.angle;
            // return _servoJoint.angle;
            if (_servoJoint.axis == new Vector3(0, 1, 0))
            {
                return convert360Euler(transform.localEulerAngles.y);
            }
            else if (_servoJoint.axis == new Vector3(1, 0, 0))
            {
                // Quaternion.
               // return convert360Euler(transform.localRotation.eulerAngles.x);
                //var localRot = Quaternion.Inverse(transform.parent.rotation) * transform.rotation;
                // return localRot.eulerAngles.x;

                var angle = Math.Abs(_servoJoint.angle);
                var point0 = transform.position + transform.forward * 10;
                var point1 = transform.position + transform.parent.forward * 10;

                if(point0.y < point1.y)
                {
                    angle = -angle;
                }
                return angle;

            }
            return _servoJoint.angle;
        }
    }
    public float ServoAngle;
    public float adjValue;
    public float oppositeValue;
    [SerializeField]
    private float _angleDif;
    public float setPoint { get; private set; } = 0;


    private float _servoForce = 500;

    private float _servoVelocityMax = 500;
    public void SetAngle(float newAngle)
    {
        // setPoint = newAngle;
        setPoint = newAngle - _endPointOffset;
    }

    public Matrix4x4 _base = new Matrix4x4();
    private Vector3 _basePosOffset;
    private Quaternion _baseRotOffset;

    [SerializeField]
    private Transform _endPointPosition;
    private float _endPointOffset = 0;

    [SerializeField]
    private HingeJoint _servoJoint;

    private Rigidbody _rigidbody;

    private PidController _velocityPID;
    private PidController _forcePID;




    bool run = false;
    public Transform debugBase;


    public bool runMotor;
    // Start is called before the first frame update
    void Start()
    {
        _servoJoint = GetComponent<HingeJoint>();
        _rigidbody = GetComponent<Rigidbody>();

        //Time.timeScale = .06f;

        _basePosOffset = transform.parent.InverseTransformPoint(transform.position);
        _baseRotOffset = Quaternion.Inverse(transform.parent.rotation) * transform.rotation;

        if (_endPointPosition)
        {
            var offset = transform.InverseTransformPoint(_endPointPosition.position);
            var d1 = Math.Atan(offset.y / offset.z);
            _endPointOffset = (float)(d1 * (180 / Math.PI));
        }
       
        _velocityPID = new PidController(10, 10, .1f, _servoVelocityMax, -_servoVelocityMax);
        _forcePID = new PidController(0, 50, 0, 1000, 100);
    }

    private float convert360Euler(float euler)
    {
        if (euler > 180)
        {
            euler = (360 - euler);
        }
        else
        {
            euler = -euler;
        }
        return euler;
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
        var baseRot = transform.parent.rotation;// * _baseRotOffset;

        if (debugBase)
        {
            debugBase.transform.position = basePos;
            debugBase.transform.rotation = baseRot;
        }
        _base.SetTRS(transform.position, baseRot, Vector3.one);
    }

    public float CalculateIKPosition(Vector3 point, Vector3 targetPoint, RoboticServo childServo = null)
    {
        _targetOffset = _base.inverse.MultiplyPoint(targetPoint);
        if (childServo)
        {
            var childTargetOffset = childServo._base.inverse.MultiplyPoint(targetPoint);

            var d1 = Math.Atan(_targetOffset.y / _targetOffset.z);
            if (_servoJoint.axis == new Vector3(0, 1, 0))
            {
                d1 = Math.Atan(_targetOffset.x / _targetOffset.z);
            }
            d1 *= (180 / Math.PI);
            var targetDistC = Vector3.Distance(targetPoint, transform.position);
            var childTargetDistB = Vector3.Distance(point, childServo.transform.position);
            var distToChildServoA = Vector3.Distance(transform.position, childServo.transform.position);
            var d2 = LawOfCosines(distToChildServoA, childTargetDistB, targetDistC);
            var hipElvAngle = d2 + d1;
            // hipElvAngle += hipElvAngleOffset;
            if (!Double.IsNaN(hipElvAngle))
            {
                SetAngle((float)hipElvAngle);
            }
            else
            {
                CalculateSingleIK(point, targetPoint);
            }
        }
        else
        {
            CalculateSingleIK(point, targetPoint);
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

        adjValue = _targetOffset.z;
        oppositeValue = _targetOffset.y;
        if (_servoJoint.axis == new Vector3(0, 1, 0))
        {
            adjValue = _targetOffset.z;
            oppositeValue = _targetOffset.x;
           
        }

        if (_targetOffset.z > 0)
        {
            jointTwoAngle = radToDegree(Math.Atan(oppositeValue / adjValue));
        }
        else
        {
            jointTwoAngle = radToDegree(Math.Atan(oppositeValue/-adjValue));
            if (_targetOffset.y > 0)
            {
                jointTwoAngle = 180 - jointTwoAngle;
            }
            else
            {
                jointTwoAngle = -180 - jointTwoAngle;
            }
        }
        if (_servoJoint.axis == new Vector3(0, 1, 0))
        {
            SetAngle(-(float)jointTwoAngle);
            return;
        }
        SetAngle((float)jointTwoAngle);
    }

    private float radToDegree(double radian)
    {
        return (float)(radian * (180 / Math.PI));
    }

    //private enum AngleAxis
    //{
    //    X,Y,Z,ServoAngle
    //}
    //[SerializeField]
    //private AngleAxis _angleAxis = AngleAxis.ServoAngle;

    private void setServoVelocity()
    {
        var motor = _servoJoint.motor;

        _timeSinceLastUpdate = Time.time - prevDeltaTime;
        prevDeltaTime = Time.time;
        var deltaTime1 = (int)(_timeSinceLastUpdate * 1000);
        var deltaTime = new System.TimeSpan(0, 0, 0, 0, (deltaTime1));

        _angleDif = setPoint - _servoAngle;

        _velocityPID.ProcessVariable = _angleDif;
        trgtVelocity = (float)_velocityPID.ControlVariable(deltaTime);

        _forcePID.ProcessVariable = _angleDif;
        trgtForce = (float)_forcePID.ControlVariable(deltaTime);

        motor.force = trgtForce;
        motor.targetVelocity = trgtVelocity;
        _servoJoint.motor = motor;
    }

  

    void Update()
    {
       
        UpdateBase();

        if (Input.GetKey(KeyCode.UpArrow) && runMotor)
        {
            run = true;
            prevDeltaTime = Time.time;
        }
        ServoAngle = _servoAngle;
    }
}
