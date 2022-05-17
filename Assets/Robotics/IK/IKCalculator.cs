using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKCalculator : MonoBehaviour
{
    private static Matrix4x4 m_anchorMatrix;



    private static Transform m_transform;

    public static KeyValuePair<float, float> CalculateDuelIK(ArticulationBody elbow, ArticulationBody wrist, Vector3 limbEndPoint, Vector3 targetPoint)
    {
  

        var wristAngle = CalculateSingleIK(wrist, targetPoint);

        var localPoint = elbow.transform.InverseTransformPoint(wrist.transform.position);
        localPoint.x = 0;
        var adjustedWristPoint = elbow.transform.TransformPoint(localPoint);

        localPoint = elbow.transform.InverseTransformPoint(targetPoint);
        localPoint.x = 0;
        var adjustedTargetPoint = elbow.transform.TransformPoint(localPoint);

        localPoint = elbow.transform.InverseTransformPoint(limbEndPoint);
        localPoint.x = 0;
        var adjustedEndPoint = elbow.transform.TransformPoint(localPoint);

       var d1 = CalculateSingleIK(elbow,targetPoint);
        // var elbowTargetOffset = CalculateTargetOffset(elbow,targetPoint);
        //var d1 = Math.Atan(-elbowTargetOffset.y / elbowTargetOffset.z);
        //d1 *= (180 / Math.PI);

        var targetDistC = Vector3.Distance(adjustedTargetPoint, elbow.transform.position);
        var childTargetDistB = Vector3.Distance(adjustedEndPoint, adjustedWristPoint);
        var distToChildServoA = Vector3.Distance(elbow.transform.position, adjustedWristPoint);
        var d2 = LawOfCosines(distToChildServoA, childTargetDistB, targetDistC);

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


    public static float CalculateSingleIK(ArticulationBody articulationBody, Vector3 targetPoint, bool flip = false)
    {
        if (m_transform == null)
        {
            m_transform = new GameObject("debug").transform;
        }

        var targetOffset = CalculateTargetOffset(articulationBody, targetPoint);
        var adjValue = targetOffset.z;
        var oppositeValue = targetOffset.y;

        float jointAngle = 0;

        if (targetOffset.z > 0)
        {
            jointAngle = RadToDegree(Math.Atan(oppositeValue / adjValue));
        }
        else
        {
            jointAngle = RadToDegree(Math.Atan(oppositeValue / -adjValue));
            if (targetOffset.y > 0)
            {
                jointAngle = 180 - jointAngle;
            }
            else
            {
                jointAngle = -180 - jointAngle;
            }
        }
        if (flip)
        {
            jointAngle *= -1;
        }
        return (float)jointAngle;
    }

    #region Tools
    private static Vector3 CalculateTargetOffset(ArticulationBody servo, Vector3 targetPoint)
    {
        m_anchorMatrix = new Matrix4x4();

        var globalPosition = servo.transform.parent.TransformPoint(servo.parentAnchorPosition);
        var globalRotation = servo.transform.parent.rotation * servo.parentAnchorRotation; //articulationBody.transform.TransformVector(articulationBody.anchorRotation.eulerAngles);

        // globalPosition = articulationBody.transform.TransformPoint(articulationBody.anchorPosition);
        // globalRotation = articulationBody.transform.rotation * articulationBody.anchorRotation; //articulationBody.transform.TransformVector(articulationBody.anchorRotation.eulerAngles);
        //m_transform.position = globalPosition;
        //m_transform.rotation = globalRotation;

        m_anchorMatrix.SetTRS(globalPosition, globalRotation, Vector3.one);


        return m_anchorMatrix.inverse.MultiplyPoint(targetPoint);
    }
    private static float RadToDegree(double radian)
    {
        return (float)(radian * (180 / Math.PI));
    }
    private static float Convert360Euler(float euler)
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
    public static float LawOfCosines(float a, float b, float c)
    {
        var topEqu = (Math.Pow(c, 2) + Math.Pow(a, 2) - Math.Pow(b, 2));
        var bottomEqu = 2 * a * c;
        var angle = topEqu / bottomEqu;
        angle = (float)Math.Acos(angle);
        angle = (float)(angle * 180 / Math.PI);
        return (float)angle;
    }
    #endregion
}
