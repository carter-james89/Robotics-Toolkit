using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCurveTesting : MonoBehaviour
{



    public AnimationCurve gaitCurve;
    public float gaitCurveY, gaitCurveX;


    private float m_strideTime = 0;

    private void Awake()
    {
    }

    private void Start()
    {
        var timeLength = Math.Abs(gaitCurve.keys[0].time) + gaitCurve.keys[gaitCurve.keys.Length - 1].time;

        var begining = gaitCurve.keys[0].time;
        var ending = gaitCurve.keys[gaitCurve.keys.Length - 1].time;

        m_strideTime = begining;

        var middle = (begining + ending)/ 2;

       // transform.position = new Vector3(middle, 0, 0);
        StrideStart.localPosition = new Vector3(-middle, 0, 0);
        StrideEnd.localPosition = new Vector3(middle, 0, 0);
    }


    public Transform StrideStart;
    public Transform StrideEnd;
    public Transform Target;

    public void Update()
    {
   


        var currentStrideTime = m_strideTime + Time.deltaTime /10000;
        if (currentStrideTime > gaitCurve.keys[gaitCurve.keys.Length - 1].time)
        {
            m_strideTime = gaitCurve.keys[gaitCurve.keys.Length - 1].time;
           // var gaitCurveY = gaitCurve.Evaluate(currentStrideTime);

        }
        else
        {
            m_strideTime = currentStrideTime;
        } 

        //gaitCurveX = (avgStridePercent + .03f) * timeLength;

        //Target.localPosition = StrideStart.localPosition + new Vector3(currentStrideTime, 0, 0);

        var gaitCurveY = gaitCurve.Evaluate(currentStrideTime);

        Target.localPosition = StrideStart.localPosition + new Vector3(currentStrideTime, gaitCurveY, 0);

        //Target.position = pointBack.TransformPoint(new Vector3(0, gaitCurveY, gaitCurveX));


    }

  
}

