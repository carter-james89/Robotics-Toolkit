using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using System;
using System.Collections;
using System.Collections.Generic;
using Toolkit.Robotics.Quadruped;
using UnityEngine;




public interface IQuadrupedRoboticController
{
  
    public void Initialize(IQuadruped quadToControl);

    public QuadrupedLimbData CalculateLimbData(IQuadruped quadToControl);
}
public class DynamicRoboticController : MonoBehaviour, IQuadrupedRoboticController
{

    private IQuadruped _quadruped;

    private QuadrupedLeg m_frMirrorLeg;
    private QuadrupedLeg m_flMirrorLeg;
    private QuadrupedLeg m_brMirrorLeg;
    private QuadrupedLeg m_blMirrorLeg;

    private QuadrupedLeg m_frIKLeg;
    private QuadrupedLeg m_flIKLeg;
    private QuadrupedLeg m_brIKLeg;
    private QuadrupedLeg m_blIKLeg;

    [SerializeField]
    private Transform m_heightController;

    //private Dictionary<IRoboticLimb, IRoboticLimb> m_limbMirrors = new Dictionary<IRoboticLimb, IRoboticLimb>();

    public class LegBundle
    {
        public IRoboticLimb MirrorLeg;
        public IRoboticLimb IKLeg;
        public IRoboticLimb RobotLeg;
        public Vector3 DefaultPositionOffset;

        public LegBundle(IRoboticLimb mirrorLeg, IRoboticLimb ikLeg, IRoboticLimb robotLeg)
        {
            this.MirrorLeg = mirrorLeg;
            this.IKLeg = ikLeg;
            this.RobotLeg = robotLeg;
        }
    }
    private List<LegBundle> m_legBundles = new List<LegBundle>();

    [SerializeField]
    private Transform m_ikTargets;

    private enum Status
    {
        NotRunning,
        Resetting,
        MovingToStartPosition,
        Ready,
    }
    private Status m_status = Status.NotRunning;

    private void Start()
    {

        
    }


