using ProcessCommunicationToolkit;
using System;
using UnityEngine;
using Toolkit.Utilities.Events;
using Toolkit.Utilities;

namespace FlightControllers.Quadcopters
{
    /// <summary>
    /// Event data class for quadcopter-related events.
    /// </summary>
    public class QuadcopterEventData : IEventData
    {
        public enum QuadcopterEventType
        {
            None,
            TakeOff,
            Land,
            BatteryLow,
            EmergencyStop
        }

        public QuadcopterEventType EventType { get; private set; }

        public QuadcopterEventData(QuadcopterEventType eventType)
        {
            EventType = eventType;
        }
    }

    /// <summary>
    /// Real-time telemetry data for a quadcopter.
    /// Implements IUplinkData for communication.
    /// </summary>
    public class QuadcopterData : IEventData, IUplinkData
    {
        public double throttle, yaw, pitch, roll;
        public float gyroYaw, gyroRoll, gyroPitch;
        public float posX, posY, posZ;
        public float height;
        public Vector3 VelocityVector;

        public Enum GetEventType() => null; // Placeholder if needed by event routing
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
    public interface IQuadcopter : IMonobehaviorInterface, IEventSource<QuadcopterData>
    {
        /// <summary>
        /// Subscribe a callback for when the quadcopter must abort mission.
        /// </summary>
        void SubscibeToAbort(Action actionToSubscribe);

        /// <summary>
        /// Unsubscribe an abort callback.
        /// </summary>
        void UnsubscribeFromAbort(Action actionToUnsubscribe);

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
        void Takeoff();

        /// <summary>
        /// Initiate landing.
        /// </summary>
        void Land();
    }
}
