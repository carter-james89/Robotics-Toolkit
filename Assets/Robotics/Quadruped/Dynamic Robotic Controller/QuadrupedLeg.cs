using RoboticToolkit.Robotics.Limbs;
using RoboticToolKit.Robotics.Servos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IQuadrupedLeg
{
    public GameObject GetGameObject();
    public IRoboticLimbSegment GetBaseSegment();
    public IRoboticLimbSegment GetHipSegment();
    public IRoboticLimbSegment GetKneeSegment();

    public IRoboticLimbSegment[] GetLimbSegments();

    // public ILimbPositioner GetPositioner();
    public void CalculateIK(bool adjustHeight = false);

    public GameObject GetContactPoint();

}
public class QuadrupedLeg : MonoBehaviour, IQuadrupedLeg
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

    Matrix4x4 m_anchorMatrix = new Matrix4x4();

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
        m_baseRoboticLimbSegment = m_baseRoboticLimbSegmentObject.GetComponent<IRoboticLimbSegment>();
        m_hipRoboticLimbSegment = m_hipRoboticLimbSegmentObject.GetComponent<IRoboticLimbSegment>();
        m_kneeRoboticLimbSegment = m_kneeRoboticLimbSegmentObject.GetComponent<IRoboticLimbSegment>();

        //   GetPositioner();
    }

    private static float RadToDegree(double radian)
    {
        return (float)(radian *Mathf.Rad2Deg);
    }

    public KeyValuePair<float, float> CalculateDuelIK(IServo elbow, IServo wrist, Vector3 limbEndPoint, Vector3 targetPoint)
    {


        var wristAngle = CalculateSingleIK(wrist, targetPoint);

        var localPoint = elbow.GetGameObject().transform.InverseTransformPoint(wrist.GetGameObject().transform.position);
        localPoint.x = 0;
        var adjustedWristPoint = elbow.GetGameObject().transform.TransformPoint(localPoint);

        localPoint = elbow.GetGameObject().transform.InverseTransformPoint(targetPoint);
        localPoint.x = 0;
        var adjustedTargetPoint = elbow.GetGameObject().transform.TransformPoint(localPoint);

        localPoint = elbow.GetGameObject().transform.InverseTransformPoint(limbEndPoint);
        localPoint.x = 0;
        var adjustedEndPoint = elbow.GetGameObject().transform.TransformPoint(localPoint);

        var d1 = CalculateSingleIK(elbow, targetPoint);
        // var elbowTargetOffset = CalculateTargetOffset(elbow,targetPoint);
        //var d1 = Math.Atan(-elbowTargetOffset.y / elbowTargetOffset.z);
        //d1 *= (180 / Math.PI);

        var targetDistC = Vector3.Distance(adjustedTargetPoint, elbow.GetGameObject().transform.position);
        var childTargetDistB = Vector3.Distance(adjustedEndPoint, adjustedWristPoint);
        var distToChildServoA = Vector3.Distance(elbow.GetGameObject().transform.position, adjustedWristPoint);
        var d2 = IKCalculator.LawOfCosines(distToChildServoA, childTargetDistB, targetDistC);

        var elbowAngle = d2 + d1;

        if (Double.IsNaN(elbowAngle))
        {
            elbowAngle = CalculateSingleIK(elbow, targetPoint);
        }

        return new KeyValuePair<float, float>(-(float)elbowAngle, -wristAngle);   // hipElvAngle += hipElvAngleOffset;

        //var targetDistC = Vector3.Distance(adjustedTargetPoint, elbow.transform.position);
        //var childTargetDistB = Vector3.Distance(adjustedEndPoint, adjustedWristPoint);
        //var distToChildServoA = Vector3.Distance(elbow.transform.position, adjustedWristPoint);
        //var d2 = LawOfCosines(distToChildServoA, childTargetDistB, targetDistC);
        //var elbowAngle = d2 + wristAngle;


        //if (Double.IsNaN(elbowAngle))
        //{
        ////  elbowAngle= CalculateSingleIK(wrist, targetPoint);
        //}

        //return new KeyValuePair<float, float>(elbowAngle,wristAngle);
    }


    public float CalculateSingleIK(IServo articulationBody, Vector3 targetPoint, bool flip = false)
    {
        var targetOffset = CalculateTargetOffset(articulationBody, targetPoint);
        var adjValue = targetOffset.z;
        var oppositeValue = targetOffset.y;

        float jointAngle = 0;

        jointAngle = RadToDegree(Math.Atan2((double)oppositeValue , (double)adjValue));

        //if (targetOffset.z > 0)
        //{
        //    jointAngle = RadToDegree(Math.Atan(oppositeValue / adjValue));
        //}
        //else
        //{
        //    jointAngle = RadToDegree(Math.Atan(oppositeValue / -adjValue));
        //    if (targetOffset.y < 0)
        //    {
        //        jointAngle = 180 - jointAngle;
        //    }
        //    else
        //    {
        //        jointAngle = -180 - jointAngle;
        //    }
        //}
        if (flip)
        {
            jointAngle *= -1;
        }
        return (float)jointAngle;
    }

    // public static Transform DebugTransform;
    #region Tools
    private Vector3 CalculateTargetOffset(IServo servo, Vector3 targetPoint)
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
    public void CalculateIK(bool adjustHeight = false)
    {
        var targetOffset = CalculateTargetOffset(GetHipSegment().GetServos()[0], IKTarget.position);
        var x = targetOffset.z;
        var y = targetOffset.y;
        CalculateIK(x, y);
    }
    public void CalculateIK(float targetX, float targetY)
    {
        double distance = Math.Sqrt(targetX * targetX + targetY * targetY);

        var armLength1 = m_hipRoboticLimbSegment.GetLength();// .13f;
        var armLength2 = m_kneeRoboticLimbSegment.GetLength();// .13f;

        float angle1;
        float angle2;

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

            //   Debug.Log("Angles " + jointAngle1 + " : " + jointAngle2);

            //IServo baseServo = GetBaseSegment().GetServos()[0];

            //var localPoint = baseServo.GetGameObject().transform.parent.InverseTransformPoint(IKTarget.position);
            //localPoint.z += GetBaseSegment().GetLength();
            //// var globalPoint = baseServo.GetGameObject().transform.parent.TransformPoint(IKTarget.position);
            //var targetOffset = localPoint;
            ////var targetOffset = CalculateTargetOffset(baseServo, globalPoint);
            //var adjValue = -targetOffset.z;
            //var oppositeValue = -targetOffset.y;

            //float jointAngle = 0;

            //jointAngle = RadToDegree(Math.Atan2((double)oppositeValue, (double)adjValue));

           // GetBaseSegment().GetServos()[0].SetServoPosition(jointAngle);

            //  if (!float.IsNaN(jointAngle1))
            {
                GetHipSegment().GetServos()[0].SetServoPosition(jointAngle1);
            }
            //if (!float.IsNaN(jointAngle2))
            {
                GetKneeSegment().GetServos()[0].SetServoPosition(jointAngle2);
            }
        }
    }
 
    //public void PositionGaitHeight(float height)
    //{
    //    //  Debug.Log("set limb height");
    //    m_positionerOffset.y = -m_desiredLimbHeight;// - height;

    //    var gaitPos = m_heightAdjustementOrigin.TransformPoint(m_positionerOffset);
    //    m_heightAdjustment.position = Vector3.Lerp(m_heightAdjustment.position, gaitPos, Time.deltaTime * 1.5f);// .8f);
    //}
    public IRoboticLimbSegment GetBaseSegment()
    {
        return m_baseRoboticLimbSegment;
    }

    //public Vector3 SetLegPosition()

    public IRoboticLimbSegment[] GetLimbSegments()
    {
        return new IRoboticLimbSegment[3] { m_baseRoboticLimbSegment, m_hipRoboticLimbSegment, m_kneeRoboticLimbSegment };
    }

    //[SerializeField]
    //public Vector3 GetFootOffset()
    //{
    //    var RoboticLimbSegment = GetComponent<ThreeJointRoboticLimb>().GetSegments()[2];
    //    return transform.InverseTransformPoint(RoboticLimbSegment.GetGameObject().transform.position);    
    //}

    //public Vector3 GetHipOffset()
    //{
    //    var RoboticLimbSegment = GetComponent<ThreeJointRoboticLimb>().GetSegments()[0];
    //    return transform.InverseTransformPoint(RoboticLimbSegment.GetGameObject().transform.position);
    //}

    public IRoboticLimbSegment GetHipSegment()
    {
        return m_hipRoboticLimbSegment;
    }

    //public Vector3 GetKneeOffset()
    //{
    //    var RoboticLimbSegment = GetKneeSegment();
    //    return transform.InverseTransformPoint(RoboticLimbSegment.GetGameObject().transform.position);
    //}

    public IRoboticLimbSegment GetKneeSegment()
    {
        return m_kneeRoboticLimbSegment;// GetComponent<ThreeJointRoboticLimb>().WristRoboticLimbSegmentController.GetRoboticLimbSegment();// GetRoboticLimbSegmentControllers()[2].GetRoboticLimbSegment();
    }


}
#endregion