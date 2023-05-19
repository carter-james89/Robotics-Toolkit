using RoboticsToolkit.Robotics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicRoboticController : MonoBehaviour
{
    [SerializeField]
    private GameObject m_robotObject;

    private IQuadruped m_robot;

    private QuadrupedLeg m_frLeg;
    private QuadrupedLeg m_flLeg;
    private QuadrupedLeg m_brLeg;
    private QuadrupedLeg m_blLeg;


    private void Start()
    {
        ConstructQuadrupedTwin(m_robotObject.GetComponent<IQuadruped>());
    }
    [SerializeField]
    private EulerServo m_servoPrefab;
    [SerializeField]
    private QuadrupedLeg m_dynamicLimbPrefab;
    private void ConstructQuadrupedTwin(IQuadruped robot)
    {
        m_robot = robot;
        transform.localPosition = m_robot.GetGameObject().transform.localPosition;
        transform.localRotation = m_robot.GetGameObject().transform.localRotation;

        m_flLeg = ConstructDynamicLeg( m_robot.GetLegs()[0]);
        m_frLeg = ConstructDynamicLeg( m_robot.GetLegs()[1]);
        m_brLeg = ConstructDynamicLeg( m_robot.GetLegs()[2]);
        m_blLeg = ConstructDynamicLeg( m_robot.GetLegs()[3]);



        m_dynamicLimbPrefab.gameObject.SetActive(false);

        foreach (var limb in m_robot.GetLegs())
        {
            //var newLeg
           // ConstructDynamicLeg(limb);
            //var newServo = Instantiate(m_servoPrefab);
            //newServo.gameObject.transform.SetParent(transform);
            //newServo.transform.localEulerAngles = limb.
        }     
        

    }

    private QuadrupedLeg ConstructDynamicLeg(IQuadrupedLeg leg, bool left = false)
    {
        var newLeg = Instantiate(m_dynamicLimbPrefab).GetComponent<QuadrupedLeg>();
        newLeg.transform.SetParent(transform);
        newLeg.transform.localPosition = leg.GetGameObject().transform.localPosition;

        if (left)
        {
            newLeg.transform.localEulerAngles = new Vector3(0, -90, 0);
        }
        else
        {
            newLeg.transform.localEulerAngles = new Vector3(0, 90, 0);
        }

        //newLeg.GetBaseServo().GetGameObject().transform.localPosition = Vector3.zero;
        var hipOffset = m_robot.GetGameObject().transform.InverseTransformPoint(leg.GetHipServo().GetGameObject().transform.position);
        newLeg.GetHipServo().GetGameObject().transform.position = transform.TransformPoint(hipOffset);
        var kneeOffset = m_robot.GetGameObject().transform.InverseTransformPoint(leg.GetKneeServo().GetGameObject().transform.position);
        newLeg.GetKneeServo().GetGameObject().transform.position = transform.TransformPoint(kneeOffset);

        //var tempos = newLeg.GetKneeServo().GetGameObject().transform.position;
       // tempos.y = 0;

       

        Debug.Log("Construct leg mirror " + leg.GetGameObject().name);
        // leg.GetKneeServo().GetGameObject().transform.localPosition;
       // newLeg.GetHipServo().GetGameObject().transform.position = leg.GetHipServo().GetGameObject().transform.position;
       // newLeg.GetKneeServo().GetGameObject().transform.position = leg.GetKneeServo().GetGameObject().transform.position;

        return newLeg;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
