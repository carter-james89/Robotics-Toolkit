using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticToolKit.Robotics.Limbs
{
    public interface IGaitEventListener
    {
        public void OnLimbAchievedTarget(Vector3 currentTarget);
    }
    public interface IGait
    {
        public enum MovementStyle { None, Rotate, Translate }
        public Transform GetTarget();
        public MovementStyle GetMovementStyle();
    }
    public class LimbGait : MonoBehaviour, IGait
    {
        [SerializeField]
        private Transform m_target;
        [SerializeField]
        private Transform m_targetOffset;

        [SerializeField]
        private Transform m_desiredTargetLocation;

        private Transform m_pivotTransform;

        private Vector3 m_targetPivotOffset;

        private List<IGaitEventListener> m_listeners = new List<IGaitEventListener>();

        public bool AtTarget { get; private set; } = true;
        private Vector3 m_currentDesiredPosition;
        private float m_currentDesiredSpeed;
        private IGait.MovementStyle m_currentMovementStyle = IGait.MovementStyle.None;

        public IGait.MovementStyle GetMovementStyle() => m_currentMovementStyle;

        private void Awake()
        {
            m_currentDesiredPosition = Vector3.zero;
            m_pivotTransform = new GameObject("Pivot").transform;
            m_pivotTransform.SetParent(transform);
        }
        public Transform GetTarget()
        {
            return m_target;
        }
        public Transform GetTargetOffset() => m_targetOffset;

        public void SubscribeToGaitEvents(IGaitEventListener listener)
        {
            m_listeners.Add(listener);
        }
        private float m_strideHeight = .103f;
        public void RotateToPosition(Vector3 position, float speed, float height)
        {
            m_currentMovementStyle = IGait.MovementStyle.Rotate;
            m_pivotTransform.localPosition = Vector3.Lerp(m_target.localPosition, position, .5f);
            m_pivotTransform.LookAt(m_target, Vector3.up);
            m_targetPivotOffset = m_pivotTransform.InverseTransformPoint(m_target.position);
            m_strideHeight = height;
            SetStrideValues(position, speed);
        }
        public void TranslateToPosition(Vector3 position, float speed)
        {
            Debug.Log(name + " set new target position : " + position);
           
            m_currentMovementStyle = IGait.MovementStyle.Translate;

            SetStrideValues(position, speed);
          
        }
        private void SetStrideValues(Vector3 position, float speed)
        {
            m_currentDesiredPosition = position;
            m_currentDesiredSpeed = speed;
            AtTarget = false;
            m_desiredTargetLocation.localPosition = position;
        }
        [SerializeField]
        private float m_rotationPercentage;
        private void Update()
        {

            if (!AtTarget)
            {
                CheckAtTarget();
                if (AtTarget)
                {
                    return;
                }
                switch (m_currentMovementStyle)
                {
                    case IGait.MovementStyle.Rotate:
                        m_pivotTransform.Rotate(new Vector3(-m_currentDesiredSpeed * Time.deltaTime, 0, 0));
                        var dir1 = m_pivotTransform.parent.forward;
                        var dir2 = m_pivotTransform.forward;
                        m_rotationPercentage = (Vector3.Dot(dir1, dir2) + 1) / 2;

                        if (m_rotationPercentage >= .5)
                        {
                            m_rotationPercentage = 1 - m_rotationPercentage;
                        }
                        float dist = Mathf.Lerp(0, m_strideHeight, m_rotationPercentage);
                        var newOffset = m_targetPivotOffset;
                        newOffset.z += dist;
                        m_target.position = m_pivotTransform.TransformPoint(newOffset);
                        break;
                    case IGait.MovementStyle.Translate:
                        Debug.Log("run translation");
                        var dir = m_target.localPosition - m_currentDesiredPosition.normalized;
                        dir = m_currentDesiredPosition - m_target.localPosition;
                        // m_target.transform.Translate(dir * m_currentDesiredSpeed * Time.deltaTime);
                        m_target.transform.localPosition = (m_target.localPosition + (dir * m_currentDesiredSpeed * Time.deltaTime));
                        break;
                    default:
                        break;
                }
            }
        }

        private void CheckAtTarget()
        {
            if (Vector3.Distance(m_target.position, transform.TransformPoint(m_currentDesiredPosition)) < .01f)
            {
                Debug.Log("AtTarget");
                AtTarget = true;
                m_target.localPosition = m_currentDesiredPosition;
                foreach (var listener in m_listeners)
                {
                    listener.OnLimbAchievedTarget(m_currentDesiredPosition);
                }
                return;
            }
            AtTarget = false;
        }
    }


}
