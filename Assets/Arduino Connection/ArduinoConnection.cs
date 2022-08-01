using ProcessCommunicationToolkit.SerialPortTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        void Awake()
        {
            m_arduinoConnection = new SerialPortConnection(m_portName, m_baudeRate);

            m_arduinoConnection.serialPortMessage += x => Debug.Log(x);
        }

        private void Start()
        {
            
        }

        private void OnDestroy()
        {
            if (m_arduinoConnection != null)
            {
                m_arduinoConnection.ShutDown();
            }
        }

        public string WriteToArduino(string message, bool waitForResponce = false)
        {
            //  Debug.Log("Send message to arduino " + message);
            if (!m_arduinoConnection.Connected)
                return null;

            m_arduinoConnection.Write(message);

            // if (waitForResponce)
            // {
           // string responce = null;
            var responce = m_arduinoConnection.ReadLine();
            Debug.Log("responce " + responce);
            return responce;

            //  }
            //  else
            //  {
            //      return null;
            //  }
        }
    }

}