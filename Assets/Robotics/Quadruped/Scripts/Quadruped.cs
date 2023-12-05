using System;
using Utilities.Events;
using UnityEngine;
using RoboticsToolkit.Robotics.Limbs;
using RoboticsToolkit.Robotics.RoboticControllers;
using RoboticsToolkit.Gimbal;

namespace RoboticsToolkit.Robotics.QuadrupedRobot
{
    [Serializable]
    public class QuadrupedLimbData
    {
        public float FLBaseAngle;
        public float FLHipAngle;
        public float FLKneeAngle;
        public Vector3 FLTargetPos;

        public float FRBaseAngle;
        public float FRHipAngle;
        public float FRKneeAngle;
        public Vector3 FRTargetPos;

        public float BRBaseAngle;
        public float BRHipAngle;
        public float BRKneeAngle;
        public Vector3 BRTargetPos;

        public float BLBaseAngle;
        public float BLHipAngle;
        public float BLKneeAngle;
        public Vector3 BLTargetPos;

        public QuadrupedLimbData() { }

        public QuadrupedLimbData(QuadrupedData data)
        {
            FLBaseAngle = data.FLBaseAngle;
            FLHipAngle = data.FLHipAngle;
            FLKneeAngle = data.FLKneeAngle;

            FRBaseAngle = data.FRBaseAngle;
            FRHipAngle = data.FRHipAngle;
            FRKneeAngle = data.FRKneeAngle;

            BRBaseAngle = data.BRBaseAngle;
            BRHipAngle = data.BRHipAngle;
            BRKneeAngle = data.BRKneeAngle;

            BLBaseAngle = data.BLBaseAngle;
            BLHipAngle = data.BLHipAngle;
            BLKneeAngle = data.BLKneeAngle;
        }

        public QuadrupedLimbData(float flBaseAngle, float flHipAngle, float flKneeAngle,
                              float frBaseAngle, float frHipAngle, float frKneeAngle,
                              float brBaseAngle, float brHipAngle, float brKneeAngle,
                              float blBaseAngle, float blHipAngle, float blKneeAngle)
        {
            FLBaseAngle = flBaseAngle;
            FLHipAngle = flHipAngle;
            FLKneeAngle = flKneeAngle;

            FRBaseAngle = frBaseAngle;
            FRHipAngle = frHipAngle;
            FRKneeAngle = frKneeAngle;

            BRBaseAngle = brBaseAngle;
            BRHipAngle = brHipAngle;
            BRKneeAngle = brKneeAngle;

            BLBaseAngle = blBaseAngle;
            BLHipAngle = blHipAngle;
            BLKneeAngle = blKneeAngle;
        }
    }

    public class Quadruped : MonoBehaviour, IRobot, IRoboticControllerEventListener
    {
        [SerializeField]
        private bool _simulationMode = false;
   
        private IRoboticLimb[] m_limbs;

        [SerializeField]
        private QuadrupedLeg m_frLimb;
        [SerializeField]
        private QuadrupedLeg m_flLimb;
        [SerializeField]
        private QuadrupedLeg m_brLimb;
        [SerializeField]
        private QuadrupedLeg m_blLimb;

        [SerializeField]
        private int _startupDelay = 10;

        protected enum Status
        {
            NotReady,
            WaitingForInitialLimbPlacement,
            MovingToReadyHeight,
            Ready,
            WaitingForPhysics
        }
        protected Status _status = Status.NotReady;

        protected bool _isRunning = false;

        [SerializeField]
        private GameObject _controllerObject;
        private IRoboticController _controller;

        

        private float _physicsInitializedTime = -1;
        [SerializeField]
        private float _readyHeight = 1f;

        protected virtual void Awake()
        {
            m_limbs = new QuadrupedLeg[4] { m_flLimb, m_frLimb, m_brLimb, m_blLimb };
        }
        protected virtual void Start()
        {
            if (SimulationMode())
            {
                var hipAngle = 70;
                var kneeAngle = -130;
                SetLimbs(new QuadrupedLimbData(0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle));
                _status = Status.WaitingForInitialLimbPlacement;
              
                ToggleColliders(false);
            }
        }
        private void ToggleColliders(bool toggle)
        {
            Debug.Log("toggle colliders " + toggle);
            foreach (var item in GetComponentsInChildren<Collider>(true))
            {
                item.enabled = toggle;
            }
        }

