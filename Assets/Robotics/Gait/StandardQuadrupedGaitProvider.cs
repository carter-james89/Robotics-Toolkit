using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticToolkit.Robotics.Gaits
{
    public class StandardQuadrupedGaitProvider : MonoBehaviour, IGateProvider
    {
        private int m_stridePosition = 0;
        [SerializeField]
        private float m_strideLength = .05f;
        [SerializeField]
        private float m_strideHeight = .1f;
        [SerializeField]
        private float m_gaitTranslateSpeed = .1f;
        [SerializeField]
        private float m_mGaitRotationSpeed = 25;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="limbPositions">
        /// 0 = fl
        /// 1 = fr
        /// 2 = br
        /// 3 = bl
        /// </param>
        /// <param name="bodyPosition"></param>
        /// <param name="bodyRotatioon"></param>
        public void GetGaitTargets(IGait[] gaits, Vector3 bodyPosition, Quaternion bodyRotatioon)
        {
            var flLimb = gaits[0];
            var frLimb = gaits[1];
            var brLimb = gaits[2];
            var blLimb = gaits[3];

            m_stridePosition++;
            if (m_stridePosition == 3)
            {
                m_stridePosition = 1;
            }
            switch (m_stridePosition)
            {
                case 1:
                    frLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);
                    blLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);

                    flLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
                    brLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
                    break;
                case 2:
                    flLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);
                    brLimb.RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);

                    frLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
                    blLimb.TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
                    break;
                default:
                    break;
            }
        }
    }
}
