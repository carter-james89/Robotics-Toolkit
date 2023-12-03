using RoboticsToolkit.Robotics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public class QuadrupedCrawlGait : MonoBehaviour, IGait
    {
        private List<ILimbPositioner> m_limbPositioner = new List<ILimbPositioner>();

        private ILimbPositioner m_placingPositioner;
        public void Begin()
        {
            m_placingPositioner = m_limbPositioner[0];
        }

        public void Initialize(IRobot robot)
        {
            var limbs = robot.GetLimbs();
            foreach (var limb in limbs)
            {
                m_limbPositioner.Add(limb.GetPositioner());
            }

        }

        public bool IsRunning()
        {
            return m_limbPositioner != null;
        }

        public void ReturnHome()
        {
            throw new System.NotImplementedException();
        }

        public void RunGait()
        {
           
        }

        public void SetNextCycle()
        {
            throw new System.NotImplementedException();
        }

        public void SetNextCycle(Vector3 direction, ILimbPositioner[] limbPositioners, bool rotate)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public void SubscribeToEvents(IGaitEventListener listener)
        {
            throw new System.NotImplementedException();
        }

        public void UnubscribeFromEvents(IGaitEventListener listener)
        {
            throw new System.NotImplementedException();
        }
    }
}
