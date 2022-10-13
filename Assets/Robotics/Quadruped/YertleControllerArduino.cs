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
        public bool SetTransformValues()
        {
            if (!m_arduinoConnection.Connected)
            {
                Debug.LogWarning("Not connected to arduino, cant get sensor data");
                return false;
            }
            // var arduinoMessage = m_arduinoConnection.ReadFromArduino();
            //Debug.Log("Received Arduino Message : " + arduinoMessage);
            m_arduinoConnection.WriteToArduino("P");

            try
            {
                var sensorDataJSON = m_arduinoConnection.ReadFromArduino();
                var sensorData = (QuadrupedSensorData)JsonConvert.DeserializeObject(sensorDataJSON, typeof(QuadrupedSensorData));

                var robotTransform = m_robot.GetGameObject().transform;

                var euler = new Vector3(-sensorData.P, sensorData.R, sensorData.Y);
                m_desiredRotation = Quaternion.Euler(euler);

               // Debug.Log(sensorDataJSON);

                var groundPosition = m_groundSensor.transform.position + (m_groundSensor.forward * (.01f * sensorData.H));

                var groundOffset = new Vector3(0, groundPosition.y, 0);

               // robotTransform.position -= groundOffset;

               m_desiredPosition = robotTransform.position - groundOffset;

              //  robotTransform.position = m_desiredPosition;
              //  robotTransform.rotation = m_desiredRotation;

                //robotTransform.

             //   PositionTransform();

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }

            //m_gyroData = new Vector3(-data.P, data.R, data.Y);
            //var sensorInfo = 
            if (true)//m_calibrationStatus > 0)
            {
                //var eulers = new Quaternion(data.X, data.Y, data.Z, data.W).eulerAngles;
                ////var newEulers = new Vector3(eulers.y, 0, 0);
                //var newEulers = new Vector3(-eulers.y, -eulers.z, -eulers.x);
                //var newRot = Quaternion.Euler(newEulers);
                //transform.rotation = newRot;

                // var euler = new Vector3(-data.P, data.R, data.Y);
                //transform.rotation = Quaternion.Euler(euler);
            }
        }

        public bool SendCommands(QuadrupedGroundStationData groundStationData)
        {
            try
            {
                m_arduinoConnection.WriteToArduino(JsonUtility.ToJson(groundStationData));
              //  Debug.Log("Echo : " + m_arduinoConnection.ReadFromArduino());
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
            if(m_initialized)
            PositionTransform();
        }

        private void PositionTransform()
        {
            var ab = m_robot.GetGameObject().GetComponent<ArticulationBody>();
            var lerpedPosition = Vector3.Lerp(m_robot.GetGameObject().transform.position,m_desiredPosition,Time.deltaTime*20);
            ab.TeleportRoot(lerpedPosition, m_desiredRotation);
            ab.velocity = Vector3.zero;
            ab.angularVelocity = Vector3.zero;
            //ab.ResetInertiaTensor();


        


            //ab.Sleep();
        }
    }
}

