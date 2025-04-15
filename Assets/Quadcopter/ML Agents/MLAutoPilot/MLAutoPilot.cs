
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

        private void Awake()
        {
            //  _autoPilotAgent.Initialize(this);
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