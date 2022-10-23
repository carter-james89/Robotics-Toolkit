using RoboticToolkit.Robotics.Gaits;
using RoboticToolkit.Robotics.Limbs;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{
    public class Quadruped : MonoBehaviour, IRobot
    {
        [SerializeField]
        private ThreeJointRoboticLimb m_frLimb;
        [SerializeField]
        private ThreeJointRoboticLimb m_flLimb;
        [SerializeField]
        private ThreeJointRoboticLimb m_brLimb;
        [SerializeField]
        private ThreeJointRoboticLimb m_blLimb;

        private enum Status
        {
            NotRunning,
            MovingToStartPosition,
            Ready,
        }
        private Status m_status = Status.NotRunning;

        [SerializeField]
        private float m_physicsTime = 1;

        [SerializeField]
        private bool m_useGimbalLimbHeight = true;

        [SerializeField]
        private float m_emergencyStopAngle = 20;

        public bool IsRunning { get; private set; } = true;

        private List<IRobotEventListener> m_listeners = new List<IRobotEventListener>();

        public IGimbal Gimbal { get; private set; }

        public GameObject GetGameObject() => gameObject;

        [SerializeField]
        private Transform m_gaits;
        [SerializeField]
        private Transform m_baseTargets;

        private ArticulationBody m_articulationBody;

        private IRoboticController m_roboticController;

        private bool m_ready = true;

        private IGaitController m_gaitController;
        private float m_startHeight;

        [SerializeField]
        private float m_walkHeight = .2f;

        [SerializeField]
        private bool m_simulate = false;

        [SerializeField]
        private bool m_static = false;

        [SerializeField]
        private GameObject m_ground;

        [SerializeField]
        private Transform m_com;

        public IRobot.RobotData GetRobotData()
        {
            return new IRobot.RobotData(m_articulationBody.velocity, m_articulationBody.angularVelocity);
        }

        private List<ThreeJointRoboticLimb> m_limbs = new List<ThreeJointRoboticLimb>();
        public IRoboticLimb[] GetLimbs() => m_limbs.ToArray();

        void Awake()
        {
            Gimbal = GetComponentInChildren<IGimbal>();
        }

        // Start is called before the first frame update
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

              //  limb.GetPositioner().GetGameObject().transform.SetParent(Gimbal.GetGameObject().transform);
            }
            m_baseTargets.transform.position = transform.position + new Vector3(0, m_walkHeight, 0);
            var controllers = GetComponents<IRoboticController>();

            if (m_simulate)
            {
                m_articulationBody.immovable = false;

                foreach (var item in controllers)
                {
                    if (item.IsSimulator())
                    {
                        m_roboticController = item;
                    }
                }
            }
            else
            {
                foreach (var item in controllers)
                {
                    if (!item.IsSimulator())
                    {
                        m_roboticController = item;
                    }
                }
                m_articulationBody.immovable = true;
            }
            m_roboticController.Initialize(this);
            if (m_gaitController != null)
            {
                m_gaitController.Initialize(this);
            }
            //if (m_arduinoConnection && m_arduinoConnection.enabled)
            //{
            //    var handShakeDataMessage = m_arduinoConnection.ReadFromArduino();
            //    Debug.Log(handShakeDataMessage);    
            //    var handShakeData = JsonUtility.FromJson<QuadrupedSensorData>(handShakeDataMessage);
            //    Debug.Log(handShakeData.H);
            //}
            m_status = Status.MovingToStartPosition;
            NotifyEventListeners(IRobotEventListener.EventType.OnRobotInitialized);
        }

        private bool SetTransformValues()
        {
            bool success = m_roboticController.SetTransformValues();
            if (!success)
            {
                Debug.Log("Failed to get sensor data");
                return false;
            }
            return true;
        }

        void Update()
        {
            Time.timeScale = m_physicsTime;
            if (!m_roboticController.IsSimulator())
            {
                RunRoboticController();
            }
            m_com.localPosition = m_articulationBody.centerOfMass;
           // m_com.localPosition = m_articulationBody.c
        }

        public void EmergencyStop()
        {
            Debug.Log("EMERGENCY STOP");
            IsRunning = false;
            foreach (var limb in m_limbs)
            {
                //     limb.ResetLimbTargetPosition();
            }

            NotifyEventListeners(IRobotEventListener.EventType.OnEmergencyStop);
        }

        private void FixedUpdate()
        {
            if (m_roboticController.IsSimulator())
            {
                RunRoboticController();
            }

        }

        public void RunRoboticController()
        {
            bool success = SetTransformValues();
            if (!success)
            {
                return;
            }
            PositionGimble();

            if(m_status == Status.MovingToStartPosition)
            {
                bool atTarget = true;
                foreach (var limb in m_limbs)
                {
                    limb.RunLimb(!m_roboticController.IsSimulator(), true);

                    if (!limb.LimbAtTarget() || !limb.BaseAtTarget())
                    {
                        atTarget = false;
                    }
                }
                if (atTarget)
                {
                    m_status = Status.Ready;

                    if (m_static)
                    {
                        m_articulationBody.immovable = true;
                        m_ground.SetActive(false);
                        var tempPos = m_baseTargets.position;
                        tempPos.y = m_blLimb.GetGameObject().transform.position.y;
                        m_baseTargets.transform.position = tempPos; 
                    }

                    NotifyEventListeners(IRobotEventListener.EventType.OnRobotInPosition);
                }
                else
                {
                    return;
                }
            }

            if (!IsRunning)
            {
                return;
            }
            foreach (var limb in m_limbs)
            {
                limb.GetPositioner().GetGameObject().transform.rotation = Quaternion.LookRotation(GetGimbal().GetGameObject().transform.forward, GetGimbal().GetGameObject().transform.up);
            }
            if (m_gaitController != null && m_gaitController.IsRunning())
            {
                m_gaitController.Run();
            }
            else
            {
                foreach (var limb in m_limbs)
                {
                    limb.RunLimb(!m_roboticController.IsSimulator(), true);
                }
            }
            //if (!IsRunning)
            //{
            //    foreach (var limb in m_limbs)
            //    {
            //        limb.RunLimb(!m_roboticController.IsSimulator(), true);
            //    }
            //}
            //else if (m_gait != null && !m_gait.IsRunning())
            //{
            //    foreach (var limb in m_limbs)
            //    {
            //        limb.RunLimb(!m_roboticController.IsSimulator(), true);
            //    }
            //}
            // m_roboticController.SendCommands();

            var digitalTwinData = new QuadrupedGroundStationData();

            digitalTwinData.FL_0 = (int)m_flLimb.GetServoControllers()[0].GetSetAngle();
            digitalTwinData.FL_1 = (int)m_flLimb.GetServoControllers()[1].GetSetAngle();
            digitalTwinData.FL_2 = (int)m_flLimb.GetServoControllers()[2].GetSetAngle();

            digitalTwinData.FR_0 = (int)m_frLimb.GetServoControllers()[0].GetSetAngle();
            digitalTwinData.FR_1 = (int)m_frLimb.GetServoControllers()[1].GetSetAngle();
            digitalTwinData.FR_2 = (int)m_frLimb.GetServoControllers()[2].GetSetAngle();

            digitalTwinData.BL_0 = (int)m_blLimb.GetServoControllers()[0].GetSetAngle();
            digitalTwinData.BL_1 = (int)m_blLimb.GetServoControllers()[1].GetSetAngle();
            digitalTwinData.BL_2 = (int)m_blLimb.GetServoControllers()[2].GetSetAngle();

            digitalTwinData.BR_0 = (int)m_brLimb.GetServoControllers()[0].GetSetAngle();
            digitalTwinData.BR_1 = (int)m_brLimb.GetServoControllers()[1].GetSetAngle();
            digitalTwinData.BR_2 = (int)m_brLimb.GetServoControllers()[2].GetSetAngle();

            m_roboticController.SendCommands(digitalTwinData);
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
            m_baseTargets.eulerAngles = tempEuler;

            var angle = Vector3.Angle(transform.up, Gimbal.GetGameObject().transform.up);
            if (angle > m_emergencyStopAngle && IsRunning)
            {
                EmergencyStop();
            }
        }

        public void ResetController()
        {
            m_articulationBody.velocity = Vector3.zero;
            m_articulationBody.angularVelocity = Vector3.zero;
            foreach (var limb in m_limbs)
            {
                limb.ResetLimb();
            }
            // m_articulationBody.TeleportRoot(m_ground.position + new Vector3(0, m_startHeight, 0), Quaternion.identity);

            NotifyEventListeners(IRobotEventListener.EventType.OnReset);
        }

        private void NotifyEventListeners(IRobotEventListener.EventType eventType)
        {
            foreach (var listener in m_listeners)
            {
                if (listener != null)
                {
                    listener.OnRobotEventOccured(new IRobotEventListener.EventData(eventType, this, m_roboticController));
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

