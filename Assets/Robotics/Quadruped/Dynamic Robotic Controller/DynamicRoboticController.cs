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

    private QuadrupedLeg m_frMirrorLeg;
    private QuadrupedLeg m_flMirrorLeg;
    private QuadrupedLeg m_brMirrorLeg;
    private QuadrupedLeg m_blMirrorLeg;

    private QuadrupedLeg m_frIKLeg;
    private QuadrupedLeg m_flIKLeg;
    private QuadrupedLeg m_brIKLeg;
    private QuadrupedLeg m_blIKLeg;

    private Dictionary<IQuadrupedLeg, IQuadrupedLeg> m_limbMirrors = new Dictionary<IQuadrupedLeg, IQuadrupedLeg>();

    public struct LegBundle
    {
        public IQuadrupedLeg MirrorLeg;
        public IQuadrupedLeg IKLeg;
        public IQuadrupedLeg RobotLeg;

        public LegBundle(IQuadrupedLeg mirrorLeg, IQuadrupedLeg ikLeg, IQuadrupedLeg robotLeg)
        {
            this.MirrorLeg = mirrorLeg;
            this.IKLeg = ikLeg;
            this.RobotLeg = robotLeg;
        }
    }
    private List<LegBundle> m_legBundles = new List<LegBundle>();

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

        m_flMirrorLeg = ConstructDynamicLeg(m_robot.GetLegs()[0], "FL Mirror Leg", Color.black, true);
        m_frMirrorLeg = ConstructDynamicLeg(m_robot.GetLegs()[1],"FR Mirror Leg", Color.black);
        m_brMirrorLeg = ConstructDynamicLeg(m_robot.GetLegs()[2],"BR Mirror Leg", Color.black);
        m_blMirrorLeg = ConstructDynamicLeg(m_robot.GetLegs()[3], "BL Mirror Leg", Color.black, true);

        m_flIKLeg = ConstructDynamicLeg(m_robot.GetLegs()[0], "FL IK Leg", Color.green, true);
        m_frIKLeg = ConstructDynamicLeg(m_robot.GetLegs()[1], "FR IK Leg", Color.green);
        m_brIKLeg = ConstructDynamicLeg(m_robot.GetLegs()[2], "BR IK Leg", Color.green);
        m_blIKLeg = ConstructDynamicLeg(m_robot.GetLegs()[3], "BL IK Leg", Color.green, true);

        m_dynamicLimbPrefab.gameObject.SetActive(false);

        m_legBundles.Add(new LegBundle(m_flMirrorLeg, m_flIKLeg, m_robot.GetLegs()[0]));
        m_legBundles.Add(new LegBundle(m_frMirrorLeg, m_frIKLeg , m_robot.GetLegs()[1]));
        m_legBundles.Add(new LegBundle(m_brMirrorLeg, m_brIKLeg, m_robot.GetLegs()[2]));
        m_legBundles.Add(new LegBundle(m_blMirrorLeg, m_blIKLeg, m_robot.GetLegs()[3]));

        foreach (var limb in m_robot.GetLegs())
        {
            //var newLeg
            // ConstructDynamicLeg(limb);
            //var newServo = Instantiate(m_servoPrefab);
            //newServo.gameObject.transform.SetParent(transform);
            //newServo.transform.localEulerAngles = limb.
        }
    }

    private QuadrupedLeg ConstructDynamicLeg(IQuadrupedLeg leg, string name, Color color, bool left = false)
    {
        var newLeg = Instantiate(m_dynamicLimbPrefab).GetComponent<QuadrupedLeg>();
        newLeg.name = name; 
        newLeg.transform.SetParent(transform);
        newLeg.transform.localPosition = leg.GetGameObject().transform.localPosition;


        newLeg.transform.localEulerAngles = new Vector3(0, 90, 0);

        if (left)
        {
        //    newLeg.transform.localEulerAngles = new Vector3(0, -90, 0);
        }
        else
        {
            //newLeg.transform.localEulerAngles = new Vector3(0, 90, 0);
            //foreach (var item in newLeg.GetLimbSegments())
            //{
            //    item.GetGameObject().transform.localEulerAngles += new Vector3(0, 180, 0);
            //}
        //    newLeg.GetHipSegment().GetGameObject().transform.parent.localEulerAngles += new Vector3(0, 180, 0);
            //   newLeg.GetKneeSegment().GetGameObject().transform.localEulerAngles += new Vector3(0, 180, 0);
        }
     //   m_limbMirrors.Add(leg, newLeg);

        newLeg.GetBaseSegment().GetGameObject().transform.localPosition = Vector3.zero;
        var hipOffset = m_robot.GetGameObject().transform.InverseTransformPoint(leg.GetHipSegment().GetGameObject().transform.position);
        newLeg.GetHipSegment().GetGameObject().transform.parent.position = transform.TransformPoint(hipOffset);
     //   var kneeOffset = m_robot.GetGameObject().transform.InverseTransformPoint(leg.GetKneeSegment().GetGameObject().transform.position);
       // newLeg.GetKneeSegment().GetGameObject().transform.parent.position = transform.TransformPoint(kneeOffset);
        newLeg.GetKneeSegment().GetGameObject().transform.parent.localPosition = new Vector3(0,0, leg.GetHipSegment().GetLength());
        //var tempos = newLeg.GetKneeServo().GetGameObject().transform.position;
        // tempos.y = 0;
        newLeg.GetContactPoint().transform.localPosition = new Vector3(0,0, leg.GetKneeSegment().GetLength());
        var ikPoint = transform.TransformPoint(m_robot.GetGameObject().transform.InverseTransformPoint(leg.GetContactPoint().transform.position));
        newLeg.IKTarget.position = ikPoint;
        //newLeg.GetKneeSegment().get

        Debug.Log("Construct leg mirror " + leg.GetGameObject().name);
        // leg.GetKneeServo().GetGameObject().transform.localPosition;
        // newLeg.GetHipServo().GetGameObject().transform.position = leg.GetHipServo().GetGameObject().transform.position;
        // newLeg.GetKneeServo().GetGameObject().transform.position = leg.GetKneeServo().GetGameObject().transform.position;
        foreach (var item in newLeg.GetLimbSegments())
        {
            
            item.SetRenderType(IRoboticLimbSegment.RenderType.Line, color);
        }
        return newLeg;
    }

    // Update is called once per frame
    void Update()
    {
        var tempPos = transform.localPosition;
        tempPos.y = m_robot.GetGameObject().transform.localPosition.y;
        transform.localPosition = tempPos;

        transform.rotation = m_robot.GetGameObject().transform.rotation;

        foreach (var limbPair in m_legBundles)
        {
           // limbPair.Key.GetPositioner().SetLimbPosition(m_robot.GetGameObject().transform.TransformPoint(GetRobotGimbalOffset((limbPair.Value as QuadrupedLeg).IKTarget.position)),false);
        }
        var servo = m_robot.GetLegs()[0].GetHipSegment().GetServos()[0];
       // Debug.Log(servo.GetGameObject().name + " " + servo.GetCurrentAngle());
       // int legCount = 0;
        foreach (var bundle in m_legBundles)
        {
           var limbSegments = bundle.MirrorLeg.GetLimbSegments();
            //  Debug.Log(limbSegments[1].GetServos()[0].GetGameObject().name, limbSegments[1].GetServos()[0].GetGameObject());

          //  if ((bundle.IKLeg as QuadrupedLeg) == m_frIKLeg)
            {
                limbSegments[1].GetServos()[0].SetServoPosition(bundle.RobotLeg.GetHipSegment().GetServos()[0].GetCurrentAngle());
                limbSegments[2].GetServos()[0].SetServoPosition(-bundle.RobotLeg.GetKneeSegment().GetServos()[0].GetCurrentAngle());
            }
      
            bundle.IKLeg.CalculateIK();
            //legCount++;
        }

        foreach (var bundle in m_legBundles)
        {
            //if ((bundle.IKLeg as QuadrupedLeg) == m_frIKLeg)
            {
               // Debug.Log(bundle.IKLeg.GetHipSegment().GetServos()[0].GetCurrentAngle());

                bundle.RobotLeg.GetKneeSegment().GetServos()[0].SetServoPosition(-bundle.IKLeg.GetKneeSegment().GetServos()[0].GetCurrentAngle());
                bundle.RobotLeg.GetHipSegment().GetServos()[0].SetServoPosition(bundle.IKLeg.GetHipSegment().GetServos()[0].GetCurrentAngle());

               // bundle.RobotLeg.GetHipSegment().GetServos()[0].GetCurrentAngle();
            }
        }

        //foreach (var limbPair in m_limbMirrors)
        //{
        //    limbPair.Value.GetBaseSegment().GetServos()[0].SetServoPosition(limbPair.Key.GetBaseSegment().GetServos()[0].GetCurrentAngle());
        //    limbPair.Value.GetHipSegment().GetServos()[0].SetServoPosition(limbPair.Key.GetHipSegment().GetServos()[0].GetCurrentAngle());
        //    limbPair.Value.GetKneeSegment().GetServos()[0].SetServoPosition(limbPair.Key.GetKneeSegment().GetServos()[0].GetCurrentAngle());
        //}
    }
    public Vector3 GetRobotGimbalOffset(Vector3 globalPos)
    {
        return m_robot.GetGameObject().transform.InverseTransformPoint(globalPos);
    }
    public Vector3 GetGimbalOffset(Vector3 globalPos)
    {
        return transform.InverseTransformPoint(globalPos);
    }
}
