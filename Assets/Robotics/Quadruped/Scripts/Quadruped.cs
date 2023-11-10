using RoboticToolkit.Robotics.Limbs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toolkit.Robotics.Quadruped
{
    public class QuadrupedLimbData
    {
        public float FLBaseAngle;
        public float FLHipAngle;
        public float FLKneeAngle;

        public float FRBaseAngle;
        public float FRHipAngle;
        public float FRKneeAngle;

        public float BRBaseAngle;
        public float BRHipAngle;
        public float BRKneeAngle;

        public float BLBaseAngle;
        public float BLHipAngle;
        public float BLKneeAngle;

        public QuadrupedLimbData(float flBaseAngle, float flHipAngle, float flKneeAngle,
                              float frBaseAngle, float frHipAngle, float frKneeAngle,
                              float brBaseAngle, float brHipAngle, float brKneeAngle,
                              float blBaseAngle, float blHipAngle, float blKneeAngle)
        {
            FLBaseAngle = flBaseAngle;
            FLHipAngle = flHipAngle;
            FLKneeAngle = flKneeAngle;

            FRBaseAngle = frBaseAngle;
            FRHipAngle = frHipAngle;
            FRKneeAngle = frKneeAngle;

            BRBaseAngle = brBaseAngle;
            BRHipAngle = brHipAngle;
            BRKneeAngle = brKneeAngle;

            BLBaseAngle = blBaseAngle;
            BLHipAngle = blHipAngle;
            BLKneeAngle = blKneeAngle;
        }
    }

    public class Quadruped : MonoBehaviour, IQuadruped
    {
        [SerializeField]
        private bool _simulationMode = false;
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

        protected bool _isRunning = false;

        [SerializeField]
        private GameObject _controllerObject;
        private IQuadrupedRoboticController _controller;

        protected virtual void Awake()
        {
            
        }
        protected virtual void Start()
        {
            //if(GetComponent<ArticulationBody>().)
            Bootup();
        }
        public void Bootup()
        {
            Debug.Log("Quadruped Bootup");

            if (SimulationMode())
            {
                var hipAngle = 80;
                var kneeAngle = -160;
                SetLimbs(new QuadrupedLimbData(0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle));
            }
        
            _controllerObject.GetComponent<IQuadrupedRoboticController>().Initialize(this);
            _controller = _controllerObject.GetComponent<IQuadrupedRoboticController>();
            _isRunning = true;
        }
        protected virtual void Update()
        {
            Run();
        }

        private void SetLimbs(QuadrupedLimbData limbData)
        {
            m_frLimb.SetLimbValues(limbData.FRBaseAngle, limbData.FRHipAngle, limbData.FRKneeAngle);
            m_flLimb.SetLimbValues(limbData.FLBaseAngle, limbData.FLHipAngle, limbData.FLKneeAngle);
            m_brLimb.SetLimbValues(limbData.BRBaseAngle, limbData.BRHipAngle, limbData.BRKneeAngle);
            m_blLimb.SetLimbValues(limbData.BLBaseAngle, limbData.BLHipAngle, limbData.BLKneeAngle);
        }
      

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public IRoboticLimb[] GetLimbs()
        {
            if(m_limbs == null)
            {
                m_limbs = new IRoboticLimb[4] { m_flLimb, m_frLimb, m_brLimb, m_blLimb };
            }
            return m_limbs;
        }

     

        public bool SimulationMode()
        {
            return _simulationMode;
        }

        public void Run()
        {
            if (!_isRunning)
            {
                return;
            }
            PositionTransform();
            PositionLimbs();
        }
        protected virtual void PositionTransform()
        {

        }
        private void PositionLimbs()
        {
            var limbData = _controller.CalculateLimbData(this);
        }

      
    }

}