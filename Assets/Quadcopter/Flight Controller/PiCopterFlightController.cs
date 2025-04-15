using ProcessCommunicationToolkit.SocketPortConnection;
using ProcessCommunicationToolkit_Csharp;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FlightControllers.Quadcopters
{
    public class PiCopterFlightController : MonoBehaviour, IFlightController
    {
        public float gyroYaw;
        public float gyroRoll;
        public float gyroPitch;

        private static ClientUplink client;
        private static ServerUplink server;

        private Action<FlightStatus> _onFlightStatusChanged;

        [SerializeField]
        private bool _sendToClient = true;

        private float _pitchOffset;
        private float _rollOffset;
        private float _yawOffset;

        private QuadcopterData _quadcopterData;
        private GroundStationData _groundStatationData;

        //private MotorThrustCalculatorGameObject _motorCalculator;
        private IMotorThrustCalculator _motorThrustCalculator;

        private IQuadcopter _quadToControl;

        private bool _isInitialized;
        private bool _serverConnected;
        private bool _clientConnected;
        public bool IsInitialized()
        {
            return _isInitialized;
        }
        public bool IsReadyToFly()
        {
            //if ((!_startServer || server.uplinkStatus == SocketUplink.Status.Connected) && (!_startClient || client.uplinkStatus == SocketUplink.Status.Connected))
            //{
            //    return true;
            //}
            //Debug.Log(server.uplinkStatus);
            //Debug.Log(_clientConnected);
            return false;
        }


        private const string ipAddress = "192.168.86.41";// "192.168.86.50";
        private const string serverIPAddress = "192.168.86.46";

        private bool _startServer = false;
        private bool _startClient = true;

        public Quaternion GetGyroRotation()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize(IQuadcopter quadToControl, Action<FlightStatus> onFlightStatusChanged)
        {
            _quadToControl = quadToControl;
            _onFlightStatusChanged = onFlightStatusChanged;



            _quadcopterData = new QuadcopterData();
            _groundStatationData = new GroundStationData();

            if (_startServer)
            {
                server = new ServerUplink(11001, serverIPAddress);
                server.uplinkMessage += x => Debug.Log("Server Log : " + x);
                server.EstablishConnection();
            }
            if (_startClient)
            {
                client = new ClientUplink(11000, ipAddress);
                client.uplinkMessage += x => Debug.Log("Client Log : " + x);
                // client.uplinkMessage += OnClientLogMessageRecieved;
                client.onConnectionEstablished += OnConnectedToServer;
                client.EstablishConnection();
            }
            //_motorCalculator = new MotorThrustCalculator();
            _motorThrustCalculator = GetComponent<IMotorThrustCalculator>();
            _motorThrustCalculator.Initialize(0);

            _isInitialized = true;
        }

        private void OnConnectedToServer(string obj)
        {
            _serverConnected = true;
            client.ListenForServerDataWithHeader(OnDataRecievedFromServer);
        }

        [SerializeField]
        private float _rawYaw;
        [SerializeField]
        private float _rawPitch;
        [SerializeField]
        private float _rawRoll;
        private byte[] OnDataRecievedFromServer(byte[] arg)
        {
            _quadcopterData = (QuadcopterData)CommunicationUtilities.ByteToType<QuadcopterData>(arg);

            //  Debug.Log(_quadcopterData.gyroPitch);
            //run calculations and return
            if (_rollOffset == 0)
            {
                _rollOffset = _quadcopterData.gyroRoll;
                _pitchOffset = _quadcopterData.gyroPitch;
                _yawOffset = _quadcopterData.gyroYaw;
            }

            _rawYaw = _quadcopterData.gyroYaw;
            _rawPitch = _quadcopterData.gyroPitch;
            _rawRoll = _quadcopterData.gyroRoll;

            gyroPitch = _quadcopterData.gyroPitch - _pitchOffset;
            gyroRoll = _quadcopterData.gyroRoll - _rollOffset;
            gyroYaw = _quadcopterData.gyroYaw - _yawOffset;

            return new byte[1];
        }

        public QuadcopterData GetSensorData()
        {
            return _quadcopterData;
        }

        public bool IsSimulator()
        {
            return false;
        }

        public void Run(FlightStatus flightStatus, IInputSource.FlightControlValues desiredInputs)
        {
            if (Time.frameCount > 500)
            {
                _groundStatationData.yaw = desiredInputs.yaw;
                _groundStatationData.pitch = desiredInputs.pitch;
                _groundStatationData.roll = desiredInputs.roll;
                _groundStatationData.throttle = desiredInputs.throttle;

                var motorValues = _motorThrustCalculator.Run(new Vector3(_quadcopterData.posX, _quadcopterData.posY, _quadcopterData.posZ), new Vector3(_quadcopterData.gyroPitch, _quadcopterData.gyroYaw, _quadcopterData.gyroRoll), desiredInputs);

                _groundStatationData.motorBRSpeed = Math.Round(motorValues.motorBR);
                _groundStatationData.motorBLSpeed = Math.Round(motorValues.motorBL);
                _groundStatationData.motorFRSpeed = Math.Round(motorValues.motorFR);
                _groundStatationData.motorFLSpeed = Math.Round(motorValues.motorFL);

                byte[] json = CommunicationUtilities.TypeToByte(_groundStatationData);
                if (_startServer)
                {
                    //byte[] responce = server.SendMessage(json, json.Length);
                    byte[] responce = server.SendMessageWithHeader(json, json.Length);
                }
            }
        }

        public void Takeoff()
        {

            _motorThrustCalculator.Initialize(_quadToControl.GetGameObject().transform.eulerAngles.y);
            _motorThrustCalculator.SetAltitudeHold(1);
            _onFlightStatusChanged.Invoke(FlightStatus.Launching);
        }

        public void Land()
        {
            throw new System.NotImplementedException();
        }

        private void OnDestroy()
        {
            if (_isInitialized)
            {
                if (client != null)
                {
                    client.onConnectionEstablished -= OnConnectedToServer;
                    client.ShutDown();
                }
                if (server != null)
                {
                    server.ShutDown();
                }

            }
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