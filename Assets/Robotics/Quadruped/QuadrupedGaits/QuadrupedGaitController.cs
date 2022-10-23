using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using UnityEngine;

namespace RoboticToolkit.Robotics.Gaits
{


    public class QuadrupedGaitController : MonoBehaviour, IGaitController
    {
        private IRoboticLimb[] m_limbs;
        private IRobot m_robot;

        [SerializeField]
        private QuadrupedCrawlGait m_crawlGait;
        [SerializeField]
        private QuadrupedTrotGait m_trotGait;

        private IGait m_activeGait;

        private IGaitController.GaitPattern m_currentPattern = IGaitController.GaitPattern.NONE;

        public void Initialize(IRobot robot)
        {
            m_robot = robot;
            m_limbs = robot.GetLimbs();

            m_trotGait.Initialize(m_robot);
            m_crawlGait.Initialize(m_robot);

            foreach (var limb in m_limbs)
            {
                //limb.GetPositioner().transform.localPosition -= new Vector3(0, .1f, 0);
            }
        }
      
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
               SetGaitPattern(IGaitController.GaitPattern.TROT);
                //SetNextGaitCycle();
            }
        }
        public void Run()
        {
            if(m_activeGait != null)
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

            m_activeGait.Begin();
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
    }
}

