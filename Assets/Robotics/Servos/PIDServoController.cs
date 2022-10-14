
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticToolKit.Robotics.Servos
{
    public interface IServoController
    {
        public IServo GetServo();
        public void SetAndRunServo(float desiredAngle, bool immediate);

        public float GetSetAngle();

        public void Reset();

    }
    public class PIDServoController : MonoBehaviour, IServoController
    {
        private PidController m_speedPID;

        private IServo m_servoToControl;

        public float DesiredAngle = 0;
        private float m_resetAngle;

        public float AngleDif = 0;
        [SerializeField]
        private float m_setSpeed;
        [SerializeField]
        private float m_currentServoAngle;

        [SerializeField]
        private float m_pidMax = 10;
        [SerializeField]
        private float m_pidMin = -10;
        [SerializeField]
        private float m_pidP = .1f;
        [SerializeField]
        private float m_pidI = 0;
        [SerializeField]
        private float m_pidD = 0;

        [SerializeField]
        private bool m_invertServoAngle = false;

        private bool m_firstFixedUpdate = true;

        public IServo Servo => m_servoToControl;

        [SerializeField]
        private bool m_autoRun = true;

        public void SetAutoRun(bool toggle)
        {
            m_autoRun = toggle;
        }

        private void Awake()
        {
            m_servoToControl = GetComponent<IServo>();

            m_resetAngle = DesiredAngle;
            //DesiredAngle = m_servoToControl.GetCurrentAngle();
         //   m_servoToControl.SetAngleImmediate(DesiredAngle);

        }
        private void Start()
        {
            //ResetServoControl();
           // m_servoToControl.SetAngleImmediate(DesiredAngle);
           // m_servoToControl.SetAngleImmediate(DesiredAngle);
        }
        public void Reset()
        {
            ResetPid(m_pidP,m_pidI,m_pidD,m_pidMax,m_pidMin);
        }
        public void ResetPid(float p, float i, float d, float max, float min)
        {
            m_speedPID = new PidController(p, i, d, max, min);
        }
        public IServo GetServo()
        {
            return m_servoToControl;
        }
        public void ResetServoControl()
        {
            m_speedPID = new PidController(m_pidP,m_pidI,m_pidD,m_pidMax,m_pidMin);
            //  DesiredAngle = m_servoToControl.GetCurrentAngle();
           // DesiredAngle = m_resetAngle;
           // m_servoToControl.ResetServo(m_resetAngle);

        }
        private void FixedUpdate()
        {
            if (m_firstFixedUpdate)
            {
                m_firstFixedUpdate = false;
             m_servoToControl.SetServoPosition(DesiredAngle);
            }
            if (m_autoRun)
            {
                CalculateServoSpeed(DesiredAngle);
            }
           
        }
        public void SetAndRunServo(float desiredAngle, bool immediate)
        {
            DesiredAngle = desiredAngle;
            CalculateServoSpeed(desiredAngle);
        }
        public void CalculateServoSpeed(float desiredAngle)
        {
            if (!enabled)
                return;

            m_currentServoAngle = m_servoToControl.GetCurrentAngle();

            if (float.IsNaN(m_currentServoAngle))
            {
                Debug.Log("Servo Angle is NAN");
            }

            AngleDif = m_currentServoAngle - desiredAngle;

            if (float.IsNaN(AngleDif))
            {
                Debug.Log("Angle DIF is NAN");
            }

            m_speedPID.ProcessVariable = AngleDif;
           // Debug.Log((Time.fixedDeltaTime * 1000);
            m_setSpeed = (float)m_speedPID.ControlVariable(new System.TimeSpan(0, 0, 0, 0, (int)(Time.fixedDeltaTime * 1000)));
            if (float.IsNaN(m_setSpeed))
            {
                Debug.Log("Set Speed is NAN");
                m_speedPID = new PidController(.1, .1f, 0, 1, -1);
                return;
            }
            if(m_invertServoAngle)
            {
                m_setSpeed *= -1;
            }
            m_servoToControl.SetServoSpeed(m_setSpeed);
            //m_servoToControl.RunServo(desiredAngle);
        }

        public float GetSetAngle()
        {
            throw new System.NotImplementedException();
        }
    }
}

