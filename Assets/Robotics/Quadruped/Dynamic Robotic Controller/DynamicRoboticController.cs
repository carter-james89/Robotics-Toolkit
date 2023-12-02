using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Gaits;
using RoboticToolkit.Robotics.Limbs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Toolkit.Robotics.Quadruped;
using Toolkit.Utilities.Events;
using UnityEngine;

public interface IQuadrupedRoboticController
{
    public void Initialize(IQuadruped quadToControl);
    public void SetRobotHeight(float height, float speed);

    public QuadrupedLimbData CalculateLimbData(IQuadruped quadToControl);
    public void SubscribeToControllerEvents(IQuadrupedRoboticControllerEventListener listener);
    public void UnsubscribeFromControllerEvents(IQuadrupedRoboticControllerEventListener listener);
}
public interface IQuadrupedRoboticControllerEventListener
{
    public enum EventType
    {
        OnControllerInitialized,
        OnHeightAdjustmentBegin,
        OnHeightAdjustmentEnd,
    }
    public class QuadrupedRoboticControllerEvendData
    {
        public EventType EventType;
        public IQuadrupedRoboticController Controller;
        public IRobot Robot;
        public QuadrupedRoboticControllerEvendData(EventType eventType, IQuadrupedRoboticController controller, IRobot robot)
        {
            this.EventType = eventType;
            this.Controller = controller;
            this.Robot = robot;
        }
    }
    public void OnControllerEventOccured(QuadrupedRoboticControllerEvendData eventData);
}
public class DynamicRoboticController : MonoBehaviour, IQuadrupedRoboticController, IRobotEventListener
{
    private InterfaceEventManager<IQuadrupedRoboticControllerEventListener> _eventManager = new InterfaceEventManager<IQuadrupedRoboticControllerEventListener>("Controlelr");
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
    private EulerServo m_servoPrefab;
    [SerializeField]
    private QuadrupedLeg m_dynamicLimbPrefab;

    private float _targetHeight;
    private float _heightAdjustmentSpeed;
    private bool _adjustingHeight = false;

    private IGaitController _gaitController;
    //  private ILimbPositioner _limbP

    [SerializeField]
    private Transform m_heightController;

    [SerializeField]
    private AdvancedLimbPositioner _limbPositionerPrefab;

    public class LegBundle
    {
        public IRoboticLimb MirrorLeg;
        public IRoboticLimb IKLeg;
        public AdvancedLimbPositioner IKLimbPositioner;

