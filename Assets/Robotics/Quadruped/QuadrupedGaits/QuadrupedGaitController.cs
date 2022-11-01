using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using UnityEngine;

namespace RoboticToolkit.Robotics.Gaits
{
    public class QuadrupedGaitController : MonoBehaviour, IGaitController, IGaitEventListener
    {
        private IRoboticLimb[] m_limbs;
        private IRobot m_robot;

        [SerializeField]
        private QuadrupedCrawlGait m_crawlGait;
        [SerializeField]
        private QuadrupedTrotGait m_trotGait;

        private bool m_beginReturnHome = false;

        private IGait m_activeGait;

        private IGaitController.GaitPattern m_currentPattern = IGaitController.GaitPattern.NONE;

        public void Initialize(IRobot robot)
        {
            m_robot = robot;
            m_limbs = robot.GetLimbs();

            m_trotGait.Initialize(m_robot);
            m_crawlGait.Initialize(m_robot);
        }
        [SerializeField]
        private float m_forwardTrotStrideDistance = .04f;
        [SerializeField]
        private float m_forwardTrotStrideTime  = .22f;
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
            if (Input.GetKey(KeyCode.UpArrow))
            {
                if(m_currentPattern == IGaitController.GaitPattern.NONE)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        m_trotGait.SetStrideValues(m_idleTrotStrideDistance, m_idleTrotStrideTime, m_idleTrotStrideCoolDownTime);                      
                    }
                    else
                    {
                        m_trotGait.SetStrideValues(m_forwardTrotStrideDistance, m_forwardTrotStrideTime, m_forwardTrotStrideCoolDownTime);
                    }
                    BeginMovement(IGaitController.GaitPattern.TROT, QuadrupedTrotGait.Direction.Forward);
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
                    m_trotGait.SetStrideValues(m_forwardTrotStrideDistance, m_forwardTrotStrideTime, m_forwardTrotStrideCoolDownTime);
                    BeginMovement(IGaitController.GaitPattern.TROT, QuadrupedTrotGait.Direction.Backward);
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                if (m_currentPattern == IGaitController.GaitPattern.NONE)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        m_trotGait.SetStrideValues(m_forwardTrotStrideDistance, m_forwardTrotStrideTime, m_forwardTrotStrideCoolDownTime);
                        BeginMovement(IGaitController.GaitPattern.TROT, QuadrupedTrotGait.Direction.StrafeRight);
                    }
                    else
                    {
                        m_trotGait.SetStrideValues(m_rotatingTrotStrideDistance, m_rotatingTrotStrideTime, m_rotatingTrotStrideCoolDownTime);
                        BeginMovement(IGaitController.GaitPattern.TROT, QuadrupedTrotGait.Direction.RotatingClockwise);
                    }                  
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (m_currentPattern == IGaitController.GaitPattern.NONE)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        m_trotGait.SetStrideValues(m_forwardTrotStrideDistance, m_forwardTrotStrideTime, m_forwardTrotStrideCoolDownTime);
                        BeginMovement(IGaitController.GaitPattern.TROT, QuadrupedTrotGait.Direction.StrafeLeft);
                    }
                    else
                    {
                        m_trotGait.SetStrideValues(m_rotatingTrotStrideDistance, m_rotatingTrotStrideTime, m_rotatingTrotStrideCoolDownTime);
                        BeginMovement(IGaitController.GaitPattern.TROT, QuadrupedTrotGait.Direction.RotatingCounterClockwise);
                    }
                }
            }
            else if (m_activeGait != null && (m_currentPattern != IGaitController.GaitPattern.NONE && m_currentPattern != IGaitController.GaitPattern.RETURNING_HOME))
            {
                m_beginReturnHome=true;             
            }
        }
        private void BeginMovement(IGaitController.GaitPattern patern, QuadrupedTrotGait.Direction direction)
        {
            SetGaitPattern(patern);
            (m_activeGait as QuadrupedTrotGait).SetDirection(direction);
            if (m_activeGait != null)
            {
                m_activeGait.SubscribeToEvents(this);
                m_activeGait.Begin();
            }
        }
        public void Run()
        {
            if (m_activeGait != null)
            {
                m_activeGait.RunGait();
            }
        }

        public bool IsRunning()
        {
            return m_currentPattern != IGaitController.GaitPattern.NONE;
        }

        public void SetGaitPattern(IGaitController.GaitPattern type)
        {
            switch (type)
            {
                case IGaitController.GaitPattern.NONE:           
                    break;
                case IGaitController.GaitPattern.STATIONARYSTEP:
                    break;
                case IGaitController.GaitPattern.CRAWL:
                    m_currentPattern = IGaitController.GaitPattern.CRAWL;
                    m_activeGait = m_crawlGait;
                    break;
                case IGaitController.GaitPattern.TROT:
                    m_currentPattern = IGaitController.GaitPattern.TROT;
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

        public void OnGaitEventOccured(IGaitEventListener.GaitEventData eventData)
        {
            switch (eventData.EventType)
            {
                case IGaitEventListener.EventType.OnGaitCycleBegin:
                    break;
                case IGaitEventListener.EventType.OnGaitCycleComplete:
                    if (m_currentPattern != IGaitController.GaitPattern.NONE)
                    {
                        if (m_beginReturnHome)
                        {
                            m_activeGait.ReturnHome();
                            m_activeGait.SetNextCycle();
                            m_currentPattern = IGaitController.GaitPattern.RETURNING_HOME;
                            m_beginReturnHome = false;
                        }
                        else if(m_currentPattern == IGaitController.GaitPattern.RETURNING_HOME)
                        {
                            Debug.Log("gait at home");
                            m_activeGait.UnubscribeFromEvents(this);
                            m_activeGait.Stop();
                            m_activeGait = null;
                            m_currentPattern = IGaitController.GaitPattern.NONE;
                        }
                        else
                        {
                            m_activeGait.SetNextCycle();
                        }
                    }
                    break;
                case IGaitEventListener.EventType.OnGaitReturnedHome:
                 
                    break;
                default:
                    break;
            }

            //   SetGaitPattern(IGaitController.GaitPattern.NONE);

           
        }
    }
}

