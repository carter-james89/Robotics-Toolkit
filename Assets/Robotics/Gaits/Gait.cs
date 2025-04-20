using System.Runtime.InteropServices.WindowsRuntime;
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

        private bool m_running = false;

        protected int _currentStrideCount = 0;
        private InterfaceEventManager<GaitEventData> _eventManager = new InterfaceEventManager<GaitEventData>("Gait");

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
            return null;
        }

        public Component GetComponent()
        {
            return null;
        }
    } 
}
