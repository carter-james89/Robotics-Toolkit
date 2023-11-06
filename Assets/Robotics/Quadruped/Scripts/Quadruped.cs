using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toolkit.Robotics.Quadruped
{
    public abstract class Quadruped : MonoBehaviour, IQuadruped
    {
        [SerializeField]
        private IRoboticLimb[] m_limbs;

        [SerializeField]
        private QuadrupedLeg m_frLimb;
        [SerializeField]
        private QuadrupedLeg m_flLimb;
        [SerializeField]
        private QuadrupedLeg m_brLimb;
        [SerializeField]
        private QuadrupedLeg m_blLimb;

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public IRoboticLimb[] GetLimbs()
        {
           return m_limbs;
        }

        public abstract void PositionTransform();
    }

}