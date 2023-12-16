using System;
using Utilities.Events;
using UnityEngine;
using RoboticsToolkit.Robotics.Limbs;
using RoboticsToolkit.Robotics.RoboticControllers;
using RoboticsToolkit.Gimbal;
using UnityEngine.Assertions;
using RoboticsToolkit.Robotics;

namespace RoboticsToolkit.Robotics.QuadrupedRobot
{
    [Serializable]
    public class QuadrupedLimbData:LimbData
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

    public abstract class Quadruped : MonoBehaviour, IRobot, IRoboticControllerEventListener
    {
        protected IRoboticLimb[] m_limbs;

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

      
        protected IRobot.Status _status = IRobot.Status.NotReady;

        protected bool _isRunning = false;

        [SerializeField]
        private QuadrupedLeg m_dynamicLimbPrefab;

   
        [SerializeField]
        private float _readyHeight = 1f;

        protected virtual void Awake()
        {
            m_limbs = new QuadrupedLeg[4] { m_flLimb, m_frLimb, m_brLimb, m_blLimb };
            //ToggleColliders(false);
        }
        protected virtual void Start()
        {
           // ToggleColliders(false);

            if (IsSimulation())
            {
                //UpdateStatus(Status.Initialized);
            }


        }
        public void Bootup()
        {
            Debug.Log("Quadruped Bootup");
            OnBootup();
      
        }
        protected virtual void OnBootup()
        {

        }

        protected void CompleteBootup()
        {
            Debug.Log("Quadruped Bootup Complete");
            _isRunning = true;

            // _controller.SetRobotHeight(_readyHeight, .09f);
            UpdateStatus(IRobot.Status.Ready);
        }
        protected void UpdateStatus(IRobot.Status newStatus)
        {
            if(_status != newStatus)
            {
                _status = newStatus;
                switch (newStatus)
                {
                    case IRobot.Status.NotReady:
                        break;
                    case IRobot.Status.Initialized:
                        NotifyRobotEventListeners(IRobotEventListener.EventType.OnRobotInitialized);
                        break;               
                    case IRobot.Status.AdjustingHeight:
                        NotifyRobotEventListeners(IRobotEventListener.EventType.OnRobotInPosition);
                        break;
                    case IRobot.Status.Ready:
                        NotifyRobotEventListeners(IRobotEventListener.EventType.OnRobotReady);                      
                        break;
                    default:
                        break;
                }
       
            }
        }
        protected void ToggleColliders(bool toggle)
        {
            foreach (var item in GetComponentsInChildren<Collider>(true))
            {
                item.enabled = toggle;
            }
        }

        protected virtual Vector3 GetLowestFoot()
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
     
        protected virtual void AtReadyHeight()
        {
            UpdateStatus(IRobot.Status.Ready);
            NotifyRobotEventListeners(IRobotEventListener.EventType.OnRobotInPosition);
        }

     

