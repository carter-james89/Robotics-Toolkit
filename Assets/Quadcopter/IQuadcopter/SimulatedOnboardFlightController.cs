using System;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    /// <summary>
    /// A simulated flight controller that emulates a quadcopter with an onboard flight controller that accepts throttle, yaw, pitch, and roll inputs.
    /// Simulates physical forces and sensor data for a quadcopter.
    /// </summary>
    public class SimulatedOnboardFlightController : MonoBehaviour, IFlightController
    {
        private IQuadcopter _quadToControl;
        private Rigidbody rigidBody;
        private Action<FlightStatus> _onFlightStatusChanged;
        private bool _isInitialized;

        [SerializeField] private float inputDrag;
        [SerializeField] private float drag;
        public float timeSpeed = 1;

        private IInputSource.FlightControlValues _craftInputs;
        private FlightStatus _flightStatus;

        /// <inheritdoc/>
        public bool IsInitialized() => _isInitialized;

        /// <inheritdoc/>
        public bool IsReadyToFly() => true;

        /// <inheritdoc/>
        public void Initialize(IQuadcopter quadToControl, Action<FlightStatus> onFlightStatusChanged)
        {
            _quadToControl = quadToControl;

            var physicsSimulator = new GameObject("Simulation Physics Simulation");
            physicsSimulator.transform.SetParent(_quadToControl.GetLocalTrackingSpace());
            physicsSimulator.transform.position = quadToControl.GetGameObject().transform.position;
            rigidBody = physicsSimulator.AddComponent<Rigidbody>();
            rigidBody.mass = quadToControl.GetGameObject().GetComponent<Rigidbody>().mass;
            rigidBody.useGravity = false;

            var boxCollider = rigidBody.gameObject.AddComponent<BoxCollider>();
            boxCollider.size = quadToControl.GetGameObject().GetComponent<BoxCollider>().size;
            boxCollider.center = quadToControl.GetGameObject().GetComponent<BoxCollider>().center;

            _onFlightStatusChanged = onFlightStatusChanged;

            Time.timeScale = timeSpeed;
            _isInitialized = true;
        }

        /// <inheritdoc/>
        public Quaternion GetGyroRotation()
        {
            return rigidBody.transform.rotation;
        }

        /// <inheritdoc/>
        public QuadcopterData GetSensorData()
        {
            return new QuadcopterData
            {
                posX = rigidBody.transform.localPosition.x,
                posY = rigidBody.transform.localPosition.y,
                posZ = rigidBody.transform.localPosition.z,
                gyroYaw = rigidBody.transform.localEulerAngles.y,
                gyroPitch = rigidBody.transform.localEulerAngles.x,
                gyroRoll = rigidBody.transform.localEulerAngles.z,
                height = rigidBody.transform.localPosition.y,
                VelocityVector = rigidBody.angularVelocity
            };
        }

        /// <inheritdoc/>
        public void Run(FlightStatus flightStatus, IInputSource.FlightControlValues craftInputs)
        {
            _craftInputs = craftInputs;
            _flightStatus = flightStatus;
            RunFixedUpdate();
        }

        /// <inheritdoc/>
        public bool IsSimulator() => true;

        /// <inheritdoc/>
        public void Takeoff()
        {
            rigidBody.Move(rigidBody.transform.position + new Vector3(0, 0.8f, 0), transform.rotation);
            rigidBody.transform.position = _quadToControl.GetGameObject().transform.position;
            rigidBody.useGravity = true;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

            _onFlightStatusChanged?.Invoke(FlightStatus.Launching);
            _onFlightStatusChanged?.Invoke(FlightStatus.Flying);
        }

        /// <inheritdoc/>
        public void Land()
        {
            rigidBody.transform.position = _quadToControl.GetGameObject().transform.position;
            _onFlightStatusChanged?.Invoke(FlightStatus.Landing);
            _onFlightStatusChanged?.Invoke(FlightStatus.PreLaunch);
        }

        /// <summary>
        /// Physics simulation that applies forces and torque to emulate flight behavior.
        /// </summary>
        private void RunFixedUpdate()
        {
            if (_flightStatus == FlightStatus.PreLaunch) return;

            rigidBody.AddForce(rigidBody.transform.up * 9.81f);

            bool receivingInput = false;

            rigidBody.AddForce(rigidBody.transform.forward * _craftInputs.pitch);
            receivingInput |= Mathf.Abs(_craftInputs.pitch) > 0;

            rigidBody.AddForce(rigidBody.transform.up * _craftInputs.throttle);
            receivingInput |= Mathf.Abs(_craftInputs.throttle) > 0;

            rigidBody.AddForce(rigidBody.transform.right * _craftInputs.roll);
            receivingInput |= Mathf.Abs(_craftInputs.roll) > 0;

            rigidBody.AddTorque(rigidBody.transform.up * _craftInputs.yaw);
            receivingInput |= Mathf.Abs(_craftInputs.yaw) > 0;

            if (receivingInput && rigidBody.drag != inputDrag)
            {
                rigidBody.drag = inputDrag;
                rigidBody.angularDrag = inputDrag;
            }
            else if (!receivingInput && rigidBody.drag != drag)
            {
                rigidBody.drag = drag;
                rigidBody.angularDrag = drag * 0.9f;
            }
        }

        /// <inheritdoc/>
        public GameObject GetGameObject()
        {
            if (this == null) return null;
            return gameObject;
        }

        /// <inheritdoc/>
        public Component GetComponent()
        {
            if (this == null) return null;
            return this;
        }
    }
}
