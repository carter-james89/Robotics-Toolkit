using Toolkit.Utilities.Events;
using UnityEngine;

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
        private InterfaceEventManager<GaitEventData> _eventManager = new InterfaceEventManager<GaitEventData>("Gait");
        //public void Initialize()
        //{
        //   // _robot = robot;
        //    //    NotifyListeners(GaitEventData.LimbPositionerEventType.)
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

        protected void NotifyListeners(GaitEventType eventType)
        {
            _eventManager.RaiseEvent(new GaitEventData(eventType, this));
        }

        //public bool RequestBeginCMD(IGaitController requestingController, ILimbPositioner[] limbPositioners)
        //{
        //    if (m_running)
        //    {
        //        return false;
        //    }
        //    m_running = true;
        //    _currentStrideCount = 0;
        // //   NotifyListeners(GaitEventData.LimbPositionerEventType.OnGaitCycleBegin);
        //    OnCMDRequestGranted();
        //    return true;
        //}
        //protected void OnCMDRequestGranted()
        //{

        //}

        public abstract GaitCycleInfo GetGaitCycleInfo();

        public abstract float GetRotationSpeedMultiplier();

        public abstract void Translate(ILimbPositioner[] limbPositioners, float speed, float strideLength, float strideHeight);

        public void SubscribeToEvents(IEventListener<GaitEventData> listenerToSubscribe)
        {
           _eventManager.AddListener(listenerToSubscribe);
        }

        public void UnsubscribeFromEvents(IEventListener<GaitEventData> listenerToUnsubscribe)
        {
           _eventManager.RemoveListener(listenerToUnsubscribe);
        }

        public GameObject GetGameObject()
        {
            throw new System.NotImplementedException();
        }

        public Component GetComponent()
        {
            throw new System.NotImplementedException();
        }
    } 
}
