
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    public class MLAutoPilot : AutoPilot
    {
        [SerializeField]
        private AutoPilotMLAgent _autoPilotAgent;


        private Vector3 _bounds = new Vector3(1, 1, 1);

        private void Awake()
        {
            //  _autoPilotAgent.Initialize(this);

        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _autoPilotAgent.Initialize(this);

            if (_autoPilotAgent.IsTraining()) 
            {
               // _quadcopterToControl.transform.localPosition = new Vector3(0, 0.5f, 0);
           
                _autoPilotAgent.OnEpisodeBeginEvent.AddListener(OnEpisodeBegin);
                _autoPilotAgent.OnEpisodeCompleteEvent.AddListener(OnEpisodeComplete);
            }

        }

        private void OnEpisodeBegin()
        {
            Debug.Log("Got Episode Begin"); 
            quadToControl.AttemptTakeoff();
            ActivateAutoPilot();
            _autoPilotAgent.SetNewTarget(transform);
        }

        private void OnEpisodeComplete()
        {
            Debug.Log("Got Episode End");
            DeactivateAutoPilot();
            quadToControl.AttemptLand();
            quadToControl.GetGameObject().transform.localPosition = Vector3.zero;
        }

        public override IInputSource.FlightControlValues Run()
        {
            //Debug.Log("run auto pilot");
            return _autoPilotAgent.GetFlightControlValues(this);
        }

        protected override void OnAutoPilotActivated()
        {

        }

        protected override void OnAutoPilotDeactivated()
        {

        }

    
    }

}