using ProcessCommunicationToolkit.SerialPortTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

namespace RoboticsToolkit.ArduinoUtilities
{
    public class ArduinoConnection : MonoBehaviour
    {
        [SerializeField]
        private string m_portName = "/dev/ttyACM0";
        [SerializeField]
        private int m_baudeRate = 115200;

        private static SerialPortConnection m_arduinoConnection;

        // Start is called before the first frame update
        public void ConnectToArduino()
        {
            //m_arduinoConnection.serialPortMessage += x => Debug.Log(x);
            m_arduinoConnection = new SerialPortConnection(m_portName, m_baudeRate);


        }

        private void Start()
        {

        }

        private void OnDestroy()
        {
            if (m_arduinoConnection != null)
            {
                m_arduinoConnection.Write("shutdown");
                Debug.Log(m_arduinoConnection.ReadLine());

                m_arduinoConnection.ShutDown();
            }
        }

        public bool Connected => m_arduinoConnection.Connected;

        public string ReadFromArduino()
        {
            if (!m_arduinoConnection.Connected)
            {
                Debug.LogWarning("Arduino Connection not Connected");
                return null;
            }


            return m_arduinoConnection.ReadLine();
        }

        public string WriteToArduino(string message, bool waitForResponce = false)
        {
            //  Debug.Log("Send message to arduino " + message);
            if (!m_arduinoConnection.Connected)
                return null;

            m_arduinoConnection.Write(message);
            // m_arduinoConnection.

            // if (waitForResponce)
            // {
            // string responce = null;
            //var responce = m_arduinoConnection.ReadLine();
            //Debug.Log("responce from down arrow" + responce);
            return null;

            //  }
            //  else
            //  {
            //      return null;
            //  }
        }
    }

}