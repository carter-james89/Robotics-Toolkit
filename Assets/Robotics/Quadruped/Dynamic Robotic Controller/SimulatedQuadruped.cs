using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IQuadruped
{
    public GameObject GetGameObject();
    public IRoboticLimb[] GetLimbs();
}


public class SimulatedQuadruped : MonoBehaviour, IQuadruped, IRobot
{
    [SerializeField]
    private IRoboticLimb[] m_limbs;

    [SerializeField]
    private QuadrupedLeg m_frLimb;
    [SerializeField]
    private QuadrupedLeg m_flLimb;
    [SerializeField]
    private QuadrupedLeg m_brLimb;
    [SerializeField]
    private QuadrupedLeg m_blLimb;

    [SerializeField]
    private bool m_runOwnIK;

    [SerializeField]
    private Transform m_baseTargets;

    public IGimbal Gimbal { get; private set; }

    [SerializeField]
    private float m_walkHeight = .2f;

    private void Awake()
    {
        Gimbal = GetComponentInChildren<IGimbal>();
        m_limbs = new IRoboticLimb[4] { m_flLimb , m_frLimb , m_brLimb , m_blLimb };
    }
    private void Start()
    {
       // m_baseTargets.transform.SetParent(transform.parent);

        PositionGimble();
        foreach (var limb in m_limbs)
        {
           // var 
           //// limb.GetBaseTarget().SetParent(m_baseTargets);
           // var tempPos = limb.GetBaseTarget().localPosition;
           // tempPos.y = 0;
           // limb.GetBaseTarget().localPosition = tempPos;
            //limb.Initialize(this, false);
        }
       // m_baseTargets.transform.position = new Vector3(transform.position.x, m_walkHeight, transform.position.z);
    }

    private void FixedUpdate()
    {
        RunRoboticController();
    }
    public void RunRoboticController()
    {
        if (!m_runOwnIK)
        {
            return;
        }
        PositionGimble();

        foreach (var limb in m_limbs)
        {
            //limb.GetPositioner().GetGameObject().transform.rotation = Quaternion.LookRotation(GetGimbal().GetGameObject().transform.forward, GetGimbal().GetGameObject().transform.up);
           // limb.RunLimb(false);
        }
    }
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    private void PositionGimble()
    {
        GetGimbal().GetGameObject().transform.position = transform.position;

        Gimbal.GetGameObject().transform.rotation = transform.rotation;
        var tempEuler = Gimbal.GetGameObject().transform.eulerAngles;
        tempEuler.x = 0;
        tempEuler.z = 0;
        Gimbal.GetGameObject().transform.rotation = Quaternion.Euler(tempEuler);

        //var tempPos = m_baseTargets.transform.position;
        //tempPos.x = transform.position.x;
        //tempPos.z = transform.position.z;
        //m_baseTargets.transform.position = tempPos;
        //tempEuler = m_baseTargets.eulerAngles;
        //tempEuler.y = Gimbal.GetGameObject().transform.eulerAngles.y;
        //m_baseTargets.eulerAngles = tempEuler;

        var angle = Vector3.Angle(transform.up, Gimbal.GetGameObject().transform.up);
        //if (angle > m_emergencyStopAngle && IsRunning)
        //{
        //    EmergencyStop();
        //}
    }
    void Update()
    {
        PositionGimble();
    }

    public IRoboticLimb[] GetLimbs()
    {
        return m_limbs;
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