        public QuadrupedLeg ConstructDynamicLeg(Transform parentTransform, IRoboticLimb leg, string name, Color color, bool left = false)
        {
            var newLeg = Instantiate(m_dynamicLimbPrefab).GetComponent<QuadrupedLeg>();
            newLeg.name = name;
            newLeg.transform.SetParent(parentTransform);
            newLeg.transform.localEulerAngles = new Vector3(0, 270, 180);

            newLeg.m_invert = left;

            var ogSegments = leg.GetSegments();
            var hipOffset = parentTransform.InverseTransformPoint(ogSegments[1].GetGameObject().transform.position);
            newLeg.GetHipSegment().GetGameObject().transform.parent.position = parentTransform.TransformPoint(hipOffset);
            newLeg.GetKneeSegment().GetGameObject().transform.parent.localPosition = new Vector3(0, 0, ogSegments[1].GetLength());
            newLeg.GetContactPoint().transform.localPosition = new Vector3(0, 0, ogSegments[2].GetLength());
            var ikPoint = parentTransform.TransformPoint(parentTransform.InverseTransformPoint(leg.GetEndPoint().transform.position));
            newLeg.IKTarget.position = ikPoint;

            foreach (var item in newLeg.GetLimbSegments())
            {
                item.SetRenderType(IRoboticLimbSegment.RenderType.Line, color);
            }

            newLeg.SetLimbValues(0, leg.GetSegments()[1].GetServoAngle(0), leg.GetSegments()[2].GetServoAngle(0));
            return newLeg;
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

        public abstract bool IsSimulation();

        public virtual void Run()
        {
            PositionTransform();
           // PositionLimbs();
        }
        protected virtual void PositionTransform()
        {
          
            //  m_frLimb.SetLimbValues(limbData.FRBaseAngle, limbData.FRHipAngle, limbData.FRKneeAngle);
         
            //  m_brLimb.SetLimbValues(limbData.BRBaseAngle, limbData.BRHipAngle, limbData.BRKneeAngle);
            //  m_blLimb.SetLimbValues(limbData.BLBaseAngle, limbData.BLHipAngle, limbData.BLKneeAngle);
        }

        public void SetLimbs(QuadrupedLimbData limbData)
        {
            m_frLimb.SetLimbValues(limbData.FRBaseAngle, limbData.FRHipAngle, limbData.FRKneeAngle);
              m_flLimb.SetLimbValues(limbData.FLBaseAngle, limbData.FLHipAngle, limbData.FLKneeAngle);
            m_brLimb.SetLimbValues(limbData.BRBaseAngle, limbData.BRHipAngle, limbData.BRKneeAngle);
            m_blLimb.SetLimbValues(limbData.BLBaseAngle, limbData.BLHipAngle, limbData.BLKneeAngle);
        }
        public void SetLimbs(LimbValues[] limbData)
        {
            for (int i = 0; i < m_limbs.Length; i++)
            {
              
                switch (i)
                {
             
                    case 0:
                        m_flLimb.SetLimbValues(limbData[i].ServoAngles[0], limbData[i].ServoAngles[1], limbData[i].ServoAngles[2]);
                        break;
                    case 1:
                        m_frLimb.SetLimbValues(limbData[i].ServoAngles[0], limbData[i].ServoAngles[1], limbData[i].ServoAngles[2]);
                        break;
                    case 2:
                        m_brLimb.SetLimbValues(limbData[i].ServoAngles[0], limbData[i].ServoAngles[1], limbData[i].ServoAngles[2]);
                        break;
                    case 3:
                        m_blLimb.SetLimbValues(limbData[i].ServoAngles[0], limbData[i].ServoAngles[1], limbData[i].ServoAngles[2]);
                        break;
                    default:
                        break;
                }
            
        }
            OnLimbsPositioned(limbData);
            //  OnLimbsPositioned(quadrupedLimbData);
        }
        protected virtual void PositionLimbs()
        {
           //var limbData = _controller.CalculateLimbData(this);
           //// limbData = limbData as QuadrupedLimbData;

           // var quadrupedLimbData = new QuadrupedLimbData();

           // for (int i = 0; i < limbData.Length; i++)
           // {
           //     switch (i)
           //     {
           //         case 0:
           //             quadrupedLimbData.FLTargetPos = limbData[i].LimbTarget;
           //             quadrupedLimbData.FLBaseAngle = limbData[i].ServoAngles[0];
           //             quadrupedLimbData.FLHipAngle = limbData[i].ServoAngles[1];
           //             quadrupedLimbData.FLKneeAngle = limbData[i].ServoAngles[2];
           //             break;
           //         case 1:
           //             quadrupedLimbData.FRTargetPos = limbData[i].LimbTarget;
           //             quadrupedLimbData.FRBaseAngle = limbData[i].ServoAngles[0];
           //             quadrupedLimbData.FRHipAngle = limbData[i].ServoAngles[1];
           //             quadrupedLimbData.FRKneeAngle = limbData[i].ServoAngles[2];
           //             break;
           //         case 2:
           //             quadrupedLimbData.BRTargetPos = limbData[i].LimbTarget;
           //             quadrupedLimbData.BRBaseAngle = limbData[i].ServoAngles[0];
           //             quadrupedLimbData.BRHipAngle = limbData[i].ServoAngles[1];
           //             quadrupedLimbData.BRKneeAngle = limbData[i].ServoAngles[2];
           //             break;
           //         case 3:
           //             quadrupedLimbData.BLTargetPos = limbData[i].LimbTarget;
           //             quadrupedLimbData.BLBaseAngle = limbData[i].ServoAngles[0];
           //             quadrupedLimbData.BLHipAngle = limbData[i].ServoAngles[1];
           //             quadrupedLimbData.BLKneeAngle = limbData[i].ServoAngles[2];
           //             break;
           //         default:
           //             break;
           //     }
           // }



           // if (IsSimulation())
           // {
           //     SetLimbs(quadrupedLimbData);
           // }

           // OnLimbsPositioned(quadrupedLimbData);
        }

        protected virtual void OnLimbsPositioned(LimbValues[] limbValues)
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
                    if(_status == IRobot.Status.AdjustingHeight)
                    AtReadyHeight();
                    break;
                default:
                    break;
            }
        }

        IGimbal IRobot.GetGimbal()
        {
            return GetComponentInChildren<IGimbal>();
        }

        public void SetHipHeight(float hipHeight)
        {
            throw new NotImplementedException();
        }

        public IRobot.Status GetStatus()
        {
            return _status;
        }
    }

}