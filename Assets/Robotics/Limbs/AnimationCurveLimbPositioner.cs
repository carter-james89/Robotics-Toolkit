using RoboticToolkit.Robotics.Limbs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCurveLimbPositioner : MonoBehaviour, ILimbPositioner
{
    public enum Status
    {
        None,
        Translating,
        Rotating,
    }
    public Status CurrentStatus { get; private set; } = Status.None;

    [SerializeField]
    private AnimationCurve m_gaitCurve;

    private AnimationCurve m_translationCurve;
    private AnimationCurve m_currentCurve;

    private IRoboticLimb m_limb;

    private List<ILimbPositionerEventListener> m_eventListeners = new List<ILimbPositionerEventListener>();


    private float m_strideTime = 0;

    private float m_gaitVelocity = 0;


    private float m_totalCurveDistance;
    private float m_totalTravelDistance;

    private Transform m_stride;

    private LineRenderer m_strideLine;

    public GameObject GetGameObject() => gameObject;

    private void Awake()
    {
        m_limb = GetComponentInParent<IRoboticLimb>();

        m_stride = new GameObject("Stride").transform;
        m_stride.SetParent(transform);

        m_strideLine = m_stride.gameObject.AddComponent<LineRenderer>();
        m_strideLine.startWidth = .01f;
        m_strideLine.endWidth = .01f;
        m_strideLine.useWorldSpace = false;
    }

    public void RotateToPosition(Vector3 position, Quaternion rotationAxis, float time, bool localSpace)
    {
        Debug.Log("Rotate End Point to " + position + " " + transform.parent.name);
        if (localSpace)
        {
            position = transform.TransformPoint(position);
        }
        CalculateAnimationCurve(position, time);
        m_strideTime = 0;
        CurrentStatus = Status.Rotating;
    }


    public void TranslateToPosition(Vector3 position, float time, bool localSpace)
    {
        Debug.Log("Translated End Point to " + position + " " + transform.parent.name);
        if (localSpace)
        {
            position = transform.TransformPoint(position);
            Debug.Log("Convert to golbal positon " + position + " " + transform.parent.name);
        }

        // m_totalCurveDistance = m_gaitCurve.keys[m_gaitCurve.keys.Length - 1].time - m_gaitCurve.keys[0].time;

        m_stride.position = m_limb.GetIKTargetPos();
        m_stride.LookAt(position, Vector3.up);

        var localEndPoint = m_stride.InverseTransformPoint(position);
        m_strideLine.SetPosition(0, Vector3.zero);
        m_strideLine.SetPosition(1, localEndPoint);

        m_totalCurveDistance = localEndPoint.z;
        m_totalTravelDistance = m_totalCurveDistance;

        Keyframe[] newFrames = new Keyframe[2];
        newFrames[0] = new Keyframe(0, 0);
        newFrames[1] = new Keyframe(m_totalCurveDistance, 0);
        m_translationCurve = new AnimationCurve(newFrames);


        m_currentCurve = m_translationCurve;

        m_gaitVelocity = m_totalCurveDistance / time;

        CurrentStatus = Status.Translating;
        m_strideTime = 0;
    }

    private void CalculateAnimationCurve(Vector3 globalEndPoint, float desiredGaitTime)
    {
        
        m_totalCurveDistance = m_gaitCurve.keys[m_gaitCurve.keys.Length - 1].time - m_gaitCurve.keys[0].time;

        m_stride.position = m_limb.GetIKTargetPos();
        m_stride.LookAt(globalEndPoint, Vector3.up);

        var localEndPoint = m_stride.InverseTransformPoint(globalEndPoint);
        m_strideLine.SetPosition(0, Vector3.zero);
        m_strideLine.SetPosition(1, localEndPoint);

        m_totalTravelDistance = localEndPoint.z;
        m_gaitVelocity = m_totalCurveDistance / desiredGaitTime;

        m_currentCurve = m_gaitCurve;
    }

    public bool Run()
    {
        if (CurrentStatus == Status.None)
        {
            return m_limb.LimbAtTarget();
        }

        var currentStrideTime = m_strideTime + Time.deltaTime * m_gaitVelocity;
        if (currentStrideTime > m_currentCurve.keys[m_currentCurve.keys.Length - 1].time)
        {
            m_strideTime = m_currentCurve.keys[m_currentCurve.keys.Length - 1].time;
            CurrentStatus = Status.None;
        }
        else
        {
            m_strideTime = currentStrideTime;
        }

        var m_currentCurveY = m_currentCurve.Evaluate(m_strideTime);
        // currentStrideTime - (.5f * m_totalCurveDistance)
        var curvePercent = currentStrideTime / m_totalCurveDistance;
        var travelDistance = curvePercent * m_totalTravelDistance;
        var desiredPos = new Vector3(0, m_currentCurveY, travelDistance);

        m_limb.SetIKTargetPos(m_stride.TransformPoint(desiredPos));

        return StrideComplete();
    }
    public bool StrideComplete()
    {
        if (CurrentStatus == Status.None && m_limb.LimbAtTarget())
        {
            return true;
        }
        return false;
    }

    public Vector3 GetLimbPosition(bool localSpace)
    {
        if (localSpace)
        {
            return transform.InverseTransformPoint(m_limb.GetIKTargetPos());
        }
        return m_limb.GetIKTargetPos();
    }
    public void SetLimbPosition(Vector3 position, bool localSpace)
    {
        m_limb.SetIKTargetPos(position);
    }

    public void SubscribeToEvents(ILimbPositionerEventListener listener)
    {
        if (m_eventListeners.Contains(listener))
        {
            return;
        }
        m_eventListeners.Add(listener);
    }
    public void UnsubscribeFromEvents(ILimbPositionerEventListener listener)
    {
        if (!m_eventListeners.Contains(listener))
        {
            return;
        }
        m_eventListeners.Remove(listener);
    }

    public void RotateToPosition(Vector3 direction, Vector3 upDirection, float distance, float time)
    {
        m_totalCurveDistance = m_gaitCurve.keys[m_gaitCurve.keys.Length - 1].time - m_gaitCurve.keys[0].time;

        m_stride.position = m_limb.GetIKTargetPos();
        m_stride.rotation = Quaternion.LookRotation(direction, upDirection);

       // var localEndPoint = m_stride.InverseTransformPoint(globalEndPoint);
        m_strideLine.SetPosition(0, Vector3.zero);
        m_strideLine.SetPosition(1, new Vector3(0,0,distance));

        m_totalTravelDistance = distance;
        m_gaitVelocity = m_totalCurveDistance / time;

        m_currentCurve = m_gaitCurve;

        m_strideTime = 0;
        CurrentStatus = Status.Rotating;
    }

    public void TranslateToPosition(Vector3 direction, float distance, float time)
    {
        // m_totalCurveDistance = m_gaitCurve.keys[m_gaitCurve.keys.Length - 1].time - m_gaitCurve.keys[0].time;

        m_stride.position = m_limb.GetIKTargetPos();
        m_stride.rotation = Quaternion.LookRotation(direction);

       // m_stride.rotation = Quaternion.LookRotation(direction,Vector3.up,)

      //  var localEndPoint = m_stride.InverseTransformPoint(new Vector3(0,0,distance));
        m_strideLine.SetPosition(0, Vector3.zero);
        m_strideLine.SetPosition(1, new Vector3(0, 0, distance));

        m_totalCurveDistance = distance;
        m_totalTravelDistance = m_totalCurveDistance;

        Keyframe[] newFrames = new Keyframe[2];
        newFrames[0] = new Keyframe(0, 0);
        newFrames[1] = new Keyframe(m_totalCurveDistance, 0);
        m_translationCurve = new AnimationCurve(newFrames);

        m_currentCurve = m_translationCurve;

        m_gaitVelocity = m_totalCurveDistance / time;

        CurrentStatus = Status.Translating;
        m_strideTime = 0;
    }
}
