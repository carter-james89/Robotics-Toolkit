
using RoboticsToolkit.Robotics.Gaits;
using System;
using UnityEngine;

public class AnimationCurveTesting : MonoBehaviour
{
    [SerializeField]
    private Transform m_target;

    public AnimationCurve gaitCurve;
    public float gaitCurveY, gaitCurveX;

    private float m_strideTime = 0;

    public Transform StrideStart;

   // private int[] m_positioningLimbs = new int[2];

    public enum StrideType
    {
        NONE,
        STATIONARYSTEP,
        WALKING
    }
    private StrideType m_currentStride = StrideType.NONE;

    private float m_stideTimeFull;

    public void Initialize()
    {

        m_stideTimeFull = Math.Abs(gaitCurve.keys[0].time) + gaitCurve.keys[gaitCurve.keys.Length - 1].time;

        var begining = gaitCurve.keys[0].time;
        var ending = gaitCurve.keys[gaitCurve.keys.Length - 1].time;

        m_strideTime = begining;

        var middle = (begining + ending) / 2;

        m_totalGaitDistance = ending;

        // transform.position = new Vector3(middle, 0, 0);
        StrideStart.localPosition = new Vector3(-middle, 0, 0);
        // StrideEnd.localPosition = new Vector3(middle, 0, 0);
    }

    private int m_totalGaitTime = 1;
    private float m_totalGaitDistance = 0;
    private float m_gaitVelocity;

    private float m_completionTime = 0;

    [SerializeField]
    private float m_desiredStrideTime = 1;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_currentStride = StrideType.WALKING;
           // m_positioningLimbs[0] = 0;
          //  m_positioningLimbs[1] = 2;

            m_totalGaitDistance = gaitCurve.keys[gaitCurve.keys.Length - 1].time;

            m_gaitVelocity = m_totalGaitDistance / m_desiredStrideTime;
            Debug.Log("Total gait distance : " + m_totalGaitDistance);
            Debug.Log("Total gait time  " + m_desiredStrideTime);
            Debug.Log("set gait velocity to : " + m_gaitVelocity);
            // SetNextGaitCycle();
        }

        if (m_currentStride == StrideType.NONE)
        {
            return;
        }

        m_completionTime += Time.deltaTime;
        var currentStrideTime = m_strideTime + Time.deltaTime * m_gaitVelocity;
        if (currentStrideTime > gaitCurve.keys[gaitCurve.keys.Length - 1].time)
        {
            m_strideTime = gaitCurve.keys[gaitCurve.keys.Length - 1].time;
            Debug.Log(m_completionTime);
            m_currentStride = StrideType.NONE;
        }
        else
        {
            m_strideTime = currentStrideTime;
        }



        if (m_strideTime == gaitCurve.keys[gaitCurve.keys.Length - 1].time)
        {
          //  m_strideTime = gaitCurve.keys[0].time;
        }

        //gaitCurveX = (avgStridePercent + .03f) * timeLength;
        //Target.localPosition = StrideStart.localPosition + new Vector3(currentStrideTime, 0, 0);    

        var gaitCurveY = gaitCurve.Evaluate(m_strideTime);
        var desiredPos = new Vector3(0, gaitCurveY, currentStrideTime);

       // Debug.Log("position to " + desiredPos);
        m_target.position = desiredPos;

        //Target.position = pointBack.TransformPoint(new Vector3(0, gaitCurveY, gaitCurveX));
        // for (int i = 0; i < m_limbs.Length; i++)
        // {
      //  m_limbs[m_positioningLimbs[0]].SetIKTargetPos(desiredPos);
       // m_limbs[m_positioningLimbs[1]].SetIKTargetPos(desiredPos);
        // }

    }

    private void SetNextGaitCycle()
    {
        //var flLimb = m_limbs[0];
        //var frLimb = m_limbs[1];
        //var brLimb = m_limbs[2];
        //var blLimb = m_limbs[3];

        //m_stridePosition++;
        //if (m_stridePosition == 3)
        //{
        //    m_stridePosition = 1;
        //}
        //switch (m_stridePosition)
        //{
        //    case 1:
        //        frLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);
        //        blLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);

        //        flLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
        //        brLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
        //        break;
        //    case 2:
        //        flLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);
        //        brLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);

        //        frLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
        //        blLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
        //        break;
        //    default:
        //        break;
        //}
    }
}


