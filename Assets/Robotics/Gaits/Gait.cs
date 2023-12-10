using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using Utilities.Events;
using UnityEngine;
using RoboticsToolkit.Gimbal;

namespace RoboticsToolkit.Robotics.Gaits
{
    public abstract class Gait :  IGait
    {
        public bool IsRunning()
        {
            return m_running;
        }
        //   private bool m_halfStride = false;
        private bool m_running = false;

        protected int _currentStrideCount = 0;

        // private bool m_halfStride = false;
       // protected IRobot _robot;
        private InterfaceEventManager<IGaitEventListener> _eventManager = new InterfaceEventManager<IGaitEventListener>("Gait");
        //public void Initialize()
        //{
        //   // _robot = robot;
        //    //    NotifyListeners(IGaitEventListener.EventType.)
        //}
        public virtual  void Reset()
        {
            Debug.Log("Begin " + Time.frameCount);
            _currentStrideCount = 0;    
            m_running = true;
          
        }

        public abstract bool CheckLimbPositions(ILimbPositioner[] limbPositioners);

    


        public void Stop()
        {
            m_running = false;
        }

        public void SubscribeToEvents(IGaitEventListener listener)
        {
            _eventManager.AddListener(listener);
        }

        public void UnubscribeFromEvents(IGaitEventListener listener)
        {
           _eventManager.RemoveListener(listener);
        }
        protected void NotifyListeners(EventType eventType)
        {
            //Debug.Log("Fire Gait Event : " + eventType.ToString());
            foreach (var item in _eventManager.GetListeners())
            {
                item.OnGaitEventOccured(new GaitEventData(eventType, this));
            }
        }

  

        //public bool RequestBeginCMD(IGaitController requestingController, ILimbPositioner[] limbPositioners)
        //{
        //    if (m_running)
        //    {
        //        return false;
        //    }
        //    m_running = true;
        //    _currentStrideCount = 0;
        // //   NotifyListeners(IGaitEventListener.EventType.OnGaitCycleBegin);
        //    OnCMDRequestGranted();
        //    return true;
        //}
        //protected void OnCMDRequestGranted()
        //{

        //}

        public abstract GaitCycleInfo GetGaitCycleInfo();

        public abstract float GetRotationSpeedMultiplier();

        public abstract void Translate(ILimbPositioner[] limbPositioners, float speed, float strideLength, float strideHeight);
    } 
}
