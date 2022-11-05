using Newtonsoft.Json;
using RoboticsToolkit.ArduinoUtilities;
using RoboticsToolkit.Robotics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoQuadrupedPositoner : MonoBehaviour, IQuadrupedPositioner
{
    private ArduinoConnection m_arduinoConnection;

    public IRobot m_robot { get; private set; }

    private Vector3 m_desiredPosition;
    private Quaternion m_desiredRotation;
    private bool m_initialized;
    [SerializeField]
    private GameObject m_ground;
    [SerializeField]
    private Transform m_groundSensor;

    private ArticulationBody m_body;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public bool Initialize(IRobot robot)
    {
        m_arduinoConnection = GetComponent<ArduinoConnection>();
        if(m_arduinoConnection == null)
        {
            return false;
        }
        m_arduinoConnection.ConnectToArduino();
        m_robot = robot;
        m_body = m_robot.GetGameObject().GetComponent<ArticulationBody>();
        m_body.immovable = true;
        m_desiredPosition = robot.GetGameObject().transform.position;
        m_desiredRotation = robot.GetGameObject().transform.rotation;
        foreach (var ab in robot.GetGameObject().GetComponentsInChildren<ArticulationBody>())
        {
            ab.useGravity = false;
        }
        m_ground.GetComponent<Collider>().enabled = false;
        m_initialized = true;
        return true;
    }

    public bool IsSimulator()
    {
        return false;
    }

    public bool PositionTransform()
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

            var lerpedPosition = Vector3.Lerp(m_robot.GetGameObject().transform.position, m_desiredPosition, Time.deltaTime * 20);
            m_body.TeleportRoot(lerpedPosition, m_desiredRotation);
            m_body.velocity = Vector3.zero;
            m_body.angularVelocity = Vector3.zero;

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
            return false;
        }
    }
    //private void FixedUpdate()
    //{
    //    var lerpedPosition = Vector3.Lerp(m_robot.GetGameObject().transform.position, m_desiredPosition, Time.deltaTime * 20);
    //    m_body.TeleportRoot(lerpedPosition, m_desiredRotation);
    //    m_body.velocity = Vector3.zero;
    //    m_body.angularVelocity = Vector3.zero;
    //}

    public void ResetPositioner()
    {
        throw new System.NotImplementedException();
    }
}
