using Newtonsoft.Json;
using RoboticsToolkit.ArduinoUtilities;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{
    public class YertleControllerArduino : MonoBehaviour, IRoboticController
    {
        private ArduinoConnection m_arduinoConnection;

        private IRobot m_robot;

        [SerializeField]
        private Transform m_ground;

        [SerializeField]
        private Transform m_groundSensor;

        private bool m_initialized = false;

        public GameObject GetGameObject() => gameObject;

        private Vector3 m_desiredPosition = Vector3.zero;
        private Quaternion m_desiredRotation = Quaternion.identity;

        public bool IsSimulator() => false;

        void Awake()
        {

        }
        //0 shutdown with confirmaton
        //1 return sensor data
        //2 set motor values
        //3 set motor values with responce
        public bool SetTransformValues()
        {
            if (!m_arduinoConnection.Connected)
            {
                Debug.LogWarning("Not connected to arduino, cant get sensor data");
                return false;
            }
            m_arduinoConnection.WriteToArduino(1);

            try
            {
                var sensorDataJSON = m_arduinoConnection.ReadFromArduino();
               // Debug.Log(sensorDataJSON);
                var sensorData = (QuadrupedSensorData)JsonConvert.DeserializeObject(sensorDataJSON, typeof(QuadrupedSensorData));
                var robotTransform = m_robot.GetGameObject().transform;

                var euler = new Vector3(-sensorData.P, sensorData.R, sensorData.Y);
                m_desiredRotation = Quaternion.Euler(euler);

                var groundPosition = m_groundSensor.transform.position + (m_groundSensor.forward * (.01f * sensorData.H));
                var groundOffset = new Vector3(0, groundPosition.y, 0);
                m_desiredPosition = robotTransform.position - groundOffset;
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }
        }
        private bool m_logCMDResponce = false;
        public bool SendCommands(QuadrupedGroundStationData groundStationData)
        {
            try
            {                            
                if (m_logCMDResponce)
                {
                    m_arduinoConnection.WriteToArduino(1, JsonUtility.ToJson(groundStationData));
                    Debug.Log(m_arduinoConnection.ReadFromArduino());
                }
                else
                {
                    m_arduinoConnection.WriteToArduino(2, JsonUtility.ToJson(groundStationData));
                }
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }
        }

        public bool Initialize(IRobot robot)
        {
            m_arduinoConnection = GetComponent<ArduinoConnection>();
            m_arduinoConnection.ConnectToArduino();
            m_robot = robot;
            m_desiredPosition = robot.GetGameObject().transform.position;
            m_desiredRotation = robot.GetGameObject().transform.rotation;
            foreach (var ab in robot.GetGameObject().GetComponentsInChildren<ArticulationBody>())
            {
                //ab.mass = 0;
                ab.useGravity = false;
            }
            m_initialized = true;

            m_ground.GetComponent<Collider>().enabled = false;
            return true;
        }

        private void FixedUpdate()
        {
            if (m_initialized)
                PositionTransform();
        }

        private void PositionTransform()
        {
            var ab = m_robot.GetGameObject().GetComponent<ArticulationBody>();
            var lerpedPosition = Vector3.Lerp(m_robot.GetGameObject().transform.position, m_desiredPosition, Time.deltaTime * 20);
            ab.TeleportRoot(lerpedPosition, m_desiredRotation);
            ab.velocity = Vector3.zero;
            ab.angularVelocity = Vector3.zero;
        }
    }
}

