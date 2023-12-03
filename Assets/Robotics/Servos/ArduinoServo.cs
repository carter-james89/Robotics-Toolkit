using ProcessCommunicationToolkit.SerialPortTools;
using RoboticsToolkit.ArduinoUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Servos
{
    public class ArduinoServo : MonoBehaviour, IServo
    {
        [SerializeField]
        private float m_servoNumber;
        [SerializeField]
        private ArduinoConnection m_arduinoConnection;
        [SerializeField]
        private float m_servoSpeed;
        private float m_currentPosition = 0;

   
        public float GetCurrentAngle()
        {
            return m_currentPosition;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public bool IsEnabled()
        {
            return gameObject.activeSelf;
        }

        public void ResetServo(float resetAngle)
        {
           m_currentPosition = resetAngle;
        }

        public void SetServoPosition(bool setServoImmediate, float position)
        {
            SetServoPosition(position, m_servoSpeed);
        }

        public void SetServoPosition(float position, float speed)
        {
            var dif = position - GetCurrentAngle();
            float rotationGoal = GetCurrentAngle() + (dif * Time.deltaTime * speed);
            m_currentPosition = rotationGoal;
            m_arduinoConnection.WriteToArduino(m_servoNumber + ":" + rotationGoal.ToString());
        }

        public void SetServoPosition(float position)
        {
            throw new System.NotImplementedException();
        }

        public void SetServoPositionImmediate(float position)
        {
            throw new System.NotImplementedException();
        }

        public void SetServoSpeed(float speed)
        {
            throw new System.NotImplementedException();
        }
    }
}
