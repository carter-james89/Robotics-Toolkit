using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboticServo : MonoBehaviour
{
    [SerializeField]
    private Transform _target;

    [SerializeField]
    private Transform _servo;

    [SerializeField]
    private HingeJoint _servoJoint;

    private Rigidbody _rigidbody;

    public bool runMotor;
    // Start is called before the first frame update
    void Start()
    {
        // _servoJoint.anchor = -transform.InverseTransformPoint(_servoJoint.transform.position);

        _servoJoint = GetComponent<HingeJoint>();
       // _servoJoint.anchor = _servoJoint.connectedAnchor;

        _rigidbody = GetComponent<Rigidbody>();

       // _servoJoint.anchor = transform.InverseTransformPoint(_servoJoint.connectedBody.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        // _servo.transform.position = transform.position;

        //var dir = transform.position - _target.position;
        var motor = _servoJoint.motor;
        motor.force = 10000000000000;
        motor.targetVelocity = 0;
        //transform.rotation = Quaternion.LookRotation(dir);
        // _target.GetComponent<Rigidbody>().useGravity = true;
        if (Input.GetKey(KeyCode.UpArrow) && runMotor)
        {
            // _target.GetComponent<Rigidbody>().useGravity = false;
            // _rigidbody.AddTorque(transform.right * 100000);
            // _servoJoint.
         
         
            motor.targetVelocity = 100;


            // _target.GetComponent<HingeJoint>().motor = motor;
        }

        else if (Input.GetKey(KeyCode.DownArrow) && runMotor)
        {
            motor.targetVelocity = -100;
            // _rigidbody.AddTorque(-transform.right * 100000);
            // _target.GetComponent<Rigidbody>().useGravity = false;
            // _target.GetComponent<Rigidbody>().AddTorque(-_servo.right);
            // _servo.GetComponent<Rigidbody>().AddTorque(_servo.right * .2f);
            //_target.GetComponent<Rigidbody>().AddTorque(transform.right *5000,ForceMode.VelocityChange );
            // _target.GetComponent<Rigidbody>().AddForce(transform.forward *1, ForceMode.Force);
            //_target.GetComponent<Rigidbody>().AddRelativeTorque(-transform.right * 100000);
        }
        
        _servoJoint.motor = motor;

        //    _servo.LookAt(_target);

        //   var tempEuler = _servo.localEulerAngles;
        // tempEuler.y = 0;
        // tempEuler.z = 0;
        //_servo.localEulerAngles = tempEuler;

    }
}
