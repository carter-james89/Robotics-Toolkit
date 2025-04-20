using RoboticsToolkit.Robotics.Limbs;
using Toolkit.Utilities.Events;
using UnityEngine;

/// <summary>
/// Interface for components that position a robotic limb in 3D space, 
/// supporting both rotational and translational movement in global or local space.
/// </summary>
public interface ILimbPositioner : IEventSource<LimbPositionerEventData>
{
    /// <summary>
    /// Rotates the limb to a target global position and rotation over a specified time.
    /// </summary>
    /// <param name="globalPosition">Target position in world space.</param>
    /// <param name="rotationAxis">Target rotation (typically quaternion from forward/up).</param>
    /// <param name="time">Time in seconds to complete the rotation.</param>
    /// <param name="localSpace">If true, interprets position and rotation relative to parent transform.</param>
    void RotateToPosition(Vector3 globalPosition, Quaternion rotationAxis, float time, bool localSpace);

    /// <summary>
    /// Rotates the limb toward a direction and up vector, at a set distance over time.
    /// </summary>
    /// <param name="direction">Direction to face.</param>
    /// <param name="upDirection">Up vector to align with.</param>
    /// <param name="distance">Distance to move in that direction.</param>
    /// <param name="time">Time in seconds to complete the motion.</param>
    void RotateToPosition(Vector3 direction, Vector3 upDirection, float distance, float time);

    /// <summary>
    /// Translates the limb to a global position over time.
    /// </summary>
    /// <param name="globalPosition">Target position in world space.</param>
    /// <param name="time">Time in seconds to reach the position.</param>
    /// <param name="localSpace">If true, interprets position relative to the local parent.</param>
    void TranslateToPosition(Vector3 globalPosition, float time, bool localSpace);

    /// <summary>
    /// Translates the limb in a direction with an up vector and distance over time.
    /// </summary>
    /// <param name="direction">Direction to move in.</param>
    /// <param name="upDir">Up vector for reference orientation.</param>
    /// <param name="distance">Distance to move.</param>
    /// <param name="time">Time in seconds to complete movement.</param>
    void TranslateToPosition(Vector3 direction, Vector3 upDir, float distance, float time);

    /// <summary>
    /// Checks whether the limb has reached its current movement target.
    /// </summary>
    /// <returns>True if the limb is at the target.</returns>
    bool LimbAtTarget();

    /// <summary>
    /// Immediately sets the limb's position.
    /// </summary>
    /// <param name="globalPosition">The new position to apply.</param>
    /// <param name="localSpace">If true, applies the position relative to the local parent.</param>
    void SetLimbPosition(Vector3 globalPosition, bool localSpace);

    /// <summary>
    /// Retrieves the current limb position.
    /// </summary>
    /// <param name="localSpace">If true, returns the position relative to local space.</param>
    /// <returns>The current position of the limb.</returns>
    Vector3 GetLimbPosition(bool localSpace);

    /// <summary>
    /// Runs internal logic, such as advancing toward the target in Update/FixedUpdate.
    /// </summary>
    /// <returns>True if an update was performed; false otherwise.</returns>
    bool Run();
}

/// <summary>
/// Event data dispatched by ILimbPositioner sources.
/// </summary>
public class LimbPositionerEventData : IEventData
{
    public enum LimbPositionerEventType
    {
        /// <summary>
        /// Raised when the limb reaches its target position.
        /// </summary>
        OnAtTarget,

        /// <summary>
        /// Raised if a movement operation is interrupted or canceled.
        /// </summary>
        OnMovementCanceled
    }

    public LimbPositionerEventType EventType;
    public IRoboticLimb Limb;
    public Vector3 TargetPosition;

    public LimbPositionerEventData(LimbPositionerEventType eventType, IRoboticLimb limb, Vector3 targetPosition)
    {
        EventType = eventType;
        Limb = limb;
        TargetPosition = targetPosition;
    }
}
