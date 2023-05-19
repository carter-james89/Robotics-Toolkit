using RoboticToolkit.Robotics.Limbs;
using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IQuadrupedLeg
{
    public GameObject GetGameObject();
    public IServo GetBaseServo();
    public IServo GetHipServo();
    public IServo GetKneeServo();

}
public class QuadrupedLeg : MonoBehaviour, IQuadrupedLeg
{
    [SerializeField]
    private GameObject m_baseServoObject;
    private IServo m_baseServo;
    [SerializeField]
    private GameObject m_hipServoObject;
    private IServo m_hipServo;
    [SerializeField]
    private GameObject m_kneeServoObject;
    private IServo m_kneeServo;

    public GameObject GetGameObject() => gameObject;

    private void Awake()
    {
        m_baseServo = m_baseServoObject.GetComponent<IServo>();
        m_hipServo = m_hipServoObject.GetComponent<IServo>();
        m_kneeServo = m_kneeServoObject.GetComponent <IServo>();
    }
    public IServo GetBaseServo()
    {
        return m_baseServo;
    }

    [SerializeField]
    public Vector3 GetFootOffset()
    {
        var servo = GetComponent<ThreeJointRoboticLimb>().GetServoControllers()[2];
        return transform.InverseTransformPoint(servo.GetServo().GetGameObject().transform.position);    
    }

    public Vector3 GetHipOffset()
    {
        var servo = GetComponent<ThreeJointRoboticLimb>().GetServoControllers()[0];
        return transform.InverseTransformPoint(servo.GetServo().GetGameObject().transform.position);
    }

    public IServo GetHipServo()
    {
        return m_hipServo;
    }

    public Vector3 GetKneeOffset()
    {
        var servo = GetKneeServo();
        return transform.InverseTransformPoint(servo.GetGameObject().transform.position);
    }

    public IServo GetKneeServo()
    {
        return m_kneeServo;// GetComponent<ThreeJointRoboticLimb>().WristServoController.GetServo();// GetServoControllers()[2].GetServo();
    }
}
