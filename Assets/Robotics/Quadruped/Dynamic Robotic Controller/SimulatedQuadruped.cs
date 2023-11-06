using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using ProcessCommunicationToolkit.SocketPortConnection;

public interface IQuadruped
{
    public GameObject GetGameObject();
    public IRoboticLimb[] GetLimbs();

    public void PositionTransform();
}


public class SimulatedQuadruped : MonoBehaviour, IQuadruped, IRobot, IPortCommunicationEventListener
{
    [SerializeField]
    private IRoboticLimb[] m_limbs;

    public void PositionTransform() { }

    [SerializeField]
    private QuadrupedLeg m_frLimb;
    [SerializeField]
    private QuadrupedLeg m_flLimb;
    [SerializeField]
    private QuadrupedLeg m_brLimb;
    [SerializeField]
    private QuadrupedLeg m_blLimb;

    private Vector3 m_startPos;

    private bool m_firstReset = true;


    [SerializeField]
    private bool m_runOwnIK;

    [SerializeField]
    private Transform m_baseTargets;

    public IGimbal Gimbal { get; private set; }

    [SerializeField]
    private float m_walkHeight = .2f;

    private enum Status
    {
        NotRunning,
        Resetting,
        MovingToStartPosition,
        Ready,
    }
    private Status m_status = Status.NotRunning;

    private void Awake()
    {
        m_startPos = transform.localPosition;

        Gimbal = GetComponentInChildren<IGimbal>();
        m_limbs = new IRoboticLimb[4] { m_flLimb, m_frLimb, m_brLimb, m_blLimb };



    }
    PortCommunication server;
    private void Start()
    {

      //  server = new PortCommunication();
      //  server.SubscribeToCommunicatonEvents(this);
        // server.ConnectToServer();
       // client = new UDPCommunicationManager(49512, "192.168.86.27");
        //client.uplinkMessage += OnUDPManagerMessageThrown;
        //client.EstablishConnection();

        ResetController();
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

    private void OnUDPManagerMessageThrown(string obj)
    {
        Debug.Log(obj);
    }
    private void OnDestroy()
    {
        client.uplinkMessage -= OnUDPManagerMessageThrown;
    }

    private float m_resetCount = 0;
    public void ResetController()
    {
        m_resetCount = 0;
        m_status = Status.Resetting;

        foreach (var limb in m_limbs)
        {
            limb.ResetLimb();
        }
    }
    private void FixedUpdate()
    {
        if (m_status == Status.Resetting)
        {
            m_resetCount++;
            if (m_resetCount > 30)
            {
                //  m_transformPositioner.CompletePositionerReset();
                //  m_ground.GetComponent<Collider>().enabled = true;

                if (m_firstReset)
                {
                    NotifyEventListeners(IRobotEventListener.EventType.OnRobotInitialized);
                    m_firstReset = false;
                }

                // m_status = Status.MovingToStartPosition;
                m_status = Status.Ready;
                m_resetCount = 0;
                NotifyEventListeners(IRobotEventListener.EventType.OnReset);
                return;
            }
            var ab = GetGameObject().GetComponent<ArticulationBody>();

            // ab.TeleportRoot(m_ground.position + new Vector3(0, m_startHeight, 0), Quaternion.identity);
            ab.TeleportRoot(m_startPos, Quaternion.identity);
            ab.velocity = Vector3.zero;
            ab.angularVelocity = Vector3.zero;
        }

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

        // Debug.Log(s);
       // if (Input.GetKeyDown(KeyCode.RightAlt))
          //  server.SendMessageToESP32("1");
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


    private List<IRobotEventListener> m_listeners = new List<IRobotEventListener>();
    private UDPCommunicationListener client;

    public void SubscribeToEvents(IRobotEventListener listener)
    {
        if (m_listeners.Contains(listener))
        {
            return;
        }
        m_listeners.Add(listener);
    }

    public void UnsubscribeToEvents(IRobotEventListener listener)
    {
        if (!m_listeners.Contains(listener))
        {
            return;
        }
        m_listeners.Remove(listener);
    }
    private void NotifyEventListeners(IRobotEventListener.EventType eventType)
    {
        foreach (var listener in m_listeners)
        {
            if (listener != null)
            {
                listener.OnRobotEventOccured(new IRobotEventListener.EventData(eventType, this, null));
            }
        }
    }

    public void OnCommunicatonEventOccured(IPortCommunicationEventListener.CommunicationEventData eventData)
    {
        var message = eventData.Message;
    }
}

