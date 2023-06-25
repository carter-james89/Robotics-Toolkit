using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
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

    //private Dictionary<IRoboticLimb, IRoboticLimb> m_limbMirrors = new Dictionary<IRoboticLimb, IRoboticLimb>();

    public struct LegBundle
    {
        public IRoboticLimb MirrorLeg;
        public IRoboticLimb IKLeg;
        public IRoboticLimb RobotLeg;

        public LegBundle(IRoboticLimb mirrorLeg, IRoboticLimb ikLeg, IRoboticLimb robotLeg)
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

        GetComponent<MeshFilter>().mesh = robot.GetGameObject().GetComponent<MeshFilter>().mesh;

        m_flMirrorLeg = ConstructDynamicLeg(m_robot.GetLimbs()[0], "FL Mirror Leg", Color.black, true);
        m_frMirrorLeg = ConstructDynamicLeg(m_robot.GetLimbs()[1],"FR Mirror Leg", Color.black);
        m_brMirrorLeg = ConstructDynamicLeg(m_robot.GetLimbs()[2],"BR Mirror Leg", Color.black);
        m_blMirrorLeg = ConstructDynamicLeg(m_robot.GetLimbs()[3], "BL Mirror Leg", Color.black, true);

        m_flIKLeg = ConstructDynamicLeg(m_robot.GetLimbs()[0], "FL IK Leg", Color.green, true);
        m_frIKLeg = ConstructDynamicLeg(m_robot.GetLimbs()[1], "FR IK Leg", Color.green);
        m_brIKLeg = ConstructDynamicLeg(m_robot.GetLimbs()[2], "BR IK Leg", Color.green);
        m_blIKLeg = ConstructDynamicLeg(m_robot.GetLimbs()[3], "BL IK Leg", Color.green, true);

        m_dynamicLimbPrefab.gameObject.SetActive(false);

        m_legBundles.Add(new LegBundle(m_flMirrorLeg, m_flIKLeg, m_robot.GetLimbs()[0]));
        m_legBundles.Add(new LegBundle(m_frMirrorLeg, m_frIKLeg , m_robot.GetLimbs()[1]));
        m_legBundles.Add(new LegBundle(m_brMirrorLeg, m_brIKLeg, m_robot.GetLimbs()[2]));
        m_legBundles.Add(new LegBundle(m_blMirrorLeg, m_blIKLeg, m_robot.GetLimbs()[3]));

        foreach (var limb in m_robot.GetLimbs())
        {
            //var newLeg
            // ConstructDynamicLeg(limb);
            //var newServo = Instantiate(m_servoPrefab);
            //newServo.gameObject.transform.SetParent(transform);
            //newServo.transform.localEulerAngles = limb.
        }
    }

    private QuadrupedLeg ConstructDynamicLeg(IRoboticLimb leg, string name, Color color, bool left = false)
    {
        var newLeg = Instantiate(m_dynamicLimbPrefab).GetComponent<QuadrupedLeg>();
        newLeg.name = name; 
        newLeg.transform.SetParent(transform);
        newLeg.transform.localPosition = leg.GetGameObject().transform.localPosition;

        newLeg.transform.localEulerAngles = new Vector3(0, 270, 180);

        newLeg.m_invert = left;

        var ogSegments = leg.GetSegments();
        //newLeg.GetBaseSegment().GetGameObject().transform.localPosition = Vector3.zero;
        var hipOffset = m_robot.GetGameObject().transform.InverseTransformPoint(ogSegments[1].GetGameObject().transform.position);
        newLeg.GetHipSegment().GetGameObject().transform.parent.position = transform.TransformPoint(hipOffset);
        newLeg.GetKneeSegment().GetGameObject().transform.parent.localPosition = new Vector3(0,0, ogSegments[1].GetLength());
        newLeg.GetContactPoint().transform.localPosition = new Vector3(0,0, ogSegments[2].GetLength());
        var ikPoint = transform.TransformPoint(m_robot.GetGameObject().transform.InverseTransformPoint(leg.GetEndPoint().transform.position));
          newLeg.IKTarget.position = ikPoint;
        //newLeg.IKTarget.position = newLeg.GetContactPoint().transform.position;

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
   //     var servo = m_robot.GetLimbs()[0].GetHipSegment().GetServos()[0];
       // Debug.Log(servo.GetGameObject().name + " " + servo.GetCurrentAngle());
       // int legCount = 0;
        foreach (var bundle in m_legBundles)
        {
           var mirrorSegments = bundle.MirrorLeg.GetSegments();
            var robotSegments = bundle.RobotLeg.GetSegments();
            //  Debug.Log(mirrorSegments[1].GetServos()[0].GetGameObject().name, mirrorSegments[1].GetServos()[0].GetGameObject());
            (bundle.IKLeg as QuadrupedLeg).CalculateIK();
            //  if ((bundle.IKLeg as QuadrupedLeg) == m_frIKLeg)
            {
                if(robotSegments[0] != null)
                mirrorSegments[0].GetServos()[0].SetServoPosition(robotSegments[0].GetServos()[0].GetCurrentAngle());
                mirrorSegments[1].GetServos()[0].SetServoPosition(robotSegments[1].GetServos()[0].GetCurrentAngle());
                mirrorSegments[2].GetServos()[0].SetServoPosition(robotSegments[2].GetServos()[0].GetCurrentAngle());
            }
         //   if ((bundle.IKLeg as QuadrupedLeg) == m_flIKLeg)
               // bundle.IKLeg.CalculateIK();
           
            //legCount++;
        }

        foreach (var bundle in m_legBundles)
        {
            var robotSegments = bundle.RobotLeg.GetSegments();
            var ikSegments = bundle.IKLeg.GetSegments();
            // if ((bundle.IKLeg as QuadrupedLeg) == m_flIKLeg)
            {
                // Debug.Log(bundle.IKLeg.GetBaseSegment().GetServos()[0].GetCurrentAngle());
                if (robotSegments[0] != null)
                    robotSegments[0].GetServos()[0].SetServoPosition(ikSegments[0].GetServos()[0].GetCurrentAngle());
                robotSegments[1].GetServos()[0].SetServoPosition( ikSegments[1].GetServos()[0].GetCurrentAngle());
                robotSegments[2].GetServos()[0].SetServoPosition( ikSegments[2].GetServos()[0].GetCurrentAngle());

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
