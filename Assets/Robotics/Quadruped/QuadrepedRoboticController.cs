using RoboticsToolkit.ArduinoUtilities;
using RoboticToolkit.Robotics.Gaits;
using RoboticToolkit.Robotics.Limbs;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{
    public interface IRoboticController
    {
        public struct RobotData
        {
            public Vector3 Velocity;
            public Vector3 AngularVelocity;

            public RobotData(Vector3 velocity, Vector3 angularVelocity)
            {
                Velocity = velocity;
                AngularVelocity = angularVelocity;
            }
        }
        public GameObject GetGameObject();
        public IRoboticLimb[] GetLimbs();
        public RobotData GetRobotData();

        public void ResetController();
    }
    public class QuadrepedRoboticController : MonoBehaviour, IRoboticController
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
        private Transform m_ground;

        private ArduinoConnection m_arduinoConnection;

        public GameObject GetGameObject() => gameObject;

        [SerializeField]
        private Transform m_gaits;
        [SerializeField]
        private Transform m_baseTargets;

        private ArticulationBody m_articulationBody;

        private bool m_ready = true;

        private IGait m_gait;
        private float m_startHeight;

        [SerializeField]
        private float m_walkHeight = .2f;

        [SerializeField]
        private bool m_simulate = false;

        public IRoboticController.RobotData GetRobotData()
        {
            return new IRoboticController.RobotData(m_articulationBody.velocity, m_articulationBody.angularVelocity);
        }

        private List<ThreeJointRoboticLimb> m_limbs = new List<ThreeJointRoboticLimb>();
        public IRoboticLimb[] GetLimbs() => m_limbs.ToArray();

        // Start is called before the first frame update
        void Start()
        {
            m_startHeight = transform.localPosition.y;
            m_gait = GetComponent<IGait>();
            m_articulationBody = GetComponent<ArticulationBody>();

            m_arduinoConnection = GetComponent<ArduinoConnection>();

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

                //if(limb.ShoulderServoController is PIDServoController)
                //{
                //    (limb.ShoulderServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
                //    (limb.ElbowServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
                //    (limb.WristServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
                //}           
            }

            if (m_simulate)
            {
                m_baseTargets.transform.position = transform.position + new Vector3(0, m_walkHeight, 0);
                m_ground.gameObject.SetActive(true);
                m_articulationBody.immovable = false;
            }
            else
            {
                m_ground.gameObject.SetActive(false);
                m_articulationBody.immovable = true;
                foreach (var limb in m_limbs)
                {
                    var tempPos = limb.GetIKTargetPos();
                    tempPos.y  -= m_walkHeight;
                    limb.SetIKTargetPos(tempPos);
                }         
            }
            if (m_gait !=null)
            {
                m_gait.Initialize(this);
            }
         
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                m_articulationBody.immovable = false;
            }
            PositionGimble();      
           // GetComponent<ArduinoConnection>().WriteToArduino("1");
        }

        private void FixedUpdate()
        {
            PositionGimble();

            if(m_gait != null)
            {
                m_gait.RunGait();
            }
            foreach (var limb in m_limbs)
            {
                limb.RunLimb(true);
            }

            if (m_arduinoConnection && m_arduinoConnection.enabled)
            {
                var digitalTwinData = new NovaDigitalTwinData();

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

                m_arduinoConnection.WriteToArduino(JsonUtility.ToJson(digitalTwinData));

             // Debug.Log("bl2 : " +digitalTwinData.BL_2);
            }
        }

        private void PositionGimble()
        {
            var tempPos = m_gaits.transform.position;
            tempPos.x = transform.position.x;
            tempPos.y = 0;
            tempPos.z = transform.position.z;
            m_gaits.transform.position = tempPos;

            var tempEuler = m_gaits.transform.eulerAngles;
            tempEuler.y = transform.eulerAngles.y;
            m_gaits.eulerAngles = tempEuler;

            tempPos = m_baseTargets.transform.position;
            tempPos.x = transform.position.x;
            tempPos.z = transform.position.z;
            m_baseTargets.transform.position = tempPos;
            //  m_baseTargets.transform.rotation = m_gaits.rotation;
            // m_baseTargets.transform.rotation = Quaternion.LookRotation(transform.forward);
            tempEuler = m_baseTargets.eulerAngles;
            //tempEuler.y = transform.eulerAngles.y;
            m_baseTargets.eulerAngles = tempEuler;
        }


        public void ResetController()
        {
            m_articulationBody.velocity = Vector3.zero;
            m_articulationBody.angularVelocity = Vector3.zero;
            foreach (var limb in m_limbs)
            {
                limb.ResetLimb();
            }
            m_articulationBody.TeleportRoot(m_ground.position + new Vector3(0, m_startHeight, 0), Quaternion.identity);
        }
    }
}

