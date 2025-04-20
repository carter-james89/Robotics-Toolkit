using System;
using Toolkit.Utilities.Events;
using UnityEngine;

namespace  FlightControllers.Quadcopters
{
    /// <summary>
    /// Base class which provides functionality for most autopilots
    /// </summary>
    public abstract class AutoPilot : MonoBehaviour, IAutoPilot
    {
        /// <summary>
        /// The <see cref="IQuadcopter"/> this autopilot will manipulate
        /// </summary>
        protected IQuadcopter quadToControl { get; private set; }

        /// <summary>
        /// Is the autopilot currently active?
        /// </summary>
        private bool autoPilotActive = false;

        private InterfaceEventManager<AutoPilotEventData> _eventListeners = new InterfaceEventManager<AutoPilotEventData>();
        private void NotifyEventListners(AutoPilotEventType eventType)
        {
            _eventListeners.RaiseEvent(new AutoPilotEventData(eventType, this));
        }

        private void Awake()
        {
           // DeactivateAutoPilot();
        }

        /// <summary>
        /// Prepare the autopilot for activation
        /// </summary>
        /// <param name="quadToControl">The quadcopter for this Autopilot to control</param>
        public void Initialize(IQuadcopter quadToControl)
        {
            if (quadToControl == null)
            {
                Debug.LogError("Provided IQuadcopter was null");
                return;
            }
            this.quadToControl = quadToControl;
            OnInitialized();
            NotifyEventListners(AutoPilotEventType.OnAutoPilotInitialized);

        }
        protected virtual void OnInitialized()
        {
            // This is called after the autopilot has been initialized
            // Override this method to add functionality
        }

        public IQuadcopter GetQuadcopterToControl()
        {
            return this.quadToControl;
        }

        /// <summary>
        /// Get the <see cref="IInputSource.FlightControlValues"/> from <see cref="Run"/>
        /// </summary>
        /// <returns>The inputs that will maniuplate the Quad in the desired manner</returns>
        public IInputSource.FlightControlValues GetInputValues()
        {
            var returnValues = Run();
            returnValues.land = false;
            returnValues.takeOff = false;
            returnValues.toggleAutoPilot = false;
            return returnValues;
        }

        /// <summary>
        /// The calculations used to manipulate <see cref="quadToControl"/> in the desired way
        /// </summary>
        /// <returns>The inputs that will maniuplate the Quad in the desired manner</returns>
        public abstract IInputSource.FlightControlValues Run();

        /// <summary>
        /// Toggle the autopilot to the opposite state that it currently is
        /// </summary>
        public virtual void ToggleAutoPilot()
        {
            Debug.Log("Toggle Autopilot : " + !autoPilotActive);
            if (autoPilotActive)
            {
                DeactivateAutoPilot();
            }
            else
            {
                ActivateAutoPilot();
            }
        }

        /// <summary>
        /// Activated the autopilot, <see cref="quadToControl"/> input source will be changed to <see cref="GetInputValues"/>
        /// </summary>
        public void ActivateAutoPilot()
        {
            if (!autoPilotActive)
            {
                autoPilotActive = true;
                gameObject.SetActive(true);
                MatchQuadTransform();
                quadToControl.OverrideInputSource(this);
                OnAutoPilotActivated();
                NotifyEventListners(AutoPilotEventType.OnAutoPilotEngaged);
            }
        }
        /// <summary>
        /// Called when autopilot has become active
        /// </summary>
        protected abstract void OnAutoPilotActivated();

        /// <summary>
        /// Deactivate the autopilot, <see cref="quadToControl"/> input values for <see cref="quadToControl"/> will be returned to default
        /// </summary>
        public void DeactivateAutoPilot()
        {
            if (autoPilotActive)
            {
                //Debug.Log("AutoPilot Disabled");
                autoPilotActive = false;
                quadToControl.RemoveInputOverride(this);
                OnAutoPilotDeactivated();
                NotifyEventListners(AutoPilotEventType.OnAutoPilotDisEngaged);
            }
            gameObject.SetActive(false);
        }
        /// <summary>
        /// Called when autopilot has been deactivated
        /// </summary>
        protected abstract void OnAutoPilotDeactivated();
        /// <summary>
        /// Get the <see cref="GameObject"/> this component belongs to
        /// </summary>
        /// <returns></returns>
        public GameObject GetGameObject()
        {
            return gameObject;
        }
        /// <summary>
        /// Is the autopilot currently active
        /// </summary>
        /// <returns>The state of the autopilot</returns> /// <summary>
        public bool IsActive()
        {
            return autoPilotActive;
        }

        /// <summary>
        /// Maniplate this objects <see cref="Transform"/> to match <see cref="quadToControl"/>
        /// </summary>
        protected void MatchQuadTransform()
        {
            Debug.Log("Move Autopilot to transfrom to : " + quadToControl.GetGameObject().name);
            transform.position = quadToControl.GetGameObject().transform.position;
            SetAutoPilotRot(quadToControl.GetGameObject().transform.rotation);
        }

        /// <summary>
        /// Set the rotaion of this objects <see cref="Transform"/> to match the provided rotation, global X and global y will be nullified
        /// </summary>
        /// <param name="newRot">The new rotation of this transform</param>
        public void SetAutoPilotRot(Quaternion newRot)
        {
            var tempEuler = newRot.eulerAngles;
            tempEuler.x = 0;
            tempEuler.z = 0;
            transform.rotation = Quaternion.Euler(tempEuler);
        }

        private void OnDestroy()
        {
            DeactivateAutoPilot();
        }
        
        public void Abort()
        {
            DeactivateAutoPilot();
        }

        public void SubscribeToEvents(IEventListener<AutoPilotEventData> listenerToSubscribe)
        {
         _eventListeners.AddListener(listenerToSubscribe);
        }

        public void UnsubscribeFromEvents(IEventListener<AutoPilotEventData> listenerToUnsubscribe)
        {
          _eventListeners.RemoveListener(listenerToUnsubscribe);
        }

        public Component GetComponent()
        {
            return this;
        }

        public void PositionAutoPilot(Vector3 globalPosition, Quaternion globalRotation)
        {
            transform.position = globalPosition;
            transform.rotation = globalRotation;
        }
    }
}
