using RoboticsToolkit.Robotics.Limbs;
using RoboticsToolkit.Robotics.Servos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuadrupedLeg : MonoBehaviour, IRoboticLimb
{
    [SerializeField]
    private GameObject m_baseRoboticLimbSegmentObject;
    private IRoboticLimbSegment m_baseRoboticLimbSegment;
    [SerializeField]
    private GameObject m_hipRoboticLimbSegmentObject;
    private IRoboticLimbSegment m_hipRoboticLimbSegment;
    [SerializeField]
    private GameObject m_kneeRoboticLimbSegmentObject;
    private IRoboticLimbSegment m_kneeRoboticLimbSegment;

    [SerializeField]
    private bool _debug;

    Matrix4x4 m_anchorMatrix = new Matrix4x4();
    [SerializeField]
    public bool m_invert;

    [SerializeField]
    private GameObject m_contactPoint;

    public Transform IKTarget;//TODO : remove this

    public GameObject GetContactPoint()
    {
        return m_contactPoint;
    }

    public GameObject GetGameObject() => gameObject;

    private void Awake()
    {
        if (m_baseRoboticLimbSegmentObject)
            m_baseRoboticLimbSegment = m_baseRoboticLimbSegmentObject.GetComponent<IRoboticLimbSegment>();
        m_hipRoboticLimbSegment = m_hipRoboticLimbSegmentObject.GetComponent<IRoboticLimbSegment>();
        m_kneeRoboticLimbSegment = m_kneeRoboticLimbSegmentObject.GetComponent<IRoboticLimbSegment>();

        //   GetPositioner();
    }

    private static float RadToDegree(double radian)
    {
        return (float)(radian * Mathf.Rad2Deg);
    }


    // public static Transform DebugTransform;
    #region Tools
    private Vector3 CalculateTargetOffset(IRoboticLimbSegment servo, Vector3 targetPoint)
    {
        return servo.GetGameObject().transform.parent.InverseTransformPoint(targetPoint);
        //m_anchorMatrix = new Matrix4x4();

        //var globalPosition = servo.transform.parent.TransformPoint(servo.parentAnchorPosition);
        //var globalRotation = servo.transform.parent.rotation * servo.parentAnchorRotation; //articulationBody.transform.TransformVector(articulationBody.anchorRotation.eulerAngles);

        ////if(DebugTransform == null)
        ////{
        ////    DebugTransform = new GameObject("Shoulder Debug").transform;

        ////}
        ////DebugTransform.position = globalPosition;
        ////DebugTransform.rotation = globalRotation;
        //// globalPosition = articulationBody.transform.TransformPoint(articulationBody.anchorPosition);
        //// globalRotation = articulationBody.transform.rotation * articulationBody.anchorRotation; //articulationBody.transform.TransformVector(articulationBody.anchorRotation.eulerAngles);
        ////m_transform.position = globalPosition;
        ////m_transform.rotation = globalRotation;

        //m_anchorMatrix.SetTRS(globalPosition, globalRotation, Vector3.one);


        //return m_anchorMatrix.inverse.MultiplyPoint(targetPoint);
    }

    private float _targetBaseAngle = 0;
    private float _targetThighAngle = 0;
    private float _targetCalfAngle = 0;

    public void SetLimbValues(float baseAngle, float hipAngle, float kneeAngle)
    {
        _targetBaseAngle = baseAngle;
        _targetThighAngle = hipAngle;
        _targetCalfAngle = kneeAngle;
        if (m_baseRoboticLimbSegment != null)
        {
            m_baseRoboticLimbSegment.SetServoAngle(baseAngle);
        }
        if (m_hipRoboticLimbSegment != null)
        {
            m_hipRoboticLimbSegment.SetServoAngle(hipAngle);
        }
        if (m_kneeRoboticLimbSegment != null)
        {
            m_kneeRoboticLimbSegment.SetServoAngle(kneeAngle);
        }
    }

    public bool SegmentsAtTarget(float tolerance)
    {
        bool isBaseAtTarget = true;
        float baseDifference = 0f;
        if (m_baseRoboticLimbSegment != null)
        {
            baseDifference = Math.Abs(m_baseRoboticLimbSegment.GetServoAngle() - _targetBaseAngle);
            isBaseAtTarget = baseDifference <= tolerance;
        }

     

        float hipDifference = Math.Abs(Math.Abs(m_hipRoboticLimbSegment.GetServoAngle()) - Math.Abs(_targetThighAngle));
        bool isHipAtTarget = hipDifference <= tolerance;

        float kneeDifference = Math.Abs(Math.Abs(m_kneeRoboticLimbSegment.GetServoAngle()) - Math.Abs(_targetCalfAngle));
        bool isKneeAtTarget = kneeDifference <= tolerance;

        var limbReady = isBaseAtTarget && isHipAtTarget && isKneeAtTarget;

        if (_debug)
        {
            Debug.Log("Log Start " + Time.frameCount);
            Debug.Log(hipDifference);
            Debug.Log(kneeDifference);
            Debug.Log($"Base Angle: {m_baseRoboticLimbSegment?.GetServoAngle()} | Target: {_targetBaseAngle} | Difference: {baseDifference} | At Target: {isBaseAtTarget}");
            Debug.Log($"Hip Angle: {m_hipRoboticLimbSegment.GetServoAngle()} | Target: {_targetThighAngle} | Difference: {hipDifference} | At Target: {isHipAtTarget}");
           Debug.Log($"Knee Angle: {m_kneeRoboticLimbSegment.GetServoAngle()} | Target: {_targetCalfAngle} | Difference: {kneeDifference} | At Target: {isKneeAtTarget}");
        }

        return limbReady;
    }



    private float m_xOffset = .03f;
    private float m_yOffset = .03f;
    public void CalculateIK(bool adjustHeight = false)
    {
        if (!gameObject.activeInHierarchy)
            return;

        var x = m_xOffset;
        var y = m_yOffset;
        if (IKTarget != null)
        {
            var targetOffset = CalculateTargetOffset(GetHipSegment(), IKTarget.position);
            x = targetOffset.z;
            y = targetOffset.y;
        }

        CalculateIK(x, y);
    }
    public void CalculateIK(float targetX, float targetY)
    {
        double distance = Math.Sqrt(targetX * targetX + targetY * targetY);

        var armLength1 = m_hipRoboticLimbSegment.GetLength();// .13f;
        var armLength2 = m_kneeRoboticLimbSegment.GetLength();// .13f;

        float angle1;
        float angle2;

        if (m_baseRoboticLimbSegment != null)
        {
            IServo baseServo = GetBaseSegment().GetServo(0);

            var localPoint = baseServo.GetGameObject().transform.parent.InverseTransformPoint((IKTarget.position + baseServo.GetGameObject().transform.forward * GetBaseSegment().GetLength()));

            if (m_invert)
            {
                localPoint = baseServo.GetGameObject().transform.parent.InverseTransformPoint((IKTarget.position - baseServo.GetGameObject().transform.forward * GetBaseSegment().GetLength()));
            }
            //  localPoint.z -= IServo baseServo = GetBaseSegment().GetServos()[0];
            //  var localPoint = baseServo.GetGameObject().transform.parent.InverseTransformPoint(IKTarget.position);
            // localPoint.z -= GetBaseSegment().GetLength();
            // var globalPoint = baseServo.GetGameObject().transform.parent.TransformPoint(IKTarget.position);
            var targetOffset = localPoint;
            //var targetOffset = CalculateTargetOffset(baseServo, globalPoint);
            var adjValue = targetOffset.z;
            var oppositeValue = targetOffset.y;

            float jointAngle = 0;

            jointAngle = RadToDegree(Math.Atan2((double)oppositeValue, (double)adjValue));
            GetBaseSegment().GetServo(0).SetServoPosition(jointAngle - 90);
        }

        if (distance > armLength1 + armLength2)
        {
            // Target is out of reach
            angle1 = 0.0f;
            angle2 = 0.0f;
        }
        else
        {
            // Calculate angles using law of cosines
            double cosAngle2 = (distance * distance - armLength1 * armLength1 - armLength2 * armLength2) / (2 * armLength1 * armLength2);
            double sinAngle2 = Math.Sqrt(1 - cosAngle2 * cosAngle2);
            angle2 = (float)Math.Atan2(sinAngle2, cosAngle2);

            double alpha = Math.Atan2(targetY, targetX);
            double beta = Math.Acos((distance * distance + armLength1 * armLength1 - armLength2 * armLength2) / (2 * distance * armLength1));
            angle1 = (float)(alpha - beta);

            var jointAngle1 = RadToDegree(angle1);
            var jointAngle2 = RadToDegree(angle2);

            //  Debug.Log("Angles " + jointAngle1 + " : " + jointAngle2);
            //  if (!float.IsNaN(jointAngle1))
            {
                GetHipSegment().GetServo(0).SetServoPosition(jointAngle1);
            }
            //if (!float.IsNaN(jointAngle2))
            {
                GetKneeSegment().GetServo(0).SetServoPosition(jointAngle2);
            }
        }
    }

    internal void CalculateIK(float v)
    {
        m_yOffset += v;
        CalculateIK(false);
    }

    public IRoboticLimbSegment GetBaseSegment()
    {
        return m_baseRoboticLimbSegment;
    }
    public IRoboticLimbSegment[] GetLimbSegments()
    {
        if (m_baseRoboticLimbSegment != null)
        {
            return new IRoboticLimbSegment[3] { m_baseRoboticLimbSegment, m_hipRoboticLimbSegment, m_kneeRoboticLimbSegment };
        }
        return new IRoboticLimbSegment[3] { null, m_hipRoboticLimbSegment, m_kneeRoboticLimbSegment };
    }
    public IRoboticLimbSegment GetHipSegment()
    {
        return m_hipRoboticLimbSegment;
    }
    public IRoboticLimbSegment GetKneeSegment()
    {
        return m_kneeRoboticLimbSegment;// GetComponent<ThreeJointRoboticLimb>().WristRoboticLimbSegmentController.GetRoboticLimbSegment();// GetRoboticLimbSegmentControllers()[2].GetRoboticLimbSegment();
    }

    public ILimbPositioner GetPositioner()
    {
        throw new NotImplementedException();
    }

    public Transform GetEndPoint()
    {
        return m_contactPoint.transform;
    }

    public Transform GetTargetBasePosition()
    {
        throw new NotImplementedException();
    }

    public IServoController[] GetServoControllers()
    {
        throw new NotImplementedException();
    }

    public IRoboticLimbSegment[] GetSegments()
    {
        return GetLimbSegments();
    }

    public void RunLimb(bool positionImmediate, bool adjustHeight = false)
    {
        CalculateIK(adjustHeight);
    }

    public void ResetLimb()
    {
        // m_baseRoboticLimbSegment.GetServo(0).ResetServo(0);
        m_hipRoboticLimbSegment.GetServo(0).ResetServo(80);
        m_kneeRoboticLimbSegment.GetServo(0).ResetServo(-140);
    }

    public void ResetLimbTargetPosition()
    {
        throw new NotImplementedException();
    }

    public void SetIKTargetPos(Vector3 globalPos)
    {
        IKTarget.position = globalPos;
    }

    public Vector3 GetIKTargetPos()
    {
        return IKTarget.position;
    }

    public bool LimbAtTarget()
    {
        throw new NotImplementedException();
    }

    public bool BaseAtTarget()
    {
        throw new NotImplementedException();
    }
}
#endregion