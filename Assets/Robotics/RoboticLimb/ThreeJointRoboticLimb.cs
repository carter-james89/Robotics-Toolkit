using RoboticsToolkit.Robotics;
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
        public ILimbPositioner GetPositioner();
        public Transform GetEndPoint();
        public Transform GetTargetBasePosition();
        public IServoController[] GetServoControllers();
        public void RunLimb(bool positionImmediate, bool adjustHeight = false);
        public void ResetLimb();
        public void ResetLimbTargetPosition();



        public void SetIKTargetPos(Vector3 globalPos);
        public Vector3 GetIKTargetPos();

        public bool LimbAtTarget();

        public bool BaseAtTarget();

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

        [SerializeField]
        private bool m_leftLeg = false;

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

        private Transform m_heightAdjustementOrigin;

        private float m_desiredLimbHeight = .15f;

        private Vector3 m_positionerOffset;

        public Transform GetTargetBasePosition() => m_baseTarget;

        private IGimbal m_gimbal;
        private IRobot m_robot;

        private bool m_useGimbalHeightAdjustment = false;


        private void Awake()
        {
            m_shoulderServoController = m_shoulderServoObject.GetComponent<IServoController>();
            m_elbowServoController = m_elbowServoObject.GetComponent<IServoController>();
            m_wristServoController = m_wristServoObject.GetComponent<IServoController>();
            // m_positionerOffset = transform.InverseTransformPoint(m_heightAdjustment.position);
            // m_positionerOffset.y = -.1f;

            m_servoControllers = new IServoController[3];
            m_servoControllers[0] = m_shoulderServoController;
            m_servoControllers[1] = m_elbowServoController;
            m_servoControllers[2] = m_wristServoController;

            m_positioner = GetComponentInChildren<ILimbPositioner>();
        }
        public void Initialize(IRobot robot, bool useGimbalHeightAdjustment)
        {
            m_robot = robot;
            m_gimbal = robot.GetGimbal();
            m_useGimbalHeightAdjustment = useGimbalHeightAdjustment;
            m_heightAdjustementOrigin = useGimbalHeightAdjustment ? m_gimbal.GetGameObject().transform : robot.GetGameObject().transform;// transform;
            m_positionerOffset = m_heightAdjustementOrigin.InverseTransformPoint(m_heightAdjustment.position);
            // m_positionerOffset.y = -.1f;

            var hipOffset = m_heightAdjustementOrigin.InverseTransformPoint(m_shoulderServoController.GetServo().GetGameObject().transform.position);
            var ikOffset = m_heightAdjustementOrigin.InverseTransformPoint(m_ikTarget.position);

            if (ikOffset.x > hipOffset.x)
            {
                m_hipFootOffset = ikOffset.x - hipOffset.x;
            }
            else
            {
                m_hipFootOffset = hipOffset.x - ikOffset.x;
            }
        }
        public void SetLimbHeight(float desiredHeight)
        {
            m_desiredLimbHeight = desiredHeight;
        }
        public void SetIKTargetPos(Vector3 localPos)
        {
            m_ikTarget.position = localPos;
        }
        public Vector3 GetIKTargetPos()
        {
            return  m_ikTarget.position;
        }
        public bool LimbAtTarget()
        {
            if (Vector3.Distance(m_endPoint.position, m_ikTarget.position) < .015f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool BaseAtTarget()
        {
            if (Vector3.Distance(transform.position, m_baseTarget.transform.position) < .015f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public Transform DebugCube;
        private ILimbPositioner m_positioner;

        public void RunLimb(bool positionServoImmediate, bool adjustHeight = false)
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
               // Debug.Log("set limb to 0 " + name);
                PositionGaitHeight(0);
            }
            //return;
            //  m_positioner.transform.position = Vector3.Lerp(m_positioner.transform.position, m_targetGaitPos, Time.deltaTime * 5);

            var tempPos = transform.InverseTransformPoint(m_ikTarget.position);
            tempPos.x -= m_hipFootOffset;
            var baseTarget = transform.TransformPoint(tempPos);
            if (DebugCube)
            {
                DebugCube.position = baseTarget;
            }
            var limbBaseAngle = IKCalculator.CalculateSingleIK(m_shoulderServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
           baseTarget, true);

            if (m_useGimbalHeightAdjustment)
            {
                var offset = m_robot.GetGameObject().transform.eulerAngles.z;
                offset = Mathf.Clamp(offset, -5, 5);
                if (!m_leftLeg)
                {
                   // limbBaseAngle += offset;
                }
                else
                {
                   // limbBaseAngle -= offset;
                }
            }
            m_shoulderServoController.SetAndRunServo(limbBaseAngle, positionServoImmediate);

            // var limbBaseAngle = IKCalculator.CalculateSingleIK(m_elbowServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
            //m_positioner.GetTarget().position, true);
            // m_shoulderServoController.SetAndRunServo(limbBaseAngle);

            var elbowWristAngles = IKCalculator.CalculateDuelIK(m_elbowServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
                m_wristServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
                m_endPoint.position, m_ikTarget.position);

            m_elbowServoController.SetAndRunServo(elbowWristAngles.Key, positionServoImmediate);
            m_wristServoController.SetAndRunServo(elbowWristAngles.Value, positionServoImmediate);
        }

        public void PositionGaitHeight(float height)
        {
            //var tempPos = m_positionerOffset;
            //tempPos.y -= height;
            //var gaitPos = m_heightAdjustementOrigin.transform.TransformPoint(tempPos);
            //m_heightAdjustment.position = Vector3.Lerp(m_heightAdjustment.position, gaitPos, Time.deltaTime * .1f);

            m_positionerOffset.y = -m_desiredLimbHeight - height;

            var gaitPos = m_heightAdjustementOrigin.TransformPoint(m_positionerOffset);

            // Debug.Log(-m_desiredLimbHeight);
            //m_heightAdjustment.position = gaitPos;
            //return;

            //if (height == 0)
            //{
            //    gaitPos.y = 0;
            //    m_heightAdjustment.position = gaitPos;

            //}
            //gaitPos.y = -height;
           // var pos  = new Vector3(gaitPos.x,m_heightAdjustment.position.y,gaitPos.z);
           // m_heightAdjustment.position = Vector3.Lerp(m_heightAdjustment.position, pos, Time.deltaTime * .5f);
            m_heightAdjustment.position = Vector3.Lerp(m_heightAdjustment.position, gaitPos, Time.deltaTime * .8f);

        }
        public void ResetLimbTargetPosition()
        {
            var gaitPos = m_heightAdjustementOrigin.TransformPoint(m_positionerOffset);
            m_heightAdjustment.position = gaitPos;
            m_ikTarget.localPosition = Vector3.zero;
        }
        public void ResetLimb()
        {
            m_shoulderServoController.Reset();
            m_elbowServoController.Reset();
            m_wristServoController.Reset();
        }

        public ILimbPositioner GetPositioner()
        {
            return m_positioner;
        }

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

        //void Update()
        //{
        //    PlaceLimb();
        //}
        //void FixedUpdate()
        //{
        //    PlaceLimb();
        //}

        //void PlaceLimb()
        //{
        //    var ab = GetComponent<ArticulationBody>();
        //   ab.TeleportRoot(transform.parent.TransformPoint(m_desiredLocalPosition), transform.parent.rotation);
        //    ab.velocity = Vector3.zero;
        //    ab.angularVelocity = Vector3.zero;
        //    ab.Sleep();
        //}
    }
}
