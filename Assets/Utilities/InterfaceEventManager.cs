using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toolkit.Utilities.Events
{
    public class InterfaceEventManager <T> where T :class
    {
        private List<T> _listeners;
        private string _customMessage;

        public InterfaceEventManager(string customMessage)
        {
            _listeners = new List<T>();
            _customMessage = customMessage;
        }

        public bool AddListener(T listener)
        {
            if(listener == null)
            {
                return false;
            }
            if(listener is T)
            {
                if (_listeners.Contains(listener))
                {
                    return false;          
                }
                _listeners.Add(listener);
                return true;
            }
            else
            {

            }
            return false;
        }

        public bool RemoveListener(T listener)
        {
            if (listener == null)
            {
                return false;
            }
            if (listener is T)
            {
                if (!_listeners.Contains(listener))
                {
                    return false;
                }
                _listeners.Remove(listener);
                return true;
            }
            else
            {

            }
            return false;
        }


        public T[] GetListeners()
        {
            _listeners.RemoveAll(x=>x==null);   
            return _listeners.ToArray();
        }
    }

}