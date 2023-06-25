using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EulerServo : MonoBehaviour, IServo
{
    [SerializeField]
    private float m_offset = 0;
    [SerializeField]
    private float m_currentAngle;
    public float GetCurrentAngle()
    {
        float rawAngle = Quaternion.Angle(Quaternion.identity, transform.localRotation);
        var point = transform.TransformPoint(new Vector3(0, 0, 10));
        var localPoint = transform.parent.InverseTransformPoint(point);
        var final = rawAngle;
        if(localPoint.y > 0)
        {
            final = -final;
        }           
       // Debug.Log(name + " Raw : " + rawAngle + " Final : " + final +" : " + Vector3.Dot(transform.up, transform.parent.up));
        return final;
    }

    public GameObject GetGameObject()
    {
       return gameObject;
    }
    private void Awake()
    {
       // m_offset = GetCurrentAngle();
    }
    public bool IsEnabled()
    {
        return enabled;
    }

    private void Update()
    {
            m_currentAngle = GetCurrentAngle();
    }

    public void ResetServo(float resetAngle)
    {
       
    }

    public void SetServoPosition(float position)
    {
      //  var delta = position - transform.localEulerAngles.x;

        //transform.Rotate(transform.right, delta,Space.Self);
      //  transform.Rotate(new Vector3(delta, 0, 0));
      //  return;
      //  position = Mathf.Abs(position);
       // float adjustedAngle = position > 180 ? position - 360 : position;

        //var tempPos = transform.localEulerAngles;
      //  tempPos.x = position + m_offset;// adjustedAngle;
                             //  Debug.Log(name + " set to " + adjustedAngle);
                             // transform.localRotation = Quaternion.Euler(tempPos);
        //transform.localRotation = Quaternion.AngleAxis(position, transform.parent.TransformDirection(transform.parent.right));
       // Debug.Log(name + " actual angle " + transform.localEulerAngles.x);
        Quaternion rotation = Quaternion.AngleAxis(position + m_offset, transform.parent.InverseTransformDirection(-transform.parent.right));
        transform.localRotation = rotation;
        //Quaternion rotation = Quaternion.Euler(30, 0, 0);
        //  transform.localRotation = Quaternion.Inverse(transform.parent.rotation) * rotation;

        //Vector3 desiredRotation = new Vector3(30, 0, 0);
        //Quaternion rotation = Quaternion.identity;
        //rotation.SetFromToRotation(transform.up, transform.parent.TransformDirection(desiredRotation));
        //transform.localRotation = rotation;

        //transform.localEulerAngles = tempPos;

        //Vector3 parentLocalXAxis = transform.parent.TransformDirection(Vector3.right);
        // transform.rotation = Quaternion.AngleAxis(30, parentLocalXAxis);

        // transform.localRotation = Quaternion.
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
}
