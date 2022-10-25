using RoboticsToolkit.Robotics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticToolkit.Robotics.Gaits
{
    public interface IGaitControllerEventListener
    {
        public void OnLimbAchievedTarget(Vector3 currentTarget);
    }
    public interface IGaitController
    {
        public enum Direction
        {
            NONE,
            FORWARD,
            BACKWARD,
            LEFT,
            RIGHT,
        }
        public enum GaitPattern
        {
            NONE,
            RETURNING_HOME,
            STATIONARYSTEP,
            CRAWL,
            TROT
        }
        public void Initialize(IRobot robot);
        public void Run();

        public void SetGaitPattern(GaitPattern type);
        public GaitPattern GetGaitPattern();
        public Direction GetDirection();
        public void SetDirection(Direction direction);

        public bool IsRunning();
    } 
}