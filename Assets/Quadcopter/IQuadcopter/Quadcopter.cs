using System;
using System.Collections.Generic;
using System.Linq;
using Toolkit.Utilities.Events;
using UnityEngine;

namespace FlightControllers.Quadcopters
{
    /// <summary>
    /// Base class for standard quadcopters, both real and simulated.
    /// Satisfies the requirements of <see cref="IQuadcopter"/>, and handles most of the heavy lifting for basic functions.
    /// </summary>
    public class Quadcopter : MonoBehaviour, IQuadcopter
    {
        #region Events

        /// <summary>
        /// Event that is called whenever the GameObject's <see cref="Transform"/> is changed.
        /// </summary>
        /// <remarks>
        /// This needs to be called manually in <see cref="OnTransformUpdated"/> for all inherited classes.
        /// </remarks>
        public Action<Vector3, Quaternion> onTransformChanged;

        #endregion

        #region Serialized Fields

        [SerializeField] private float heightOffset = 0;
        [SerializeField] private float assumedHeightOffset = 0;
        [SerializeField] private float elvInput;
        [SerializeField] private float yawInput;
        [SerializeField] protected GameObject groundSensorPoint;
        [SerializeField] protected TrailVisualizer _trailVisualizer;
        [SerializeField] protected FlightStatus _flightStatus;
        [SerializeField] private bool _headLessMode = false;
        [SerializeField] private bool _selfInitialize = false;

        #endregion

        #region State and Input

        public Vector3 homePoint { get; private set; }
        protected IInputSource defaultInputSource;
        protected IInputSource currentInputSource;
        protected IInputSource.FlightControlValues currentInputs;
        protected IFlightController _flightController;
        private List<GameObject> sensorPoints;
        public float deltaHeight;
        private float _prevHeight = 0;
        private bool m_initialized = false;

        #endregion

        #region Unity Lifecycle

        protected void Awake()
        {
            if (_selfInitialize)
            {
                Initialize(GetComponent<IFlightController>(), GetComponent<IInputSource>());
            }
        }

        public void Update()
        {
            if (_flightController?.IsInitialized() == true)
            {
                ProcessInputs();
            }
            if (m_initialized && !IsSimulator())
            {
                RunQuadcopterUpdate();
            }
        }

        public void FixedUpdate()
        {
            if (m_initialized && IsSimulator())
            {
                RunQuadcopterUpdate();
            }
        }

        protected virtual void OnDestroy() { }

        #endregion

        #region Initialization

        /// <inheritdoc/>
        public virtual void Initialize(IFlightController flightController, IInputSource defaultInputSource)
        {
            _flightStatus = FlightStatus.PreLaunch;
            _flightController = flightController;
            _flightController.Initialize(this, OnFlightStatusChanged);
            this.defaultInputSource = defaultInputSource;

            if (_trailVisualizer)
            {
                _trailVisualizer.Initialize(this);
            }

            m_initialized = true;
        }

        #endregion

        #region IQuadcopter Implementation

        /// <inheritdoc/>
        public Transform GetLocalTrackingSpace() => transform.parent;

        /// <inheritdoc/>
        public bool IsSimulator() => _flightController.IsSimulator();

        /// <inheritdoc/>
        public FlightStatus GetFlightStatus() => _flightStatus;

        /// <inheritdoc/>
        public QuadcopterData GetSensorData() => _flightController.GetSensorData();

        /// <inheritdoc/>
        public bool IsTracking() => true;

        /// <inheritdoc/>
        public void SetHomePoint(Vector3 newHomePoint) => homePoint = newHomePoint;

        /// <inheritdoc/>
        public void OverrideInputSource(IInputSource inputValueSource) => currentInputSource = inputValueSource;

        /// <inheritdoc/>
        public void RemoveInputOverride(IInputSource inputValueSource)
        {
            if (inputValueSource == currentInputSource)
            {
                currentInputSource = null;
            }
        }

