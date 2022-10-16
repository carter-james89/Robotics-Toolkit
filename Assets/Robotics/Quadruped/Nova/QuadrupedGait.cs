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

        public enum StrideType
        {
            NONE,
            STATIONARYSTEP,
            WALKING
        }
        private StrideType m_currentStride = StrideType.NONE;

        public void Initialize(IRobot robot)
        {
            m_limbs = robot.GetLimbs();

            foreach (var limb in m_limbs)
            {
                //limb.GetPositioner().transform.localPosition -= new Vector3(0, .1f, 0);
            }
        }
        private void SetNextGaitCycle()
        {
            var flLimb = m_limbs[0].GetGameObject().GetComponentInChildren<LimbPositioner>();
            var frLimb = m_limbs[1].GetGameObject().GetComponentInChildren<LimbPositioner>();
            var brLimb = m_limbs[2].GetGameObject().GetComponentInChildren<LimbPositioner>();
            var blLimb = m_limbs[3].GetGameObject().GetComponentInChildren<LimbPositioner>();

            m_stridePosition++;
            if (m_stridePosition == 3)
            {
                m_stridePosition = 1;
            }
            switch (m_stridePosition)
            {
                case 1:
                    frLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);
                    blLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);

                    flLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
                    brLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
                    break;
                case 2:
                    flLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);
                    brLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);

                    frLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
                    blLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
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
                foreach (var limb in m_limbs)
                {
                    limb.GetGameObject().GetComponentInChildren<LimbPositioner>().Run();
                }
                bool strideComplete = true;
                foreach (var limb in m_limbs)
                {
                    if (limb.GetGameObject().GetComponentInChildren<LimbPositioner>().CheckLimbAtTarget() == false)
                    {
                        strideComplete = false;
                    }
                }
                if (strideComplete)
                {
                    SetNextGaitCycle();
                }
            }

            foreach (var limb in m_limbs)
            {
                //if (limb.GetPositioner().GetMovementStyle() != LimbPositioner.MovementStyle.Rotate)
                //{
                //    // heightOffset = m_baseTarget.position.y - m_shoulderServoController.GetServo().GetGameObject().transform.position.y;
                //    // heightOffset = transform.InverseTransformPoint(m_baseTarget.position).y;

                //    var heightOffset = limb.GetGameObject().transform.position.y - limb.GetTargetBasePosition().position.y;

                //    limb.GetPositioner().transform.localPosition PositionGaitHeight(-heightOffset);
                //}
                //else
                //{
                //    limb.PositionGaitHeight(0);
                //}
                limb.RunLimb(true);
            }
        }

        public bool IsRunning()
        {
            throw new System.NotImplementedException();
        }
    }
}

