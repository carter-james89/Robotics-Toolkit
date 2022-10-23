using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using System.Collections.Generic;
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

        public void Begin();
        public void RunGait();

        public bool IsRunning();
    }

    public class QuadrupedTrotGait : MonoBehaviour, IGait
    {
        private int m_stridePosition = 1;
        [SerializeField]
        private float m_strideLength = .05f;
        [SerializeField]
        private float m_strideHeight = .1f;
        [SerializeField]
        private float m_gaitTranslateSpeed = .1f;
        [SerializeField]
        private float m_mGaitRotationSpeed = 25;

        private bool m_firstStride = true;

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
            var direction = Direction.Forward;
            switch (direction)
            {
                case Direction.Forward:
                    float distance = .04f;
                    float time = .2f;
                    if (m_firstStride)
                    {
                        distance /= 2;
                        time /= 2;
                        m_firstStride = false;
                    }
                    foreach (var limb in m_rotatingLimbs)
                    {
                        //limb.GetPositioner().RotateToPosition(new Vector3(0, 0, .05f), Quaternion.identity, .5f, true);
                        limb.GetPositioner().RotateToPosition(m_robot.GetGimbal().GetGameObject().transform.forward, m_robot.GetGimbal().GetGameObject().transform.up, distance, time - (time * .25f));
                    }
                    foreach (var limb in m_translatingLimbs)
                    {
                        //limb.GetPositioner().TranslateToPosition(new Vector3(0, 0, -.05f), .5f, true);
                        limb.GetPositioner().TranslateToPosition(-m_robot.GetGimbal().GetGameObject().transform.forward, m_robot.GetGimbal().GetGameObject().transform.up, distance, time);
                    }
                    break;
                case Direction.Rotating:
                    distance = .02f;
                    time = .4f;
                    m_rotatingLimbs[0].GetPositioner().RotateToPosition(-m_robot.GetGimbal().GetGameObject().transform.right, Vector3.up, distance, time);
                    m_rotatingLimbs[1].GetPositioner().RotateToPosition(m_robot.GetGimbal().GetGameObject().transform.right, Vector3.up, distance, time);

                    m_translatingLimbs[0].GetPositioner().TranslateToPosition(m_robot.GetGimbal().GetGameObject().transform.right, m_robot.GetGimbal().GetGameObject().transform.up, distance, time);
                    m_translatingLimbs[1].GetPositioner().TranslateToPosition(-m_robot.GetGimbal().GetGameObject().transform.right, m_robot.GetGimbal().GetGameObject().transform.up, distance, time);
                    break;
                default:
                    break;
            }
            m_stridePosition++;
            if (m_stridePosition == 2)
            {

                m_stridePosition = 0;
            }
        }

        private bool m_postStrideCooldown = false;
        private float m_postStrideCooldownTime = 0;
        private float m_postStrideCooldownTargetTime = 1f;
        public void RunGait()
        {
            if (m_postStrideCooldown)
            {
                m_postStrideCooldownTime += Time.deltaTime;
                if (m_postStrideCooldownTime >= m_postStrideCooldownTargetTime)
                {
                    SetNextGaitCycle();
                    m_postStrideCooldown = false;
                    m_postStrideCooldownTime = 0;
                }
            }
            if (m_postStrideCooldown)
            {
               // Debug.Log("cooldown");
                foreach (var limb in m_limbs)
                {
                    limb.RunLimb(true, true);
                }
                return;
            }
            if (m_currentStride != StrideType.NONE)
            {
                List<ILimbPositioner> m_limbsAtTarget = new List<ILimbPositioner>();
                foreach (var limb in m_limbs)
                {

                    limb.GetPositioner().Run();
                    if (m_rotatingLimbs[0] == limb || m_rotatingLimbs[1] == limb)
                    {
                        limb.RunLimb(true, true);
                    }
                    else
                    {
                        limb.RunLimb(true, true);
                    }
                    if (limb.GetPositioner().StrideComplete() == true)
                    {
                      //   Debug.Log(limb.GetPositioner().cu);
                        m_limbsAtTarget.Add(limb.GetPositioner());  
                    }
                    else
                    {
                        Debug.Log("Waiting for : " + limb.GetGameObject().name);
                    }
                }
                //Debug.Log(m_limbsAtTarget.Count);
                if (m_limbsAtTarget.Count >=3)
                {
                    m_postStrideCooldown = true;
                    // SetNextGaitCycle();
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

        public void Begin()
        {
            m_currentStride = StrideType.WALKING;
            SetNextGaitCycle();
        }
    }
}


