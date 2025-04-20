using UnityEngine;
using Toolkit.Utilities.Events;

namespace RoboticsToolkit.Robotics.Gaits
{
    /// <summary>
    /// Handles movement of an IK target for a robotic limb using arc-based rotation or linear translation.
    /// Reports status and progress through event dispatching.
    /// </summary>
    public class AdvancedLimbPositioner : MonoBehaviour, ILimbPositioner
    {
        [SerializeField] private Transform _targetEndPoint;
        public Transform _ikTarget;

        [Tooltip("Movement speed in units per second.")]
        public float speed = 1.0f;

        [Tooltip("Arc height in units used for rotating/stepping.")]
        public float arcHeight = 1.0f;

        /// <summary>
        /// Represents the current movement mode.
        /// </summary>
        public enum Status { None, Translating, Rotating }

        public Status CurrentStatus { get; private set; } = Status.None;

        private bool isMoving = false;
        private bool _useEasing = false;

        private Vector3 startPosition;
        private Vector3 endPosition;
        private float trajectoryDuration;
        private float elapsedTime;

        private readonly InterfaceEventManager<LimbPositionerEventData> _eventManager =
            new InterfaceEventManager<LimbPositionerEventData>("Advanced Limb Positioner");

        #region Movement Setup

        /// <summary>
        /// Starts an arc-based movement to the target using a specified duration.
        /// </summary>
        /// <param name="position">Global target position.</param>
        /// <param name="height">Arc height.</param>
        /// <param name="seconds">Time to complete the move.</param>
        public void RotateToPositionViaTime(Vector3 position, float height, float seconds)
        {
            if (_ikTarget == null)
            {
                Debug.LogError("IK Target is not assigned!");
                return;
            }

            Vector3 startLocal = _ikTarget.transform.localPosition;
            Vector3 endLocal = transform.InverseTransformPoint(position);

            float horizontalDistance = Vector3.Distance(
                new Vector3(startLocal.x, 0, startLocal.z),
                new Vector3(endLocal.x, 0, endLocal.z));

            float effectiveDistance = CalculateArcDistance(horizontalDistance, height);
            float calculatedSpeed = effectiveDistance / seconds;

            RotateToPosition(position, calculatedSpeed, height);
        }

        /// <summary>
        /// Starts an arc-based rotation to the specified position.
        /// </summary>
        /// <param name="position">Global target position.</param>
        /// <param name="speed">Speed of movement.</param>
        /// <param name="height">Arc height.</param>
        public void RotateToPosition(Vector3 position, float speed, float height)
        {
            if (_ikTarget == null)
            {
                Debug.LogError("IK Target is not assigned!");
                return;
            }

            _targetEndPoint.position = position;
            startPosition = _ikTarget.transform.localPosition;
            endPosition = transform.InverseTransformPoint(position);
            arcHeight = height;
            this.speed = speed;

            float horizontalDistance = Vector3.Distance(
                new Vector3(startPosition.x, 0, startPosition.z),
                new Vector3(endPosition.x, 0, endPosition.z));

            float effectiveDistance = CalculateArcDistance(horizontalDistance, arcHeight);
            trajectoryDuration = effectiveDistance / speed;
            elapsedTime = 0f;

            isMoving = true;
            CurrentStatus = Status.Rotating;
        }

        /// <summary>
        /// Starts a direct translation to a position.
        /// </summary>
        /// <param name="position">Target global position.</param>
        /// <param name="speed">Speed in units per second.</param>
        public void TranslateToPosition(Vector3 position, float speed)
        {
            if (_ikTarget == null)
            {
                Debug.LogError("IK Target is not assigned!");
                return;
            }

            _targetEndPoint.position = position;
            startPosition = _ikTarget.transform.localPosition;
            endPosition = transform.InverseTransformPoint(position);
            this.speed = speed;

            float distance = Vector3.Distance(startPosition, endPosition);
            trajectoryDuration = distance / speed;

            elapsedTime = 0f;
            isMoving = true;
            CurrentStatus = Status.Translating;
        }

        #endregion

        #region Movement Execution

        private void MoveAlongArc()
        {
            if (_ikTarget == null) return;

            elapsedTime += Time.deltaTime;

            if (elapsedTime < trajectoryDuration)
            {
                float linearT = elapsedTime / trajectoryDuration;
                float t = _useEasing ? 1 - (1 - linearT) * (1 - linearT) : linearT;

                float heightT = (startPosition == endPosition)
                    ? (linearT <= 0.5f
                        ? Mathf.Lerp(0, arcHeight, linearT * 2)
                        : Mathf.Lerp(arcHeight, 0, (linearT - 0.5f) * 2))
                    : Mathf.Sin(Mathf.PI * linearT) * arcHeight;

                Vector3 basePosition = Vector3.Lerp(startPosition, endPosition, t);
                Vector3 arcPosition = basePosition + Vector3.up * heightT;

                _ikTarget.transform.localPosition = arcPosition;
            }
            else
            {
                AtTarget();
            }
        }

        private void MoveToPosition()
        {
            if (_ikTarget == null) return;

            elapsedTime += Time.deltaTime;

            if (elapsedTime < trajectoryDuration)
            {
                float linearT = elapsedTime / trajectoryDuration;
                Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, linearT);
                _ikTarget.transform.localPosition = newPosition;
            }
            else
            {
                AtTarget();
            }
        }

        private void AtTarget()
        {
            _ikTarget.transform.localPosition = endPosition;
            isMoving = false;
            CurrentStatus = Status.None;
        }

        #endregion

        #region ILimbPositioner Interface

        /// <inheritdoc/>
        public bool Run()
        {
            switch (CurrentStatus)
            {
                case Status.Translating:
                    MoveToPosition();
                    break;
                case Status.Rotating:
                    MoveAlongArc();
                    break;
            }

            return CurrentStatus == Status.None;
        }

        /// <inheritdoc/>
        public bool LimbAtTarget() => !isMoving;

        /// <inheritdoc/>
        public GameObject GetGameObject() => gameObject;

        /// <inheritdoc/>
        public Component GetComponent() => this;

        /// <inheritdoc/>
        public Vector3 GetTargetGlobalPosition() => _ikTarget.position;

        /// <inheritdoc/>
        public void SetLimbPosition(Vector3 globalPosition, bool localSpace)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Vector3 GetLimbPosition(bool localSpace)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void RotateToPosition(Vector3 globalPosition, Quaternion rotationAxis, float time, bool localSpace)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void RotateToPosition(Vector3 direction, Vector3 upDirection, float distance, float time)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void TranslateToPosition(Vector3 globalPosition, float time, bool localSpace)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void TranslateToPosition(Vector3 direction, Vector3 upDir, float distance, float time)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Events

        /// <inheritdoc/>
        public void SubscribeToEvents(IEventListener<LimbPositionerEventData> listenerToSubscribe)
        {
            _eventManager.AddListener(listenerToSubscribe);
        }

        /// <inheritdoc/>
        public void UnsubscribeFromEvents(IEventListener<LimbPositionerEventData> listenerToUnsubscribe)
        {
            _eventManager.RemoveListener(listenerToUnsubscribe);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Calculates an effective arc path distance based on height and horizontal span.
        /// </summary>
        private float CalculateArcDistance(float horizontalDistance, float height)
        {
            if (horizontalDistance < 0.01f) return 2 * height;
            if (Mathf.Approximately(height, 0f)) return horizontalDistance;

            float h = height;
            float l = horizontalDistance;
            return l * (1 + (2 * h / l) * (2 * h / l));
        }

        #endregion
    }
}
