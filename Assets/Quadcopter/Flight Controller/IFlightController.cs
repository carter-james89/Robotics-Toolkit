using System;
using UnityEngine;
using Toolkit.Utilities;
using Toolkit.Utilities.Events;

namespace FlightControllers.Quadcopters
{
    public enum FlightControllerEventType
    {
        OnTakeOffBegin,
        OnTakeOffEnd,
        OnLandBegin,
        OnInitialized,
    }
    public class FlightControllerEventData : IEventData
    {
        public FlightControllerEventType EventType;
        public IFlightController FlightController;
        public FlightControllerEventData(FlightControllerEventType eventType, IFlightController flightController)
        {
            EventType = eventType;
            FlightController = flightController;
        }
    }
    /// <summary>
    /// Interface that defines the contract for all flight controllers (simulated or real) used by a quadcopter.
    /// </summary>
    public interface IFlightController : IMonobehaviorInterface, IEventSource<FlightControllerEventData>
    {
        /// <summary>
        /// Check if the flight controller has been initialized.
        /// </summary>
        bool IsInitialized();

        /// <summary>
        /// Check if the flight controller is ready to enter flight mode.
        /// </summary>
        bool IsReadyToFly();

        /// <summary>
        /// Initialize the flight controller with a reference to the quadcopter and a status callback.
        /// </summary>
        /// <param name="quadToControl">The quadcopter this controller will manage.</param>
        void Initialize(IQuadcopter quadToControl);

        /// <summary>
        /// Get the current orientation of the craft from the gyroscope.
        /// </summary>
        Quaternion GetGyroRotation();

        /// <summary>
        /// Get the current telemetry/sensor data for the quadcopter.
        /// </summary>
        QuadcopterData GetSensorData();

        /// <summary>
        /// Execute a flight update based on current status and input.
        /// </summary>
        /// <param name="flightStatus">The current state of the quadcopter.</param>
        /// <param name="craftInputs">Inputs for controlling the craft.</param>
        void Run(FlightStatus flightStatus, IInputSource.FlightControlValues craftInputs);

        /// <summary>
        /// Returns true if the controller represents a simulated quadcopter.
        /// </summary>
        bool IsSimulator();

        /// <summary>
        /// Command the quadcopter to take off.
        /// </summary>
        bool AttemptTakeoff();

        /// <summary>
        /// Command the quadcopter to land.
        /// </summary>
        bool AttemptLand();
    }
}
