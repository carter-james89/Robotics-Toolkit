using System;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    /// <summary>
    /// A simulated flight controller that emulates a quadcopter with an onboard flight controller that accepts throttle, yaw, pitch, and roll inputs.
    /// Simulates physical forces and sensor data for a quadcopter.
    /// </summary>
    public class SimulatedOnboardFlightController : QuadcopterFlightController
    {
    
        private Rigidbody simulatedRB;
        private Action<FlightStatus> _onFlightStatusChanged;
      
        [SerializeField] private float inputDrag;
        [SerializeField] private float drag;
        public float timeSpeed = 1;

        private IInputSource.FlightControlValues _craftInputs;
        private FlightStatus _flightStatus;



        /// <inheritdoc/>
        public override bool IsReadyToFly() => true;

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
    

            var physicsSimulator = new GameObject("Simulation Physics Simulation");
            physicsSimulator.transform.SetParent(quadToControl.GetLocalTrackingSpace());
            physicsSimulator.transform.position = quadToControl.GetGameObject().transform.position;
            simulatedRB = physicsSimulator.AddComponent<Rigidbody>();
            var quadRB = quadToControl.GetGameObject().GetComponent<Rigidbody>();
            simulatedRB.mass = quadRB.mass;
            simulatedRB.linearDamping = quadRB.linearDamping;
            simulatedRB.angularDamping = quadRB.angularDamping;
            simulatedRB.useGravity = quadRB.useGravity;
            simulatedRB.interpolation = quadRB.interpolation;
            simulatedRB.collisionDetectionMode = quadRB.collisionDetectionMode;
            //  rigidBody.useGravity = false;

            var boxCollider = simulatedRB.gameObject.AddComponent<BoxCollider>();
            boxCollider.size = quadToControl.GetGameObject().GetComponent<BoxCollider>().size;
            boxCollider.center = quadToControl.GetGameObject().GetComponent<BoxCollider>().center;

     

            Time.timeScale = timeSpeed;
            _isInitialized = true;
        }

        /// <inheritdoc/>
        public override Quaternion GetGyroRotation()
        {
            return simulatedRB.transform.rotation;
        }

        /// <inheritdoc/>
        public override QuadcopterData GetSensorData()
        {
            return new QuadcopterData
            {
                posX = simulatedRB.transform.localPosition.x,
                posY = simulatedRB.transform.localPosition.y,
                posZ = simulatedRB.transform.localPosition.z,
                gyroYaw = simulatedRB.transform.localEulerAngles.y,
                gyroPitch = simulatedRB.transform.localEulerAngles.x,
                gyroRoll = simulatedRB.transform.localEulerAngles.z,
                height = simulatedRB.transform.localPosition.y,
                VelocityVector = simulatedRB.angularVelocity
            };
        }

        /// <inheritdoc/>
        public override void Run(FlightStatus flightStatus, IInputSource.FlightControlValues craftInputs)
        {
            _craftInputs = craftInputs;
            _flightStatus = flightStatus;
            RunFixedUpdate();
        }

        /// <inheritdoc/>
        public override  bool IsSimulator() => true;

        /// <inheritdoc/>
        public override bool AttemptTakeoff()
        {
            simulatedRB.Move(simulatedRB.transform.position + new Vector3(0, 0.8f, 0), transform.rotation);
            simulatedRB.transform.position = quadToControl.GetGameObject().transform.position;
            simulatedRB.useGravity = true;
            simulatedRB.linearVelocity = Vector3.zero;
            simulatedRB.angularVelocity = Vector3.zero;

            _onFlightStatusChanged?.Invoke(FlightStatus.Launching);
            _onFlightStatusChanged?.Invoke(FlightStatus.Flying);

            NotifyListeners(FlightControllerEventType.OnTakeOffEnd);    
            return true;
        }

        /// <inheritdoc/>
        public override bool AttemptLand()
        {
            simulatedRB.transform.position = quadToControl.GetGameObject().transform.position;
            _onFlightStatusChanged?.Invoke(FlightStatus.Landing);
            _onFlightStatusChanged?.Invoke(FlightStatus.PreLaunch);
            return true;
        }

        /// <summary>
        /// Physics simulation that applies forces and torque to emulate flight behavior.
        /// </summary>
        private void RunFixedUpdate()
        {
            if (_flightStatus == FlightStatus.PreLaunch) return;

            var rigidBody = simulatedRB;
            rigidBody.AddForce(Vector3.up * Physics.gravity.magnitude * rigidBody.mass);


            bool receivingInput = false;

            simulatedRB.AddForce(simulatedRB.transform.forward * _craftInputs.pitch);
            receivingInput |= Mathf.Abs(_craftInputs.pitch) > 0;

            simulatedRB.AddForce(simulatedRB.transform.up * _craftInputs.throttle);
            receivingInput |= Mathf.Abs(_craftInputs.throttle) > 0;
            float hoverThrottle = 0.5f;
            float effectiveThrottle = (_craftInputs.throttle - hoverThrottle) * .5f;
           // rigidBody.AddForce(rigidBody.transform.up * effectiveThrottle);


            simulatedRB.AddForce(simulatedRB.transform.right * _craftInputs.roll);
            receivingInput |= Mathf.Abs(_craftInputs.roll) > 0;

            simulatedRB.AddTorque(simulatedRB.transform.up * _craftInputs.yaw);
            receivingInput |= Mathf.Abs(_craftInputs.yaw) > 0;

            //if (receivingInput && simulatedRB.drag != inputDrag)
            //{
            //    simulatedRB.drag = inputDrag;
            //    simulatedRB.angularDrag = inputDrag;
            //}
            //else if (!receivingInput && simulatedRB.drag != drag)
            //{
            //    simulatedRB.drag = drag;
            //    simulatedRB.angularDrag = drag * 0.9f;
            //}
        }

    }
}
