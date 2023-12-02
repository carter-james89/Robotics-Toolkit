using RoboticToolkit.Robotics.Gaits;
using RoboticToolkit.Robotics.Limbs;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{
    public class Quadruped : MonoBehaviour, IRobot
    {
        private enum ControlType
        {
            Simulation,
            Arduino,
            ArduinoSimulatedSensors
        }
        [SerializeField]
        private ControlType m_controlType;
        [SerializeField]
        private float m_walkHeight = .2f;
        [SerializeField]
        private float m_physicsTime = 1;
        [SerializeField]
        private bool m_useGimbalLimbHeight = true;
        [SerializeField]
        private float m_emergencyStopAngle = 20;
        [SerializeField]
        private Transform m_gaits;
        [SerializeField]
        private Transform m_baseTargets;
        [SerializeField]
        private ThreeJointRoboticLimb m_frLimb;
        [SerializeField]
        private ThreeJointRoboticLimb m_flLimb;
        [SerializeField]
        private ThreeJointRoboticLimb m_brLimb;
        [SerializeField]
        private ThreeJointRoboticLimb m_blLimb;
        [SerializeField]
        private GameObject m_ground;
        [SerializeField]
        private Transform m_com;

       // private bool m_resetPosition = false;
        private QuadrupedGroundStationData m_servoValues;
        private bool m_firstReset = true;
        private float m_resetCount = 0;

        private List<IRobotEventListener> m_listeners = new List<IRobotEventListener>();
        private enum Status
        {
            NotRunning,
            Resetting,
            MovingToStartPosition,
            Ready,
        }
        private Status m_status = Status.NotRunning;
        private ArticulationBody m_articulationBody;
        private IServoCMDRelay m_servoCMDRelay;
        private IQuadrupedPositioner m_transformPositioner;
        private bool m_ready = true;
        private IGaitController m_gaitController;
        private float m_startHeight;
        private List<ThreeJointRoboticLimb> m_limbs = new List<ThreeJointRoboticLimb>();

        public bool IsRunning { get; private set; } = true;
        public IGimbal Gimbal { get; private set; }
        public GameObject GetGameObject() => gameObject;
        public IRobot.RobotData GetRobotData() { return new IRobot.RobotData(m_articulationBody.velocity, m_articulationBody.angularVelocity); }
        public IRoboticLimb[] GetLimbs() => m_limbs.ToArray();

        void Awake()
        {
            Gimbal = GetComponentInChildren<IGimbal>();
        }


        void Start()
        {
            m_startHeight = transform.localPosition.y;
            m_gaitController = GetComponent<IGaitController>();
            m_articulationBody = GetComponent<ArticulationBody>();

            if (m_flLimb)
            {
                m_limbs.Add(m_flLimb);
            }
            if (m_frLimb)
            {
                m_limbs.Add(m_frLimb);
            }
            if (m_brLimb)
            {
                m_limbs.Add(m_brLimb);
            }
            if (m_blLimb)
            {
                m_limbs.Add(m_blLimb);
            }

            m_gaits.transform.SetParent(transform.parent);
            m_baseTargets.transform.SetParent(transform.parent);

            PositionGimble();
            foreach (var limb in m_limbs)
            {
                limb.GetBaseTarget().SetParent(m_baseTargets);
                var tempPos = limb.GetBaseTarget().localPosition;
                tempPos.y = 0;
                limb.GetBaseTarget().localPosition = tempPos;
                limb.Initialize(this, m_useGimbalLimbHeight);
            }
            m_baseTargets.transform.position = new Vector3(transform.position.x, m_walkHeight, transform.position.z);

            var controllers = GetComponents<IRoboticController>();

            switch (m_controlType)
            {
                case ControlType.Simulation:
                    m_transformPositioner = GetComponent<QuadrupedSimulationPositioner>();
                    break;
                case ControlType.Arduino:
                    m_transformPositioner = GetComponent<ArduinoQuadrupedPositoner>();
                    m_servoCMDRelay = GetComponent<IServoCMDRelay>();
                    break;
                case ControlType.ArduinoSimulatedSensors:
                    m_servoCMDRelay = GetComponent<IServoCMDRelay>();
                    break;
                default:
                    break;
            }
            if (m_transformPositioner != null)
            {
                m_transformPositioner.Initialize(this);
            }
            if (m_servoCMDRelay != null)
            {
                m_servoCMDRelay.Initialize(this);
            }

            if (m_gaitController != null)
            {
                m_gaitController.Initialize(this);
            }
            m_status = Status.MovingToStartPosition;
            NotifyEventListeners(IRobotEventListener.EventType.OnRobotInitialized);
        }

        void Update()
        {
            PositionGimble();
            Time.timeScale = m_physicsTime;

            if (m_controlType == ControlType.Arduino)
            {
                SetTransformValues();
                RunRoboticController();
                m_servoCMDRelay.RelayServoCommands(m_servoValues);
            }

            m_com.localPosition = m_articulationBody.centerOfMass;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                foreach (var limb in m_limbs)
                {
                    limb.ReturnToStartHeight();
                }
            }
        }
        public void ResetController()
        {
            m_resetCount = 0;
            m_status = Status.Resetting;

            m_ground.GetComponent<Collider>().enabled = false;
            m_transformPositioner.BeginResetPositioner();

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
                    m_transformPositioner.CompletePositionerReset();
                    m_ground.GetComponent<Collider>().enabled = true;
             
                    m_status = Status.MovingToStartPosition;
                    m_resetCount = 0;
                    NotifyEventListeners(IRobotEventListener.EventType.OnReset);
                }
                return;
            }

            if (m_controlType != ControlType.Arduino)
            {
                SetTransformValues();
                RunRoboticController();
                if(m_servoCMDRelay != null)
                m_servoCMDRelay.RelayServoCommands(m_servoValues);
            }
        }
        private bool SetTransformValues()
        {
            bool success = m_transformPositioner.PositionTransform();
            if (!success)
            {
                Debug.Log("Failed to get sensor data");
                return false;
            }
            return true;
        }

        public void RunRoboticController()
        {
            PositionGimble();

            if (m_status == Status.MovingToStartPosition)
            {
                bool atTarget = true;
                foreach (var limb in m_limbs)
                {
                    if (!limb.LimbAtTarget() || !limb.BaseAtTarget())
                    {
                        atTarget = false;
                    }
                }
                if (atTarget)
                {
                    m_status = Status.Ready;
                    NotifyEventListeners(IRobotEventListener.EventType.OnRobotInPosition);
                }
            }
            else
            {
                foreach (var limb in m_limbs)
                {
                    limb.GetPositioner().GetGameObject().transform.rotation = Quaternion.LookRotation(GetGimbal().GetGameObject().transform.forward, GetGimbal().GetGameObject().transform.up);
                }
                if (m_gaitController != null && m_gaitController.IsRunning() && IsRunning)
                {
                  //  m_gaitController.Run();
                }
            }

            foreach (var limb in m_limbs)
            {
                limb.RunLimb(true, true);
            }

            NotifyEventListeners(IRobotEventListener.EventType.OnLimbsPositioned);

            m_servoValues = new QuadrupedGroundStationData();

            m_servoValues.FL0 = (int)m_flLimb.GetServoControllers()[0].GetSetAngle();
            m_servoValues.FL1 = (int)m_flLimb.GetServoControllers()[1].GetSetAngle();
            m_servoValues.FL2 = (int)m_flLimb.GetServoControllers()[2].GetSetAngle();

            m_servoValues.FR0 = (int)m_frLimb.GetServoControllers()[0].GetSetAngle();
            m_servoValues.FR1 = (int)m_frLimb.GetServoControllers()[1].GetSetAngle();
            m_servoValues.FR2 = (int)m_frLimb.GetServoControllers()[2].GetSetAngle();

            m_servoValues.BL0 = (int)m_blLimb.GetServoControllers()[0].GetSetAngle();
            m_servoValues.BL1 = (int)m_blLimb.GetServoControllers()[1].GetSetAngle();
            m_servoValues.BL2 = (int)m_blLimb.GetServoControllers()[2].GetSetAngle();

            m_servoValues.BR0 = (int)m_brLimb.GetServoControllers()[0].GetSetAngle();
            m_servoValues.BR1 = (int)m_brLimb.GetServoControllers()[1].GetSetAngle();
            m_servoValues.BR2 = (int)m_brLimb.GetServoControllers()[2].GetSetAngle();

          //  Debug.Log(m_status);
        }

        private void PositionGimble()
        {
            this.Gimbal.GetGameObject().transform.position = transform.position;

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
            if (angle > m_emergencyStopAngle && IsRunning)
            {
                EmergencyStop();
            }
        }

        public void EmergencyStop()
        {
            Debug.Log("EMERGENCY STOP");
            IsRunning = false;
            foreach (var limb in m_limbs)
            {
                //limb.ResetLimbTargetPosition();
            }
         //   NotifyEventListeners(IRobotEventListener.EventType.OnEmergencyStop);
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

        public IGimbal GetGimbal()
        {
            return Gimbal;
        }
    }
}

