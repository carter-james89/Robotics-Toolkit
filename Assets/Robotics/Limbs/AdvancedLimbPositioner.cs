using Utilities.Events;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public class AdvancedLimbPositioner : MonoBehaviour, ILimbPositioner
    {
        [SerializeField]
        private Transform _targetEndPoint;
        public Transform _ikTarget;
        public float speed = 1.0f;
        public float arcHeight = 1.0f;

        private bool isMoving = false;
        private Vector3 startPosition;
        private Vector3 endPosition;
        private float trajectoryDuration;
        private float elapsedTime;

        public Vector3 GetTargetGlobalPosition()
        {
            return _ikTarget.position;
        }

        public enum Status
        {
            None,
            Translating,
            Rotating,
        }
        public Status CurrentStatus { get; private set; } = Status.None;

        private InterfaceEventManager<ILimbPositionerEventListener> _eventManager = new InterfaceEventManager<ILimbPositionerEventListener>("Advanced Limb Positioner");
        private bool _useEasing = false;

        public void SubscribeToEvents(ILimbPositionerEventListener listener)
        {
            _eventManager.AddListener(listener);
        }

        public void UnsubscribeFromEvents(ILimbPositionerEventListener listener)
        {
            _eventManager.RemoveListener(listener);
        }

        void Update()
        {
      
        }

        private float CalculateArcDistance(float horizontalDistance, float height)
        {
            Debug.Log("Calculate arc with horizontal distance : " + horizontalDistance + " - height " + height);

            // If horizontal distance is zero, return double the height (up and down)
            if (horizontalDistance < .01f)
            {
                Debug.Log("CalculatedArcDistance " + 2 * height);
                return 2 * height;
            }

            // Check if height is negligible
            if (Mathf.Approximately(height, 0f))
            {
                Debug.Log("CalculatedArcDistance " + horizontalDistance);
                return horizontalDistance;
            }

            // Calculate the arc distance using an approximation
            // This is a simplified formula and works well for small heights relative to the horizontal distance
            float h = height; // Maximum height of the arc
            float l = horizontalDistance; // Base length of the arc

            // The formula for arc length in a parabolic trajectory
            float arcDistance = l * (1 + (2 * h / l) * (2 * h / l));

            Debug.Log("CalculatedArcDistance " + arcDistance);
            return arcDistance;
        }


        public void RotateToPositionViaTime(Vector3 position, float height, float seconds)
        {
            if (_ikTarget == null)
            {
                Debug.LogError("IK Target is not assigned!");
                return;
            }
            Debug.Log("rotate to position in seconds : " + seconds);
            // Calculate the horizontal distance
            Vector3 startPositionLocal = _ikTarget.transform.localPosition;
            Vector3 endPositionLocal = transform.InverseTransformPoint(position);
            float horizontalDistance = Vector3.Distance(new Vector3(startPositionLocal.x, 0, startPositionLocal.z), new Vector3(endPositionLocal.x, 0, endPositionLocal.z));
            float effectiveDistance = CalculateArcDistance(horizontalDistance,height);
            // Calculate the speed (distance per second)
            float calculatedSpeed = effectiveDistance / seconds;
            Debug.Log("Calculated speed to acheive time " + calculatedSpeed);
            // Call the existing RotateToPosition with the calculated speed
            RotateToPosition(position, calculatedSpeed, height);
        }

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

            // Calculate only the horizontal distance
            float horizontalDistance = Vector3.Distance(new Vector3(startPosition.x, 0, startPosition.z), new Vector3(endPosition.x, 0, endPosition.z));

            // Include a portion of the arc height in the distance calculation
            // This is a simplification. For more accuracy, especially for large arc heights, a more complex calculation would be needed.
            float effectiveDistance = CalculateArcDistance(horizontalDistance, arcHeight);
            Debug.Log("effectiveDistance " + effectiveDistance);
            Debug.Log("speed " + this.speed);
            // Calculate trajectory duration based on the effective distance
            trajectoryDuration = effectiveDistance / speed;
            Debug.Log("set trajecotry duration " + trajectoryDuration);
            elapsedTime = 0;
            isMoving = true;
            CurrentStatus = Status.Rotating;
        }



        private void MoveAlongArc()
        {
            if (_ikTarget == null)
            {
                return;
            }

            elapsedTime += Time.deltaTime;
           // Debug.Log("ElipsedTime " + elapsedTime);
           // Debug.Log("trajecotryDuration " + trajectoryDuration);
            if (elapsedTime < trajectoryDuration)
            {
                float linearT = elapsedTime / trajectoryDuration;
                float t = _useEasing ? 1 - (1 - linearT) * (1 - linearT) : linearT; // Apply easing if useEasing is true

                float heightT;
                if (startPosition == endPosition)
                {
                    // Vertical lift and descent
                    if (linearT <= 0.5f)
                    {
                        heightT = Mathf.Lerp(0, arcHeight, linearT * 2);
                    }
                    else
                    {
                        heightT = Mathf.Lerp(arcHeight, 0, (linearT - 0.5f) * 2);
                    }
                }
                else
                {
                    // Normal arc
                    heightT = Mathf.Sin(Mathf.PI * linearT) * arcHeight;
                }

                Vector3 basePosition = Vector3.Lerp(startPosition, endPosition, t);
                Vector3 arcPosition = basePosition + Vector3.up * heightT;

                if (elapsedTime > 0)
                {
                    Vector3 previousPosition = Vector3.Lerp(startPosition, endPosition, t - Time.deltaTime / trajectoryDuration) + Vector3.up * heightT;
                    Debug.DrawLine(previousPosition, arcPosition, Color.red);
                }

                _ikTarget.transform.localPosition = arcPosition;
            }
            else
            {
                AtTarget();
            }
        }


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

            // Calculate the duration based on the distance to the new position and the speed
            float distance = Vector3.Distance(startPosition, endPosition);
            trajectoryDuration = distance / speed;

            elapsedTime = 0;
            isMoving = true;
            CurrentStatus = Status.Translating;
        }

 
        private void MoveToPosition()
        {
            if (_ikTarget == null)
            {
                return;
            }
            Debug.DrawLine(startPosition, endPosition, Color.green);

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

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void RotateToPosition(Vector3 globalPosition, Quaternion rotationAxis, float time, bool localSpace)
        {
            throw new System.NotImplementedException();
        }

        public void RotateToPosition(Vector3 direction, Vector3 upDirection, float distance, float time)
        {
            throw new System.NotImplementedException();
        }

        public void TranslateToPosition(Vector3 globalPosition, float time, bool localSpace)
        {
            throw new System.NotImplementedException();
        }

        public void TranslateToPosition(Vector3 direction, Vector3 upDir, float distance, float time)
        {
            throw new System.NotImplementedException();
        }

        public bool LimbAtTarget()
        {
            return !isMoving;
        }

        public void SetLimbPosition(Vector3 globalPosition, bool localSpace)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetLimbPosition(bool localSpace)
        {
            throw new System.NotImplementedException();
        }

        public bool Run()
        {
            switch (CurrentStatus)
            {
                case Status.None:
                    break;
                case Status.Translating:
                    MoveToPosition();
                    break;
                case Status.Rotating:
                    MoveAlongArc();
                    break;
                default:
                    break;
            }
           return CurrentStatus == Status.None;
        }
    }
}
