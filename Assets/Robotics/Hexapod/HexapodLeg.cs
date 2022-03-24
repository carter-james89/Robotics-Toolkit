using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexapodLeg : MonoBehaviour
{
    [SerializeField]
    private RoboticServo _hipServo;
    [SerializeField]
    private RoboticServo _kneeServo;
    [SerializeField]
    private RoboticServo _footServo;

    [SerializeField]
    private float _legHeightError;

    [SerializeField]
    private LimbGait _gait;

    private Transform _hipTarget;
    public void SetHipTarget(Transform newTarget)
    {
        _hipTarget = newTarget;
        _hipTarget.transform.position = _kneeServo.transform.position;
    }


    [SerializeField]
    private Transform _endPoint;

    [SerializeField]
    private Transform _endPointTarget;
    public Transform GetLegTarget() => _endPointTarget;
    private void Awake()
    {

    }

    public void Initialize()
    {

    }


    public void AttachToRigidbody(Rigidbody attachBody)
    {
        _hipServo.gameObject.GetComponent<HingeJoint>().connectedBody = attachBody.transform.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //var hipRotTargetOffset = hipRotServo.transform.parent.InverseTransformPoint(legTarget.position);
        //var hipElvTargetOffset = _kneeServo._base.inverse.MultiplyPoint(_endPointTarget.position);
        //var footTargetOffset = _footServo._base.inverse.MultiplyPoint(_endPointTarget.position);

        //var d1 = Math.Atan(hipElvTargetOffset.y / hipElvTargetOffset.z);
        //d1 *= (180 / Math.PI);
        //var hipElvTargetDist = Vector3.Distance(_endPointTarget.position, _kneeServo.transform.position);
        //var kneeFootDist = Vector3.Distance(_endPoint.position, _footServo.transform.position);
        //var d2 = LawOfCosines(Vector3.Distance(_kneeServo.transform.position,_footServo.transform.position), kneeFootDist, hipElvTargetDist);
        //var hipElvAngle = -(d2 + d1);
        //// hipElvAngle += hipElvAngleOffset;
        //if (!Double.IsNaN(hipElvAngle))
        _hipServo.CalculateIKPosition(_endPoint.position, _endPointTarget.position);
        _kneeServo.CalculateIKPosition(_endPoint.position, _endPointTarget.position, _footServo);
       // _kneeServo.SetAngle(0);
        _footServo.CalculateIKPosition(_endPoint.position, _endPointTarget.position);
        //_kneeServo.CalculateIKPosition(0);

      

       
    }
    public void SetGaitHeight()
    {
        _legHeightError = _hipTarget.position.y - _kneeServo.transform.position.y;
        var tempPos = _gait.transform.position;
        tempPos.y = -_legHeightError;
        _gait.transform.position = tempPos;
    }

}
