using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticToolkit.Robotics.Gaits
{
    public interface IGateProvider 
    {
        public void Initialize(IRoboticLimb[] limbs, IGait[] gaits);
        public void GetGaitTargets(Vector3 bodyPosition, Quaternion bodyRotatioon);
    }
}