using System;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    /// <summary>
    /// A simulator Quadcopter meant to replicate the DJI RemoteQuadcopter
    /// Can be used to test <see cref="IAutoPilot"/> or any new features without destroying RemoteQuadcopter
    /// </summary>
    /// <remarks>
    /// Tried my best to tune the simulator to match real life RemoteQuadcopter, but dont expect PID tunings for simulator to work for RemoteQuadcopter
    /// </remarks>
    public class SimulationQuadcopter : Quadcopter
    {
        /// <summary>
        /// Rigidbody to control the physics of the simulator
        /// </summary>
        private Rigidbody rigidBody;
        /// <summary>
        /// "Aerodynamic" drag when the user is inputing control values
        /// </summary>
        [SerializeField]
        private float inputDrag;
        /// <summary>
        /// "Aerodynamic" drag when the user is not inputing control values
        /// </summary>
        [SerializeField]
        private float drag;

        public float timeSpeed = 1;



        //public override void Initialize(Func<IInputSource.FlightControlValues> defaultInputSource)
        //{
        //    base.Initialize(defaultInputSource);

        //    var physicsCalculator = new GameObject("Simulation Physics Simulation");
        //    simulatedRB = physicsCalculator.AddComponent<Rigidbody>();

        //    Time.timeScale = timeSpeed;
        //}

        //public float deltaHeight;
        //private float _prevHeight = 0;
        //[SerializeField]
        //private float heightOffset = 0;

        //public float elvInput;

        //public void ResetOffset()
        //{
        //    heightOffset = 0;
        //}

        private void Update()
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(rigidBody.transform.position, rigidBody.transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                Debug.DrawRay(rigidBody.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            }
            //SetVirtualPosition(new Vector3(simulatedRB.transform.position.x, simulatedRB.transform.position.z), hit.distance,simulatedRB.transform.rotation, simulatedRB.velocity.y);
            //deltaHeight = _prevHeight - hit.distance;
            //_prevHeight = hit.distance;
            //elvInput = currentInputs.throttle;

            //var tempPos = simulatedRB.transform.position;

            //if (Math.Abs(deltaHeight) > .1)
            //{
            //    if (Math.Abs(currentInputs.throttle) < .05)
            //    {
            //        heightOffset += deltaHeight;
            //    }
            //    tempPos.y = hit.distance + heightOffset;
            //}

            //var newGroundSensorPoint = Instantiate(groundSensorPoint);
            //newGroundSensorPoint.transform.position = transform.position + (Vector3.down * hit.distance);

            //transform.position = tempPos;
            //transform.rotation = simulatedRB.transform.rotation;
            ProcessInputs();
        }

        /// <summary>
        /// All the physics for the simulator
        /// </summary>
        /// <remarks>
        /// Tried my best to tune the simulator to match real life RemoteQuadcopter, but dont expect PID tunings for simulator to work for RemoteQuadcopter
        /// </remarks>
        public void FixedUpdate()
        {
            if (_flightStatus != FlightStatus.PreLaunch)
            {
                rigidBody.AddForce(transform.up * 9.81f);
                bool receivingInput = false;
                var pitchInput = currentInputs.pitch;
                rigidBody.AddForce(rigidBody.transform.forward * pitchInput);
                if (System.Math.Abs(pitchInput) > 0)
                {
                    receivingInput = true;
                }
                var elvInput = currentInputs.throttle;
                rigidBody.AddForce(rigidBody.transform.up * elvInput);
                if (System.Math.Abs(elvInput) > 0)
                {
                    receivingInput = true;
                }
                var rollInput = currentInputs.roll;
                rigidBody.AddForce(rigidBody.transform.right * rollInput);
                if (System.Math.Abs(rollInput) > 0)
                {

                    receivingInput = true;
                }

                var yawInput = currentInputs.yaw;
                rigidBody.AddTorque(rigidBody.transform.up * yawInput);
                if (System.Math.Abs(yawInput) > 0)
                {

                    receivingInput = true;
                }

                if (receivingInput & rigidBody.linearDamping != inputDrag)
                {
                    rigidBody.linearDamping = inputDrag;
                    rigidBody.angularDamping = inputDrag;
                }
                else if (!receivingInput & rigidBody.linearDamping != drag)
                {
                    rigidBody.linearDamping = drag;
                    rigidBody.angularDamping = drag * .9f;
                }

                OnTransformUpdated();
            }
        }

        public override bool AttemptLand()
        {
            //TODO: write something to land the simulator, not really important
            return true;
        }

        /// <summary>
        /// Move the simulator into <see cref="FlightStatus.Flying"/> mode, and activate physics
        /// </summary>
        public override bool AttemptTakeoff()
        {
            Debug.Log("Simulator TakeOff");
            rigidBody.transform.position += new Vector3(0, .8f, 0);
            transform.position = rigidBody.transform.position;
            Update();
            ResetKnownOffset();
            rigidBody.useGravity = true;
            SetHomePoint(transform.position);
            _flightStatus = FlightStatus.Flying;
            return true;
        }


    }
}