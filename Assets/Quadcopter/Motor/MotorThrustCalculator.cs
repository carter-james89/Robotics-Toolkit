
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    public class MotorThrustCalculator : MonoBehaviour, IMotorThrustCalculator
    {
        private PidController _elevationPid;
        private PidController _pitchPid;
        private PidController _rollPid;
        private PidController _yawPid;

        [SerializeField]
        private Transform _targetGimbal;

        /// <summary>
        /// How long has it been since the last Update, required for <see cref="PidController"/>
        /// </summary>
        /// <remarks>
        /// Exposed in Inspector solely for debuging
        /// </remarks>
        [SerializeField]
        private float _timeSinceLastUpdate;
        /// <summary>
        /// The time of the last update
        /// </summary>
        private float prevDeltaTime = 0;



        public float altitudeHoldPos { get; private set; }
        public void SetAltitudeHold(float newHoldHeight)
        {
            altitudeHoldPos = newHoldHeight;
        }

        public void Initialize(float currentHeading)
        {
            Debug.Log("Initialize PID Motor Thrust Calculator");
            _yawHoldHeading = currentHeading;
            _elevationPid = new PidController(.03f, .04f, 0.04f, .8f, .1f);
            _elevationPid.SetPoint = 0;

            // _targetGimbal.transform.localEulerAngles = Vector3.zero;

            var translateP = .001f;
            var translateI = 0;
            var translateD = 0f;
            var translateLimit = .1f;

            _pitchPid = new PidController(translateP, translateI, translateD, translateLimit, -translateLimit);
            _pitchPid.SetPoint = 0;

            _rollPid = new PidController(translateP, translateI, translateD, translateLimit, -translateLimit);
            _rollPid.SetPoint = 0;

            _yawPid = new PidController(.03f, 0.05f, 0.008, .1f, -.1f);
            _yawPid.SetPoint = 0;

            prevDeltaTime = Time.time;
        }

        private float _yawHoldHeading;
        public IMotorThrustCalculator.MotorThrustValues Run(Vector3 currentPos, Vector3 currentEuler, IInputs.FlightControlValues inputs)
        {
            _timeSinceLastUpdate = Time.time - prevDeltaTime;
            prevDeltaTime = Time.time;
            var deltaTime1 = (int)(_timeSinceLastUpdate * 1000);
            var deltaTime = new System.TimeSpan(0, 0, 0, 0, (deltaTime1));

            var throttleValue = calculateThrottle(inputs.throttle, currentPos.y, deltaTime);
            if (CheckIsNAN(throttleValue))
            {
                Debug.LogWarning("Throttle value is NAN");
                throttleValue = 0;
                return new IMotorThrustCalculator.MotorThrustValues();
            }

            var eulerDif = currentEuler - calculateDesiredAngle(inputs);
            var pitchOffset = eulerDif.x;

            if (pitchOffset < -180)
                pitchOffset = 360 - System.Math.Abs(pitchOffset);
            else if (pitchOffset > 180)
                pitchOffset = -(360 - pitchOffset);

            float pitchValue = 0;
            if (!CheckIsNAN(pitchOffset))
            {
                _pitchPid.ProcessVariable = -pitchOffset;
                double trgtPitch = _pitchPid.ControlVariable(deltaTime);
                pitchValue = (float)trgtPitch;

                if (CheckIsNAN(pitchValue))
                {
                    Debug.LogWarning("Pitch value is NAN");
                    Debug.Log(pitchValue);
                    pitchValue = 0;
                    return new IMotorThrustCalculator.MotorThrustValues();
                }
            }
            else
            {
                return new IMotorThrustCalculator.MotorThrustValues();
            }

            //ROLL
            var rollOffset = eulerDif.z;
            if (rollOffset < -180)
                rollOffset = 360 - System.Math.Abs(rollOffset);
            else if (rollOffset > 180)
                rollOffset = -(360 - rollOffset);

            float rollValue = 0;
            if (!CheckIsNAN(rollOffset))
            {
                //   Debug.Log("Roll Offset : " + rollOffset);
                _rollPid.ProcessVariable = -rollOffset;

                double trgtRoll = _rollPid.ControlVariable(deltaTime);
                rollValue = (float)trgtRoll;

                if (CheckIsNAN(rollValue))
                {
                    Debug.LogWarning("Roll value is NAN");
                    Debug.Log("Roll Offset " + rollOffset);
                    rollValue = 0;
                    return new IMotorThrustCalculator.MotorThrustValues();
                }

            }
            else
            {
                return new IMotorThrustCalculator.MotorThrustValues();
            }



            //yaw
            var yawOffset = eulerDif.y;
            if (yawOffset < -180)
                yawOffset = 360 - Math.Abs(yawOffset);
            else if (yawOffset > 180)
                yawOffset = -(360 - yawOffset);

            _yawPid.ProcessVariable = -yawOffset;

            var trgtyaw = _yawPid.ControlVariable(deltaTime);
            float yawValue = (float)trgtyaw;




            if (CheckIsNAN(yawValue))
            {
                Debug.LogWarning("YAw value is NAN");
                yawValue = 0;
            }

            var motorValues = new IMotorThrustCalculator.MotorThrustValues();
            motorValues.motorFR = throttleValue + pitchValue - rollValue + yawValue;
            motorValues.motorFL = throttleValue + pitchValue + rollValue - yawValue;
            motorValues.motorBR = throttleValue - pitchValue - rollValue - yawValue;
            motorValues.motorBL = throttleValue - pitchValue + rollValue + yawValue;

            // Debug.Log(motorValues.motorFR);
            //Debug.Log(throttleValue);
            //Debug.Log(pitchValue);
            //Debug.Log(rollValue);
            //Debug.Log(yawValue)/*;*/

            //if (CheckIsNAN(motorValues.motorFR) || CheckIsNAN(motorValues.motorFL)||
            //    CheckIsNAN(motorValues.motorBR) || CheckIsNAN(motorValues.motorBL))
            //{
            //    Debug.LogWarning("NAN values at Motor Thrust Controller");
            //    motorValues = new IMotorThrustCalculator.MotorThrustValues();

            //}
            //else
            //{
            //    //motorValues = new IMotorThrustCalculator.MotorThrustValues();
            //}

            return motorValues;
        }

        private bool CheckIsNAN(float value)
        {
            return float.IsNaN(value);
        }

        private Vector3 calculateDesiredAngle(IInputs.FlightControlValues inputs)
        {
            float pitchAngle = 0;
            if (inputs.pitch > 0)
            {
                pitchAngle = Mathf.Lerp(0, 15, inputs.pitch);
            }
            else
            {
                pitchAngle = Mathf.Lerp(0, -15, -inputs.pitch);
            }
            float rollAngle = 0;
            if (inputs.roll > 0)
            {
                rollAngle = Mathf.Lerp(0, 15, inputs.roll);
            }
            else
            {
                rollAngle = Mathf.Lerp(0, -15, -inputs.roll);
            }

            float yawAngle = 0;
            if (inputs.yaw > 0)
            {
                yawAngle = Mathf.Lerp(0, 15, inputs.yaw);
                _yawHoldHeading = _yawHoldHeading - yawAngle;
            }
            else if (inputs.yaw < 0)
            {
                yawAngle = Mathf.Lerp(0, 15, -inputs.yaw);
                _yawHoldHeading = _yawHoldHeading + yawAngle;
            }
            return new Vector3(pitchAngle, _yawHoldHeading, -rollAngle);
        }

        private float calculateThrottle(float throttle, float currentHeight, TimeSpan deltaTime)
        {
            float throttleValue = 0;
            if (Math.Abs(throttle) > 0)
            {
                altitudeHoldPos = currentHeight;
                throttleValue = throttle;
            }
            else
            {
                var heightOffset = currentHeight - altitudeHoldPos;
                _elevationPid.ProcessVariable = heightOffset;
                double trgtThrottle = _elevationPid.ControlVariable(deltaTime);
                throttleValue = (float)trgtThrottle;
            }
            return throttleValue;
        }

        private void calculatePitch(float desiredPitch, TimeSpan deltaTime)
        {

        }
    } 
}