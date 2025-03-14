using RoboticsToolkit.Robotics.QuadrupedRobot;
using RoboticsToolkit.Robotics.RoboticControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace RoboticsToolkit.Robotics
{
    public class RobotManager : MonoBehaviour, IRobotEventListener, IRoboticControllerEventListener
    {
        public enum InstanceType
        {
            Simulation,
            PhysicalRobot,
            Both,
        }
        [SerializeField]
        private InstanceType _instanceType;

        [SerializeField] private RoboticEnvironment _controllerEnvironment;
        [SerializeField] private RoboticEnvironment _physicalEnvironment;
        [SerializeField] private RoboticEnvironment _simulatedEnvironment;
 
        private IRoboticController _roboticController;
        private IRobot _physcialRobot;
        private IRobot _simulatedRobot;

        [SerializeField]
        private float _hipHeight = .085f;

    


        private bool _ready = false;

        //  private 

        private void Awake()
        {


        }

        void Start()
        {
            _roboticController = _controllerEnvironment.GetController();
            _roboticController.SubscribeToControllerEvents(this);
            // _controller.SetH
            _simulatedEnvironment.gameObject.SetActive(false);
           _physicalEnvironment.gameObject.SetActive(false);
            Debug.Log("Start Manager in mode : " + _instanceType);
            if (_instanceType != InstanceType.PhysicalRobot)
            {
                _simulatedEnvironment.gameObject.SetActive(true);
                _simulatedRobot = _simulatedEnvironment.GetRobot();
                Assert.IsNotNull(_simulatedRobot);
                _simulatedRobot.SubscribeToEvents(this);
                _simulatedRobot.Bootup();
            }
            if (_instanceType != InstanceType.Simulation)
            {
               _physicalEnvironment.gameObject.SetActive(true);
                _physcialRobot =_physicalEnvironment.GetRobot();
                Assert.IsNotNull(_physcialRobot);
                _physcialRobot.SubscribeToEvents(this);
                _physcialRobot.Bootup();
              
            }
            Vector3 slot1 = new Vector3(0,.2f,0);
            Vector3 slot2 = new Vector3(0, .4f, 0);
            switch (_instanceType)
            {
                case InstanceType.Simulation:
                  //  _simulatedEnvironment.transform.localPosition = slot1;
                    break;
                case InstanceType.PhysicalRobot:
                  //  _physicalEnvironment.transform.localPosition = slot1;
                    break;
                case InstanceType.Both:
                   // _physicalEnvironment.transform.localPosition = slot1;
                   // _simulatedEnvironment.transform.localPosition = slot2;
                    break;
                default:
                    break;
            }
        }

        private void Update()
        {
            if (_instanceType == InstanceType.Simulation)
            {
                if (_simulatedRobot != null && _simulatedRobot.GetStatus() == IRobot.Status.Ready)
                {
                    _simulatedRobot.Run();
                    var limbData = _roboticController.CalculateLimbData(_simulatedRobot);
                    _simulatedRobot.SetLimbs(limbData);
                }
                   
            }
            else if(_instanceType == InstanceType.PhysicalRobot)
            {
                if (_physcialRobot != null && _physcialRobot.GetStatus() == IRobot.Status.Ready)
                {
                    _physcialRobot.Run();
                    var limbData = _roboticController.CalculateLimbData(_physcialRobot);
                    _physcialRobot.SetLimbs(limbData);
                }
            }
            else
            {
                if(_simulatedRobot.GetStatus() == IRobot.Status.Ready && _physcialRobot.GetStatus() == IRobot.Status.Ready)
                {
                    _simulatedRobot.Run();
                    _physcialRobot.Run();
                    var limbData = _roboticController.CalculateLimbData(_simulatedRobot);
                    _simulatedRobot.SetLimbs(limbData);
                    _physcialRobot.SetLimbs(limbData);
                }
            }
        }

        private void OnSimulationReady()
        {
            var hipAngle = 70;
            var kneeAngle = -130;
            //   _simulationSetLimbs(new QuadrupedLimbData(0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle));
           // _roboticController.Initialize(_simulatedRobot);
        }

        private void ManagerReady()
        {
            _roboticController.Initialize(_simulatedRobot);
        }

        public void OnRobotEventOccured(IRobotEventListener.EventData eventData)
        {
            switch (eventData.EventType)
            {
                case IRobotEventListener.EventType.OnRobotInitialized:
                    break;
                case IRobotEventListener.EventType.OnRobotInPosition:
                    break;
                case IRobotEventListener.EventType.OnRobotReady:
                    switch (_instanceType)
                    {
                        case InstanceType.Simulation:
                            ManagerReady();
                            break;
                        case InstanceType.PhysicalRobot:
                            ManagerReady();
                            break;
                        case InstanceType.Both:
                            if(eventData.Robot == _simulatedRobot)
                            {
                                if(_physcialRobot.GetStatus() == IRobot.Status.Ready)
                                {
                                    ManagerReady();
                                }
                            }
                            else if(eventData.Robot == _physcialRobot)
                            {
                                if(_simulatedRobot.GetStatus() == IRobot.Status.Ready)
                                {
                                    ManagerReady();
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case IRobotEventListener.EventType.OnLimbsPositioned:
                    break;
                case IRobotEventListener.EventType.OnEmergencyStop:
                    break;
                case IRobotEventListener.EventType.OnReset:
                    break;
                default:
                    break;
            }



        }


   
        private bool _bootupComplete = false;
        public void OnControllerEventOccured(IRoboticControllerEventListener.QuadrupedRoboticControllerEvendData eventData)
        {
          //  if (_instanceType == InstanceType.Simulation)
            {
                switch (eventData.EventType)
                {
                    case IRoboticControllerEventListener.EventType.OnControllerInitialized:
                        break;
                    case IRoboticControllerEventListener.EventType.OnHeightAdjustmentBegin:
                        break;
                    case IRoboticControllerEventListener.EventType.OnHeightAdjustmentEnd:
                        if (!_bootupComplete)
                        {
                            _bootupComplete = true;
                            // _gaitController.PerformHighStep(GaitType.Crawl,  .05f, .01f);
                            //  (_gaitController as GaitController).CrawlForward(IKLimbPositioners, .03f, .01f, .04f);
                            //(_gaitController as GaitController).TrotForward(IKLimbPositioners, .015f, .05f, .01f);
                            // ManagerReady();
                             (_roboticController as DynamicRoboticController).BeginCrawl();
                            //(_roboticController as DynamicRoboticController).BeginTrot();
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
