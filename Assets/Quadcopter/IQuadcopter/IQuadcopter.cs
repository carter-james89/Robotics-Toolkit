using ProcessCommunicationToolkit;
using System;
using UnityEngine;
using Toolkit.Utilities.Events;
using Toolkit.Utilities;

namespace FlightControllers.Quadcopters
{
    public enum QuadcopterEventType
    {
        None,
        TakeOff,
        Land,
        BatteryLow,
        EmergencyStop
    }
    /// <summary>
    /// Event data class for quadcopter-related events.
    /// </summary>
    public class QuadcopterEventData : IEventData
    {
        public IQuadcopter Quadcopter { get; private set; }
        public QuadcopterEventType EventType { get; private set; }

        public QuadcopterData QuadcopterData { get; private set; }

        public QuadcopterEventData(QuadcopterEventType eventType, IQuadcopter quadcopter, QuadcopterData quadcopterData)
        {
            EventType = eventType;
            Quadcopter = quadcopter;
            QuadcopterData = quadcopterData;
        }
    }

    /// <summary>
    /// Real-time telemetry data for a quadcopter.
    /// Implements IUplinkData for communication.
    /// </summary>
    public class QuadcopterData : IUplinkData
    {
        public double throttle, yaw, pitch, roll;
        public float gyroYaw, gyroRoll, gyroPitch;
        public float posX, posY, posZ;
        public float height;
        public Vector3 AngularVelocityVector;
        public Vector3 LinearVelocityVector;
    }

    /// <summary>
    /// Ground station input data to be sent to the quad.
    /// </summary>
    public class GroundStationData
    {
        public double throttle = 0.0, yaw = 0.0, pitch = 0.0, roll = 0.0;
        public double motorFRSpeed = 0.0, motorFLSpeed = 0.0, motorBRSpeed = 0.0, motorBLSpeed = 0.0;
    }

    /// <summary>
    /// Current flight state of the quadcopter.
    /// </summary>
    public enum FlightStatus
    {
        PreLaunch,
        PrimingProps,
        Launching,
        Flying,
        Landing
    }

    /// <summary>
    /// Interface for all quadcopters, real or simulated.
    /// Provides control, telemetry, and event handling functionality.
    /// </summary>
    public interface IQuadcopter : IMonobehaviorInterface, IEventSource<QuadcopterEventData>, IEventListener<FlightControllerEventData>
    {
        /// <summary>
        /// Initialize autopilot and set dependencies.
        /// </summary>
        void Initialize(IFlightController flightController, IInputSource defaultInputSource);

        /// <summary>
        /// Override the input source with a new one.
        /// </summary>
        void OverrideInputSource(IInputSource inputValueSource);

        /// <summary>
        /// Remove the input override and return to default.
        /// </summary>
        void RemoveInputOverride(IInputSource inputValueSource);

        /// <summary>
        /// Get the tracking space transform local to the quad.
        /// </summary>
        Transform GetLocalTrackingSpace();

        /// <summary>
        /// Set the quadcopter's return-to-home location.
        /// </summary>
        void SetHomePoint(Vector3 newHomePoint);

        /// <summary>
        /// Whether this is a simulator or a real vehicle.
        /// </summary>
        bool IsSimulator();

        /// <summary>
        /// Get the current flight status.
        /// </summary>
        FlightStatus GetFlightStatus();

        /// <summary>
        /// Get sensor data for the current frame.
        /// </summary>
        QuadcopterData GetSensorData();

        /// <summary>
        /// Determine if tracking systems are valid.
        /// </summary>
        bool IsTracking();

        /// <summary>
        /// Convert inputs into headless mode.
        /// </summary>
        IInputSource.FlightControlValues ConvertToHeadlessInputs(IInputSource.FlightControlValues rawInputs);

        /// <summary>
        /// Initiate takeoff.
        /// </summary>
        bool AttemptTakeoff();

        /// <summary>
        /// Initiate landing.
        /// </summary>
        bool AttemptLand();
    }
}
