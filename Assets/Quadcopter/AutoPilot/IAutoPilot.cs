using Toolkit.Utilities.Events;
using UnityEngine;

namespace  FlightControllers.Quadcopters
{
    public enum AutoPilotEventType
    {
        OnAutoPilotInitialized = 0,
        OnAutoPilotEngaged = 1,
        OnAutoPilotDisEngaged = 2,
    }
    public class AutoPilotEventData : IEventData
    {
        public AutoPilotEventType EventType;
        public IAutoPilot AutoPilot;

        public AutoPilotEventData(AutoPilotEventType eventType, IAutoPilot autoPilot)
        {
            EventType = eventType;
            AutoPilot = autoPilot;
        }
    }
    /// <summary>
    /// Interface for all Autopilot modules, designed to work with <see cref="IQuadcopter"/>
    /// </summary>
    /// <remarks>
    /// <see cref="AutoPilot"/> should be a valid solution for most autopilots, this exists if drastic changes are needed
    /// </remarks>
    public interface IAutoPilot : IInputSource, IEventSource<AutoPilotEventData>
    {
        /// <summary>
        /// Prepare the autopilot for activation
        /// </summary>
        /// <param name="quadToControl">The quadcopter this autopilot will control</param>
        public void Initialize(IQuadcopter quadToControl);


        /// <summary>
        /// Get the <see cref="IQuadcopter"/> this <see cref="IAutoPilot"/> is set to control
        /// </summary>
        /// <returns>The <see cref="IQuadcopter"/> being controllee</returns>
        public IQuadcopter GetQuadcopterToControl();

        /// <summary>
        /// Set the autopilot to its opposite state
        /// </summary>
        public void ToggleAutoPilot();

        /// <summary>
        /// Activate the autopilot
        /// </summary>
        public void ActivateAutoPilot();

        /// <summary>
        /// Deactivate the autopilot
        /// </summary>
        public void DeactivateAutoPilot();

        public void PositionAutoPilot(Vector3 globalPosition, Quaternion globalRotation);

        /// <summary>
        /// Is the autopilot currently active
        /// </summary>
        /// <returns>The state of the autopilot</returns>
        public bool IsActive();


    }

}