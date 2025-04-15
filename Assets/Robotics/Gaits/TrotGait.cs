using RoboticsToolkit.Gimbal;
using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public class TrotGait : Gait
    {
        private int m_stridePosition = 0;

        public override bool CheckLimbPositions(ILimbPositioner[] limbPositioners)
        {
            foreach (ILimbPositioner limb in limbPositioners)
            {
                if (!limb.LimbAtTarget())
                {
                    return false;
                }
            }
            NotifyListeners(GaitEventType.OnGaitPointHit);

            if (_currentStrideCount == 3)
            {
                NotifyListeners(GaitEventType.OnGaitCycleComplete);
            }
            return true;
        }

        public override GaitCycleInfo GetGaitCycleInfo()
        {
            throw new System.NotImplementedException();
        }

        public override float GetRotationSpeedMultiplier()
        {
            throw new System.NotImplementedException();
        }

        public override void Translate(ILimbPositioner[] limbPositioners, float speed, float strideLength,float strideHeight)
        {
            throw new System.NotImplementedException();
        }
    }
}
