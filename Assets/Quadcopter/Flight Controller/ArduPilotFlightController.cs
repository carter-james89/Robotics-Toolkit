using ProcessCommunicationToolkit.UDP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    public class ArduPilotFlightController : MonoBehaviour, IFlightController
    {
        public Quaternion GetGyroRotation()
        {
            throw new NotImplementedException();
        }

        public QuadcopterData GetSensorData()
        {
            return new QuadcopterData();
        }
        private UDPClient client;
        public void Initialize(IQuadcopter quadToControl, Action<FlightStatus> onFlightStatusChanged)
        {
            client = new UDPClient();
            client.Connect("192.168.10.1", 8889);
        }

        public bool IsInitialized()
        {
            return true;
        }

        public bool IsReadyToFly()
        {
            return true;
        }

        public bool IsSimulator()
        {
            throw new NotImplementedException();
        }

        public void Land()
        {
            throw new NotImplementedException();
        }

        public void Run(FlightStatus flightStatus, IInputSource.FlightControlValues craftInputs)
        {

        }

        public void Takeoff()
        {
            throw new NotImplementedException();
        }

        private void OnDestroy()
        {
            client.Shutdown();
        }

        public GameObject GetGameObject()
        {
            throw new NotImplementedException();
        }

        public Component GetComponent()
        {
            throw new NotImplementedException();
        }
    }

}