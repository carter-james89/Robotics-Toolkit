using ProcessCommunicationToolkit.SerialPortTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toolkit.Robotics.ESP32
{
    public class ESP32Connection : MonoBehaviour
    {
        [SerializeField]
        private string _portName = "/COM3";
        [SerializeField]
        private int _baudeRate = 115200;

        private static SerialPortConnection _serialConnection;

        public void ConnectToESP32()
        {
            if (_serialConnection != null)
                return;
            //m_arduinoConnection.serialPortMessage += x => Debug.Log(x);
            _serialConnection = new SerialPortConnection(_portName, _baudeRate);

        }


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}