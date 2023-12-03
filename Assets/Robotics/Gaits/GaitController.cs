using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Limbs;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public class GaitController : MonoBehaviour, IGaitController//,/ IGaitEventListener
    {
        private IRoboticLimb[] m_limbs;
        private IRobot m_robot;

        private bool m_postStrideCooldown = false;
        private float m_postStrideCooldownTime = 0;
        private float m_postStrideCooldownTargetTime = .2f;

        private Vector3 _currentWalkDirection;
        private bool _rotating = false;

        [SerializeField]
        private QuadrupedCrawlGait m_crawlGait;
        [SerializeField]
        private TrotGait m_trotGait;

        private bool m_beginReturnHome = false;

        private IGait m_activeGait;

        private IGaitController.GaitPattern m_currentPattern = IGaitController.GaitPattern.NONE;

        public void Initialize(IRobot robot)
        {
            m_robot = robot;
            m_limbs = robot.GetLimbs();

            // m_activeGait = GetComponent<IGait>();

            foreach (var item in m_limbs)
            {

            }

            //  m_trotGait.Initialize(m_robot);
            // m_crawlGait.Initialize(m_robot);
        }
        [SerializeField]
        private float m_forwardTrotStrideDistance = .04f;
        [SerializeField]
        private float m_forwardTrotStrideTime = .22f;
        [SerializeField]
        private float m_forwardTrotStrideCoolDownTime = .2f;
        [SerializeField]
        private float m_idleTrotStrideDistance = .0001f;
        [SerializeField]
        private float m_idleTrotStrideTime = .1f;
        [SerializeField]
        private float m_idleTrotStrideCoolDownTime = .01f;
        [SerializeField]
        private float m_rotatingTrotStrideDistance = .04f;
        [SerializeField]
        private float m_rotatingTrotStrideTime = .22f;
        [SerializeField]
        private float m_rotatingTrotStrideCoolDownTime = .2f;
        void Update()
        {

        }

        public void BeginMovement(ILimbPositioner[] limbs, IGaitController.GaitPattern pattern, Vector3 direction, bool rotate)
        {
            _currentWalkDirection = direction;
            m_currentPattern = pattern;
            switch (pattern)
            {
                case IGaitController.GaitPattern.NONE:
                    break;
                case IGaitController.GaitPattern.STATIONARYSTEP:
                    m_activeGait = m_trotGait;
                    m_trotGait.SetStrideDistance(0);
                    break;
                case IGaitController.GaitPattern.CRAWL:
                    m_activeGait = m_crawlGait;
                    break;
                case IGaitController.GaitPattern.TROT:
                    m_activeGait = m_trotGait;
                    break;
                default:
                    break;
            }
            //  _rotating = rotating;
            // SetGaitPattern(patern);
            // (m_activeGait as QuadrupedTrotGait).SetDirection(direction);
            if (m_activeGait != null)
            {
                // m_activeGait.SubscribeToEvents(this);
                m_activeGait.Begin();
                m_activeGait.SetNextCycle(direction, limbs, _rotating);
            }
        }

        private void ProcessUserInput(ILimbPositioner[] limbs)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                if (m_currentPattern == IGaitController.GaitPattern.NONE)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                       // m_trotGait.SetStrideValues(m_idleTrotStrideDistance, m_idleTrotStrideTime, m_idleTrotStrideCoolDownTime);
                    }
                    else
                    {
                       // m_trotGait.SetStrideValues(m_forwardTrotStrideDistance, m_forwardTrotStrideTime, m_forwardTrotStrideCoolDownTime);
                    }
                    BeginMovement(limbs, IGaitController.GaitPattern.TROT, m_robot.GetGimbal().GetGameObject().transform.forward,false);
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    foreach (var limb in m_limbs)
                    {
                        // if(limb.is)
                    }
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                if (m_currentPattern == IGaitController.GaitPattern.NONE)
                {
                  //  m_trotGait.SetStrideValues(m_forwardTrotStrideDistance, m_forwardTrotStrideTime, m_forwardTrotStrideCoolDownTime);
                    BeginMovement(limbs, IGaitController.GaitPattern.TROT, -m_robot.GetGimbal().GetGameObject().transform.forward,false);
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                if (m_currentPattern == IGaitController.GaitPattern.NONE)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                      //  m_trotGait.SetStrideValues(m_forwardTrotStrideDistance, m_forwardTrotStrideTime, m_forwardTrotStrideCoolDownTime);
                        BeginMovement(limbs, IGaitController.GaitPattern.TROT, m_robot.GetGimbal().GetGameObject().transform.right,true);
                    }
                    else
                    {
                      //  m_trotGait.SetStrideValues(m_rotatingTrotStrideDistance, m_rotatingTrotStrideTime, m_rotatingTrotStrideCoolDownTime);
                        BeginMovement(limbs, IGaitController.GaitPattern.TROT, m_robot.GetGimbal().GetGameObject().transform.right, true);
                    }
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (m_currentPattern == IGaitController.GaitPattern.NONE)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                     //   m_trotGait.SetStrideValues(m_forwardTrotStrideDistance, m_forwardTrotStrideTime, m_forwardTrotStrideCoolDownTime);
                        BeginMovement(limbs, IGaitController.GaitPattern.TROT, -m_robot.GetGimbal().GetGameObject().transform.right, false);
                    }
                    else
                    {
                     //   m_trotGait.SetStrideValues(m_rotatingTrotStrideDistance, m_rotatingTrotStrideTime, m_rotatingTrotStrideCoolDownTime);
                        BeginMovement(limbs, IGaitController.GaitPattern.TROT, m_robot.GetGimbal().GetGameObject().transform.right, false);
                    }
                }
            }
            else if (m_activeGait != null && (m_currentPattern != IGaitController.GaitPattern.NONE && m_currentPattern != IGaitController.GaitPattern.RETURNING_HOME))
            {
                m_beginReturnHome = true;
            }
        }
        public void Run(IRoboticLimb[] mirrorLimbs, ILimbPositioner[] limbPositioners)
        {
          //  ProcessUserInput(limbPositioners);
            //if(m_currentPattern == IGaitController.GaitPattern.NONE)
            //{
            //    return;
            //}
            //  if (m_running)
            //  {
            List<ILimbPositioner> ikTargetsAtTarget = new List<ILimbPositioner>();

            foreach (var limb in limbPositioners)
            {
                if (limb.StrideComplete() == true)
                {
                    ikTargetsAtTarget.Add(limb);
                }
                else
                {
                    // Debug.Log("Waiting for : " + limb.GetGameObject().name);
                }
            }
            //Debug.Log(m_limbsAtTarget.Count);
            if (ikTargetsAtTarget.Count >= 3)
            {
                m_postStrideCooldown = true;
            }


            if (m_postStrideCooldown)
            {
                m_postStrideCooldownTime += Time.deltaTime;
                if (m_postStrideCooldownTime >= m_postStrideCooldownTargetTime)
                {
                    m_postStrideCooldown = false;
                    m_postStrideCooldownTime = 0;

                    m_activeGait.SetNextCycle(_currentWalkDirection, limbPositioners, _rotating);
                  

                    //NotifyListeners(IGaitEventListener.EventType.OnGaitCycleComplete);
                    bool atHome = true;
                    foreach (var item in limbPositioners)
                    {
                        //  if (item.GetIKTargetPos() != item.GetPositioner().GetGameObject().transform.position)
                        // {
                        //      atHome = false;
                        //  }
                    }
                    if (atHome)
                    {
                        //  NotifyListeners(IGaitEventListener.EventType.OnGaitReturnedHome);
                    }
                }
            }
          //  return false;
        }

        public bool IsRunning()
        {
            return m_currentPattern != IGaitController.GaitPattern.NONE;
        }

        public void SetGaitPattern(IGaitController.GaitPattern type)
        {
            m_currentPattern = type;
            switch (type)
            {
                case IGaitController.GaitPattern.NONE:
                    break;
                case IGaitController.GaitPattern.STATIONARYSTEP:
                    m_activeGait = m_trotGait;
                    m_trotGait.SetStrideDistance(0);
                    
                    break;
                case IGaitController.GaitPattern.CRAWL:
                    m_activeGait = m_crawlGait;
                    break;
                case IGaitController.GaitPattern.TROT:
                    m_activeGait = m_trotGait;
                    break;
                default:
                    break;
            }
        }

        public IGaitController.GaitPattern GetGaitPattern()
        {
            throw new System.NotImplementedException();
        }

        public IGaitController.Direction GetDirection()
        {
            throw new System.NotImplementedException();
        }

        public void SetDirection(IGaitController.Direction direction)
        {
            throw new System.NotImplementedException();
        }

    //    public void OnGaitEventOccured(IGaitEventListener.GaitEventData eventData)
    //    {
    //        switch (eventData.EventType)
    //        {
    //            case IGaitEventListener.EventType.OnGaitCycleBegin:
    //                break;
    //            case IGaitEventListener.EventType.OnGaitCycleComplete:
    //                if (m_currentPattern != IGaitController.GaitPattern.NONE)
    //                {
    //                    if (m_beginReturnHome)
    //                    {
    //                        m_activeGait.ReturnHome();
    //                        m_activeGait.SetNextCycle();
    //                        m_currentPattern = IGaitController.GaitPattern.RETURNING_HOME;
    //                        m_beginReturnHome = false;
    //                    }
    //                    else if (m_currentPattern == IGaitController.GaitPattern.RETURNING_HOME)
    //                    {
    //                        Debug.Log("gait at home");
    //                        m_activeGait.UnubscribeFromEvents(this);
    //                        m_activeGait.Stop();
    //                        m_activeGait = null;
    //                        m_currentPattern = IGaitController.GaitPattern.NONE;
    //                    }
    //                    else
    //                    {
    //                        m_activeGait.SetNextCycle();
    //                    }
    //                }
    //                break;
    //            case IGaitEventListener.EventType.OnGaitReturnedHome:

    //                break;
    //            default:
    //                break;
    //        }

    //        //   SetGaitPattern(IGaitController.GaitPattern.NONE);


    //    }
    }
}

