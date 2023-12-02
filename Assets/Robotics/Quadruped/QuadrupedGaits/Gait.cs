using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using Toolkit.Utilities.Events;
using UnityEngine;

namespace RoboticToolkit.Robotics.Gaits
{
    public abstract class Gait : MonoBehaviour, IGait
    {
     //   private bool m_halfStride = false;
        private bool m_running = false;

        protected int _currentStrideCount = 0;

        protected IGait.Direction m_direction = IGait.Direction.NONE;

        protected float m_strideDistance = 1;
        protected float m_strideHeight = 1;
        // private bool m_halfStride = false;

        public void SetStrideDistance(float strideDistance)
        {
            m_strideDistance = strideDistance;
        }
        public void SetStrideHeight(float newHeight)
        {
            m_strideHeight = newHeight;
        }

        protected IRobot _robot;
        private InterfaceEventManager<IGaitEventListener> _eventManager = new InterfaceEventManager<IGaitEventListener>("Gait");
        public void Initialize(IRobot robot)
        {
            _robot = robot;
            //    NotifyListeners(IGaitEventListener.EventType.)
        }
        public void Begin()
        {
            Debug.Log("Begin " + Time.frameCount);
            m_running = true;
            //   m_halfStride = true;
            _currentStrideCount = 0;
          //  SetNextCycle(limbPositioners);
            NotifyListeners(IGaitEventListener.EventType.OnGaitCycleBegin);
        }

        public void RunGait()
        {
          
        }

        public bool IsRunning()
        {
            return m_running;
        }

        public void ReturnHome()
        {
            throw new System.NotImplementedException();
        }

        public abstract void SetNextCycle(Vector3 direction, ILimbPositioner[] limbPositioners, bool rotate);

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public void SubscribeToEvents(IGaitEventListener listener)
        {
            _eventManager.AddListener(listener);
        }

        public void UnubscribeFromEvents(IGaitEventListener listener)
        {
           _eventManager.RemoveListener(listener);
        }
        private void NotifyListeners(IGaitEventListener.EventType eventType)
        {
            foreach (var item in _eventManager.GetListeners())
            {
                item.OnGaitEventOccured(new IGaitEventListener.GaitEventData(eventType, _robot, this));
            }
        }
    } 
}
