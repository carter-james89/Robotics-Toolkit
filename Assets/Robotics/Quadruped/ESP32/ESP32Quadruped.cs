using UnityEngine;
using System.Net.Sockets;
using Utilities.Network;
using System.Net;
using System.Text;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoboticsToolkit.Robotics.Limbs;
using RoboticsToolkit.Robotics.RoboticControllers;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RoboticsToolkit.Robotics.QuadrupedRobot
{
    public class QuadrupedData
    {
        public short VelocityX;
        public short VelocityY;
        public short VelocityZ;
        public short GyroX;
        public short GyroY;
        public short GyroZ;
        public int FLBaseAngle;
        public int FLHipAngle;
        public int FLKneeAngle;
        public int FRBaseAngle;
        public int FRHipAngle;
        public int FRKneeAngle;
        public int BRBaseAngle;
        public int BRHipAngle;
        public int BRKneeAngle;
        public int BLBaseAngle;
        public int BLHipAngle;
        public int BLKneeAngle;

        // Constructor
        public QuadrupedData(short velocityX, short velocityY, short velocityZ, short gyroX, short gyroY, short gyroZ,
                             int flBaseAngle, int flHipAngle, int flKneeAngle,
                             int frBaseAngle, int frHipAngle, int frKneeAngle,
                             int brBaseAngle, int brHipAngle, int brKneeAngle,
                             int blBaseAngle, int blHipAngle, int blKneeAngle)
        {
            VelocityX = velocityX;
            VelocityY = velocityY;
            VelocityZ = velocityZ;
            GyroX = gyroX;
            GyroY = gyroY;
            GyroZ = gyroZ;
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
    [RequireComponent(typeof(UDPCommunicationListener))]
    public class ESP32Quadruped : Quadruped, IUDPConnectionEventListener
    {
        UdpClient udpClient;
        IPEndPoint remoteEndPoint;
        IPAddress _ipAddress;
        private bool _connected = false;
        private System.Diagnostics.Stopwatch connectionStopwatch = new System.Diagnostics.Stopwatch();

        private QuadrupedData _receivedData;

        [SerializeField]
        private string _name = "bittle";
        protected override void Start()
        {
            base.Start();
        }

        protected override void OnBootup()
        {
            base.OnBootup();
            UDPConnectionListener.Instance.SubscribeToConnectionEvents(this);
            UpdateStatus(IRobot.Status.Initialized);
        }
        /// <summary>
        /// the udp listener got a broadcast so connect to ti
        /// </summary>
        /// <param name="eventData"></param>
        public void OnConnectionEventOccured(IUDPConnectionEventListener.EventData eventData)
        {
            Debug.Log("Check broadcast for correct esp32 info");
            if (eventData.ConnectionData.BoardType.ToLower().Contains("esp32"))
            {
                if (eventData.ConnectionData.Name.ToLower().Contains(_name))
                {
                    EstablishConnection(eventData);
                }
                else
                {
                    Debug.Log("Not a bittle");
                }
            }
            else
            {
                Debug.Log("Not a ESP32");
            }
        }

        public void EstablishConnection(IUDPConnectionEventListener.EventData eventData)
        {
            Debug.Log("Attempt to establish connection");
            _ipAddress = IPAddress.Parse(eventData.ConnectionData.IP);
            udpClient = new UdpClient(eventData.ConnectionData.Port);
            remoteEndPoint = new IPEndPoint(_ipAddress, eventData.ConnectionData.Port);
            SendUDPMessageAndWaitForResponse(0, new byte[0]);

            Debug.Log("Connection response received");
            connectionStopwatch.Start();

            _connected = true;
            Debug.Log("CONNECTED");
            UDPConnectionListener.Instance.UnsubscribeFromConnectionEvents(this);

            Task.Run(async () => await WaitForQuadHeartbeatAsync(10));
        }
        protected void Update()
        {
            if (_status == IRobot.Status.Initialized && _receivedData != null)
            {
                //   SetLimbs(new QuadrupedLimbData(0, _receivedData.FLHipAngle, _receivedData.FLKneeAngle, 0, _receivedData.FLHipAngle, _receivedData.FLKneeAngle, 0, _receivedData.FLHipAngle, _receivedData.FLKneeAngle, 0, _receivedData.FLHipAngle, _receivedData.FLKneeAngle));
                SetLimbsFromReceivedData();
                CompleteBootup();
            }
        }

       

        private void SetLimbsFromReceivedData()
        {
            var limbData = new LimbValues[4] {
                    new LimbValues(Vector3.zero, new float[3] { 0, _receivedData.FLHipAngle, _receivedData.FLKneeAngle }),
                     new LimbValues(Vector3.zero, new float[3] { 0, _receivedData.FRHipAngle, _receivedData.FRKneeAngle }),
                      new LimbValues(Vector3.zero, new float[3] { 0, _receivedData.BRHipAngle, _receivedData.BRKneeAngle }),
                       new LimbValues(Vector3.zero, new float[3] { 0, _receivedData.BLHipAngle, _receivedData.BLKneeAngle })
                };
            // SetLimbs(limbData);
            for (int i = 0; i < m_limbs.Length; i++)
            {

                switch (i)
                {

                    case 0:
                        m_flLimb.SetLimbValues(limbData[i].ServoAngles[0], limbData[i].ServoAngles[1], limbData[i].ServoAngles[2]);
                        break;
                    case 1:
                        m_frLimb.SetLimbValues(limbData[i].ServoAngles[0], limbData[i].ServoAngles[1], limbData[i].ServoAngles[2]);
                        break;
                    case 2:
                        m_brLimb.SetLimbValues(limbData[i].ServoAngles[0], limbData[i].ServoAngles[1], limbData[i].ServoAngles[2]);
                        break;
                    case 3:
                        m_blLimb.SetLimbValues(limbData[i].ServoAngles[0], limbData[i].ServoAngles[1], limbData[i].ServoAngles[2]);
                        break;
                    default:
                        break;
                }

            }
        }

        protected override void PositionTransform()
        {
            if (_receivedData == null)
            {
                return;
            }
           //ebug.Log("position limbs from physical bittle");
            SetLimbsFromReceivedData();
        }

        private async Task WaitForQuadHeartbeatAsync(int secondTimeout)
        {
            Debug.Log("start heartbeat");
            var heartbeatStopwatch = new System.Diagnostics.Stopwatch();
            heartbeatStopwatch.Start();
            while (_connected)
            {
                // Debug.Log("Listen for heartbeat");
                if (heartbeatStopwatch.Elapsed.TotalSeconds > secondTimeout)
                {
                    Debug.Log("TimeOut");
                    OnTimeOut();
                    return;
                }
                try
                {
                    IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    byte[] bytes = result.Buffer;

                    // Check if the received bytes array is long enough to contain a header
                    if (bytes.Length >= sizeof(int))
                    {
                        //  Debug.Log("Got heartbeat");
                        // Extract the header from the received bytes
                        int receivedHeader = BitConverter.ToInt32(bytes, 0);
                        // Assuming the time information is right after the header
                        long esp32TimeSinceConnection = BitConverter.ToInt32(bytes, sizeof(int));
                        // Get the time since connection in milliseconds from the Stopwatch
                        long unityTimeSinceConnection = connectionStopwatch.ElapsedMilliseconds;
                        // Compare the two times
                        long timeDifference = unityTimeSinceConnection - esp32TimeSinceConnection;
                        Debug.Log($"HeartBeat Time difference: {timeDifference} ms");

                        // Create a new array to hold the remaining bytes after the header and time
                        int remainingBytesLength = bytes.Length - sizeof(int) - sizeof(int);
                        byte[] remainingBytes = new byte[remainingBytesLength];
                        Buffer.BlockCopy(bytes, sizeof(int) + sizeof(int), remainingBytes, 0, remainingBytes.Length);
                        _receivedData = ParsePhysicalRobotData(remainingBytes);  // Update the shared data

                        heartbeatStopwatch.Restart();


                    }
                    else
                    {
                        Debug.LogWarning("Received bytes do not contain a complete header.");
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        OnTimeOut();
                    }
                    else
                    {
                        OnConnectionError("SocketException occurred: " + ex.Message);
                    }
                }
            }
        }

        private byte[] SendUDPMessageAndWaitForResponse(int header, byte[] message, int timeout = 10000)
        {
            if (IsSimulation())
            {
                Debug.LogWarning("Should not be sending udp messages in simulation mode");
                return null;
            }
            // Combine the header and the message into one byte array
            byte[] headerBytes = BitConverter.GetBytes(header);
            byte[] packet = new byte[headerBytes.Length + message.Length];
            Buffer.BlockCopy(headerBytes, 0, packet, 0, headerBytes.Length);
            Buffer.BlockCopy(message, 0, packet, headerBytes.Length, message.Length);
            udpClient.Send(packet, packet.Length, remoteEndPoint);
            udpClient.Client.ReceiveTimeout = timeout;

            try
            {

                // This will block the thread until a message is received or timeout occurs
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = udpClient.Receive(ref remoteIpEndPoint);

                // Check if the received bytes array is long enough to contain a header
                if (bytes.Length >= sizeof(int))
                {
                    // Extract the header from the received bytes
                    int receivedHeader = BitConverter.ToInt32(bytes, 0);

                    // Confirm that the received header matches the sent header
                    if (receivedHeader == header)
                    {
                        // Assuming the time information is right after the header
                        long esp32TimeSinceConnection = BitConverter.ToInt32(bytes, sizeof(int));

                        // Get the time since connection in milliseconds from the Stopwatch
                        long unityTimeSinceConnection = connectionStopwatch.ElapsedMilliseconds;

                        // Compare the two times
                        long timeDifference = unityTimeSinceConnection - esp32TimeSinceConnection;

                        Debug.Log($"Time difference: {timeDifference} ms");

                        // Create a new array to hold the remaining bytes after the header and time
                        int remainingBytesLength = bytes.Length - sizeof(int) - sizeof(int);
                        byte[] remainingBytes = new byte[remainingBytesLength];
                        Buffer.BlockCopy(bytes, sizeof(int) + sizeof(int), remainingBytes, 0, remainingBytes.Length);
                        return remainingBytes; // Return the remaining bytes
                    }
                    else
                    {
                        Debug.LogWarning("Received header does not match the sent header.");
                    }
                }
                else
                {
                    Debug.LogWarning("Received bytes do not contain a complete header.");
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    OnTimeOut();
                }
                else
                {
                    OnConnectionError("SocketException occurred: " + ex.Message);
                }
            }
            return null; // Return null if no response was received or if headers do not match
        }

        private void OnTimeOut()
        {
            Debug.LogWarning("Timeout, no response received.");
            OnConnectionShutdown();
        }
        private void OnConnectionError(string error)
        {
            Debug.LogWarning(error);
            OnConnectionShutdown();
        }
        private void OnConnectionShutdown()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            return;
#endif
            UDPConnectionListener.Instance.SubscribeToConnectionEvents(this);
            UpdateStatus(IRobot.Status.Ready);
        }

        public override void SetLimbs(LimbValues[] limbData)
        {
            //  base.SetLimbs(limbData);
            OnLimbsPositioned(limbData);
        }

        /// <summary>
        /// Send the data to esp32
        /// </summary>
        /// <param name="limbValues"></param>
        protected override void OnLimbsPositioned(LimbValues[] limbValues)
        {
           // base.OnLimbsPositioned(limbValues);

            List<byte> byteList = new List<byte>();
            for (int i = 0; i < m_limbs.Length; i++)
            {
                byteList.AddRange(BitConverter.GetBytes(limbValues[i].ServoAngles[0]));
                byteList.AddRange(BitConverter.GetBytes(limbValues[i].ServoAngles[1]));
                byteList.AddRange(BitConverter.GetBytes(limbValues[i].ServoAngles[2]));
            }

            byte[] serializedData = byteList.ToArray();

            int header = 2;
            byte[] headerBytes = BitConverter.GetBytes(header);
            byte[] packet = new byte[headerBytes.Length + serializedData.Length];
            Buffer.BlockCopy(headerBytes, 0, packet, 0, headerBytes.Length);
            Buffer.BlockCopy(serializedData, 0, packet, headerBytes.Length, serializedData.Length);
            udpClient.Send(packet, packet.Length, remoteEndPoint);
        }

        private QuadrupedData ParsePhysicalRobotData(byte[] bytes)
        {
            int offset = 0;
            short velocityX = BitConverter.ToInt16(bytes, offset); offset += 2;
            short velocityY = BitConverter.ToInt16(bytes, offset); offset += 2;
            short velocityZ = BitConverter.ToInt16(bytes, offset); offset += 2;
            short gyroX = BitConverter.ToInt16(bytes, offset); offset += 2;
            short gyroY = BitConverter.ToInt16(bytes, offset); offset += 2;
            short gyroZ = BitConverter.ToInt16(bytes, offset); offset += 2;
            int flBaseAngle = BitConverter.ToInt32(bytes, offset); offset += 4;
            int flHipAngle = BitConverter.ToInt32(bytes, offset); offset += 4;
            int flKneeAngle = BitConverter.ToInt32(bytes, offset); offset += 4;
            int frBaseAngle = BitConverter.ToInt32(bytes, offset); offset += 4;
            int frHipAngle = BitConverter.ToInt32(bytes, offset); offset += 4;
            int frKneeAngle = BitConverter.ToInt32(bytes, offset); offset += 4;
            int brBaseAngle = BitConverter.ToInt32(bytes, offset); offset += 4;
            int brHipAngle = BitConverter.ToInt32(bytes, offset); offset += 4;
            int brKneeAngle = BitConverter.ToInt32(bytes, offset); offset += 4;
            int blBaseAngle = BitConverter.ToInt32(bytes, offset); offset += 4;
            int blHipAngle = BitConverter.ToInt32(bytes, offset); offset += 4;
            int blKneeAngle = BitConverter.ToInt32(bytes, offset);

            return new QuadrupedData(velocityX, velocityY, velocityZ, gyroX, gyroY, gyroZ,
                                     flBaseAngle, flHipAngle, flKneeAngle,
                                     frBaseAngle, frHipAngle, frKneeAngle,
                                     brBaseAngle, brHipAngle, brKneeAngle,
                                     blBaseAngle, blHipAngle, blKneeAngle);

            PhysicalRobotData data = new PhysicalRobotData();
            try
            {
                offset = 0;

                data.GyroVelocityX = BitConverter.ToInt64(bytes, offset);
                offset += sizeof(long);

                data.GyroVelocityY = BitConverter.ToInt64(bytes, offset);
                offset += sizeof(long);

                data.GyroVelocityZ = BitConverter.ToInt64(bytes, offset);
                offset += sizeof(long);

                // Repeat for all other fields
                data.FlBaseServoAngle = BitConverter.ToInt32(bytes, offset);
                offset += sizeof(int);
                // ... Continue for the rest of the integers ...

                Debug.Log(data.FlBaseServoAngle);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
            return null;
        }
        public override bool IsSimulation()
        {
            return false;
        }
        void OnDestroy()
        {
            if (IsSimulation())
            {
                return;
            }
            udpClient?.Close();
            UDPConnectionListener.Instance.Shutdown();
        }
    }
    public class PhysicalRobotData
    {
        public long GyroVelocityX { get; set; }
        public long GyroVelocityY { get; set; }
        public long GyroVelocityZ { get; set; }
        public int FlBaseServoAngle { get; set; }
        public int FlHipServoAngle { get; set; }
        public int FlKneeServoAngle { get; set; }
        public int FrBaseServoAngle { get; set; }
        public int FrHipServoAngle { get; set; }
        public int FrKneeServoAngle { get; set; }
        public int BrBaseServoAngle { get; set; }
        public int BrHipServoAngle { get; set; }
        public int BrKneeServoAngle { get; set; }
        public int BlBaseServoAngle { get; set; }
        public int BlHipServoAngle { get; set; }
        public int BlKneeServoAngle { get; set; }
    }
}