using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Gaits;
using RoboticsToolkit.Robotics.Limbs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Utilities.Events;
using UnityEngine;
using RoboticsToolkit.Robotics.RoboticControllers;



public class DynamicRoboticController : MonoBehaviour, IRoboticController, IRobotEventListener
{
    private InterfaceEventManager<IRoboticControllerEventListener> _eventManager = new InterfaceEventManager<IRoboticControllerEventListener>("Controlelr");
    // private IQuadruped _quadruped;

    private QuadrupedLeg m_frMirrorLeg;
    private QuadrupedLeg m_flMirrorLeg;
    private QuadrupedLeg m_brMirrorLeg;
    private QuadrupedLeg m_blMirrorLeg;

    private QuadrupedLeg m_frIKLeg;
    private QuadrupedLeg m_flIKLeg;
    private QuadrupedLeg m_brIKLeg;
    private QuadrupedLeg m_blIKLeg;

    //[SerializeField]
    //private EulerServo m_servoPrefab;
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

    private IRoboticLimb[] MirrorLimbs;
    private IRoboticLimb[] IKLimbs;
    private AdvancedLimbPositioner[] IKLimbPositioners;

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

    private IRobot _robot;

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

    public bool Initialize(IRobot quadToControl)
    {
        Debug.Log("Initialize Robotic Controller");
        _robot = quadToControl;
        transform.localPosition = quadToControl.GetGameObject().transform.localPosition;
        transform.localRotation = quadToControl.GetGameObject().transform.localRotation;

        GetComponent<MeshFilter>().mesh = quadToControl.GetGameObject().GetComponent<MeshFilter>().mesh;
        ConstructQuadrupedTwin(_robot.GetLimbs());
    

        _gaitController = GetComponent<GaitController>();
        _gaitController.Initialize(IKLimbPositioners, quadToControl.GetLimbs());

        CalculateLimbData(_robot);

        (quadToControl as IRobot).SubscribeToEvents(this);


        NotifyEventListeners(IRoboticControllerEventListener.EventType.OnControllerInitialized);
        return true;
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
        NotifyEventListeners(IRoboticControllerEventListener.EventType.OnHeightAdjustmentBegin);
    }

    #region Construction

    public void ConstructQuadrupedTwin(IRoboticLimb[] limbs)
    {
        Debug.Log("Construct digital twin");
        //  _quadruped = robot;
        //  transform.localPosition = _quadruped.GetGameObject().transform.localPosition;
        //  transform.localRotation = _quadruped.GetGameObject().transform.localRotation;

        //  GetComponent<MeshFilter>().mesh = robot.GetGameObject().GetComponent<MeshFilter>().mesh;

        m_flMirrorLeg = ConstructDynamicLeg(limbs[0], "FL Mirror Leg", Color.black, true);
        m_frMirrorLeg = ConstructDynamicLeg(limbs[1], "FR Mirror Leg", Color.black);
        m_brMirrorLeg = ConstructDynamicLeg(limbs[2], "BR Mirror Leg", Color.black);
        m_blMirrorLeg = ConstructDynamicLeg(limbs[3], "BL Mirror Leg", Color.black, true);

        m_flIKLeg = ConstructDynamicLeg(limbs[0], "FL IK Leg", Color.green, true);
        m_frIKLeg = ConstructDynamicLeg(limbs[1], "FR IK Leg", Color.green);
        m_brIKLeg = ConstructDynamicLeg(limbs[2], "BR IK Leg", Color.green);
        m_blIKLeg = ConstructDynamicLeg(limbs[3], "BL IK Leg", Color.green, true);

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
          
        
        }

        MirrorLimbs = new IRoboticLimb[4] { m_flMirrorLeg, m_frMirrorLeg, m_brMirrorLeg, m_blMirrorLeg };
        IKLimbs = new IRoboticLimb[4] { m_flIKLeg, m_frIKLeg, m_brIKLeg, m_blIKLeg };
        IKLimbPositioners = new AdvancedLimbPositioner[4];
        for (int i = 0; i < 4; i++)
        {
            IKLimbPositioners[i] = Instantiate(_limbPositionerPrefab).GetComponent<AdvancedLimbPositioner>();
            IKLimbPositioners[i].name += MirrorLimbs[i].GetGameObject().name;
            IKLimbPositioners[i].gameObject.SetActive(true);
            IKLimbPositioners[i].transform.SetParent(m_ikTargets, false);
            IKLimbPositioners[i].transform.position = MirrorLimbs[i].GetEndPoint().position;
            _robotHeight = -transform.InverseTransformPoint(IKLimbPositioners[i].transform.position).y;
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
        var hipOffset = _robot.GetGameObject().transform.InverseTransformPoint(ogSegments[1].GetGameObject().transform.position);
        newLeg.GetHipSegment().GetGameObject().transform.parent.position = transform.TransformPoint(hipOffset);
        newLeg.GetKneeSegment().GetGameObject().transform.parent.localPosition = new Vector3(0, 0, ogSegments[1].GetLength());
        newLeg.GetContactPoint().transform.localPosition = new Vector3(0, 0, ogSegments[2].GetLength());
        var ikPoint = transform.TransformPoint(_robot.GetGameObject().transform.InverseTransformPoint(leg.GetEndPoint().transform.position));
        newLeg.IKTarget.position = ikPoint;

        foreach (var item in newLeg.GetLimbSegments())
        {
            item.SetRenderType(IRoboticLimbSegment.RenderType.Line, color);
        }

        newLeg.SetLimbValues(0, leg.GetSegments()[1].GetServoAngle(0), leg.GetSegments()[2].GetServoAngle(0));
        return newLeg;
    }
    #endregion