        /// <inheritdoc/>
        public IInputSource.FlightControlValues ConvertToHeadlessInputs(IInputSource.FlightControlValues rawInputs)
        {
            var headLessDir = new Vector3(rawInputs.roll, 0, rawInputs.pitch);
            LineRenderer line = GetComponent<LineRenderer>();
            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position + headLessDir);

            var headLessDirX = Vector3.Project(headLessDir, transform.right);
            rawInputs.roll = headLessDirX.magnitude;
            var headLessDirZ = Vector3.Project(headLessDir, transform.forward);
            rawInputs.pitch = headLessDirZ.magnitude;

            if (Vector3.Dot(headLessDirZ, transform.forward) < 0) rawInputs.pitch = -rawInputs.pitch;
            if (Vector3.Dot(headLessDirX, transform.right) < 0) rawInputs.roll = -rawInputs.roll;

            return rawInputs;
        }

        /// <inheritdoc/>
        public virtual void Takeoff()
        {
            _flightController.Takeoff();
            var quadData = _flightController.GetSensorData();
            SetVirtualPosition(quadData);
            ResetKnownOffset();
            SetHomePoint(transform.localPosition);
            _flightStatus = FlightStatus.Flying;
        }

        /// <inheritdoc/>
        public virtual void Land()
        {
            _flightController.Land();
            _flightStatus = FlightStatus.PreLaunch;
        }

        /// <inheritdoc/>
        public GameObject GetGameObject() {
            if (this == null)
            {
                return null;
            }
            return gameObject;
        }

        /// <inheritdoc/>
        public Component GetComponent() {
            if (this == null)
            {
                return null;
            }
            return this;
        }
        /// <inheritdoc/>
        public void SubscribeToEvents(IEventListener<QuadcopterData> listenerToSubscribe) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void UnsubscribeFromEvents(IEventListener<QuadcopterData> listenerToUnsubscribe) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void SubscibeToAbort(Action actionToSubscribe) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void UnsubscribeFromAbort(Action actionToUnsubscribe) => throw new NotImplementedException();

        #endregion

        #region Update Methods

        private void RunQuadcopterUpdate()
        {
            if (_flightController?.IsInitialized() == true && _flightController.IsReadyToFly())
            {
                var quadData = _flightController.GetSensorData();
                SetVirtualPosition(quadData);
                _flightController.Run(_flightStatus, currentInputs);
                OnTransformUpdated();
            }
        }

        protected void ProcessInputs()
        {
            var defaultInputs = defaultInputSource.GetInputValues();

            if (defaultInputs.yaw != 0 || defaultInputs.pitch != 0 || defaultInputs.roll != 0 ||
                defaultInputs.throttle != 0 || defaultInputs.takeOff || defaultInputs.land)
            {
                if (currentInputSource != null)
                {
                    Debug.LogWarning("Inputs detected from Default Input Source, aborting override");
                    currentInputSource.Abort();
                    currentInputSource = null;
                }
            }

            currentInputs = currentInputSource == null
                ? (_headLessMode ? ConvertToHeadlessInputs(defaultInputs) : defaultInputs)
                : currentInputSource.GetInputValues();

            elvInput = currentInputs.throttle;
            yawInput = currentInputs.yaw;

            if (currentInputs.takeOff && _flightStatus == FlightStatus.PreLaunch)
                Takeoff();
            else if (currentInputs.land)
                Land();
        }

        #endregion

        #region Transform / Sensor Logic

        protected void OnTransformUpdated()
        {
            onTransformChanged?.Invoke(transform.localPosition, transform.localRotation);
        }

        private void OnFlightStatusChanged(FlightStatus newStatus)
        {
            _flightStatus = newStatus;
        }

        public void SetVirtualPosition(QuadcopterData quadData)
        {
            deltaHeight = _prevHeight - quadData.height;
            _prevHeight = quadData.height;
            transform.localPosition = new Vector3(quadData.posX, quadData.height + heightOffset, quadData.posZ);
            transform.localRotation = Quaternion.Euler(quadData.gyroPitch, quadData.gyroYaw, quadData.gyroRoll);
        }

        public void ResetOffset() => assumedHeightOffset = 0;

        public void ResetKnownOffset() => heightOffset = 0;

        #endregion
    }
}