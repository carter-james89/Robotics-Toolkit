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
    public void CalculateIK1()
    {
        //  m_anchorMatrix.SetTRS(GetHipSegment().GetGameObject().transform.position, GetBaseSegment().GetGameObject().transform.rotation, Vector3.one);
        var offset = IKTarget.localPosition - m_hipRoboticLimbSegment.GetGameObject().transform.localPosition;
        // var offset = m_anchorMatrix.inverse.MultiplyPoint(IKTarget.transform.position);
        //var offset = m_hipRoboticLimbSegment.GetGameObject().transform.InverseTransformPoint(IKTarget.transform.position);
        double targetX = offset.x; // Desired end effector x-coordinate
        double targetY = offset.y; // Desired end effector y-coordinate
        double segmentLength1 = m_hipRoboticLimbSegment.GetLength(); // Length of the first robot arm segment
        double segmentLength2 = m_kneeRoboticLimbSegment.GetLength(); // Length of the second robot arm segment

        //double[] angles = CalculateInverseKinematics(targetX, targetY, segmentLength1, segmentLength2);

        //  GetHipSegment().GetServos()[0].SetServoPosition(RadToDegree(angles[0]));
        //GetKneeSegment().GetServos()[0].SetServoPosition(RadToDegree(angles[1]));

        // Debug.Log((RadToDegree(angles[1])));

        //Console.WriteLine("Joint angles: Theta1 = " + angles[0] + ", Theta2 = " + angles[1]);
    }

    private static float RadToDegree(double radian)
    {
        return (float)(radian * (180 / Math.PI));
    }

    public void CalculateIK()
    {
       // var offset = IKTarget.localPosition - m_hipRoboticLimbSegment.GetGameObject().transform.localPosition;
        // var offset = m_anchorMatrix.inverse.MultiplyPoint(IKTarget.transform.position);
        var offset = m_hipRoboticLimbSegment.GetGameObject().transform.parent.InverseTransformPoint(IKTarget.transform.position);

       // Debug.Log(offset);

        double desiredX = .2f;// -offset.y; // Desired end effector x-coordinate
        double desiredY = .2f;// offset.z; // Desired end effector y-coordinate
        IKTarget.transform.position = m_hipRoboticLimbSegment.GetGameObject().transform.parent.TransformPoint(new Vector3(0, -(float)desiredY, (float)desiredX));

        // Arm segment lengths
        double segment1Length = .13;// m_hipRoboticLimbSegment.GetLength(); 
        double segment2Length = .13;// m_kneeRoboticLimbSegment.GetLength();

        // Calculate the distance from the base to the end effector
        double distance = Math.Sqrt((desiredX * desiredX) + (desiredY * desiredY));

        // Check if the desired position is reachable
        if (distance > segment1Length + segment2Length)
        {
            Debug.LogWarning("Desired position is out of reach!");
            return;
        }

        // Calculate the angles using inverse kinematics
        double cosAngle2 = (distance * distance - segment1Length * segment1Length - segment2Length * segment2Length) / (2 * segment1Length * segment2Length);
        double sinAngle2 = Math.Sqrt(1 - (cosAngle2 * cosAngle2));

        double angle1 = Math.Atan2(desiredY, desiredX) - Math.Atan2(segment2Length * sinAngle2, segment1Length + segment2Length * cosAngle2);
        double angle2 = Math.Atan2(sinAngle2, cosAngle2);

        // Convert the angles from radians to degrees
        double angle1Degrees = angle1 * (180.0 / Math.PI);
        double angle2Degrees = angle2 * (180.0 / Math.PI);

        // Print the calculated angles
       // Console.WriteLine("Joint 1 angle: " + angle1Degrees + " degrees");
       // Console.WriteLine("Joint 2 angle: " + angle2Degrees + " degrees");

        GetHipSegment().GetServos()[0].SetServoPosition((float)angle1Degrees);
        GetKneeSegment().GetServos()[0].SetServoPosition((float)angle2Degrees);
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
        // var servoValues = CalculateDuelIK(m_hipRoboticLimbSegment.GetServos()[0], m_kneeRoboticLimbSegment.GetServos()[0], m_contactPoint.transform.position, IKTarget.position);

        //    GetHipSegment().GetServos()[0].SetServoPosition(servoValues.Key);
        //   GetKneeSegment().GetServos()[0].SetServoPosition(servoValues.Value);

        var targetOffset = CalculateTargetOffset(GetHipSegment().GetServos()[0], IKTarget.position);
        var adjValue = targetOffset.z;
        var oppositeValue = targetOffset.y;

        var targetZ = targetOffset.z;
        var targetY = targetOffset.y;

        var servo1Length = .13f;
        var servo2Length = .13f;

        double dist = Math.Sqrt(Math.Pow(targetZ, 2) + Math.Pow(targetY, 2));

        // Calculate the angle between the leg and the target position
        double theta1 = Math.Atan2(targetY, targetZ);

        // Law of Cosines to calculate the angle between the servos
        double theta2 = Math.Acos((Math.Pow(dist, 2) + Math.Pow(servo1Length, 2) - Math.Pow(servo2Length, 2)) / (2 * dist * servo1Length));

        // Calculate the angles for the servos
        double servo1Angle = theta1 - Math.Asin((servo1Length * Math.Sin(theta2)) / dist);
        double servo2Angle = Math.PI - theta2;


        GetHipSegment().GetServos()[0].SetServoPosition(RadToDegree(servo1Angle));
        GetKneeSegment().GetServos()[0].SetServoPosition(RadToDegree(servo2Angle));

        //CalculateIK();
        //if (adjustHeight)
        //{
        //    heightOffset = transform.position.y - m_baseTarget.position.y;
        //    PositionGaitHeight(-heightOffset);
        //}
        //else
        //{
        //    PositionGaitHeight(0);
        //}
        // var tempPos = transform.InverseTransformPoint(IKTarget.position);
        // tempPos.x -= m_hipFootOffset;
        // var baseTarget = transform.TransformPoint(tempPos);
        // if (DebugCube)
        // {
        //     DebugCube.position = baseTarget;
        // }
        // var limbBaseAngle = IKCalculator.CalculateSingleIK(m_shoulderServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
        //baseTarget, true);

        // m_shoulderServoController.SetAndRunServo(limbBaseAngle, positionServoImmediate);


        //  var elbowWristAngles = IKCalculator.CalculateDuelIK(GetHipSegment().GetServos()[0].GetGameObject().GetComponent<ArticulationBody>(),
        //      m_kneeRoboticLimbSegment.GetServos()[0].GetGameObject().GetComponent<ArticulationBody>(),
        //      m_contactPoint.transform.position, IKTarget.position);

        ////  m_elbowServoController.SetAndRunServo(elbowWristAngles.Key, positionServoImmediate);
        ////  m_wristServoController.SetAndRunServo(elbowWristAngles.Value, positionServoImmediate);

        //  GetHipSegment().GetServos()[0].SetServoPosition(elbowWristAngles.Key);
        //  GetKneeSegment().GetServos()[0].SetServoPosition(elbowWristAngles.Value);

        //m_elbowServoController.SetAndRunServo(ServoAngle, positionServoImmediate);
        //m_wristServoController.SetAndRunServo(ServoAngle, positionServoImmediate);
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