using RoboticToolkit.Robotics.Gaits;
using RoboticToolkit.Robotics.Limbs;
using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RoboticsToolkit.Robotics
{
    public interface IRoboticController
    {
        public void Reset();
    }
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


        [SerializeField]
        private Transform m_gaits;
        [SerializeField]
        private Transform m_baseTargets;

        private ArticulationBody m_articulationBody;

        private IGateProvider m_gaitProvider;
        private float m_startHeight;

        public bool Walking { get; private set; } = false;

        private Dictionary<ThreeJointRoboticLimb, LimbGait> m_limbs = new Dictionary<ThreeJointRoboticLimb, LimbGait>();

        // Start is called before the first frame update
        void Start()
        {
            m_startHeight = transform.localPosition.y;
            m_gaitProvider = GetComponent<IGateProvider>();
            m_articulationBody = GetComponent<ArticulationBody>();
            m_limbs.Add(m_flLimb, m_flLimb.GetGait());
            m_limbs.Add(m_frLimb, m_frLimb.GetGait());
            m_limbs.Add(m_brLimb, m_brLimb.GetGait());
            m_limbs.Add(m_blLimb, m_blLimb.GetGait());


            // m_gaits.transform.position = transform.position;
            m_gaits.transform.SetParent(null);
            m_baseTargets.transform.SetParent(null);
            foreach (var limbGaitPair in m_limbs)
            {
                limbGaitPair.Value.gameObject.name += "(" + limbGaitPair.Key.name + ")";
                //limb.GetGait().transform.SetParent(m_gaits);
                //var tempPos = limb.GetGait().transform.localPosition;
                //tempPos.y = 0;
                //limb.GetGait().transform.localPosition = tempPos;

                limbGaitPair.Key.GetBaseTarget().SetParent(m_baseTargets);
                var tempPos = limbGaitPair.Key.GetBaseTarget().localPosition;
                tempPos.y = 0;
                limbGaitPair.Key.GetBaseTarget().localPosition = tempPos;

                //if(limb.ShoulderServoController is PIDServoController)
                //{
                //    (limb.ShoulderServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
                //    (limb.ElbowServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
                //    (limb.WristServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
                //}

            }


        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                m_articulationBody.immovable = false;
            }
            PositionGimble();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Walking = true;
                SetNextGaitCycle();
                // m_gaitProvider.SetGaitTargets()
                //  m_stridePosition = 1;
                //   m_frLimb.GetGait().MoveToPosition(new Vector3(0, 0, .05f), 25f, RoboticToolKit.Robotics.Limbs.IGait.MovementStyle.Rotate);
                //   m_flLimb.GetGait().MoveToPosition(new Vector3(0, 0, -.05f), .1f, RoboticToolKit.Robotics.Limbs.IGait.MovementStyle.Translate);
                //   m_rrLimb.GetGait().MoveToPosition(new Vector3(0, 0, -.05f), .1f, RoboticToolKit.Robotics.Limbs.IGait.MovementStyle.Translate);
                //   m_rlLimb.GetGait().MoveToPosition(new Vector3(0, 0, -.05f), .1f, RoboticToolKit.Robotics.Limbs.IGait.MovementStyle.Translate);
            }


            if (Walking)
            {
                bool strideComplete = true;
                foreach (var limb in m_limbs.Keys)
                {
                    if (limb.LimbAtTarget() == false)
                    {
                        strideComplete = false;
                    }
                }
                if (strideComplete)
                {
                    SetNextGaitCycle();
                }
            }
        }

        public void Reset()
        {
            m_articulationBody.TeleportRoot(m_ground.position + new Vector3(0, m_startHeight, 0), Quaternion.identity);
            m_articulationBody.velocity = Vector3.zero;
            m_articulationBody.angularVelocity = Vector3.zero;
            foreach (var limbPair in m_limbs)
            {
                limbPair.Key.Reset();
              
            }      
        }

        private void SetNextGaitCycle()
        {
            m_gaitProvider.SetGaitTargets(m_limbs.Values.ToArray(), transform.localPosition, transform.localRotation);
        }

        private void FixedUpdate()
        {
            PositionGimble();
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
