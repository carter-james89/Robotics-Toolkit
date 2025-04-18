using System.Collections;
using System.Collections.Generic;
using Toolkit.Utilities.Events;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    public abstract class QuadcopterFlightController : MonoBehaviour, IFlightController
    {
        private InterfaceEventManager<FlightControllerEventData> _eventManager = new InterfaceEventManager<FlightControllerEventData>();
        /// <summary>
          protected QuadcopterData _quadcopterData = new QuadcopterData();

        protected bool _isInitialized = false;

        protected IQuadcopter quadToControl;

        public bool IsInitialized()
        {
            return _isInitialized;
        }

        public void Initialize(IQuadcopter quadToControl)
        {
            if (_isInitialized)
            {
                return;
            }

         this.quadToControl = quadToControl;
            _isInitialized = true;
            OnInitialized();

            NotifyListeners(FlightControllerEventType.OnInitialized);
        }
        protected virtual void OnInitialized() { }

        public abstract bool IsReadyToFly();


        public abstract Quaternion GetGyroRotation();

        public abstract QuadcopterData GetSensorData();

        public abstract void Run(FlightStatus flightStatus, IInputSource.FlightControlValues craftInputs);

        public abstract bool IsSimulator();


        public abstract bool AttemptTakeoff();


        public abstract bool AttemptLand();


        public void SubscribeToEvents(IEventListener<FlightControllerEventData> listenerToSubscribe)
        {
            _eventManager.AddListener(listenerToSubscribe);
        }

        public void UnsubscribeFromEvents(IEventListener<FlightControllerEventData> listenerToUnsubscribe)
        {
            _eventManager.AddListener(listenerToUnsubscribe);
        }

        public GameObject GetGameObject()
        {
           if(this == null)
            {
                return null;
            }
           return gameObject;
        }

        public Component GetComponent()
        {
            return this;
        }

        protected void NotifyListeners(FlightControllerEventType eventType)
        {
            _eventManager.RaiseEvent(new FlightControllerEventData(eventType, this));
        }

  
    }

}