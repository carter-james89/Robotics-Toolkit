using System;
using UnityEngine;
using Toolkit.Utilities;

namespace FlightControllers.Quadcopters
{
    /// <summary>
    /// Interface that defines the contract for all flight controllers (simulated or real) used by a quadcopter.
    /// </summary>
    public interface IFlightController : IMonobehaviorInterface
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
        /// <param name="onFlightStatusChanged">Callback to notify the quadcopter of status changes.</param>
        void Initialize(IQuadcopter quadToControl, Action<FlightStatus> onFlightStatusChanged);

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
        void Takeoff();

        /// <summary>
        /// Command the quadcopter to land.
        /// </summary>
        void Land();
    }
}
