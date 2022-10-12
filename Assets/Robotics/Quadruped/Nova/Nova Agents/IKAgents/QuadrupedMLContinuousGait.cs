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
    /// Gait which uses ML to set the position of the gate ever frame
    /// </summary>
    public class QuadrupedMLContinuousGait : Agent, IGait
    {
        [SerializeField]
        private Transform m_target;

        [SerializeField]
        private TMPro.TextMeshPro m_angleText;
        [SerializeField]
        private TMPro.TextMeshPro m_velocityText;

        private IRoboticLimb[] m_limbs;

        private bool m_firstComplition = true;

        private IRobot m_robot;

        private bool m_firstEpisode = true;

        [SerializeField]
        private MeshRenderer m_meshRenderer;


        protected  void Awake()
        {
           // base.Awake();
            m_robot = GetComponent<IRobot>();
            m_meshRenderer.material.color = Color.white;
        }
        public void Initialize(IRobot robot)
        {
            m_limbs = robot.GetLimbs();

        }
        private bool m_pauseAtReset = false;
        private bool m_paused = false;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("Pause");
                m_pauseAtReset = true;
            }
        }
        public override void OnEpisodeBegin()
        {

            m_firstComplition = true;

            //Debug.Log("On Episode begin");

        }
        public void RunGait()
        {

            if (m_firstComplition)
            {
                m_robot.ResetController();
                m_prevDist = Vector3.Distance(m_target.localPosition, transform.localPosition);
                foreach (var limb in m_limbs)
                {
                    limb.GetGameObject().GetComponentInChildren<LimbPositioner>().SetTargetPosition(Vector3.zero);
                }
                if (m_pauseAtReset)
                {
                    m_paused = true;
                }
                m_firstComplition = false;
                return;
            }
            if (m_paused)
            {
                return;
            }
            RequestDecision();
        }



        public override void CollectObservations(VectorSensor sensor)
        {
            //19
            //  Debug.Log("collect observations");
            base.CollectObservations(sensor);
            //foreach (var gait in m_gaitsToSet)
            //{
            //   // sensor.AddObservation(gait.GetTarget().position);
            //}
            foreach (var limb in m_limbs)
            {
                m_robot.GetGameObject().transform.InverseTransformPoint(limb.GetEndPoint().transform.position);
            }
            // sensor.AddObservation(CalculateForwardError());
            var robotData = m_robot.GetRobotData();
            //sensor.AddObservation(robotData.Velocity);
            //sensor.AddObservation(robotData.AngularVelocity);
            sensor.AddObservation(m_target.transform.localEulerAngles - transform.localEulerAngles);
            sensor.AddObservation(m_target.transform.forward - m_robot.GetRobotData().Velocity);
            // sensor.AddObservation(m_target.localPosition - transform.localPosition);

        }

        [SerializeField]
        private Vector3 m_maxBounds = Vector3.one;
        [SerializeField]
        private Vector3 m_minBounds = -Vector3.one;
        public override void OnActionReceived(ActionBuffers actions)
        {
            var flLimb = m_limbs[0].GetGameObject().GetComponentInChildren<LimbPositioner>();
            var frLimb = m_limbs[1].GetGameObject().GetComponentInChildren<LimbPositioner>();
            var brLimb = m_limbs[2].GetGameObject().GetComponentInChildren<LimbPositioner>();
            var blLimb = m_limbs[3].GetGameObject().GetComponentInChildren<LimbPositioner>();

            SetPosition(flLimb, actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[2]);
            SetPosition(frLimb, actions.ContinuousActions[3], actions.ContinuousActions[4], actions.ContinuousActions[5]);
            SetPosition(brLimb, actions.ContinuousActions[6], actions.ContinuousActions[7], actions.ContinuousActions[8]);
            SetPosition(blLimb, actions.ContinuousActions[9], actions.ContinuousActions[10], actions.ContinuousActions[11]);

            foreach (var limb in m_limbs)
            {
                limb.RunLimb(true);
            }
            CalculateRewards();
        }

        private void SetPosition(LimbPositioner positioner, float x, float y, float z)
        {
            var posX = Mathf.Clamp(x * m_maxBounds.x, m_minBounds.x, m_maxBounds.x);
            var posY = Mathf.Clamp(y * m_maxBounds.y, m_minBounds.y, m_maxBounds.y);
            var posZ = Mathf.Clamp(z * m_maxBounds.z, m_minBounds.z, m_maxBounds.z);
            Vector3 position = new Vector3(posX, posY, posZ);
            positioner.SetTargetPosition(position);
        }

        private float m_prevDist;
        private void CalculateRewards()
        {
            var upAngle = Vector3.Angle(transform.up, Vector3.up);
            var forwardAngle = Vector3.Angle(transform.forward, m_target.forward);
            if (upAngle > 20 || forwardAngle > 20) 
            {
                SetReward(-1);
                EndEpisode();
                m_firstEpisode = false;
                m_meshRenderer.material.color = Color.red;
                return;
            }
            var dist = Vector3.Distance(m_target.localPosition, transform.localPosition);
            // SetReward(m_prevDist - dist);
            // m_prevDist = dist;
            //var angleReward = Vector3.Dot(m_target.transform.forward, transform.forward) * .1f;
            //var angleReward = (m_target.localEulerAngles - transform.localEulerAngles).magnitude *.01f;
            //AddReward(-angleReward);
            //if (m_angleText)
            //    m_angleText.text = angleReward.ToString();

            // var velocityReward = Vector3.Dot(m_target.transform.forward, m_robot.GetRobotData().Velocity);
            var velocityReward = (m_target.transform.forward - m_robot.GetRobotData().Velocity).magnitude;
            AddReward(-velocityReward);
            if (m_velocityText)
            {
                m_velocityText.text = velocityReward.ToString();
                // m_velocityText.text = (m_target.transform.forward).ToString();
                //m_velocityText.text = Vector3.Dot(m_target.transform.right, new Vector3(.6f, 0, .3f)).ToString();
            }

            if (dist < .15f)
            {
                m_meshRenderer.material.color = Color.green;
                SetReward(1);
                EndEpisode();
            }

            if (StepCount == MaxStep - 1)
            {
                m_meshRenderer.material.color = Color.blue;
            }
        }

        public bool IsRunning()
        {
            throw new System.NotImplementedException();
        }
    }
}

