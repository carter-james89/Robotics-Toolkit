using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IQuadruped
{
    public GameObject GetGameObject();

    public IQuadrupedLeg[] GetLegs();
}


public class SimulatedQuadruped : MonoBehaviour, IQuadruped, IRobot
{
    [SerializeField]
    private IQuadrupedLeg[] m_legs;

    [SerializeField]
    private ThreeJointRoboticLimb m_frLimb;
    [SerializeField]
    private ThreeJointRoboticLimb m_flLimb;
    [SerializeField]
    private ThreeJointRoboticLimb m_brLimb;
    [SerializeField]
    private ThreeJointRoboticLimb m_blLimb;
    private List<ThreeJointRoboticLimb> m_limbs = new List<ThreeJointRoboticLimb>();

    private List<QuadrupedLeg> m_quadrupedLegs = new List<QuadrupedLeg>();

    [SerializeField]
    private Transform m_baseTargets;

    public IGimbal Gimbal { get; private set; }

    [SerializeField]
    private float m_walkHeight = .2f;

    private void Awake()
    {
        Gimbal = GetComponentInChildren<IGimbal>();
        if (m_flLimb)
        {
            m_limbs.Add(m_flLimb);
            m_quadrupedLegs.Add(m_flLimb.GetComponent<QuadrupedLeg>());
        }
        if (m_frLimb)
        {
            m_limbs.Add(m_frLimb);
            m_quadrupedLegs.Add(m_frLimb.GetComponent<QuadrupedLeg>());
        }
        if (m_brLimb)
        {
            m_limbs.Add(m_brLimb);
            m_quadrupedLegs.Add(m_brLimb.GetComponent<QuadrupedLeg>());
        }
        if (m_blLimb)
        {
            m_limbs.Add(m_blLimb);
            m_quadrupedLegs.Add(m_blLimb.GetComponent<QuadrupedLeg>());
        }
    }
    private void Start()
    {

        m_baseTargets.transform.SetParent(transform.parent);

        PositionGimble();
        foreach (var limb in m_limbs)
        {
            limb.GetBaseTarget().SetParent(m_baseTargets);
            var tempPos = limb.GetBaseTarget().localPosition;
            tempPos.y = 0;
            limb.GetBaseTarget().localPosition = tempPos;
            limb.Initialize(this, false);
        }
        m_baseTargets.transform.position = new Vector3(transform.position.x, m_walkHeight, transform.position.z);

    }

    private void FixedUpdate()
    {

        // SetTransformValues();
        RunRoboticController();

    }
    public void RunRoboticController()
    {
        PositionGimble();

        //if (m_status == Status.MovingToStartPosition)
        //{
        //    bool atTarget = true;
        //    foreach (var limb in m_limbs)
        //    {
        //        if (!limb.LimbAtTarget() || !limb.BaseAtTarget())
        //        {
        //            atTarget = false;
        //        }
        //    }
        //    if (atTarget)
        //    {
        //        m_status = Status.Ready;
        //        NotifyEventListeners(IRobotEventListener.EventType.OnRobotInPosition);
        //    }
        //}
        //else
        //{
        foreach (var limb in m_limbs)
        {
            limb.GetPositioner().GetGameObject().transform.rotation = Quaternion.LookRotation(GetGimbal().GetGameObject().transform.forward, GetGimbal().GetGameObject().transform.up);
            limb.RunLimb(false);
        }
        //if (m_gaitController != null && m_gaitController.IsRunning() && IsRunning)
        //{
        //    m_gaitController.Run();
        //}

    }

    //private List<QuadrupedLeg> m_legs = new List<QuadrupedLeg>();
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public IQuadrupedLeg[] GetLegs()
    {
        return m_quadrupedLegs.ToArray();
    }
    private void PositionGimble()
    {
        GetGimbal().GetGameObject().transform.position = transform.position;

        Gimbal.GetGameObject().transform.rotation = transform.rotation;
        var tempEuler = Gimbal.GetGameObject().transform.eulerAngles;
        tempEuler.x = 0;
        tempEuler.z = 0;
        Gimbal.GetGameObject().transform.rotation = Quaternion.Euler(tempEuler);

        var tempPos = m_baseTargets.transform.position;
        tempPos.x = transform.position.x;
        tempPos.z = transform.position.z;
        m_baseTargets.transform.position = tempPos;
        tempEuler = m_baseTargets.eulerAngles;
        tempEuler.y = Gimbal.GetGameObject().transform.eulerAngles.y;
        m_baseTargets.eulerAngles = tempEuler;

        var angle = Vector3.Angle(transform.up, Gimbal.GetGameObject().transform.up);
        //if (angle > m_emergencyStopAngle && IsRunning)
        //{
        //    EmergencyStop();
        //}
    }
    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        PositionGimble();
    }

    public IRoboticLimb[] GetLimbs()
    {
        throw new System.NotImplementedException();
    }

    public IRobot.RobotData GetRobotData()
    {
        throw new System.NotImplementedException();
    }

    public IGimbal GetGimbal()
    {
        return Gimbal;
    }

    public void EmergencyStop()
    {
        throw new System.NotImplementedException();
    }

    public void ResetController()
    {
        throw new System.NotImplementedException();
    }

    public void SubscribeToEvents(IRobotEventListener listener)
    {
        throw new System.NotImplementedException();
    }

    public void UnsubscribeToEvents(IRobotEventListener listener)
    {
        throw new System.NotImplementedException();
    }
}

