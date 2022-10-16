using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCurveLimbPositioner : MonoBehaviour, ILimbPositioner
{
    [SerializeField]
    private AnimationCurve m_gaitCurve;

    private IRoboticLimb m_limb;

    private List<ILimbPositionerEventListener> m_eventListeners = new List<ILimbPositionerEventListener>();

    private void Awake()
    {
        m_limb = GetComponent<IRoboticLimb>();

        m_gaitCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
        //  m_gaitCurve.AddKey(new Keyframe(0, 0));
        //  m_gaitCurve.AddKey(new Keyframe(1, 1));
        //  m_gaitCurve.AddKey(new Keyframe(2, 0));

        //  m_gaitCurve.SmoothTangents(0,10);
        ////  m_gaitCurve.SmoothTangents(1, 1);
        //  m_gaitCurve.SmoothTangents(2, 1);
    }


    public void RotateToPosition(Vector3 globalPosition, Quaternion rotationAxis, float time)
    {
        throw new System.NotImplementedException();
    }


    public void TranslateToPosition(Vector3 globalPosition, float time)
    {
        throw new System.NotImplementedException();
    }




    public Vector3 GetLimbPosition()
    {
        return m_limb.GetIKTargetPos();
    }
    public void SetLimbPosition(Vector3 globalPosition)
    {
        m_limb.SetIKTargetPos(globalPosition);
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
}