        private Vector3 GetLowestFoot()
        {
            Vector3 returnPoint = transform.position;
            foreach (var item in m_limbs)
            {
                if (item.GetEndPoint().transform.position.y < returnPoint.y)
                {
                    returnPoint.y = item.GetEndPoint().transform.position.y;
                }
            }
            return returnPoint;
        }
        public void Bootup()
        {
            Debug.Log("Quadruped Bootup");
            _controller = _controllerObject.GetComponent<IRoboticController>();
            _controller.SubscribeToControllerEvents(this);
            _controller.Initialize(this);
            _isRunning = true;    
        //  


            _controller.SetRobotHeight(_readyHeight, .01f);
            _status =  Status.MovingToReadyHeight;      
        }

        protected virtual void Update()
        {
            Debug.Log("Update" + _status);
            switch (_status)
            {
                case Status.NotReady:
                    break;
                case Status.Ready:
                    Run();
                    break;
                case Status.WaitingForInitialLimbPlacement:
                    bool allServosReady = true;
                    foreach (var limb in m_limbs)
                    {
                        if(!(limb as QuadrupedLeg).SegmentsAtTarget(1f))
                        {
                            allServosReady = false;
                        }
                    }
                    if (allServosReady)
                    {
                        var height = transform.position.y - GetLowestFoot().y;
                        GetComponent<ArticulationBody>().TeleportRoot(new Vector3(transform.position.x, height + .05f, transform.position.z), transform.rotation);
                        ToggleColliders(true);
                        GetComponent<ArticulationBody>().immovable = false;
                        _physicsInitializedTime = Time.timeSinceLevelLoad;
                        _status = Status.WaitingForPhysics;
                    }
                    break;
                case Status.WaitingForPhysics:
                    if (Time.timeSinceLevelLoad > _physicsInitializedTime + 10)
                    {
                        Bootup();
                    }
                    break;
                case Status.MovingToReadyHeight:
                    Run();
                    break;
                default:
                    break;
            }
        }
        protected virtual void AtReadyHeight()
        {
           // (_controller as DynamicRoboticController).OnQuadrupedReady(this);
            _status = Status.Ready;
            Debug.Log("Quadruped Ready");
            NotifyRobotEventListeners(IRobotEventListener.EventType.OnRobotInPosition);
        }

        protected void SetLimbs(QuadrupedLimbData limbData)
        {
            m_frLimb.SetLimbValues(limbData.FRBaseAngle, limbData.FRHipAngle, limbData.FRKneeAngle);
            m_flLimb.SetLimbValues(limbData.FLBaseAngle, limbData.FLHipAngle, limbData.FLKneeAngle);
            m_brLimb.SetLimbValues(limbData.BRBaseAngle, limbData.BRHipAngle, limbData.BRKneeAngle);
            m_blLimb.SetLimbValues(limbData.BLBaseAngle, limbData.BLHipAngle, limbData.BLKneeAngle);
        }


        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public IRoboticLimb[] GetLimbs()
        {
            if (m_limbs == null)
            {
                m_limbs = new IRoboticLimb[4] { m_flLimb, m_frLimb, m_brLimb, m_blLimb };
            }
            return m_limbs;
        }



        public bool SimulationMode()
        {
            return _simulationMode;
        }

