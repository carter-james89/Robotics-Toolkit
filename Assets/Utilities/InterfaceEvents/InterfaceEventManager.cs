using System;
using System.Collections.Generic;
using UnityEngine;
using Toolkit.Utilities;
using System.Linq;

namespace Toolkit.Utilities.Events
{

    /// <summary>
    /// Base interface for all event data types. Each event type must implement this.
    /// </summary>
    public interface IEventData
    {
    }

    /// <summary>
    /// A generic listener interface that handles events of type T.
    /// </summary>
    /// <typeparam name="T">A specific event data type implementing IEventData</typeparam>
    public interface IEventListener<T> : IMonobehaviorInterface where T : IEventData
    {
        void OnEventOccured(T eventData);
    }

    /// <summary>
    /// A generic event source that allows subscription and unsubscription of listeners.
    /// </summary>
    /// <typeparam name="T">The type of event data this source dispatches</typeparam>
    public interface IEventSource<T> : IMonobehaviorInterface where T : IEventData
    {
        void SubscribeToEvents(IEventListener<T> listenerToSubscribe);
        void UnsubscribeFromEvents(IEventListener<T> listenerToUnsubscribe);
    }
    /// <summary>
    /// Manages a list of listeners for a specific event type and handles dispatch.
    /// </summary>
    /// <typeparam name="T">The event data type</typeparam>
    public class InterfaceEventManager<T> where T : IEventData
    {
        private readonly List<IEventListener<T>> _subscribedListeners = new();
        private readonly string _debugString;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InterfaceEventManager() : this("EventManager") { }

        /// <summary>
        /// Constructs an event manager with a custom debug string identifier.
        /// </summary>
        /// <param name="debugString">Label to include in debug logs</param>
        public InterfaceEventManager(string debugString)
        {
            _debugString = debugString;
        }

        /// <summary>
        /// Adds a listener to the manager.
        /// </summary>
        /// <param name="listener">The listener to subscribe</param>
        /// <returns>True if added successfully, false if already present</returns>
        public bool AddListener(IEventListener<T> listener)
        {
            if (_subscribedListeners.Contains(listener))
            {
                Debug.LogWarning($"[{_debugString}] Listener already subscribed: {listener}");
                return false;
            }

            _subscribedListeners.Add(listener);
            return true;
        }

        /// <summary>
        /// Removes a listener from the manager.
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        /// <returns>True if removed, false if not found</returns>
        public bool RemoveListener(IEventListener<T> listener)
        {
            if (!_subscribedListeners.Contains(listener))
            {
                Debug.LogWarning($"[{_debugString}] Attempted to remove non-existent listener: {listener}");
                return false;
            }

            _subscribedListeners.Remove(listener);
            return true;
        }

        /// <summary>
        /// Dispatches the event to all subscribed listeners.
        /// Removes any null listeners before dispatch.
        /// </summary>
        /// <param name="eventData">The event data to send</param>
        public void RaiseEvent(T eventData)
        {
            int initialCount = _subscribedListeners.Count;
            _subscribedListeners.RemoveAll(listener => listener == null);
            int removedCount = initialCount - _subscribedListeners.Count;

            if (removedCount > 0)
            {
                Debug.LogWarning($"[{_debugString}] Removed {removedCount} null listener(s) before dispatching event.");
            }

            foreach (var listener in _subscribedListeners.ToList())
            {
                //try
                //{ 
                    Debug.Log(listener.GetGameObject().name);   
                    listener.OnEventOccured(eventData);
                //}
                //catch (Exception e)
                //{
                //    Debug.LogError($"[{_debugString}] Error while dispatching event to listener: {e.Message} : " + listener.GetGameObject().name);
                //}
            }
        }

        /// <summary>
        /// Returns all subscribed listeners.
        /// </summary>
        public List<IEventListener<T>> GetListeners() => _subscribedListeners;
    }

    /// <summary>
    /// Sample implementation of a custom event data class.
    /// </summary>
    public class TestEventData : IEventData
    {
        public bool GetTestBool() => true;

        public Enum GetEventType()
        {
            return TestEventType.SampleEvent;
        }
    }

    /// <summary>
    /// Sample enumeration for identifying event types.
    /// </summary>
    public enum TestEventType
    {
        SampleEvent
    }

    /// <summary>
    /// Sample listener implementation for TestEventData.
    /// </summary>
    public class TestListener : IEventListener<TestEventData>
    {
        public Component GetComponent()
        {
            throw new NotImplementedException();
        }

        public GameObject GetGameObject()
        {
            throw new NotImplementedException();
        }

        public void OnEventOccured(TestEventData eventData)
        {
            Debug.Log($"TestBool result: {eventData.GetTestBool()}");
        }
    }

    /// <summary>
    /// Example source that manages subscription to TestEventData events.
    /// </summary>
    public class TestSource : IEventSource<TestEventData>
    {
        private InterfaceEventManager<TestEventData> eventManager = new();

        public void SubscribeToEvents(IEventListener<TestEventData> listenerToSubscribe)
        {
            if (!eventManager.AddListener(listenerToSubscribe))
            {
                Debug.LogWarning("Listener could not be subscribed (already present).");
            }
        }

        public void UnsubscribeFromEvents(IEventListener<TestEventData> listenerToUnsubscribe)
        {
            if (!eventManager.RemoveListener(listenerToUnsubscribe))
            {
                Debug.LogWarning("Listener could not be unsubscribed (not found).");
            }
        }

        public void DispatchTestEvent()
        {
            TestEventData data = new();
            eventManager.RaiseEvent(data);
        }

        public GameObject GetGameObject()
        {
            throw new NotImplementedException();
        }

        public Component GetComponent()
        {
            throw new NotImplementedException();
        }
    }
}
