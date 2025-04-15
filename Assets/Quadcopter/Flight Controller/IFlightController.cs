using System;
using UnityEngine;
using Toolkit.Utilities;

namespace FlightControllers.Quadcopters
{
    public interface IFlightController : IMonobehaviorInterface
    {
        public bool IsInitialized();

        public bool IsReadyToFly();

        public void Initialize(IQuadcopter quadToControl, Action<FlightStatus> onFlightStatusChanged);

        public Quaternion GetGyroRotation();

        public QuadcopterData GetSensorData();

        public void Run(FlightStatus flightStatus, IInputs.FlightControlValues craftInputs);

        public bool IsSimulator();

        public void Takeoff();
        public void Land();
    }

}