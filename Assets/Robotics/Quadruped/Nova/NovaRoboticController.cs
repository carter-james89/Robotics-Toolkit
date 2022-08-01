using RoboticsToolkit.ArduinoUtilities;
using RoboticToolkit.Robotics.Gaits;
using RoboticToolkit.Robotics.Limbs;
using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NovaDigitalTwinData
{
    //public int[] Motors;
    // public int[] MotorPositions;

    public int FL_CoaxMotorPosition;
    public int FL_ShoulderMotorPosition;
    public int FL_ElbowMotorPosition;
}


namespace RoboticsToolkit.Robotics
{
  
    public class NovaRoboticController : MonoBehaviour, IRoboticController
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

        // private List<ThreeJointRoboticLimb> m_limbs = new List<ThreeJointRoboticLimb>();

        public GameObject GetGameObject() => gameObject;

        [SerializeField]
        private Transform m_gaits;
        [SerializeField]
        private Transform m_baseTargets;

        private ArticulationBody m_articulationBody;

        private bool m_ready =true;

        private IGait m_gait;
        private float m_startHeight;

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
            foreach (var limb in m_limbs)
            {
                limb.GetPositioner().gameObject.name += "(" + limb.name + ")";
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

            m_gait.Initialize(this);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                m_articulationBody.immovable = false;
            }
            PositionGimble();

            var digitalTwinData = new NovaDigitalTwinData();
       
            digitalTwinData.FL_CoaxMotorPosition = (int)m_flLimb.GetServoControllers()[0].GetServo().GetCurrentAngle();
            digitalTwinData.FL_ShoulderMotorPosition = -(int)m_flLimb.GetServoControllers()[1].GetServo().GetCurrentAngle();
            digitalTwinData.FL_ElbowMotorPosition = -(int)m_flLimb.GetServoControllers()[2].GetServo().GetCurrentAngle();

           GetComponent<ArduinoConnection>().WriteToArduino(JsonUtility.ToJson(digitalTwinData));
            //GetComponent<ArduinoConnection>().WriteToArduino("1");
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



        private void FixedUpdate()
        {
            PositionGimble();
            m_gait.RunGait();
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
    }
}