        public void Run()
        {
            PositionTransform();
            PositionLimbs();
        }
        protected virtual void PositionTransform()
        {
          //  m_frLimb.SetLimbValues(limbData.FRBaseAngle, limbData.FRHipAngle, limbData.FRKneeAngle);
          //  m_flLimb.SetLimbValues(limbData.FLBaseAngle, limbData.FLHipAngle, limbData.FLKneeAngle);
          //  m_brLimb.SetLimbValues(limbData.BRBaseAngle, limbData.BRHipAngle, limbData.BRKneeAngle);
          //  m_blLimb.SetLimbValues(limbData.BLBaseAngle, limbData.BLHipAngle, limbData.BLKneeAngle);
        }
        protected virtual void PositionLimbs()
        {
            var limbData = _controller.CalculateLimbData(this);

            var quadrupedLimbData = new QuadrupedLimbData();

            for (int i = 0; i < limbData.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        quadrupedLimbData.FLTargetPos = limbData[i].LimbTarget;
                        quadrupedLimbData.FLBaseAngle = limbData[i].ServoAngles[0];
                        quadrupedLimbData.FLHipAngle = limbData[i].ServoAngles[1];
                        quadrupedLimbData.FLKneeAngle = limbData[i].ServoAngles[2];
                        break;
                    case 1:
                        quadrupedLimbData.FRTargetPos = limbData[i].LimbTarget;
                        quadrupedLimbData.FRBaseAngle = limbData[i].ServoAngles[0];
                        quadrupedLimbData.FRHipAngle = limbData[i].ServoAngles[1];
                        quadrupedLimbData.FRKneeAngle = limbData[i].ServoAngles[2];
                        break;
                    case 2:
                        quadrupedLimbData.BRTargetPos = limbData[i].LimbTarget;
                        quadrupedLimbData.BRBaseAngle = limbData[i].ServoAngles[0];
                        quadrupedLimbData.BRHipAngle = limbData[i].ServoAngles[1];
                        quadrupedLimbData.BRKneeAngle = limbData[i].ServoAngles[2];
                        break;
                    case 3:
                        quadrupedLimbData.BLTargetPos = limbData[i].LimbTarget;
                        quadrupedLimbData.BLBaseAngle = limbData[i].ServoAngles[0];
                        quadrupedLimbData.BLHipAngle = limbData[i].ServoAngles[1];
                        quadrupedLimbData.BLKneeAngle = limbData[i].ServoAngles[2];
                        break;
                    default:
                        break;
                }
            }



            if (SimulationMode())
            {
                SetLimbs(quadrupedLimbData);
            }

            OnLimbsPositioned(quadrupedLimbData);
        }

        protected virtual void OnLimbsPositioned(QuadrupedLimbData limbData)
        {

        }

        public IRobot.RobotData GetRobotData()
        {
            return new IRobot.RobotData();
        }

        public IGimbal GetGimbal()
        {
            return null;
        }
        //IGimbal IRobot.GetGimbal()
        //{
        //    throw new NotImplementedException();
        //}
        public void EmergencyStop()
        {
           
        }

        public void ResetController()
        {
           
        }

        public void SubscribeToEvents(IRobotEventListener listener)
        {
           _robotEventManager.AddListener(listener);
        }
        private InterfaceEventManager<IRobotEventListener> _robotEventManager = new InterfaceEventManager<IRobotEventListener>("Robot");
        public void UnsubscribeToEvents(IRobotEventListener listener)
        {
           _robotEventManager.RemoveListener(listener);
        }
        private void NotifyRobotEventListeners(IRobotEventListener.EventType eventType)
        {
            foreach (var item in _robotEventManager.GetListeners())
            {
                item.OnRobotEventOccured(new IRobotEventListener.EventData(eventType, this, null));
            }
        }

        public void OnControllerEventOccured(IRoboticControllerEventListener.QuadrupedRoboticControllerEvendData eventData)
        {
            Debug.Log(eventData.EventType);
            switch (eventData.EventType)
            {
                case IRoboticControllerEventListener.EventType.OnControllerInitialized:
                    break;
                case IRoboticControllerEventListener.EventType.OnHeightAdjustmentBegin:
                    break;
                case IRoboticControllerEventListener.EventType.OnHeightAdjustmentEnd:
                    if(_status == Status.MovingToReadyHeight)
                    AtReadyHeight();
                    break;
                default:
                    break;
            }
        }

        IGimbal IRobot.GetGimbal()
        {
            throw new NotImplementedException();
        }
    }

}