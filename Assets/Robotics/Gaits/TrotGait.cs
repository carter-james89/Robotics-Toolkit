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

        public override void CheckLimbPositions(ILimbPositioner[] limbPositioners)
        {
            foreach (ILimbPositioner limb in limbPositioners)
            {
                if (!limb.LimbAtTarget())
                {
                    return;
                }
            }
            NotifyListeners(EventType.OnGaitPointHit);

            if (_currentStrideCount == 3)
            {
                NotifyListeners(EventType.OnGaitCycleComplete);
            }
        }


        public override void SetNextCycle(Vector3 direction, ILimbPositioner[] m_limbs, float speed, bool rotate)
        {
            Debug.Log("Set next crawl cycle : " + m_strideDistance);
            m_rotatingLimbs.Clear();
            m_translatingLimbs.Clear();
            var distance = m_strideDistance / 2;
            switch (_currentStrideCount)
            {
                case 0:
                    m_rotatingLimbs.Add(m_limbs[2]);//br
                    m_rotatingLimbs.Add(m_limbs[0]);
                    m_translatingLimbs.Add(m_limbs[1]);
                    m_translatingLimbs.Add(m_limbs[3]);
                    break;
                case 1:
                    m_rotatingLimbs.Add(m_limbs[1]);//fr
                    m_translatingLimbs.Add(m_limbs[0]);
                    m_translatingLimbs.Add(m_limbs[2]);
                    m_rotatingLimbs.Add(m_limbs[3]);
                    break;
            }
            foreach (var limb in m_rotatingLimbs)
            {
                (limb as AdvancedLimbPositioner).RotateToPosition(limb.GetGameObject().transform.position + direction * distance, speed, m_strideHeight);
            }
            foreach (var limb in m_translatingLimbs)
            {
                (limb as AdvancedLimbPositioner).TranslateToPosition(limb.GetGameObject().transform.position - direction * distance, speed);
            }

            _currentStrideCount++;
            if (_currentStrideCount > 2)
            {
                _currentStrideCount = 0;
            }
        }
    }
}
