using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Limbs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public class GaitController : MonoBehaviour, IGaitController, IGaitEventListener
    {
        private IRoboticLimb[] _puppetLimbs;
        private ILimbPositioner[] _limbPositioners;
        // private IRobot m_robot;

        private bool m_postStrideCooldown = false;
        private float m_postStrideCooldownTime = 0;
        private float m_postStrideCooldownTargetTime = .5f;

        private Vector3 _currentWalkDirection;
        private bool _rotating = false;

        [SerializeField]
        private TrotGait m_trotGait;
        [SerializeField]
        private CrawlGait m_crawlGait;

        private bool m_beginReturnHome = false;

        private IGait m_activeGait;

        private IGaitController.GaitPattern m_currentPattern = IGaitController.GaitPattern.NONE;

        private enum GaitType
        {
            Crawl,
            Trot,
        }
        private GaitType _gaitType = GaitType.Crawl;

        public void Initialize(ILimbPositioner[] limbPositioners, IRoboticLimb[] puppetLimbs)
        {
            _puppetLimbs = puppetLimbs;
            _limbPositioners = limbPositioners;
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
        private float _speed;

        public void PerformHighStep(float height, float speed)
        {
            _speed = speed;
            m_activeGait = m_crawlGait;
            bool approved = m_activeGait.RequestBeginCMD(this, _limbPositioners);
           
            if (approved)
            {
                m_activeGait.SubscribeToEvents(this);
                m_activeGait.SetStrideValues(0, height);
                m_activeGait.SetNextCycle(transform.forward, _limbPositioners, speed, false);

                m_currentPattern = IGaitController.GaitPattern.STATIONARYSTEP;
            }
        }

        public void CrawlForward(ILimbPositioner[] limbs, float height, float speed, float stride)
        {
            _speed = speed;
            m_activeGait = m_crawlGait;
            bool approved = m_activeGait.RequestBeginCMD(this, limbs);

            if (approved)
            {
                m_activeGait.SubscribeToEvents(this);
                m_activeGait.SetStrideValues(stride, height);
                m_activeGait.SetNextCycle(transform.forward, limbs, speed, false);

                m_currentPattern = IGaitController.GaitPattern.CRAWL;
            }
        }
        public void TrotForward(ILimbPositioner[] limbs, float height, float speed, float stride)
        {
            _speed = speed;
            m_activeGait = m_trotGait;
            bool approved = m_activeGait.RequestBeginCMD(this, limbs);

            if (approved)
            {
                m_activeGait.SubscribeToEvents(this);
                m_activeGait.SetStrideValues(stride, height);
                m_activeGait.SetNextCycle(transform.forward, limbs, speed, false);

                m_currentPattern = IGaitController.GaitPattern.TROT;
            }
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
                    // m_activeGait = m_crawlGait;
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
                //  m_activeGait.SetNextCycle(direction, limbs, _rotating);
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
                    //    BeginMovement(limbs, IGaitController.GaitPattern.TROT, m_robot.GetGimbal().GetGameObject().transform.forward,false);
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    foreach (var limb in _puppetLimbs)
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
                    //  BeginMovement(limbs, IGaitController.GaitPattern.TROT, -m_robot.GetGimbal().GetGameObject().transform.forward,false);
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                if (m_currentPattern == IGaitController.GaitPattern.NONE)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        //  m_trotGait.SetStrideValues(m_forwardTrotStrideDistance, m_forwardTrotStrideTime, m_forwardTrotStrideCoolDownTime);
                        //   BeginMovement(limbs, IGaitController.GaitPattern.TROT, m_robot.GetGimbal().GetGameObject().transform.right,true);
                    }
                    else
                    {
                        //  m_trotGait.SetStrideValues(m_rotatingTrotStrideDistance, m_rotatingTrotStrideTime, m_rotatingTrotStrideCoolDownTime);
                        //   BeginMovement(limbs, IGaitController.GaitPattern.TROT, m_robot.GetGimbal().GetGameObject().transform.right, true);
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
                        //    BeginMovement(limbs, IGaitController.GaitPattern.TROT, -m_robot.GetGimbal().GetGameObject().transform.right, false);
                    }
                    else
                    {
                        //   m_trotGait.SetStrideValues(m_rotatingTrotStrideDistance, m_rotatingTrotStrideTime, m_rotatingTrotStrideCoolDownTime);
                        //  BeginMovement(limbs, IGaitController.GaitPattern.TROT, m_robot.GetGimbal().GetGameObject().transform.right, false);
                    }
                }
            }
            else if (m_activeGait != null && (m_currentPattern != IGaitController.GaitPattern.NONE && m_currentPattern != IGaitController.GaitPattern.RETURNING_HOME))
            {
                m_beginReturnHome = true;
            }
        }
        public void Run()
        {
            if (m_currentPattern == IGaitController.GaitPattern.NONE)
            {
                return;
            }
            if (!m_postStrideCooldown)
            {
                if(m_activeGait != null)
                {
                    m_activeGait.CheckLimbPositions(_limbPositioners);
                }
            }
            else
            {
                m_postStrideCooldownTime += Time.deltaTime;
                if (m_postStrideCooldownTime >= m_postStrideCooldownTargetTime)
                {
                    m_postStrideCooldown = false;
                    m_activeGait.SetNextCycle(transform.forward, _limbPositioners, _speed, _rotating);
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
                    // m_activeGait = m_crawlGait;
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

        public void OnGaitEventOccured(GaitEventData eventData)
        {
            switch (eventData.EventType)
            {
                case EventType.OnGaitCycleBegin:
                    break;
                case EventType.OnGaitPointHit:
                    m_postStrideCooldownTime = 0;
                    m_postStrideCooldown = true;
                //    m_activeGait.SetNextCycle(_currentWalkDirection, _limbPositioners, _speed, _rotating);
                    break;
                case EventType.OnGaitCycleComplete:
                    break;
                case EventType.OnGaitReturnedHome:
                    break;
                default:
                    break;
            }
            //switch (eventData.EventType)
            //{
            //    case IGaitEventListener.EventType.OnGaitCycleBegin:
            //        break;
            //    case IGaitEventListener.EventType.OnGaitCycleComplete:
            //        if (m_currentPattern != IGaitController.GaitPattern.NONE)
            //        {
            //            if (m_beginReturnHome)
            //            {
            //                m_activeGait.ReturnHome();
            //                m_activeGait.SetNextCycle();
            //                m_currentPattern = IGaitController.GaitPattern.RETURNING_HOME;
            //                m_beginReturnHome = false;
            //            }
            //            else if (m_currentPattern == IGaitController.GaitPattern.RETURNING_HOME)
            //            {
            //                Debug.Log("gait at home");
            //                m_activeGait.UnubscribeFromEvents(this);
            //                m_activeGait.Stop();
            //                m_activeGait = null;
            //                m_currentPattern = IGaitController.GaitPattern.NONE;
            //            }
            //            else
            //            {
            //                m_activeGait.SetNextCycle();
            //            }
            //        }
            //        break;
            //    case IGaitEventListener.EventType.OnGaitReturnedHome:

            //        break;
            //    default:
            //        break;
            //}

            //   SetGaitPattern(IGaitController.GaitPattern.NONE);


        }

      
    }
}

