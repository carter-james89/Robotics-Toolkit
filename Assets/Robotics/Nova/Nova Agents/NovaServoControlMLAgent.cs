using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Robotics.Nova.ML
{
    public class NovaServoControlMLAgent : Agent
    {
        private List<PIDServoController> m_servoControllers = new List<PIDServoController>();

        private float m_startHeight;

        private bool m_firstEpisode = true;

        [SerializeField]
        private Transform m_ground;

        [SerializeField]
        private Transform m_target;

        private ArticulationBody m_articulation;

        private Quaternion m_startRot;

        bool m_firstStep;

        private float m_prevForwardError = 0;

        protected void Start()
        {

            m_startHeight = transform.position.y - m_ground.position.y;
            var foundControllers = GetComponentsInChildren<PIDServoController>();

            m_startRot = transform.rotation;

            m_articulation = GetComponent<ArticulationBody>();

            foreach (var controller in foundControllers)
            {
                controller.SetAutoRun(false);
                if (controller.Servo.IsEnabled())
                {
                    m_servoControllers.Add(controller);
                }
            }

            // GetComponent<BehaviorParameters>().BrainParameters.ActionSpec = new ActionSpec(m_legServos.Count);
        }

        private float CalculateForwardError()
        {
            var forwardError = Mathf.Abs(m_target.localEulerAngles.y - transform.localEulerAngles.y);
            if (forwardError < -180)
                forwardError = 360 - System.Math.Abs(forwardError);
            else if (forwardError > 180)
                forwardError = -(360 - forwardError);

            return forwardError;
        }
        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            m_firstStep = true;

            //do
            //{
            //    SetNewTarget(m_target);
            //    m_target.transform.position = transform.position;
            //} while (Vector3.Distance(m_target.position, transform.position) < 1);


            SetNewTarget(m_target);
           // m_target.transform.position = transform.position;

            // m_articulation.TeleportRoot(m_ground.position + new Vector3(0, m_startHeight, 0), m_startRot);
            // m_articulation.velocity = Vector3.zero;
            // m_articulation.angularVelocity = Vector3.zero;
            // m_articulation.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
            // m_articulation.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
            // m_articulation.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
            // m_articulation.ResetInertiaTensor();
            // m_articulation.ResetCenterOfMass();
            //// m_articulation.


            // for (int i = 0; i < m_servoControllers.Count; i++)
            // {
            //     m_servoControllers[i].ResetServoControl();
            // }


        }

        private void FixedUpdate()
        {
            if (m_firstStep)
            {
                for (int i = 0; i < m_servoControllers.Count; i++)
                {
                    m_servoControllers[i].ResetServoControl();
                }
                m_prevForwardError = CalculateForwardError();
                m_articulation.TeleportRoot(m_ground.position + new Vector3(0, m_startHeight, 0), m_startRot);
                m_articulation.velocity = Vector3.zero;
                m_articulation.angularVelocity = Vector3.zero;
                ////m_articulation.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
                ////m_articulation.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
                ////m_articulation.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
                //m_articulation.ResetInertiaTensor();
                //m_articulation.ResetCenterOfMass();
                // m_articulation.
                m_firstStep = false;
                return;
            }
            // Debug.Log("request action");
            RequestDecision();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            //  Debug.Log("collect observations");
            base.CollectObservations(sensor);
           // sensor.AddObservation(CalculateForwardError());
            sensor.AddObservation(transform.localEulerAngles);
            sensor.AddObservation(transform.localPosition.y);
           // sensor.AddObservation(m_target.localPosition - transform.localPosition);

        }
        public override void OnActionReceived(ActionBuffers actions)
        {
            // Debug.Log("on action received");
            for (int i = 0; i < m_servoControllers.Count; i++)
            {
                m_servoControllers[i].SetAndRunServo(actions.ContinuousActions[i]*15);
            }
            CalculateRewards();
        }

        
        private void CalculateRewards()
        {
            if (Mathf.Abs(transform.localPosition.x) > 1.2f || Mathf.Abs(transform.localPosition.z) > 1.2f)
            {
                SetGroundColor(Color.blue);
                EndEpisode();
                return;
            }
            var upAngle = Vector3.Angle(transform.up, Vector3.up);

            //Debug.Log(upAngle);
            if (upAngle > 30)
            {
                SetGroundColor(Color.red);
                SetReward(-1);
                EndEpisode();
                return;
            }
            if (upAngle < 10)
            {
                AddReward(.1f);
            }
            else
            {
                AddReward(-.1f);
            }

          //  var currentForwardError = CalculateForwardError();
          //var forwardErrorDelta = m_prevForwardError - currentForwardError;
          //  m_prevForwardError = currentForwardError;

          // // Debug.Log("Prev forward error")
          //  if (Mathf.Abs(currentForwardError) < 10)
          //  {
          //      SetGroundColor(Color.green);
          //      AddReward(10);
          //      EndEpisode();
          //  }
          //  else
          //  {
          //     AddReward(forwardErrorDelta);
          //  }

            //var heightOffset = Mathf.Abs(2f - transform.localPosition.y);
            //if (heightOffset < .1f)
            //{
            //    AddReward(.1f);
            //   // return;
            //}
            //if (heightOffset > .3)
            //{
            //    AddReward(-1);
            //    EndEpisode();
            //    return;
            //}
            //AddReward(-heightOffset);
            if (transform.localPosition.y > .3f || transform.localPosition.y < .15f)
            {
                SetGroundColor(Color.black);
                AddReward(-1);
                EndEpisode();
                return;
            }

            //if(Vector3.Distance(m_target.position, transform.position) < .25f)
            //{
            //    SetGroundColor(Color.green);
            //    AddReward(5);
            //    EndEpisode();
            //}
            //else
            //{
            //    AddReward(-.1f);
            //}

            if (StepCount == MaxStep - 1)
            {
                SetGroundColor(Color.white);
            }
        }

        private void SetGroundColor(Color color)
        {
            m_ground.GetComponent<MeshRenderer>().material.color = color;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            //  Debug.Log("get heuristic");

            var descreteActions = actionsOut.DiscreteActions;
            var contActions = actionsOut.ContinuousActions;
            // descreteActions[0] = ((int)Input.GetAxisRaw("Horizontal")) * 10; //FR
            //descreteActions[1] = (int)Input.GetAxisRaw("Vertical") * 10; //fl
            //descreteActions[2] = (int)Input.GetAxisRaw("Horizontal") * 10; //br
            //  descreteActions[3] = (int)Input.GetAxisRaw("Vertical") * 10; //bl

            //if (Input.GetKeyDown(KeyCode.UpArrow))
            //{
            //    for (int i = 0; i < m_servoControllers.Count; i++)
            //    {
            //        contActions[i] = 0;
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < m_startAngles.Count; i++)
            //    {
            //        contActions[i] = m_startAngles[i];
            //    }
            //}
            // actionsOut.ContinuousActions = contActions;
        }
        private Vector3 _bounds = new Vector3(1.7f, 0, 1.7f);
        private void SetNewTarget(Transform transformToSet)
        {
            var randomX = UnityEngine.Random.Range(-(_bounds.x - .5f), _bounds.x - .5f);
            var randomZ = UnityEngine.Random.Range(-(_bounds.z - .5f), _bounds.z - .5f);

            // _atTargetSeconds = 0;

            // transformToSet.localPosition = new Vector3(randomX, .15f, randomZ);
            transformToSet.localEulerAngles += new Vector3(0, UnityEngine.Random.Range(-180, 180));
        }
    }
}
