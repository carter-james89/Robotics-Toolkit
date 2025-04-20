using RoboticsToolkit.Gimbal;
using RoboticsToolkit.Robotics.Limbs;
using Toolkit.Utilities;

namespace RoboticsToolkit.Robotics.Gaits
{
    /// <summary>
    /// Specifies the type of gait the robot will use.
    /// </summary>
    public enum GaitType
    {
        /// <summary>
        /// Move one leg at a time.
        /// </summary>
        Crawl,

        /// <summary>
        /// Move two legs at a time.
        /// </summary>
        Trot,
    }

    /// <summary>
    /// Interface for implementing robotic gait controllers.
    /// Controls gait type, direction, and execution.
    /// </summary>
    public interface IGaitController : IMonobehaviorInterface
    {
        /// <summary>
        /// Direction the gait should move in.
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// No movement.
            /// </summary>
            NONE,

            /// <summary>
            /// Move forward.
            /// </summary>
            FORWARD,

            /// <summary>
            /// Move backward.
            /// </summary>
            BACKWARD,

            /// <summary>
            /// Move left.
            /// </summary>
            LEFT,

            /// <summary>
            /// Move right.
            /// </summary>
            RIGHT,
        }

        /// <summary>
        /// Pattern or mode of the gait.
        /// </summary>
        public enum GaitPattern
        {
            /// <summary>
            /// No active gait pattern.
            /// </summary>
            NONE,

            /// <summary>
            /// Legs are moving to a default "home" position.
            /// </summary>
            RETURNING_HOME,

            /// <summary>
            /// Stationary stepping without translation.
            /// </summary>
            STATIONARYSTEP,

            /// <summary>
            /// Crawl gait pattern.
            /// </summary>
            CRAWL,

            /// <summary>
            /// Trot gait pattern.
            /// </summary>
            TROT
        }

        /// <summary>
        /// Initializes the gait controller with limb positioners, robotic limbs, and a gimbal reference.
        /// </summary>
        /// <param name="limbPositioners">Array of limb positioners for coordinating leg movement.</param>
        /// <param name="puppetLimbs">Array of robotic limbs to be controlled.</param>
        /// <param name="gimbal">Gimbal interface for body orientation stabilization.</param>
        public void Initialize(ILimbPositioner[] limbPositioners, IRoboticLimb[] puppetLimbs, IGimbal gimbal);

        /// <summary>
        /// Executes the current gait pattern. Should be called per frame or at a fixed rate.
        /// </summary>
        public void Run();

        /// <summary>
        /// Performs a high step motion for a given gait type.
        /// </summary>
        /// <param name="type">The gait type (e.g., Crawl or Trot).</param>
        /// <param name="height">The height of the step in units.</param>
        /// <param name="speed">The speed of the stepping motion.</param>
        public void PerformHighStep(GaitType type, float height, float speed);

        /// <summary>
        /// Sets the current gait pattern.
        /// </summary>
        /// <param name="type">The gait pattern to activate.</param>
        public void SetGaitPattern(GaitPattern type);

        /// <summary>
        /// Gets the current gait pattern.
        /// </summary>
        /// <returns>The currently active gait pattern.</returns>
        public GaitPattern GetGaitPattern();

        /// <summary>
        /// Gets the direction the gait is currently moving in.
        /// </summary>
        /// <returns>The current movement direction.</returns>
        public Direction GetDirection();

        /// <summary>
        /// Sets the direction for the gait to move in.
        /// </summary>
        /// <param name="direction">Desired movement direction.</param>
        public void SetDirection(Direction direction);

        /// <summary>
        /// Returns whether the gait is actively running.
        /// </summary>
        /// <returns>True if the gait is running, false otherwise.</returns>
        public bool IsRunning();
    }
}
