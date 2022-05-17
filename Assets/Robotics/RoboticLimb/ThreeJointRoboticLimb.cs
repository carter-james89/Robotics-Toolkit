using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticToolkit.Robotics.Limbs
{
    public class ThreeJointRoboticLimb : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_shoulderServoObject;
        private IServoController m_shoulderServoController;

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

        [SerializeField]
        private Transform m_shoulderTarget;

        [SerializeField]
        private RoboticToolKit.Robotics.Limbs.LimbGait m_gait;

        [SerializeField]
        private Transform m_baseTarget;

        public Transform GetBaseTarget() => m_baseTarget;

        private float m_desiredLimbHeight = .15f;
        public void SetLimbHeight(float desiredHeight)
        {
            m_desiredLimbHeight = desiredHeight;
        }

        public bool LimbAtTarget()
        {
            if (!m_gait.AtTarget)
            {
                return false;
            }
            if (Vector3.Distance(m_endPoint.position, GetGait().GetTargetOffset().position) < .01f)
            {
                return true;
            }
            return false;
        }
        private Vector3 m_gaitOffset;

        public RoboticToolKit.Robotics.Limbs.LimbGait GetGait()
        {
            return m_gait;
        }

        private void Awake()
        {
            m_shoulderServoController = m_shoulderServoObject.GetComponent<IServoController>();
            m_elbowServoController = m_elbowServoObject.GetComponent<IServoController>();
            m_wristServoController = m_wristServoObject.GetComponent<IServoController>();
            m_gaitOffset = transform.InverseTransformPoint(m_gait.GetTarget().position);

            // m_gait.transform.position = m_endPoint.transform.position;

            // m_wristServoController.
        }

        public float heightOffset;

        private void FixedUpdate()
        {
            if (m_gait.GetMovementStyle() != RoboticToolKit.Robotics.Limbs.IGait.MovementStyle.Rotate)
            {
                // heightOffset = m_baseTarget.position.y - m_shoulderServoController.GetServo().GetGameObject().transform.position.y;
                heightOffset = transform.InverseTransformPoint(m_baseTarget.position).y;

                PositionGaitHeight(heightOffset);
            }
            else
            {
                PositionGaitHeight(0);
            }

            //  m_gait.transform.position = Vector3.Lerp(m_gait.transform.position, m_targetGaitPos, Time.deltaTime * 5);


            m_shoulderServoController.SetAndRunServo(
           IKCalculator.CalculateSingleIK(m_shoulderServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
           m_gait.GetTarget().position,
           true));

            var elbowWristAngles = IKCalculator.CalculateDuelIK(m_elbowServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
                m_wristServoController.GetServo().GetGameObject().GetComponent<ArticulationBody>(),
                m_endPoint.position, m_gait.GetTargetOffset().transform.position);

            m_elbowServoController.SetAndRunServo(elbowWristAngles.Key - 8);
            m_wristServoController.SetAndRunServo(elbowWristAngles.Value);
        }
        private Vector3 m_targetGaitPos;
        [SerializeField]
        private Vector3 m_currentGaitOffset;
        [SerializeField]
        private Vector3 m_currentGaitLocalOffset;
        public void PositionGaitHeight(float height)
        {
           // var tempGlobalPos = m_gait.transform.position;
           // tempGlobalPos.y = height;

           // m_currentGaitLocalOffset = new Vector3(m_gaitOffset.x, m_gaitOffset.y - height, m_gaitOffset.z);
            //var newGaitPos = transform.InverseTransformPoint(m_gait.transform.position);
            var gaitPos = transform.TransformPoint(new Vector3(m_gaitOffset.x, 0, m_gaitOffset.z));
            gaitPos.y = -height;
            // m_gait.transform.position = gaitPos;
            m_targetGaitPos = gaitPos;
            //m_gait.transform.position = m_targetGaitPos;
              m_gait.transform.position = Vector3.Lerp(m_gait.transform.position, m_targetGaitPos, Time.deltaTime * 3);
            m_currentGaitOffset = transform.InverseTransformPoint(m_gait.transform.position);
            //var gatePos = m_gait.transform.localPosition;
            //gatePos.y = height;
            //m_gait.transform.localPosition = gatePos;



        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
