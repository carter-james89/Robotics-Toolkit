using Newtonsoft.Json;
using RoboticsToolkit.ArduinoUtilities;
using RoboticsToolkit.Robotics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadrupedArduinoTesting : MonoBehaviour
{
    private ArduinoConnection m_arduinoConnection;
    // Start is called before the first frame update

    [SerializeField]
    private float m_groundHeight = 0;

    [SerializeField]
    private int m_calibrationStatus = 0;

    [SerializeField]
    private Vector3 m_gyroData;

    private void Awake()
    {
        m_arduinoConnection = GetComponent<ArduinoConnection>();

       // m_arduinoConnection.onM
    }
    // Update is called once per frame
    void Update()
    {

        if (true)//Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (!m_arduinoConnection.Connected)
            {
                return;
            }
           // var arduinoMessage = m_arduinoConnection.ReadFromArduino();
            //Debug.Log("Received Arduino Message : " + arduinoMessage);
            m_arduinoConnection.WriteToArduino("P");
            var sensorData = m_arduinoConnection.ReadFromArduino();
            var data = (QuadrupedSensorData)JsonConvert.DeserializeObject(sensorData, typeof(QuadrupedSensorData));
            m_groundHeight = data.H;
            m_calibrationStatus = data.C;
            //m_gyroData = new Vector3(-data.P, data.R, data.Y);
            //var sensorInfo = 
            if(true)//m_calibrationStatus > 0)
            {
                //var eulers = new Quaternion(data.X, data.Y, data.Z, data.W).eulerAngles;
                ////var newEulers = new Vector3(eulers.y, 0, 0);
                //var newEulers = new Vector3(-eulers.y, -eulers.z, -eulers.x);
                //var newRot = Quaternion.Euler(newEulers);
                //transform.rotation = newRot;

                var euler = new Vector3(-data.P, data.R, data.Y);
                transform.rotation = Quaternion.Euler(euler);
            }
            

           // transform.eulerAngles = m_gyroData;
           

            var digitalTwinData = new QuadrupedGroundStationData();
            m_arduinoConnection.WriteToArduino(JsonUtility.ToJson(digitalTwinData));

           var testEcho = m_arduinoConnection.ReadFromArduino();
            Debug.Log("Confirmation Echo : " + testEcho);
            return;
            // Debug.Log("Attempt " + m_arduinoConnection.Connected);
           
           // Debug.Log("Attempt handshake");
            try
            {
                WriteArduinoData();
                // var handShakeDataMessage = m_arduinoConnection.ReadFromArduino();

                // Debug.Log("message from arduino : " + handShakeDataMessage);
                //var data = (QuadrupedSensorData)JsonConvert.DeserializeObject(handShakeDataMessage, typeof(QuadrupedSensorData));
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error Retrieving sensor data : " + e.ToString());
               // WriteArduinoData();

                return;
            }
           // WriteArduinoData();
        }
    }


    private void WriteArduinoData()
    {
        if (m_arduinoConnection && m_arduinoConnection.enabled)
        {
            //var digitalTwinData = new ();



           // m_arduinoConnection.WriteToArduino(JsonUtility.ToJson(digitalTwinData));

            // Debug.Log("bl2 : " +digitalTwinData.BL_2);
        }
    }
}
