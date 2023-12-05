using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
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
        public void Initialize(IRoboticLimb[] limbs);
        public void Run(IRoboticLimb[] mirrorLimbs, ILimbPositioner[] limbs);


        public void PerformHighStep(ILimbPositioner[] limbs, float height, float speed);

        public void BeginMovement(ILimbPositioner[] limbs, IGaitController.GaitPattern patern, Vector3 direction, bool rotate);

        public void SetGaitPattern(GaitPattern type);
        public GaitPattern GetGaitPattern();
        public Direction GetDirection();
        public void SetDirection(Direction direction);

        public bool IsRunning();
    } 
}