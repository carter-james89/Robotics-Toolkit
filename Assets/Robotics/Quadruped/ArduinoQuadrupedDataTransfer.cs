using Newtonsoft.Json;
using RoboticsToolkit.ArduinoUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{
    public class ArduinoQuadrupedDataTransfer : MonoBehaviour, IQuadrupedDataTransfer
    {
        private ArduinoConnection m_arduinoConnection;

        private IRoboticController m_roboticController;

        [SerializeField]
        private Transform m_ground;

        [SerializeField]
        private Transform m_groundSensor;

        void Awake()
        {
            m_arduinoConnection = GetComponent<ArduinoConnection>();
        }
        public QuadrupedSensorData GetSensorData()
        {
            if (!m_arduinoConnection.Connected)
            {
                Debug.LogWarning("Not connected to arduino, cant get sensor data");
                return null;
            }
            // var arduinoMessage = m_arduinoConnection.ReadFromArduino();
            //Debug.Log("Received Arduino Message : " + arduinoMessage);
            m_arduinoConnection.WriteToArduino("P");

            try
            {
                var sensorDataJSON = m_arduinoConnection.ReadFromArduino();
                var sensorData = (QuadrupedSensorData)JsonConvert.DeserializeObject(sensorDataJSON, typeof(QuadrupedSensorData));

                var robotTransform = m_roboticController.GetGameObject().transform;

                var euler = new Vector3(-sensorData.P, sensorData.R, sensorData.Y);
                transform.rotation = Quaternion.Euler(euler);

               Debug.Log(sensorDataJSON);

                var groundPosition = m_groundSensor.transform.position + (m_groundSensor.forward * (.01f * sensorData.H));

                var groundOffset = new Vector3(0,groundPosition.y, 0);
        
                robotTransform.position -= groundOffset;


                return sensorData;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return null;
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
                Debug.Log("Echo : " + m_arduinoConnection.ReadFromArduino());
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }
        }

        public bool Initialize(IRoboticController roboticController)
        {
            m_roboticController = roboticController;
           return true;
        }
    }
    public interface IQuadrupedDataTransfer
    {
        public bool Initialize(IRoboticController roboticController);
        public QuadrupedSensorData GetSensorData();

        public bool SendCommands(QuadrupedGroundStationData groundStationData);
    }

    public class QuadrupedSensorData
    {
        public float Y;
        public float P;
        public float R;
        public float H;
        public float W;
        public float X;
        public float QX;
        public float Z;
        public int C;

        // public int FL_0;
        // public int FL_1;
        //public int FL_2;

        //public int FR_0;
        //public int FR_1;
        //public int FR_2;

        //public int BL_0;
        //public int BL_1;
        //public int BL_2;

        //public int BR_0;
        //public int BR_1;
        //public int BR_2;

    }
    public class QuadrupedGroundStationData
    {
        //public int[] Motors;
        // public int[] MotorPositions;

        public int FL_0;
        public int FL_1;
        public int FL_2;

        public int FR_0;
        public int FR_1;
        public int FR_2;

        public int BL_0;
        public int BL_1;
        public int BL_2;

        public int BR_0;
        public int BR_1;
        public int BR_2;
    }
}
