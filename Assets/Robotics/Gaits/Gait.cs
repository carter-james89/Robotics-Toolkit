using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using Utilities.Events;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public abstract class Gait : MonoBehaviour, IGait
    {

        protected List<ILimbPositioner> m_rotatingLimbs = new List<ILimbPositioner>();
        protected List<ILimbPositioner> m_translatingLimbs = new List<ILimbPositioner>();


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

       // protected IRobot _robot;
        private InterfaceEventManager<IGaitEventListener> _eventManager = new InterfaceEventManager<IGaitEventListener>("Gait");
        public void Initialize()
        {
           // _robot = robot;
            //    NotifyListeners(IGaitEventListener.EventType.)
        }
        public void Begin()
        {
            Debug.Log("Begin " + Time.frameCount);
          
        }

        public abstract void CheckLimbPositions(ILimbPositioner[] limbPositioners);

        public bool IsRunning()
        {
            return m_running;
        }

        public void ReturnHome()
        {
            throw new System.NotImplementedException();
        }

        public abstract void SetNextCycle(Vector3 direction, ILimbPositioner[] limbPositioners, float speed, bool rotate);

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
        protected void NotifyListeners(EventType eventType)
        {
            Debug.Log("Fire Gait Event : " + eventType.ToString());
            foreach (var item in _eventManager.GetListeners())
            {
                item.OnGaitEventOccured(new GaitEventData(eventType, this));
            }
        }

        public void SetStrideValues(float strideDistance, float strideHeight)
        {
            SetStrideDistance(strideDistance);
            SetStrideHeight(strideHeight);
        }

        public bool RequestBeginCMD(IGaitController requestingController, ILimbPositioner[] limbPositioners)
        {
            if (m_running)
            {
                return false;
            }
            m_running = true;
            _currentStrideCount = 0;
         //   NotifyListeners(IGaitEventListener.EventType.OnGaitCycleBegin);
            OnCMDRequestGranted();
            return true;
        }
        protected void OnCMDRequestGranted()
        {

        }
    } 
}
