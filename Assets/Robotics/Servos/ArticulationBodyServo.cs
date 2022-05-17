using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RoboticToolKit.Robotics.Servos
{
    public interface IServo
    {
        public bool IsEnabled();
        public GameObject GetGameObject();
        public float GetCurrentAngle();



      //  public Matrix4x4 GetGlobalAnchorMatrix();

        public void RunServo(float calculatedSpeed);

        public void SetAngleImmediate(float angle);

        public void ResetServo(float resetAngle);
    }

    public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };
    public class ArticulationBodyServo : MonoBehaviour, IServo
    {
        public bool IsEnabled() => enabled;
        public GameObject GetGameObject() => gameObject;
        public RotationDirection rotationState = RotationDirection.None;
        public float speed = 300.0f;

        private ArticulationBody m_articulation;

        //private float m_startAngle;

        private bool m_firstSet = true;

    //    public bool PrintLog = false;

        private void Awake()
        {
            m_articulation = GetComponent<ArticulationBody>();
            var xDrive = m_articulation.xDrive;
            xDrive.forceLimit *= 2;
            m_articulation.xDrive = xDrive;
        }
        private void Start()
        {
       
        }
        public float GetCurrentAngle()
        {
            if (m_articulation == null)
            {
                m_articulation = GetComponent<ArticulationBody>();

            }
            return m_articulation.jointPosition[0] * Mathf.Rad2Deg;
        }
        public void RunServo(float calculatedSpeed)
        {
            //if (m_firstSet)
            //{
            //    m_firstSet = false;
            //}
            //float rotationChange = calculatedSpeed * Time.fixedDeltaTime;
            //float rotationGoal = GetCurrentAngle() + rotationChange;
            //RotateTo(rotationGoal);

            var dif = calculatedSpeed - GetCurrentAngle();
            float rotationGoal = GetCurrentAngle() +  (dif * Time.deltaTime * 10);
            RotateTo(rotationGoal);
        }

        public void ResetServo(float resetAngle)
        {
          //  SetAngleImmediate(resetAngle);
          //  m_articulation.jointR
            // m_articulation.jointPosition = new ArticulationReducedSpace(resetAngle, 0f, 0f);
            //m_articulation.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
            //m_articulation.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
            //m_articulation.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);

            //m_articulation.velocity = Vector3.zero;
            //m_articulation.angularVelocity = Vector3.zero;
            //m_articulation.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
            //m_articulation.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
            //m_articulation.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
            //m_articulation.ResetInertiaTensor();
            //m_articulation.ResetCenterOfMass();
        }
        

        public void SetAngleImmediate(float angle)
        {
            Debug.Log(name + " Force To Angle : " + angle);
            
            var drive = m_articulation.xDrive;
            drive.target = angle;
            m_articulation.xDrive = drive;
            Debug.Log(name + " Angle : " + GetCurrentAngle());
        }


        private void Update()
        {
            //  Debug.Log(GetCurrentAngle());
        }

        // MOVEMENT HELPERS
        //private float CurrentPrimaryAxisRotation()
        //{
        //    float currentRotationRads = m_articulation.jointPosition[0];
        //    float currentRotation = Mathf.Rad2Deg * currentRotationRads;
        //    return currentRotation;
        //}

        private void RotateTo(float primaryAxisRotation)
        {
            var drive = m_articulation.xDrive;
            drive.target = primaryAxisRotation;
         
            m_articulation.xDrive = drive;
        }
    }
}