    [SerializeField]
    private float _activeBalanceSpeed = 1;





    public LimbValues[] CalculateLimbData(IRobot quadToControl)
    {
        if (_robot == null)
        {
            return null;
        }
        var tempPos = transform.localPosition;
        tempPos.y = _robot.GetGameObject().transform.localPosition.y;
        transform.localPosition = tempPos;

        transform.rotation = _robot.GetGameObject().transform.rotation;

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
                mirrorSegments[0].SetServoAngle(0, robotSegments[0].GetServoAngle(0));

            mirrorSegments[1].SetServoAngle(0, robotSegments[1].GetServoAngle(0));
            mirrorSegments[2].SetServoAngle(0, robotSegments[2].GetServoAngle(0));
        }

        //_gaitController.Run(m_legBundles.Select(bundle => bundle.MirrorLeg).ToArray(), m_legBundles.Select(bundle => bundle.IKLimbPositioner).ToArray());//have the gait controller position the ik targets
       _gaitController.Run();
        // QuadrupedLimbData returnData = new QuadrupedLimbData();

        var returnData = new LimbValues[4];
     
        for (int i = 0; i < 4; i++)
        {
            var ikSegments = IKLimbs[i].GetSegments();
            var positioner = IKLimbPositioners[i];

            positioner.transform.localPosition = new Vector3(positioner.transform.localPosition.x, -_robotHeight, positioner.transform.localPosition.z);
            positioner.Run();//have the positioner move its target
            IKLimbs[i].SetIKTargetPos((positioner as AdvancedLimbPositioner).GetTargetGlobalPosition());// put the IK leg target at the same place as it's positioner target

            (IKLimbs[i] as QuadrupedLeg).CalculateIK();//calculate the IK

            returnData[i].LimbTarget = (positioner as AdvancedLimbPositioner).GetTargetGlobalPosition();

            returnData[i].ServoAngles = new float[4];

            returnData[i].ServoAngles[0] = ikSegments[0].GetServoAngle();
            returnData[i].ServoAngles[1] = ikSegments[1].GetServoAngle();
            returnData[i].ServoAngles[2] = ikSegments[2].GetServoAngle();
        }

        if (_adjustingHeight)
        {
            if (_robotHeight == _targetHeight)
            {
                _adjustingHeight = false; // Stop adjusting when the target height is reached
                NotifyEventListeners(IRoboticControllerEventListener.EventType.OnHeightAdjustmentEnd);
            }
        }
      //  return null;
        return returnData;
    }
    public Vector3 GetRobotGimbalOffset(Vector3 globalPos)
    {
        return _robot.GetGameObject().transform.InverseTransformPoint(globalPos);
    }
    public Vector3 GetGimbalOffset(Vector3 globalPos)
    {
        return transform.InverseTransformPoint(globalPos);
    }

    private float _robotHeight;
    void Update()
    {


    }

    public void SubscribeToControllerEvents(IRoboticControllerEventListener listener)
    {
        _eventManager.AddListener(listener);
    }
    public void UnsubscribeFromControllerEvents(IRoboticControllerEventListener listener)
    {
        _eventManager.RemoveListener(listener);
    }
    private void NotifyEventListeners(IRoboticControllerEventListener.EventType type)
    {
        foreach (var item in _eventManager.GetListeners())
        {
            item.OnControllerEventOccured(new IRoboticControllerEventListener.QuadrupedRoboticControllerEvendData(type, this, _robot));
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
                //AdvancedLimbPositioner[] positioners = m_legBundles.Select(bundle => bundle.IKLimbPositioner).ToArray();

                // _gaitController.BeginMovement(positioners, IGaitController.GaitPattern.STATIONARYSTEP, Vector3.zero, false);
                // _gaitController.PerformHighStep(.05f, .01f);
                (_gaitController as GaitController).CrawlForward(IKLimbPositioners, .03f, .01f, .04f);
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

    public GameObject GetGameObject()
    {
        throw new NotImplementedException();
    }

  

    public bool SetTransformValues()
    {
        throw new NotImplementedException();
    }

    public void ResetController()
    {
        throw new NotImplementedException();
    }

    public bool IsSimulator()
    {
        throw new NotImplementedException();
    }
}
