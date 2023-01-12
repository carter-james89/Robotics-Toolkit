using RoboticsToolkit.ArduinoUtilities;
using RoboticsToolkit.Robotics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoQuadrupedServoCMDRelay : MonoBehaviour, IServoCMDRelay
{
    private ArduinoConnection m_arduinoConnection;

    public IRobot m_robot { get; private set; }


    private bool m_initialized;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public bool Initialize(IRobot robot)
    {
        m_arduinoConnection = GetComponent<ArduinoConnection>();
        if (m_arduinoConnection == null)
        {
            return false;
        }
        m_arduinoConnection.ConnectToArduino();
        m_robot = robot;
        m_initialized = true;
        return true;
    }

    public void ResetController()
    {
        //throw new System.NotImplementedException();
    }

    public bool IsSimulator()
    {
        return false;
    }
    private bool m_logCMDResponce = false;
    public bool RelayServoCommands(QuadrupedGroundStationData groundStationData)
    {
        try
        {
            if (m_logCMDResponce)
            {
                m_arduinoConnection.WriteToArduino(3, JsonUtility.ToJson(groundStationData));
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
}
