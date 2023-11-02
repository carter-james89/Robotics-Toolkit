using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InterfaceEventListenerManager : MonoBehaviour
{
    private List<object> m_eventListeners;

    private Type m_eventListnerType;

    private string m_warningKey;

    public InterfaceEventListenerManager(Type eventListenerType, string warningMessage)
    {
        m_eventListeners = new List<object>();
        m_eventListnerType = eventListenerType;
        m_warningKey = warningMessage;
    }

    public void AddListener(object listener)
    {
        if (listener == null)
        {
            return;
        }
        if (!listener.GetType().GetInterfaces().ToList().Contains(m_eventListnerType))
        {
            return;
        }
        if (m_eventListeners.Contains(listener))
        {
            return;
        }
        m_eventListeners.Add(listener); 
    }

    public void RemoveListner(object listener)
    {
        if (listener == null)
        {
            return;
        }
        if (!listener.GetType().GetInterfaces().ToList().Contains(m_eventListnerType))
        {
            return;
        }
        if (!m_eventListeners.Contains(listener))
        {
            return;
        }
        m_eventListeners.Remove(listener);
    }

    public object[] GetListeners()
    {
        m_eventListeners.RemoveAll(item => item == null);
        return m_eventListeners.ToArray();
    }
}
