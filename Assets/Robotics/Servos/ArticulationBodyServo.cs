using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Servos
{
    public interface IServo
    {
        bool IsEnabled();
        GameObject GetGameObject();
        float GetCurrentAngle();
        void SetServoSpeed(float speed);
        void SetServoPosition(float position);
        void SetServoPosition(float position, float speed);
        void SetServoPositionImmediate(float position);
        void ResetServo(float resetAngle);
    }

    public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };

    public class ArticulationBodyServo : MonoBehaviour, IServo
    {
        [SerializeField]
        private float m_servoSpeed = 1.0f;

        [SerializeField]
        private float m_offset = 0.0f;

        private ArticulationBody m_articulation;
        private Transform m_anchorTransform;

        private void Awake()
        {
            m_articulation = GetComponent<ArticulationBody>();
            if (m_articulation == null)
            {
                Debug.LogError("ArticulationBody component not found on the GameObject.");
                return;
            }

            var xDrive = m_articulation.xDrive;
            m_articulation.xDrive = xDrive;

            m_articulation.parentAnchorPosition = transform.localPosition;

            m_anchorTransform = new GameObject("Anchor").transform;
            m_anchorTransform.SetParent(transform);
        }

        public bool IsEnabled() => enabled;

        public GameObject GetGameObject() => gameObject;

        public float GetCurrentAngle()
        {
            if (m_articulation == null) return 0.0f;
            return -((m_articulation.jointPosition[0] * Mathf.Rad2Deg) + m_offset);
        }

        public void SetServoSpeed(float speed)
        {
            float rotationChange = speed * Time.fixedDeltaTime;
            float rotationGoal = GetCurrentAngle() + rotationChange;
            RotateTo(rotationGoal);
        }


        public void SetServoPosition(float position)
        {
            RotateTo(position - m_offset);
        }


        private void RotateTo(float angle)
        {
            if (m_articulation == null) return;

            var drive = m_articulation.xDrive;
            drive.target = angle;
            m_articulation.xDrive = drive;
        }

        public void ResetServo(float resetAngle)
        {
            SetServoPosition(resetAngle);
        }

        private void Update()
        {
            if (m_articulation == null) return;

            var globalPosition = transform.parent.TransformPoint(m_articulation.parentAnchorPosition);
            var globalRotation = transform.parent.rotation * m_articulation.parentAnchorRotation;

            m_anchorTransform.position = globalPosition;
            m_anchorTransform.rotation = globalRotation;
        }

        public void SetServoPosition(float position, float speed)
        {
            throw new System.NotImplementedException();
        }

        public void SetServoPositionImmediate(float position)
        {
            throw new System.NotImplementedException();
        }
    }
}
