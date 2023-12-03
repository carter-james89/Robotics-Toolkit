using Utilities.Events;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public class AdvancedLimbPositioner : MonoBehaviour, ILimbPositioner
    {
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

        public void RotateToPosition(Vector3 position, float speed, float height)
        {
            if (_ikTarget == null)
            {
                Debug.LogError("IK Target is not assigned!");
                return;
            }

            startPosition = _ikTarget.transform.position;
            endPosition = position;
            arcHeight = height;
            this.speed = speed;

            // Calculate the effective distance considering both horizontal and vertical components
            float horizontalDistance = Vector3.Distance(new Vector3(startPosition.x, 0, startPosition.z), new Vector3(endPosition.x, 0, endPosition.z));
            float verticalDistance = Mathf.Abs(endPosition.y - startPosition.y) + arcHeight; // Adding arcHeight to account for the vertical movement
            float effectiveDistance = horizontalDistance + verticalDistance;

            trajectoryDuration = effectiveDistance / speed;
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
            if (elapsedTime < trajectoryDuration)
            {
                float linearT = elapsedTime / trajectoryDuration;
                float heightT;

                if (startPosition == endPosition)
                {
                    // Vertical lift and descent
                    if (linearT <= 0.5f)
                    {
                        // First half of the trajectory (going up)
                        heightT = Mathf.Lerp(0, arcHeight, linearT * 2);
                    }
                    else
                    {
                        // Second half of the trajectory (going down)
                        heightT = Mathf.Lerp(arcHeight, 0, (linearT - 0.5f) * 2);
                    }
                }
                else
                {
                    // Normal arc
                    heightT = Mathf.Sin(Mathf.PI * linearT) * arcHeight;
                }

                Vector3 basePosition = Vector3.Lerp(startPosition, endPosition, linearT);
                Vector3 arcPosition = basePosition + Vector3.up * heightT;

                _ikTarget.transform.position = arcPosition;
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

            startPosition = _ikTarget.transform.position;
            endPosition = position;
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

            elapsedTime += Time.deltaTime;
            if (elapsedTime < trajectoryDuration)
            {
                float linearT = elapsedTime / trajectoryDuration;
                Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, linearT);
                _ikTarget.transform.position = newPosition;
            }
            else
            {
                AtTarget();
            }
        }


        private void AtTarget()
        {
            _ikTarget.transform.position = endPosition;
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

        public bool StrideComplete()
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
