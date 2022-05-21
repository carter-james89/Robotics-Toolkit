using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticToolKit.Robotics.Servos
{
    public class DirectServoController : MonoBehaviour, IServoController
    {
        private IServo m_servo;
        private void Awake()
        {
            GetServo();
        }

        public void Reset()
        {
            m_servo.ResetServo(0);
        }
        public IServo GetServo()
        {
           if(m_servo == null)
            {
                m_servo = GetComponent<IServo>();
            }
           return m_servo;  
        }

        public void SetAndRunServo(float desiredAngle)
        {
            m_servo.SetServoPosition(desiredAngle,15);
        }
    }
}
