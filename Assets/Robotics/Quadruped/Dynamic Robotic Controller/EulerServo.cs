using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EulerServo : MonoBehaviour, IServo
{
    public float GetCurrentAngle()
    {
        return transform.rotation.eulerAngles.x;
    }

    public GameObject GetGameObject()
    {
       return gameObject;
    }

    public bool IsEnabled()
    {
        return enabled;
    }

    public void ResetServo(float resetAngle)
    {
       
    }

    public void SetServoPosition(float position)
    {
       
    }

    public void SetServoPosition(float position, float speed)
    {
       
    }

    public void SetServoPositionImmediate(float position)
    {
       
    }

    public void SetServoSpeed(float speed)
    {
       
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
