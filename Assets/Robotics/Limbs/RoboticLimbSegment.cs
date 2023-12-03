using RoboticsToolkit.Robotics.Servos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboticLimbSegment : MonoBehaviour, IRoboticLimbSegment
{
    [SerializeField]
    private GameObject m_endpoint;
    
    public Vector3 GetEndPoint()
    {
        // var childrenLimb = gameObject.GetComponentsInChildren<IRoboticLimbSegment>();

        // foreach (var item in childrenLimb)
        // {
        //     if((item as RoboticLimbSegment) != this)
        //     {
        //         return item.GetGameObject().transform.localPosition;
        //     }
        // }
        //return Vector3.zero;
       // Debug.Log(name);
       return m_endpoint.transform.localPosition;
    }

    public GameObject GetGameObject()
    {
       return gameObject;
    }

    public float GetLength()
    {
        return Math.Abs(GetEndPoint().z);
    }

    public IServo[] GetServos()
    {
        return GetComponents<IServo>(); 
    }

    public void SetRenderType(IRoboticLimbSegment.RenderType type, Color color)
    {
        switch (type)
        {
            case IRoboticLimbSegment.RenderType.Line:
                //Debug.Log(name + " draw line to " + GetEndPoint());
                var lineRenderer = gameObject.GetComponent<LineRenderer>();
                lineRenderer.useWorldSpace = false;
                lineRenderer.SetPosition(1, GetEndPoint()); 
                lineRenderer.material.color = color;    
                break;
            case IRoboticLimbSegment.RenderType.Mesh:
                break;
            default:
                break;
        }
    }
}
