using RoboticToolkit.Robotics.Limbs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toolkit.Robotics.Quadruped
{
    [Serializable]
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

        public QuadrupedLimbData() { }

        public QuadrupedLimbData(QuadrupedData data)
        {
            FLBaseAngle = data.FLBaseAngle;
            FLHipAngle = data.FLHipAngle;
            FLKneeAngle = data.FLKneeAngle;

            FRBaseAngle = data.FRBaseAngle;
            FRHipAngle = data.FRHipAngle;
            FRKneeAngle = data.FRKneeAngle;

            BRBaseAngle = data.BRBaseAngle;
            BRHipAngle = data.BRHipAngle;
            BRKneeAngle = data.BRKneeAngle;

            BLBaseAngle = data.BLBaseAngle;
            BLHipAngle = data.BLHipAngle;
            BLKneeAngle = data.BLKneeAngle;
        }

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

        [SerializeField]
        private int _startupDelay = 100;

        protected bool _isRunning = false;

        [SerializeField]
        private GameObject _controllerObject;
        private IQuadrupedRoboticController _controller;

        protected virtual void Awake()
        {
   
        }
        protected virtual void Start()
        {
            if (SimulationMode())
            {
                var hipAngle = 80;
                var kneeAngle = -160;
                SetLimbs(new QuadrupedLimbData(0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle, 0, hipAngle, kneeAngle));
            }
        }
        public void Bootup()
        {
            Debug.Log("Quadruped Bootup");



          //  _controllerObject.GetComponent<IQuadrupedRoboticController>().Initialize(this);
            _controller = _controllerObject.GetComponent<IQuadrupedRoboticController>();
            _controller.Initialize(this);
            _isRunning = true;
        }
        protected virtual void Update()
        {
            Run();
        }

        protected void SetLimbs(QuadrupedLimbData limbData)
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
            if (m_limbs == null)
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
                if (SimulationMode() && Time.frameCount > _startupDelay)
                {
                    Bootup();
                }
                return;
            }
            PositionTransform();
            PositionLimbs();
        }
        protected virtual void PositionTransform()
        {

        }
        protected virtual void PositionLimbs()
        {
            var limbData = _controller.CalculateLimbData(this);

            if (SimulationMode())
            {
                SetLimbs(limbData);
            }

            OnLimbsPositioned(limbData);
        }

        protected virtual void OnLimbsPositioned(QuadrupedLimbData limbData)
        {

        }


    }

}