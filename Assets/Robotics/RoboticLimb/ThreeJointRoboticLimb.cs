using RoboticToolkit.Robotics.Gaits;
using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticToolkit.Robotics.Limbs
{
    public interface IRoboticLimb
    {
        public GameObject GetGameObject();
       // public LimbPositioner GetPositioner();
        public Transform GetEndPoint();
        public Transform GetTargetBasePosition();
        public IServoController[] GetServoControllers();
        public void RunLimb(bool adjustHeight = false);
        public void ResetLimb();

        public void SetIKTargetPos(Vector3 globalPos);
        public Vector3 GetIKTargetPos();

        public bool LimbAtTarget();

    }
    public class ThreeJointRoboticLimb : MonoBehaviour, IRoboticLimb
    {    
        [SerializeField]
        private Vector3 m_currentGaitOffset;
        [SerializeField]
        private Vector3 m_currentGaitLocalOffset;
        [SerializeField]
        private Transform m_shoulderTarget;
        [SerializeField]
        private Transform m_baseTarget;

        public float heightOffset;
        private Vector3 m_targetGaitPos;

        public GameObject GetGameObject() => gameObject;
        [SerializeField]
        private GameObject m_shoulderServoObject;
        private IServoController m_shoulderServoController;
        private IServoController[] m_servoControllers;
        public IServoController[] GetServoControllers() => m_servoControllers;

        private float m_hipFootOffset = 0;

        // [SerializeField]
        // private LimbPositioner m_positioner;

        // private float m_hipOffset = 0;

        [SerializeField]
        private Transform m_heightAdjustment;

        [SerializeField]
        private Transform m_ikTarget;

        [SerializeField]
        private GameObject m_elbowServoObject;
        private IServoController m_elbowServoController;

        [SerializeField]
        private GameObject m_wristServoObject;
        private IServoController m_wristServoController;

        public IServoController ShoulderServoController => m_shoulderServoController;
        public IServoController ElbowServoController => m_elbowServoController;
        public IServoController WristServoController => m_wristServoController;

        [SerializeField]
        private Transform m_endPoint;
        public Transform GetEndPoint() => m_endPoint; 

        public Transform GetBaseTarget() => m_baseTarget;

        private float m_desiredLimbHeight = .15f;

        private Vector3 m_positionerOffset;

        public Transform GetTargetBasePosition() => m_baseTarget;


        private void Awake()
        {
            m_shoulderServoController = m_shoulderServoObject.GetComponent<IServoController>();
            m_elbowServoController = m_elbowServoObject.GetComponent<IServoController>();
            m_wristServoController = m_wristServoObject.GetComponent<IServoController>();
            m_positionerOffset = transform.InverseTransformPoint(m_heightAdjustment.position);
            m_positionerOffset.y = -.1f;

            m_servoControllers = new IServoController[3];
            m_servoControllers[0] = m_shoulderServoController;
            m_servoControllers[1] = m_elbowServoController;
            m_servoControllers[2] = m_wristServoController;

            var hipOffset = transform.InverseTransformPoint(m_shoulderServoController.GetServo().GetGameObject().transform.position);
            var offset = transform.InverseTransformPoint(m_ikTarget.position);
            m_hipFootOffset = offset.x - hipOffset.x;

            // m_positioner.transform.position = m_endPoint.transform.position;

            // m_wristServoController.
        }
        public void SetLimbHeight(float desiredHeight)
        {
            m_desiredLimbHeight = desiredHeight;
        }
        public void SetIKTargetPos(Vector3 localPos)
        {
            m_ikTarget.localPosition = localPos;
        }
        public Vector3 GetIKTargetPos()
        {
            return m_ikTarget.localPosition;
        }
        public bool LimbAtTarget()
        {
            if(Vector3.Distance(m_endPoint.position,m_ikTarget.position) < .015f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void RunLimb(bool adjustHeight = false)
        {
            if (adjustHeight)
            {
               // if (m_positioner.GetMovementStyle() != LimbPositioner.MovementStyle.Rotate)
               // {
                    // heightOffset = m_baseTarget.position.y - m_shoulderServoController.GetServo().GetGameObject().transform.position.y;
                    // heightOffset = transform.InverseTransformPoint(m_baseTarget.position).y;

                    heightOffset = transform.position.y - m_baseTarget.position.y;

                    PositionGaitHeight(-heightOffset);
               // }
               // else
               // {
                  //  PositionGaitHeight(0);
              //  }
            }
            else
            {
                PositionGaitHeight(0);
            }
            //return;
            //  m_positioner.transform.position = Vector3.Lerp(m_positioner.transform.position, m_targetGaitPos, Time.deltaTime * 5);

            var tempPos = transform.InverseTransformPoint(m_ikTarget.position);
            tempPos.x -= m_hipFootOffset;
            var baseTarget = transform.TransformPoint(tempPos);
            var limbBaseAngle = IKCalculator.CalculateSingleIK(m_shoulderServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
           baseTarget, true);
            m_shoulderServoController.SetAndRunServo(limbBaseAngle);

            // var limbBaseAngle = IKCalculator.CalculateSingleIK(m_elbowServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
            //m_positioner.GetTarget().position, true);
            // m_shoulderServoController.SetAndRunServo(limbBaseAngle);

            var elbowWristAngles = IKCalculator.CalculateDuelIK(m_elbowServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
                m_wristServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
                m_endPoint.position, m_ikTarget.position);

            m_elbowServoController.SetAndRunServo(elbowWristAngles.Key);
            m_wristServoController.SetAndRunServo(elbowWristAngles.Value);
        }
      
        public void PositionGaitHeight(float height)
        {
            var gaitPos = transform.TransformPoint(new Vector3(m_positionerOffset.x, 0, m_positionerOffset.z));
            gaitPos.y = -height;
           // gaitPos.y = transform.position.y - .2f;

           m_targetGaitPos = transform.InverseTransformPoint( gaitPos);
            m_heightAdjustment.localPosition = Vector3.Lerp(m_heightAdjustment.localPosition, m_targetGaitPos, Time.deltaTime * 2);

        }

        public void ResetLimb()
        {
            m_shoulderServoController.Reset();
            m_elbowServoController.Reset();
            m_wristServoController.Reset();
        }

        //public LimbPositioner GetPositioner()
        //{
        //    return m_positioner;
        //}

        //public bool LimbAtTarget()
        //{
        //    if (!m_positioner.LimbAtTarget)
        //    {
        //        return false;
        //    }
        //    if (Vector3.Distance(m_endPoint.position, GetPositioner().GetTargetOffset().position) < .007f)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
    }
}
