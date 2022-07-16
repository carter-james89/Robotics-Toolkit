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
        public void Reset();

        public Transform GetEndPoint();

        public IServoController[] GetServoControllers();

        public void RunLimb();

        public LimbPositioner GetPositioner();
    }
    public class ThreeJointRoboticLimb : MonoBehaviour, IRoboticLimb
    {
        public GameObject GetGameObject() => gameObject;
        [SerializeField]
        private GameObject m_shoulderServoObject;
        private IServoController m_shoulderServoController;

        private IServoController[] m_servoControllers;
        public IServoController[] GetServoControllers() => m_servoControllers;

        [SerializeField]
        private LimbPositioner m_positioner;

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

        [SerializeField]
        private Transform m_shoulderTarget;


        [SerializeField]
        private Transform m_baseTarget;

        public Transform GetBaseTarget() => m_baseTarget;

        private float m_desiredLimbHeight = .15f;
        public void SetLimbHeight(float desiredHeight)
        {
            m_desiredLimbHeight = desiredHeight;
        }

        public void Reset()
        {
            m_shoulderServoController.Reset();
            m_elbowServoController.Reset();
            m_wristServoController.Reset();
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
        private Vector3 m_positionerOffset;


        private void Awake()
        {
            m_shoulderServoController = m_shoulderServoObject.GetComponent<IServoController>();
            m_elbowServoController = m_elbowServoObject.GetComponent<IServoController>();
            m_wristServoController = m_wristServoObject.GetComponent<IServoController>();
            m_positionerOffset = transform.InverseTransformPoint(m_positioner.GetTarget().position);

            m_servoControllers = new IServoController[3];
            m_servoControllers[0] = m_shoulderServoController;
            m_servoControllers[1] = m_elbowServoController;
            m_servoControllers [2] = m_wristServoController;

            // m_positioner.transform.position = m_endPoint.transform.position;

            // m_wristServoController.
        }

        public float heightOffset;

        public void RunLimb()
        {
            if (m_positioner.GetMovementStyle() != LimbPositioner.MovementStyle.Rotate)
            {
                // heightOffset = m_baseTarget.position.y - m_shoulderServoController.GetServo().GetGameObject().transform.position.y;
                heightOffset = transform.InverseTransformPoint(m_baseTarget.position).y;

                PositionGaitHeight(heightOffset);
            }
            else
            {
                PositionGaitHeight(0);
            }

            //  m_positioner.transform.position = Vector3.Lerp(m_positioner.transform.position, m_targetGaitPos, Time.deltaTime * 5);


            m_shoulderServoController.SetAndRunServo(
           IKCalculator.CalculateSingleIK(m_shoulderServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
           m_positioner.GetTarget().position,
           true));

            var elbowWristAngles = IKCalculator.CalculateDuelIK(m_elbowServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
                m_wristServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
                m_endPoint.position, m_positioner.GetTargetOffset().transform.position);

            m_elbowServoController.SetAndRunServo(elbowWristAngles.Key - 8);//hard coded guess for angle offset with elbow
            m_wristServoController.SetAndRunServo(elbowWristAngles.Value);
        }
        private Vector3 m_targetGaitPos;
        [SerializeField]
        private Vector3 m_currentGaitOffset;
        [SerializeField]
        private Vector3 m_currentGaitLocalOffset;
        public void PositionGaitHeight(float height)
        {
           // var tempGlobalPos = m_positioner.transform.position;
           // tempGlobalPos.y = height;

           // m_currentGaitLocalOffset = new Vector3(m_positionerOffset.x, m_positionerOffset.y - height, m_positionerOffset.z);
            //var newGaitPos = transform.InverseTransformPoint(m_positioner.transform.position);
            var gaitPos = transform.TransformPoint(new Vector3(m_positionerOffset.x, 0, m_positionerOffset.z));
            gaitPos.y = -height;
            // m_positioner.transform.position = gaitPos;
            m_targetGaitPos = gaitPos;
           // m_positioner.transform.position = m_targetGaitPos;
             m_positioner.transform.position = Vector3.Lerp(m_positioner.transform.position, m_targetGaitPos, Time.deltaTime * 3);
            m_currentGaitOffset = transform.InverseTransformPoint(m_positioner.transform.position);
            //var gatePos = m_positioner.transform.localPosition;
            //gatePos.y = height;
            //m_positioner.transform.localPosition = gatePos;



        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public LimbPositioner GetPositioner()
        {
         return m_positioner;
        }
    }
}
