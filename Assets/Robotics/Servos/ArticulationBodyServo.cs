using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RoboticToolKit.Robotics.Servos
{
    public interface IServo
    {
        public bool IsEnabled();
        public GameObject GetGameObject();
        public float GetCurrentAngle();
        public void SetServoSpeed(float speed);
        public void SetServoPosition(float position);
        public void SetServoPosition(float position,float speed);
        public void SetServoPositionImmediate(float position);
        public void ResetServo(float resetAngle);
    }

    public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };
    public class ArticulationBodyServo : MonoBehaviour, IServo
    {
        [SerializeField]
        private float m_servoSpeed = 1;
        public bool IsEnabled() => enabled;
        public GameObject GetGameObject() => gameObject;
        public RotationDirection rotationState = RotationDirection.None;
        //public float speed = 300.0f;

        private Transform m_anchorTransform;
        private ArticulationBody m_articulation;

        //private float m_startAngle;

        private bool m_firstSet = true;

    //    public bool PrintLog = false;

        private void Awake()
        {
            m_articulation = GetComponent<ArticulationBody>();
            var xDrive = m_articulation.xDrive;
          //  xDrive.forceLimit *= 2;
            m_articulation.xDrive = xDrive;

            SetServoPositionImmediate(0);

            m_anchorTransform = new GameObject("Anchor").transform;
            m_anchorTransform.SetParent(transform);

        }
        private void Start()
        {
       
        }
        public float GetCurrentAngle()
        {
            if (m_articulation == null)
            {
                m_articulation = GetComponent<ArticulationBody>();
            }
            return m_articulation.jointPosition[0] * Mathf.Rad2Deg;
        }
        public void SetServoSpeed(float speed)
        {
            float rotationChange = speed * Time.fixedDeltaTime;
            float rotationGoal = GetCurrentAngle() + rotationChange;
            RotateTo(rotationGoal);
        }
        public void SetServoPosition(float position, float speed)
        {
            var dif = position- GetCurrentAngle();
            float rotationGoal = GetCurrentAngle() + (dif * Time.deltaTime * speed);
            RotateTo(rotationGoal);
        }
        public void SetServoPosition(float position)
        {
            SetServoPosition(position, m_servoSpeed);
            RotateTo(position);
        }
        public void SetServoPositionImmediate(float position)
        {
            //var rotation = Quaternion.Euler(position, 0, 0);
            //m_articulation.TeleportRoot(m_articulation.transform.position, transform.rotation * rotation);
            //m_articulation.velocity = Vector3.zero;
            //m_articulation.angularVelocity = Vector3.zero;
             RotateTo(position);
        }

        private void RotateTo(float angle)
        {
            var drive = m_articulation.xDrive;
            drive.target = angle;
            m_articulation.xDrive = drive;

           // m_articulation.
        }

        public void ResetServo(float resetAngle)
        {
            //  SetAngleImmediate(resetAngle);
            //  m_articulation.jointR
            // m_articulation.jointPosition = new ArticulationReducedSpace(resetAngle, 0f, 0f);
            //m_articulation.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
            //m_articulation.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
            //m_articulation.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);

            //m_articulation.velocity = Vector3.zero;
            //m_articulation.angularVelocity = Vector3.zero;
            //m_articulation.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
            //m_articulation.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
            //m_articulation.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
            //m_articulation.ResetInertiaTensor();
            //m_articulation.ResetCenterOfMass();
            RotateTo(resetAngle);   
        }
        



        private void Update()
        {
            //  Debug.Log(GetCurrentAngle());
            var globalPosition = transform.parent.TransformPoint(m_articulation.parentAnchorPosition);
            var globalRotation = transform.parent.rotation * m_articulation.parentAnchorRotation;

            m_anchorTransform.position = globalPosition;
            m_anchorTransform.rotation = globalRotation;

           // m_anchorTransform.position = m_articulation.parentAnchorPosition;
           // m_anchorTransform.rotation = m_articulation.parentAnchorRotation;
        }

        // MOVEMENT HELPERS
        //private float CurrentPrimaryAxisRotation()
        //{
        //    float currentRotationRads = m_articulation.jointPosition[0];
        //    float currentRotation = Mathf.Rad2Deg * currentRotationRads;
        //    return currentRotation;
        //}
    }
}