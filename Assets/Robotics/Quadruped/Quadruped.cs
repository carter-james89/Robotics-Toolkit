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

        [SerializeField]
        private float m_physicsTime = 1;

        [SerializeField]
        private bool m_useGimbalLimbHeight = true;

        [SerializeField]
        private float m_emergencyStopAngle = 20;

        public bool IsRunning { get; private set; } = true;

        public IGimbal Gimbal { get; private set; }    

        public GameObject GetGameObject() => gameObject;

        [SerializeField]
        private Transform m_gaits;
        [SerializeField]
        private Transform m_baseTargets;

        private ArticulationBody m_articulationBody;

        private IRoboticController m_roboticController;

        private bool m_ready = true;

        private IGait m_gait;
        private float m_startHeight;

        [SerializeField]
        private float m_walkHeight = .2f;

        [SerializeField]
        private bool m_simulate = false;

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
            m_gait = GetComponent<IGait>();
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

            // m_gaits.transform.position = transform.position;
            m_gaits.transform.SetParent(transform.parent);
            m_baseTargets.transform.SetParent(transform.parent);
            // m_baseTargets.transform.localPosition = Vector3.zero;
            PositionGimble();
            foreach (var limb in m_limbs)
            {
                // limb.GetPositioner().gameObject.name += "(" + limb.name + ")";
                //limb.GetGait().transform.SetParent(m_gaits);
                //var tempPos = limb.GetGait().transform.localPosition;
                //tempPos.y = 0;
                //limb.GetGait().transform.localPosition = tempPos;

                limb.GetBaseTarget().SetParent(m_baseTargets);
                var tempPos = limb.GetBaseTarget().localPosition;
                tempPos.y = 0;
                limb.GetBaseTarget().localPosition = tempPos;

                limb.Initialize(Gimbal, m_useGimbalLimbHeight);

                //if(limb.ShoulderServoController is PIDServoController)
                //{
                //    (limb.ShoulderServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
                //    (limb.ElbowServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
                //    (limb.WristServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
                //}           
            }

            var controllers = GetComponents<IRoboticController>();
            if (m_simulate)
            {
                m_baseTargets.transform.position = transform.position + new Vector3(0, m_walkHeight, 0);
                //   m_ground.gameObject.SetActive(true);
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
                //foreach (var limb in m_limbs)
                //{
                //    var tempPos = limb.GetIKTargetPos();
                //    tempPos.y -= m_walkHeight;
                //    limb.SetIKTargetPos(tempPos);
                //}
            }
            m_roboticController.Initialize(this);
            if (m_gait != null)
            {
                m_gait.Initialize(this);
            }
            //if (m_arduinoConnection && m_arduinoConnection.enabled)
            //{
            //    var handShakeDataMessage = m_arduinoConnection.ReadFromArduino();
            //    Debug.Log(handShakeDataMessage);    
            //    var handShakeData = JsonUtility.FromJson<QuadrupedSensorData>(handShakeDataMessage);
            //    Debug.Log(handShakeData.H);
            //}
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
           
        }

        public void EmergencyStop()
        {
            Debug.Log("EMERGENCY STOP");
            IsRunning = false;
            foreach (var limb in m_limbs)
            {
           //     limb.ResetLimbTargetPosition();
            }
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

            if (m_gait != null && IsRunning)
            {
                m_gait.RunGait();
            }
            if (!IsRunning)
            {
                foreach (var limb in m_limbs)
                {
                    limb.RunLimb(!m_roboticController.IsSimulator(), true);
                }
            }
            else if(m_gait != null && !m_gait.IsRunning())
            {
                foreach (var limb in m_limbs)
                {
                    limb.RunLimb(!m_roboticController.IsSimulator(), true);
                }
            }
           // m_roboticController.SendCommands();

            var digitalTwinData = new QuadrupedGroundStationData();
            digitalTwinData.FL_0 = (int)m_flLimb.GetServoControllers()[0].GetServo().GetCurrentAngle();
            digitalTwinData.FL_1 = (int)m_flLimb.GetServoControllers()[1].GetServo().GetCurrentAngle();
            digitalTwinData.FL_2 = (int)m_flLimb.GetServoControllers()[2].GetServo().GetCurrentAngle();

            digitalTwinData.FR_0 = (int)m_frLimb.GetServoControllers()[0].GetServo().GetCurrentAngle();
            digitalTwinData.FR_1 = (int)m_frLimb.GetServoControllers()[1].GetServo().GetCurrentAngle();
            digitalTwinData.FR_2 = (int)m_frLimb.GetServoControllers()[2].GetServo().GetCurrentAngle();

            digitalTwinData.BL_0 = (int)m_blLimb.GetServoControllers()[0].GetServo().GetCurrentAngle();
            digitalTwinData.BL_1 = (int)m_blLimb.GetServoControllers()[1].GetServo().GetCurrentAngle();
            digitalTwinData.BL_2 = (int)m_blLimb.GetServoControllers()[2].GetServo().GetCurrentAngle();

            digitalTwinData.BR_0 = (int)m_brLimb.GetServoControllers()[0].GetServo().GetCurrentAngle();
            digitalTwinData.BR_1 = (int)m_brLimb.GetServoControllers()[1].GetServo().GetCurrentAngle();
            digitalTwinData.BR_2 = (int)m_brLimb.GetServoControllers()[2].GetServo().GetCurrentAngle();
            m_roboticController.SendCommands(digitalTwinData);          
        }


        private void PositionGimble()
        {
              this.Gimbal.GetGameObject().transform.position = transform.position;
            // this.Gimbal.GetGameObject().transform.rotation = Quaternion.identity;
          //  Gimbal.GetGameObject().transform.position = Vector3.zero;

            Gimbal.GetGameObject().transform.rotation = transform.rotation;
            var tempEuler = Gimbal.GetGameObject().transform.eulerAngles;
            tempEuler.x = 0;
            tempEuler.z = 0;
            Gimbal.GetGameObject().transform.rotation = Quaternion.Euler(tempEuler);

            //var tempPos = m_gaits.transform.position;
            //tempPos.x = transform.position.x;
            //tempPos.y = 0;
            //tempPos.z = transform.position.z;
            //m_gaits.transform.position = tempPos;

            //var tempEuler = m_gaits.transform.eulerAngles;
            //tempEuler.y = transform.eulerAngles.y;
            //m_gaits.eulerAngles = tempEuler;

            var tempPos = m_baseTargets.transform.position;
            tempPos.x = transform.position.x;
            tempPos.z = transform.position.z;
            m_baseTargets.transform.position = tempPos;
            //  m_baseTargets.transform.rotation = m_gaits.rotation;
            // m_baseTargets.transform.rotation = Quaternion.LookRotation(transform.forward);
             tempEuler = m_baseTargets.eulerAngles;
            //tempEuler.y = transform.eulerAngles.y;
            m_baseTargets.eulerAngles = tempEuler;

            var angle = Vector3.Angle(transform.up, Gimbal.GetGameObject().transform.up);
            if(angle > m_emergencyStopAngle && IsRunning)
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
        }
    }



}