    [SerializeField]
    private EulerServo m_servoPrefab;
    [SerializeField]
    private QuadrupedLeg m_dynamicLimbPrefab;
    public void ConstructQuadrupedTwin(IQuadruped robot)
    {
        Debug.Log("Construct digital twin");
        _quadruped = robot;
        transform.localPosition = _quadruped.GetGameObject().transform.localPosition;
        transform.localRotation = _quadruped.GetGameObject().transform.localRotation;

        GetComponent<MeshFilter>().mesh = robot.GetGameObject().GetComponent<MeshFilter>().mesh;

      Debug.Log(_quadruped.GetLimbs()[0]);

        m_flMirrorLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[0], "FL Mirror Leg", Color.black, true);
        m_frMirrorLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[1],"FR Mirror Leg", Color.black);
        m_brMirrorLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[2],"BR Mirror Leg", Color.black);
        m_blMirrorLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[3], "BL Mirror Leg", Color.black, true);

        m_flIKLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[0], "FL IK Leg", Color.green, true);
        m_frIKLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[1], "FR IK Leg", Color.green);
        m_brIKLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[2], "BR IK Leg", Color.green);
        m_blIKLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[3], "BL IK Leg", Color.green, true);

        m_dynamicLimbPrefab.gameObject.SetActive(false);

        m_legBundles.Add(new LegBundle(m_flMirrorLeg, m_flIKLeg, _quadruped.GetLimbs()[0]));
        m_legBundles.Add(new LegBundle(m_frMirrorLeg, m_frIKLeg , _quadruped.GetLimbs()[1]));
        m_legBundles.Add(new LegBundle(m_brMirrorLeg, m_brIKLeg, _quadruped.GetLimbs()[2]));
        m_legBundles.Add(new LegBundle(m_blMirrorLeg, m_blIKLeg, _quadruped.GetLimbs()[3]));

        m_ikTargets.transform.position = transform.position;
        m_ikTargets.transform.rotation = transform.rotation;

        foreach (var bundle in m_legBundles)
        {
            (bundle.IKLeg as QuadrupedLeg).IKTarget.SetParent(m_ikTargets);
        }

            foreach (var limb in _quadruped.GetLimbs())
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
       // Debug.Log("Got limb segments " + ogSegments.Length);
        //Debug.Log(_quadruped.GetGameObject());
       // Debug.Log(ogSegments[1].GetGameObject());
        //newLeg.GetBaseSegment().GetGameObject().transform.localPosition = Vector3.zero;
        var hipOffset = _quadruped.GetGameObject().transform.InverseTransformPoint(ogSegments[1].GetGameObject().transform.position);
        newLeg.GetHipSegment().GetGameObject().transform.parent.position = transform.TransformPoint(hipOffset);
        newLeg.GetKneeSegment().GetGameObject().transform.parent.localPosition = new Vector3(0,0, ogSegments[1].GetLength());
        newLeg.GetContactPoint().transform.localPosition = new Vector3(0,0, ogSegments[2].GetLength());
        var ikPoint = transform.TransformPoint(_quadruped.GetGameObject().transform.InverseTransformPoint(leg.GetEndPoint().transform.position));
          newLeg.IKTarget.position = ikPoint;
        //newLeg.IKTarget.position = newLeg.GetContactPoint().transform.position;

        foreach (var item in newLeg.GetLimbSegments())
        {          
            item.SetRenderType(IRoboticLimbSegment.RenderType.Line, color);
        }

        newLeg.SetLimbValues(0, leg.GetSegments()[1].GetServos()[0].GetCurrentAngle(), leg.GetSegments()[2].GetServos()[0].GetCurrentAngle());
        return newLeg;
    }

    public QuadrupedLimbData CalculateLimbData(IQuadruped quadToControl)
    {
        if(_quadruped == null)
        {
            return null;
        }
        var tempPos = transform.localPosition;
        tempPos.y = _quadruped.GetGameObject().transform.localPosition.y;
        transform.localPosition = tempPos;

        transform.rotation = _quadruped.GetGameObject().transform.rotation;



        //foreach (var bundle in m_legBundles)
        //{
        //   var mirrorSegments = bundle.MirrorLeg.GetSegments();
        //    var robotSegments = bundle.RobotLeg.GetSegments();

        //    (bundle.IKLeg as QuadrupedLeg).CalculateIK();
        //    {
        //        if(robotSegments[0] != null)
        //        mirrorSegments[0].GetServos()[0].SetServoPosition(robotSegments[0].GetServos()[0].GetCurrentAngle());
        //        mirrorSegments[1].GetServos()[0].SetServoPosition(robotSegments[1].GetServos()[0].GetCurrentAngle());
        //        mirrorSegments[2].GetServos()[0].SetServoPosition(robotSegments[2].GetServos()[0].GetCurrentAngle());
        //    }
        //}
        QuadrupedLimbData returnData = new QuadrupedLimbData();
        for (int i = 0; i < 4; i++)
        {
            var mirrorSegments = m_legBundles[i].MirrorLeg.GetSegments();
            var robotSegments = m_legBundles[i].RobotLeg.GetSegments();
            var ikSegments = m_legBundles[i].IKLeg.GetSegments();

            (m_legBundles[i].IKLeg as QuadrupedLeg).CalculateIK();

            if (robotSegments[0] != null)
                mirrorSegments[0].GetServos()[0].SetServoPosition(robotSegments[0].GetServos()[0].GetCurrentAngle());
            mirrorSegments[1].GetServos()[0].SetServoPosition(robotSegments[1].GetServos()[0].GetCurrentAngle());
            mirrorSegments[2].GetServos()[0].SetServoPosition(robotSegments[2].GetServos()[0].GetCurrentAngle());

            if (i == 0)
            {
                returnData.FLBaseAngle = ikSegments[0].GetServos()[0].GetCurrentAngle();
                returnData.FLHipAngle = ikSegments[1].GetServos()[0].GetCurrentAngle();
                returnData.FLKneeAngle = ikSegments[2].GetServos()[0].GetCurrentAngle();
            }
            else if (i == 1)
            {
                returnData.FRBaseAngle = ikSegments[0].GetServos()[0].GetCurrentAngle();
                returnData.FRHipAngle = ikSegments[1].GetServos()[0].GetCurrentAngle();
                returnData.FRKneeAngle = ikSegments[2].GetServos()[0].GetCurrentAngle();
            }
            else if (i == 2)
            {
                returnData.BRBaseAngle = ikSegments[0].GetServos()[0].GetCurrentAngle();
                returnData.BRHipAngle = ikSegments[1].GetServos()[0].GetCurrentAngle();
                returnData.BRKneeAngle = ikSegments[2].GetServos()[0].GetCurrentAngle();
            }
            else if (i == 3)
            {
                returnData.BLBaseAngle = ikSegments[0].GetServos()[0].GetCurrentAngle();
                returnData.BLHipAngle = ikSegments[1].GetServos()[0].GetCurrentAngle();
                returnData.BLKneeAngle = ikSegments[2].GetServos()[0].GetCurrentAngle();
            }    
        }
        return returnData;
    }
    public Vector3 GetRobotGimbalOffset(Vector3 globalPos)
    {
        return _quadruped.GetGameObject().transform.InverseTransformPoint(globalPos);
    }
    public Vector3 GetGimbalOffset(Vector3 globalPos)
    {
        return transform.InverseTransformPoint(globalPos);
    }

    void Update()
    {
        foreach (var limbBundle in m_legBundles)
        {
            Debug.Log("set ik to correct position");
            //  limbBundle.IKLeg.SetIKTargetPos(limbBundle.IKLeg.GetGameObject().transform.TransformPoint(limbBundle.DefaultPositionOffset));
            //   limbBundle.IKLeg.SetIKTargetPos(limbBundle.MirrorLeg.GetEndPoint().position);


          //  limbBundle.DefaultPositionOffset = limbBundle.MirrorLeg.GetGameObject().transform.InverseTransformPoint(limbBundle.MirrorLeg.GetEndPoint().position);
          //  limbBundle.IKLeg.SetIKTargetPos(limbBundle.IKLeg.GetGameObject().transform.TransformPoint(limbBundle.DefaultPositionOffset));
        }
    }

    Vector3 _defaultOffset;


    public void Initialize(IQuadruped quadToControl)
    {
        Debug.Log("Initialize Robotic Controller");
        ConstructQuadrupedTwin(quadToControl);
        CalculateLimbData(quadToControl);

        foreach (var limbBundle in m_legBundles)
        {
            Debug.Log("set ik to correct position");
           
            Debug.Log("Mirror End Point", limbBundle.MirrorLeg.GetEndPoint());
            //  Debug.Log("IK End Point", limbBundle.IKLeg.GetIKTargetPos());
            limbBundle.DefaultPositionOffset = limbBundle.MirrorLeg.GetGameObject().transform.InverseTransformPoint(limbBundle.MirrorLeg.GetEndPoint().position);
         //   limbBundle.IKLeg.SetIKTargetPos(limbBundle.MirrorLeg.GetEndPoint().position);
        }
    }

}
