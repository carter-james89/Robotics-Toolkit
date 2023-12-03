using RoboticsToolkit.Robotics.Limbs;
using UnityEngine;


namespace RoboticsToolkit.Robotics.QuadrupedRobot
{
    public interface IQuadruped
    {
        public void Bootup();
        public GameObject GetGameObject();
        public IRoboticLimb[] GetLimbs();

        public bool SimulationMode();

        public void Run();
    }

}