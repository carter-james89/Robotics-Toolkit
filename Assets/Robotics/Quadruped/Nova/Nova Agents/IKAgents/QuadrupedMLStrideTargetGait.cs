using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace RoboticToolkit.Robotics.Gaits
{
    /// <summary>
    /// Uses Machine learning to try and set the stride target for a limb. the limb then moves to that position
    /// before where another Decision is requested
    /// </summary>
    public class QuadrupedMLStrideTargetGait : Agent, IGait
    {
        [SerializeField]
        private MonoBehaviour m_robotComponent;
        [SerializeField]
        private float m_strideHeight = .1f;
        [SerializeField]
        private float m_gaitTranslateSpeed = .1f;
        [SerializeField]
        private float m_mGaitRotationSpeed = 25;
        [SerializeField]
        private Transform m_target;

        private Vector3 m_currentBodyPosition;
        private Quaternion m_currentBodyRotation;
        private bool m_firstComplition = true;

        private IRobot m_robot;

        protected  void Awake()
        {
           // base.Awake();
            m_robot = GetComponent<IRobot>();
        }
        public void Initialize(IRobot robot)
        {
            throw new System.NotImplementedException();
        }

        public void RunGait()
        {
            throw new System.NotImplementedException();
        }


        public override void OnEpisodeBegin()
        {
            Debug.Log("On Episode begin");
            m_robot.ResetController();
        }


        public void GetGaitTargets(Vector3 bodyPosition, Quaternion bodyRotation)
        {
            Debug.Log("Get Gait Targets");
            if (!m_firstComplition)
            {
                CalculateRewards();
            }
            m_firstComplition = false;
            m_currentBodyPosition = bodyPosition;
            m_currentBodyRotation = bodyRotation;
            RequestDecision();
        }
        public override void CollectObservations(VectorSensor sensor)
        {
            //19
            //  Debug.Log("collect observations");
            base.CollectObservations(sensor);
            //foreach (var gait in m_gaitsToSet)
            //{
            //    // sensor.AddObservation(gait.GetTarget().position);
            //}
            // sensor.AddObservation(CalculateForwardError());
            sensor.AddObservation(m_currentBodyRotation);
            sensor.AddObservation(m_target.transform.localPosition - m_currentBodyPosition);
            // sensor.AddObservation(m_target.localPosition - transform.localPosition);

        }
        public override void OnActionReceived(ActionBuffers actions)
        {
            // Debug.Log("on action received");
            int translateFLGait = actions.DiscreteActions[0];
            int translateFRGait = actions.DiscreteActions[1];
            int translateBRGait = actions.DiscreteActions[2];
            int translateBLGait = actions.DiscreteActions[3];

            //Vector3 flPosition = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
            //RunGait(m_gaitsToSet[0], flPosition, translateFLGait);

            //Vector3 frPosition = new Vector3(actions.ContinuousActions[2], 0, actions.ContinuousActions[3]);
            //RunGait(m_gaitsToSet[1], frPosition, translateFRGait);

            //Vector3 brPosition = new Vector3(actions.ContinuousActions[4], 0, actions.ContinuousActions[5]);
            //RunGait(m_gaitsToSet[2], brPosition, translateBRGait);

            //Vector3 blPosition = new Vector3(actions.ContinuousActions[6], 0, actions.ContinuousActions[7]);
            //RunGait(m_gaitsToSet[3], blPosition, translateBLGait);
            //CalculateRewards();
        }
        private void RunGait(IGait gait, Vector3 position, int translate)
        {
            if (translate == 0)
            {
                // gait.RotateToPosition(position, m_gaitTranslateSpeed, m_strideHeight);
            }
            else
            {
                // gait.TranslateToPosition(position, m_gaitTranslateSpeed);
            }
        }
        private void Update()
        {
            var upAngle = Vector3.Angle(transform.up, Vector3.up);
            if (upAngle > 30)
            {
                SetReward(-1);
                EndEpisode();
                return;
            }
        }
        private void CalculateRewards()
        {

            SetReward(-Vector3.Distance(m_target.localPosition, m_currentBodyPosition));
        }

        public bool IsRunning()
        {
            throw new System.NotImplementedException();
        }

        public void Begin()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public void SetNextCycle()
        {
            throw new System.NotImplementedException();
        }

        public void SubscribeToEvents(IGaitEventListener listener)
        {
            throw new System.NotImplementedException();
        }

        public void UnubscribeFromEvents(IGaitEventListener listener)
        {
            throw new System.NotImplementedException();
        }

        public void ReturnHome()
        {
            throw new System.NotImplementedException();
        }
    }
}

