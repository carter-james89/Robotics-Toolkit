using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using UnityEngine;

namespace RoboticToolkit.Robotics.Gaits
{
    public interface IGaitEventListener
    {
        public void OnLimbAchievedTarget(Vector3 currentTarget);
    }
    public interface IGait
    {
        public void Initialize(IRobot robot);
        public void RunGait();

        public bool IsRunning();
    }

    public class QuadrupedGait : MonoBehaviour, IGait
    {
        private int m_stridePosition = 0;
        [SerializeField]
        private float m_strideLength = .05f;
        [SerializeField]
        private float m_strideHeight = .1f;
        [SerializeField]
        private float m_gaitTranslateSpeed = .1f;
        [SerializeField]
        private float m_mGaitRotationSpeed = 25;

        private IRoboticLimb[] m_limbs;

        private IRoboticLimb[] m_rotatingLimbs = new IRoboticLimb[2];
        private IRoboticLimb[] m_translatingLimbs = new IRoboticLimb[2];

        private IRobot m_robot;

        public enum Direction
        {
            Forward,
            Rotating,
        }
        public enum StrideType
        {
            NONE,
            STATIONARYSTEP,
            WALKING
        }
        private StrideType m_currentStride = StrideType.NONE;

        public void Initialize(IRobot robot)
        {
            m_robot = robot;
            m_limbs = robot.GetLimbs();

            foreach (var limb in m_limbs)
            {
                //limb.GetPositioner().transform.localPosition -= new Vector3(0, .1f, 0);
            }
        }
        private void SetNextGaitCycle()
        {
            m_stridePosition++;
            if (m_stridePosition == 2)
            {
                m_stridePosition = 0;
            }
            switch (m_stridePosition)
            {
                case 0:
                    m_rotatingLimbs[0] = m_limbs[0];
                    m_rotatingLimbs[1] = m_limbs[2];

                    m_translatingLimbs[0] = m_limbs[1];
                    m_translatingLimbs[1] = m_limbs[3];
                    break;
                case 1:
                    m_rotatingLimbs[0] = m_limbs[1];
                    m_rotatingLimbs[1] = m_limbs[3];

                    m_translatingLimbs[0] = m_limbs[0];
                    m_translatingLimbs[1] = m_limbs[2];
                    break;
                default:
                    break;
            }
             // var direction = Direction.Rotating;
            var direction = Direction.Forward;
            switch (direction)
            {
                case Direction.Forward:
                    foreach (var limb in m_rotatingLimbs)
                    {
                        //limb.GetPositioner().RotateToPosition(new Vector3(0, 0, .05f), Quaternion.identity, .5f, true);
                        limb.GetPositioner().RotateToPosition(m_robot.GetGimbal().GetGameObject().transform.forward, transform.up, .05f, .3f);
                    }
                    foreach (var limb in m_translatingLimbs)
                    {
                        //limb.GetPositioner().TranslateToPosition(new Vector3(0, 0, -.05f), .5f, true);
                        limb.GetPositioner().TranslateToPosition(-m_robot.GetGimbal().GetGameObject().transform.forward, .05f, .3f);
                    }
                    break;
                case Direction.Rotating:
                    float distance = .02f;
                    float time = .6f;
                    m_rotatingLimbs[0].GetPositioner().RotateToPosition(-m_robot.GetGimbal().GetGameObject().transform.right, transform.up, distance, time);
                    m_rotatingLimbs[1].GetPositioner().RotateToPosition(m_robot.GetGimbal().GetGameObject().transform.right, transform.up, distance, time);

                    m_translatingLimbs[0].GetPositioner().TranslateToPosition(m_robot.GetGimbal().GetGameObject().transform.right, distance, time);
                    m_translatingLimbs[1].GetPositioner().TranslateToPosition(-m_robot.GetGimbal().GetGameObject().transform.right, distance, time);
                    break;
                default:
                    break;
            }

        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_currentStride = StrideType.WALKING;
                SetNextGaitCycle();
            }
        }
        public void RunGait()
        {
            if (m_currentStride != StrideType.NONE)
            {
                bool strideComplete = true;
                foreach (var limb in m_limbs)
                {
                    limb.GetPositioner().Run();
                    if(m_rotatingLimbs[0] == limb || m_rotatingLimbs[1] == limb)
                    {
                        limb.RunLimb(false, false);
                    }
                    else
                    {
                        limb.RunLimb(false, true);
                    }
                    if (limb.GetPositioner().StrideComplete() == false)
                    {
                        strideComplete = false;
                    }
                }

                if (strideComplete)
                {
                    SetNextGaitCycle();
                }
            }

            //foreach (var limb in m_limbs)
            //{
            //    limb.RunLimb(false,true);
            //}
        }

        public bool IsRunning()
        {
            return m_currentStride != StrideType.NONE;
        }
    }
}