        public LegBundle(IRoboticLimb mirrorLeg, IRoboticLimb ikLeg)
        {
            this.MirrorLeg = mirrorLeg;
            this.IKLeg = ikLeg;
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

    void Awake()
    {
        _limbPositionerPrefab.gameObject.SetActive(false);
    }

    public void Initialize(IQuadruped quadToControl)
    {
        Debug.Log("Initialize Robotic Controller");
        ConstructQuadrupedTwin(quadToControl);
        CalculateLimbData(quadToControl);

        _gaitController = GetComponent<GaitController>();
        _gaitController.Initialize(quadToControl as IRobot);

        (quadToControl as IRobot).SubscribeToEvents(this);


        NotifyEventListeners(IQuadrupedRoboticControllerEventListener.EventType.OnControllerInitialized);
    }

    public float GetCurrentHeight()
    {
        return -transform.InverseTransformPoint(m_ikTargets.position).y;
    }
    public void SetRobotHeight(float height, float speed)
    {
        var dif = GetCurrentHeight() - height;
        // _targetHeight = m_ikTargets.localPosition.y + dif;
        _targetHeight = height;
        Debug.Log("Set new Height : " + _targetHeight);
        _heightAdjustmentSpeed = speed;
        _adjustingHeight = true;
        NotifyEventListeners(IQuadrupedRoboticControllerEventListener.EventType.OnHeightAdjustmentBegin);
    }

    #region Construction

    public void ConstructQuadrupedTwin(IQuadruped robot)
    {
        Debug.Log("Construct digital twin");
        _quadruped = robot;
        transform.localPosition = _quadruped.GetGameObject().transform.localPosition;
        transform.localRotation = _quadruped.GetGameObject().transform.localRotation;

        GetComponent<MeshFilter>().mesh = robot.GetGameObject().GetComponent<MeshFilter>().mesh;

        m_flMirrorLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[0], "FL Mirror Leg", Color.black, true);
        m_frMirrorLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[1], "FR Mirror Leg", Color.black);
        m_brMirrorLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[2], "BR Mirror Leg", Color.black);
        m_blMirrorLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[3], "BL Mirror Leg", Color.black, true);

        m_flIKLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[0], "FL IK Leg", Color.green, true);
        m_frIKLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[1], "FR IK Leg", Color.green);
        m_brIKLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[2], "BR IK Leg", Color.green);
        m_blIKLeg = ConstructDynamicLeg(_quadruped.GetLimbs()[3], "BL IK Leg", Color.green, true);

        m_dynamicLimbPrefab.gameObject.SetActive(false);

        m_legBundles.Add(new LegBundle(m_flMirrorLeg, m_flIKLeg));
        m_legBundles.Add(new LegBundle(m_frMirrorLeg, m_frIKLeg));
        m_legBundles.Add(new LegBundle(m_brMirrorLeg, m_brIKLeg));
        m_legBundles.Add(new LegBundle(m_blMirrorLeg, m_blIKLeg));

        m_ikTargets.transform.position = transform.position;
        m_ikTargets.transform.rotation = transform.rotation;

        // m_ikTargets.position = transform.position;// new Vector3(transform.position.x, m_blMirrorLeg.GetEndPoint().position.y, transform.position.z);

        foreach (var limbBundle in m_legBundles)
        {
            limbBundle.IKLimbPositioner = Instantiate(_limbPositionerPrefab).GetComponent<AdvancedLimbPositioner>();
            limbBundle.IKLimbPositioner.gameObject.SetActive(true);
            limbBundle.IKLimbPositioner.transform.SetParent(m_ikTargets, false);
            limbBundle.IKLimbPositioner.transform.position = limbBundle.MirrorLeg.GetEndPoint().position;
            _robotHeight = -transform.InverseTransformPoint(limbBundle.IKLimbPositioner.transform.position).y;
        }
        Debug.Log("Start Robot Height : " + _robotHeight);
        //foreach (var bundle in m_legBundles)
        //{
        //    (bundle.IKLeg as QuadrupedLeg).IKTarget.SetParent(m_ikTargets);
        //}
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
        var hipOffset = _quadruped.GetGameObject().transform.InverseTransformPoint(ogSegments[1].GetGameObject().transform.position);
        newLeg.GetHipSegment().GetGameObject().transform.parent.position = transform.TransformPoint(hipOffset);
        newLeg.GetKneeSegment().GetGameObject().transform.parent.localPosition = new Vector3(0, 0, ogSegments[1].GetLength());
        newLeg.GetContactPoint().transform.localPosition = new Vector3(0, 0, ogSegments[2].GetLength());
        var ikPoint = transform.TransformPoint(_quadruped.GetGameObject().transform.InverseTransformPoint(leg.GetEndPoint().transform.position));
        newLeg.IKTarget.position = ikPoint;

        foreach (var item in newLeg.GetLimbSegments())
        {
            item.SetRenderType(IRoboticLimbSegment.RenderType.Line, color);
        }

        newLeg.SetLimbValues(0, leg.GetSegments()[1].GetServos()[0].GetCurrentAngle(), leg.GetSegments()[2].GetServos()[0].GetCurrentAngle());
        return newLeg;
    }
    #endregion

    [SerializeField]
    private float _activeBalanceSpeed = 1;
    public QuadrupedLimbData CalculateLimbData(IQuadruped quadToControl)
    {
        if (_quadruped == null)
        {
            return null;
        }
        var tempPos = transform.localPosition;
        tempPos.y = _quadruped.GetGameObject().transform.localPosition.y;
        transform.localPosition = tempPos;

        transform.rotation = _quadruped.GetGameObject().transform.rotation;

        if (!_adjustingHeight && (Vector3.Angle(transform.up, Vector3.up) > 5))
        {
            var tempRot = m_ikTargets.localEulerAngles;
            // tempRot.x = 0;
            tempRot.y = 0;
            m_ikTargets.localEulerAngles = tempRot;
            var targetRot = transform.eulerAngles;
            targetRot.y = 0;
            //  m_ikTargets.localRotation = Quaternion.Lerp(m_ikTargets.localRotation, Quaternion.Euler(targetRot), Time.deltaTime*_activeBalanceSpeed);

        }
        else
        {
            m_ikTargets.transform.localRotation = Quaternion.identity;
        }
        if (_adjustingHeight)
        {
            _robotHeight = Mathf.MoveTowards(_robotHeight, _targetHeight, _heightAdjustmentSpeed * Time.deltaTime);
            Debug.Log("adjusting height : " + _robotHeight);
        }

        for (int i = 0; i < 4; i++)//set the mirror legs to match the real robot
        {
            var mirrorSegments = m_legBundles[i].MirrorLeg.GetSegments();
            var robotSegments = quadToControl.GetLimbs()[i].GetSegments();
            if (robotSegments[0] != null)
                mirrorSegments[0].GetServos()[0].SetServoPosition(robotSegments[0].GetServos()[0].GetCurrentAngle());
            mirrorSegments[1].GetServos()[0].SetServoPosition(robotSegments[1].GetServos()[0].GetCurrentAngle());
            mirrorSegments[2].GetServos()[0].SetServoPosition(robotSegments[2].GetServos()[0].GetCurrentAngle());
        }

        _gaitController.Run(m_legBundles.Select(bundle => bundle.MirrorLeg).ToArray(), m_legBundles.Select(bundle => bundle.IKLimbPositioner).ToArray());//have the gait controller position the ik targets
       
        QuadrupedLimbData returnData = new QuadrupedLimbData();
        for (int i = 0; i < 4; i++)
        {
            var ikSegments = m_legBundles[i].IKLeg.GetSegments();
            var positioner = m_legBundles[i].IKLimbPositioner;

            positioner.transform.localPosition = new Vector3(positioner.transform.localPosition.x, -_robotHeight, positioner.transform.localPosition.z);
            positioner.Run();//have the positioner move its target
            m_legBundles[i].IKLeg.SetIKTargetPos((positioner as AdvancedLimbPositioner).GetTargetGlobalPosition());// put the IK leg target at the same place as it's positioner target

            (m_legBundles[i].IKLeg as QuadrupedLeg).CalculateIK();


            if (i == 0)
            {
                returnData.FLTargetPos = m_legBundles[i].IKLeg.GetIKTargetPos();
                returnData.FLBaseAngle = ikSegments[0].GetServos()[0].GetCurrentAngle();
                returnData.FLHipAngle = ikSegments[1].GetServos()[0].GetCurrentAngle();
                returnData.FLKneeAngle = ikSegments[2].GetServos()[0].GetCurrentAngle();
            }
            else if (i == 1)
            {
                returnData.FRTargetPos = m_legBundles[i].IKLeg.GetIKTargetPos();
                returnData.FRBaseAngle = ikSegments[0].GetServos()[0].GetCurrentAngle();
                returnData.FRHipAngle = ikSegments[1].GetServos()[0].GetCurrentAngle();
                returnData.FRKneeAngle = ikSegments[2].GetServos()[0].GetCurrentAngle();
            }
            else if (i == 2)
            {
                returnData.BRTargetPos = m_legBundles[i].IKLeg.GetIKTargetPos();
                returnData.BRBaseAngle = ikSegments[0].GetServos()[0].GetCurrentAngle();
                returnData.BRHipAngle = ikSegments[1].GetServos()[0].GetCurrentAngle();
                returnData.BRKneeAngle = ikSegments[2].GetServos()[0].GetCurrentAngle();
            }
            else if (i == 3)
            {
                returnData.BLTargetPos = m_legBundles[i].IKLeg.GetIKTargetPos();
                returnData.BLBaseAngle = ikSegments[0].GetServos()[0].GetCurrentAngle();
                returnData.BLHipAngle = ikSegments[1].GetServos()[0].GetCurrentAngle();
                returnData.BLKneeAngle = ikSegments[2].GetServos()[0].GetCurrentAngle();
            }
        }


        if (_adjustingHeight)
        {
            if (_robotHeight == _targetHeight)
            {
                _adjustingHeight = false; // Stop adjusting when the target height is reached
                NotifyEventListeners(IQuadrupedRoboticControllerEventListener.EventType.OnHeightAdjustmentEnd);
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

    private float _robotHeight;
    void Update()
    {


    }

    public void SubscribeToControllerEvents(IQuadrupedRoboticControllerEventListener listener)
    {
        _eventManager.AddListener(listener);
    }
    public void UnsubscribeFromControllerEvents(IQuadrupedRoboticControllerEventListener listener)
    {
        _eventManager.RemoveListener(listener);
    }
    private void NotifyEventListeners(IQuadrupedRoboticControllerEventListener.EventType type)
    {
        foreach (var item in _eventManager.GetListeners())
        {
            item.OnControllerEventOccured(new IQuadrupedRoboticControllerEventListener.QuadrupedRoboticControllerEvendData(type, this, (_quadruped as IRobot)));
        }
    }

    public void OnRobotEventOccured(IRobotEventListener.EventData eventData)
    {
        switch (eventData.EventType)
        {
            case IRobotEventListener.EventType.OnRobotInitialized:
                break;
            case IRobotEventListener.EventType.OnRobotInPosition:
                //foreach (var item in m_legBundles)
                //{
                //    item.IKLimbPositioner.RotateToPosition(item.IKLimbPositioner.transform.position, .1f, 1);
                //}
                AdvancedLimbPositioner[] positioners = m_legBundles.Select(bundle => bundle.IKLimbPositioner).ToArray();

                _gaitController.BeginMovement(positioners, IGaitController.GaitPattern.STATIONARYSTEP, Vector3.zero,false);
                break;
            case IRobotEventListener.EventType.OnLimbsPositioned:
                break;
            case IRobotEventListener.EventType.OnEmergencyStop:
                break;
            case IRobotEventListener.EventType.OnReset:
                break;
            default:
                break;
        }
    }
}
