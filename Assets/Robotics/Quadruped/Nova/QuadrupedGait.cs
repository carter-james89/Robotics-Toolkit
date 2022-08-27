using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticToolkit.Robotics.Gaits
{
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

        public bool Walking { get; private set; } = false;

        public void Initialize(IRoboticController robot)
        {
            m_limbs = robot.GetLimbs();

            foreach (var limb in m_limbs)
            {
                //limb.GetPositioner().transform.localPosition -= new Vector3(0, .1f, 0);
            }
        }
        private void SetNextGaitCycle()
        {
            var flLimb = m_limbs[0].GetPositioner();
            var frLimb = m_limbs[1].GetPositioner();
            var brLimb = m_limbs[2].GetPositioner();
            var blLimb = m_limbs[3].GetPositioner();

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
                    //case 1:
                    //    frLimb.RotateToPosition(new Vector3(m_strideLength,0,0), m_mGaitRotationSpeed, m_strideHeight);
                    //    blLimb.RotateToPosition(new Vector3(-m_strideLength, 0, 0), m_mGaitRotationSpeed, m_strideHeight);

                    //    flLimb.TranslateToPosition(new Vector3(-m_strideLength, 0, 0), m_gaitTranslateSpeed);
                    //    brLimb.TranslateToPosition(new Vector3(m_strideLength, 0, 0), m_gaitTranslateSpeed);
                    //    break;
                    //case 2:
                    //    flLimb.RotateToPosition(new Vector3(-m_strideLength, 0, 0), m_mGaitRotationSpeed, m_strideHeight);
                    //    brLimb.RotateToPosition(new Vector3(m_strideLength, 0, 0), m_mGaitRotationSpeed, m_strideHeight);

                    //    frLimb.TranslateToPosition(new Vector3(-m_strideLength, 0, 0), m_gaitTranslateSpeed);
                    //    blLimb.TranslateToPosition(new Vector3(m_strideLength, 0, 0), m_gaitTranslateSpeed);
                    //    break;
                    //default:
                    //    break;
            }
        }

        void Update()
        {
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
           

        }
        public void RunGait()
        {
            if (Walking)
            {
                foreach (var limb in m_limbs)
                {
                    limb.GetPositioner().Run();
                }
                bool strideComplete = true;
                foreach (var limb in m_limbs)
                {
                    if (limb.GetPositioner().CheckLimbAtTarget() == false)
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
    }
}

