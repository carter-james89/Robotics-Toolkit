using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public class CrawlGait : Gait
    {
        // private int m_stridePosition = 0;


        public override void CheckLimbPositions(ILimbPositioner[] limbPositioners)
        {
            if (m_rotatingLimbs[0].LimbAtTarget())
            {
                NotifyListeners(EventType.OnGaitPointHit);

                if(_currentStrideCount == 3)
                {
                    NotifyListeners(EventType.OnGaitCycleComplete);
                }
            }
        }


        public override void SetNextCycle(Vector3 direction, ILimbPositioner[] m_limbs, float speed, bool rotate)
        {
            Debug.Log("Set next crawl cycle : " + m_strideDistance);
            m_rotatingLimbs.Clear();
            m_translatingLimbs.Clear();
            var distance = m_strideDistance/2;
            switch (_currentStrideCount)
            {
                case 3:
                    m_rotatingLimbs.Add(m_limbs[0]);//fl
                    m_translatingLimbs.Add(m_limbs[1]);
                    m_translatingLimbs.Add(m_limbs[2]);
                    m_translatingLimbs.Add(m_limbs[3]);
                    break;
                case 1:
                    m_rotatingLimbs.Add(m_limbs[1]);//fr
                    m_translatingLimbs.Add(m_limbs[0]);
                    m_translatingLimbs.Add(m_limbs[2]);
                    m_translatingLimbs.Add(m_limbs[3]);
                    break;
                case 0:
                    m_rotatingLimbs.Add(m_limbs[2]);//br
                    m_translatingLimbs.Add(m_limbs[0]);
                    m_translatingLimbs.Add(m_limbs[1]);             
                    m_translatingLimbs.Add(m_limbs[3]);
                    break;
                case 2:
                    m_rotatingLimbs.Add(m_limbs[3]);//bl
                    m_translatingLimbs.Add(m_limbs[0]);
                    m_translatingLimbs.Add(m_limbs[1]);
                    m_translatingLimbs.Add(m_limbs[2]);
                  
                    break;

            }
            Debug.Log("Rotating Limb : " + m_rotatingLimbs[0].GetGameObject().name);
            (m_rotatingLimbs[0] as AdvancedLimbPositioner).RotateToPosition(m_rotatingLimbs[0].GetGameObject().transform.position + direction.normalized * distance, speed*10, m_strideHeight);

            foreach (var limb in m_translatingLimbs)
            {
              
                (limb as AdvancedLimbPositioner).TranslateToPosition(limb.GetGameObject().transform.position - direction * (distance), speed);
            }
            //(m_rotatingLimbs[1] as AdvancedLimbPositioner).RotateToPosition(m_rotatingLimbs[1].GetGameObject().transform.position + direction * distance, .1f, m_strideHeight);



            //float distance = m_strideDistance;
            //if (_currentStrideCount == 0)
            //{
            //    distance /= 2;
            //}
            //switch (m_stridePosition)
            //{
            //    case 0:
            //        m_rotatingLimbs[0] = m_limbs[0];
            //        m_rotatingLimbs[1] = m_limbs[2];
            //        m_translatingLimbs[0] = m_limbs[1];
            //        m_translatingLimbs[1] = m_limbs[3];
            //        break;
            //    case 1:
            //        m_rotatingLimbs[0] = m_limbs[1];
            //        m_rotatingLimbs[1] = m_limbs[3];
            //        m_translatingLimbs[0] = m_limbs[0];
            //        m_translatingLimbs[1] = m_limbs[2];
            //        break;
            //    default:
            //        break;
            //}

            //if (!rotate)
            //{
            //    foreach (var limb in m_rotatingLimbs)
            //    {
            //        (limb as AdvancedLimbPositioner).RotateToPosition(limb.GetGameObject().transform.position + direction * distance, .1f, m_strideHeight);
            //    }
            //    foreach (var limb in m_translatingLimbs)
            //    {
            //        (limb as AdvancedLimbPositioner).TranslateToPosition(limb.GetGameObject().transform.position - direction * distance, .1f);
            //    }
            //}
            //else
            //{
            //    (m_rotatingLimbs[0] as AdvancedLimbPositioner).RotateToPosition(m_rotatingLimbs[0].GetGameObject().transform.position - direction * distance, .1f, m_strideHeight);
            //    (m_rotatingLimbs[1] as AdvancedLimbPositioner).RotateToPosition(m_rotatingLimbs[1].GetGameObject().transform.position + direction * distance, .1f, m_strideHeight);

            //    (m_translatingLimbs[0] as AdvancedLimbPositioner).TranslateToPosition(m_rotatingLimbs[0].GetGameObject().transform.position + direction * distance, .1f);
            //    (m_translatingLimbs[1] as AdvancedLimbPositioner).TranslateToPosition(m_rotatingLimbs[1].GetGameObject().transform.position - direction * distance, .1f);
            //}

            //m_stridePosition++;
            //if (m_stridePosition == 2)
            //{
            //    m_stridePosition = 0;
            //}
          
            _currentStrideCount++;
            if (_currentStrideCount > 3)
            {
                _currentStrideCount = 0;
            }
        }
    }
}
