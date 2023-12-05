using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public class RoboticGaitController : MonoBehaviour, IGaitController
    {
        private IGait _activeGait;

        private IGaitController.GaitPattern _currentPattern = IGaitController.GaitPattern.NONE;

        [SerializeField]
        private TrotGait _trotGait;

        public IGaitController.Direction GetDirection()
        {
            throw new System.NotImplementedException();
        }

        public IGaitController.GaitPattern GetGaitPattern()
        {
            throw new System.NotImplementedException();
        }


        //Called from keyboard
        //private void BeginMovement(IGaitController.GaitPattern patern, QuadrupedTrotGait.Direction direction)
        //{
        //    SetGaitPattern(patern);
        //   // (m_activeGait as QuadrupedTrotGait).SetDirection(direction);
        //    if (_activeGait != null)
        //    {
        //       // _activeGait.SubscribeToEvents(this);
        //        _activeGait.Begin();
        //    }
        //}

        public void Initialize(IRoboticLimb[] limbs)
        {
            throw new System.NotImplementedException();
        }

        public bool IsRunning()
        {
            throw new System.NotImplementedException();
        }

        public void Run()
        {
            throw new System.NotImplementedException();
        }

        public void SetDirection(IGaitController.Direction direction)
        {
            throw new System.NotImplementedException();
        }

        public void SetGaitPattern(IGaitController.GaitPattern type)
        {
            _currentPattern = type;
            switch (type)
            {
                case IGaitController.GaitPattern.NONE:
                    break;
                case IGaitController.GaitPattern.STATIONARYSTEP:
                    break;
                case IGaitController.GaitPattern.CRAWL:
                  //  _activeGait = m_crawlGait;
                    break;
                case IGaitController.GaitPattern.TROT:
                    _activeGait = _trotGait;
                    break;
                default:
                    break;
            }
        }

        public void Run(IRoboticLimb[] mirrorLimbs, ILimbPositioner[] limbs)
        {
            throw new System.NotImplementedException();
        }

        public void BeginMovement(ILimbPositioner[] limbs, IGaitController.GaitPattern patern, Vector3 direction, bool rotate)
        {
            throw new System.NotImplementedException();
        }
    }
}
