using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticToolKit.Robotics.Servos
{
    public class DirectServoController : MonoBehaviour, IServoController
    {
        private IServo[] m_servos;

        private float m_setAngle;
        private void Awake()
        {
            GetServo();
        }

        public void Reset()
        {
            foreach (var servo in m_servos)
            {
                servo.ResetServo(0);
            }

        }
        public IServo GetServo()
        {
            if (m_servos == null)
            {
                m_servos = GetComponents<IServo>();
            }
            return m_servos[0];
        }

        public void SetAndRunServo(float desiredAngle, bool immediate)
        {
            m_setAngle = desiredAngle;
            if (immediate)
            {
                foreach (var servo in m_servos)
                {
                    servo.SetServoPositionImmediate(desiredAngle);
                }
                return;
            }
            {
                foreach (var servo in m_servos)
                {
                    servo.SetServoPosition(desiredAngle);
                }
            }
        }

        public float GetSetAngle()
        {
            return m_setAngle;
        }
    }
}
