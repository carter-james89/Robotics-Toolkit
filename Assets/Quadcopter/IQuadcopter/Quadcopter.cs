using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Toolkit.Utilities.Events;
using UnityEngine;
using UnityEngine.UI;

namespace FlightControllers.Quadcopters
{
    /// <summary>
    /// Base class for standard quadcopters, both real and simulated.
    /// Satisfies the requirements of <see cref="IQuadcopter"/>, and handles most of the heavy lifting for basic functions.
    /// </summary>
    public class Quadcopter : MonoBehaviour, IQuadcopter
    {
        [SerializeField] private LineRenderer _posXRay;
        [SerializeField] private LineRenderer _negXRay;
        [SerializeField] private LineRenderer _posYRay;
        [SerializeField] private LineRenderer _negYRay;
        [SerializeField] private LineRenderer _posZRay;
        [SerializeField] private LineRenderer _negZRay;
        [SerializeField] private float _maxRayLength = 10f;
        [SerializeField] private float maxRayLength = 10f;
        #region Events

        /// <summary>
        /// Event that is called whenever the GameObject's <see cref="Transform"/> is changed.
        /// </summary>
        /// <remarks>
        /// This needs to be called manually in <see cref="OnTransformUpdated"/> for all inherited classes.
        /// </remarks>
        public Action<Vector3, Quaternion> onTransformChanged;

        private InterfaceEventManager<QuadcopterEventData> _eventManager = new InterfaceEventManager<QuadcopterEventData>();

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

        [SerializeField] private Button _abortButton;
        [SerializeField] private TextMeshProUGUI _currentInputSourceText;
        [SerializeField] private TextMeshProUGUI _flightStatusText;
        [SerializeField] private TextMeshProUGUI _yawText;
        [SerializeField] private TextMeshProUGUI _pitchText;
        [SerializeField] private TextMeshProUGUI _rollText;
        [SerializeField] private TextMeshProUGUI _throttleText;

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
            if(_abortButton != null)
            {
                _abortButton.onClick.AddListener(() =>
                {
                    AttemptLand();
                });
            }
        }

        public void Update()
        {
            if (_flightController?.IsInitialized() == true)
            {
                ProcessInputs();
            }
            else
            {
                return;
            }
            if (m_initialized && !IsSimulator())
            {
                RunQuadcopterUpdate();
            }
            DrawAxisRays();
            if(_currentInputSourceText != null)
            {
                _currentInputSourceText.text = currentInputSource.ToString();
                _flightStatusText.text = _flightStatus.ToString();
            }
          
        }
        private void DrawAxisRays()
        {
            Vector3 _origin = transform.position;

            DrawRay(_posXRay, _origin, transform.right);
            DrawRay(_negXRay, _origin, -transform.right);
            DrawRay(_posYRay, _origin, transform.up);
            DrawRay(_negYRay, _origin, -transform.up);
            DrawRay(_posZRay, _origin, transform.forward);
            DrawRay(_negZRay, _origin, -transform.forward);
        }

        private void DrawRay(LineRenderer _line, Vector3 _origin, Vector3 _direction)
        {
            if (_line == null) return;

            RaycastHit _hit;
            Vector3 _end;

            if (Physics.Raycast(_origin, _direction, out _hit, _maxRayLength))
            {
                _end = _hit.point;
            }
            else
            {
                _end = _origin + _direction * _maxRayLength;
            }

            _line.SetPosition(0, _origin);
            _line.SetPosition(1, _end);
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
            if(flightController == null)
            {
                Debug.LogError("Flight controller is null : " + name);
            }
            _flightStatus = FlightStatus.PreLaunch;
            _flightController = flightController;
            _flightController.SubscribeToEvents(this);
            _flightController.Initialize(this);
            this.defaultInputSource = defaultInputSource;
            currentInputSource = defaultInputSource;

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
                currentInputSource = defaultInputSource;
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
        public virtual bool AttemptTakeoff()
        {
            bool success = _flightController.AttemptTakeoff();
            if(!success)
            {
                Debug.LogError("Takeoff failed");
                return false;
            }

            var quadData = _flightController.GetSensorData();
            SetVirtualPosition(quadData);
            ResetKnownOffset();
            SetHomePoint(transform.localPosition);
            NotifyEventListeners(QuadcopterEventType.TakeOff);
            return true;
        }

        /// <inheritdoc/>
        public virtual bool AttemptLand()
        {
           if(currentInputSource != defaultInputSource)
            {
                currentInputSource.Abort();
                RemoveInputOverride(currentInputSource);
            }
            if (!_flightController.AttemptLand())
            {
                return false;
            }
         
            return true;
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
        public void SubscribeToEvents(IEventListener<QuadcopterEventData> listenerToSubscribe) {
        _eventManager.AddListener(listenerToSubscribe);
        }

        /// <inheritdoc/>
        public void UnsubscribeFromEvents(IEventListener<QuadcopterEventData> listenerToUnsubscribe) { _eventManager.RemoveListener(listenerToUnsubscribe); }

        private void NotifyEventListeners(QuadcopterEventType eventType)
        {
            _eventManager.RaiseEvent(new QuadcopterEventData(eventType, this, GetSensorData()));
        }
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
                if (currentInputSource != null && currentInputSource != defaultInputSource)
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

            if(_yawText != null)
            {
                _yawText.text = currentInputs.yaw.ToString("F2");
                _pitchText.text = currentInputs.pitch.ToString("F2");
                _rollText.text = currentInputs.roll.ToString("F2");
                _throttleText.text = currentInputs.throttle.ToString("F2");
            }
          

            //if (currentInputs.takeOff && _flightStatus == FlightStatus.PreLaunch)
            //    AttemptTakeoff();
            //else if (currentInputs.land)
            //    AttemptLand();
        }

        #endregion

        #region Transform / Sensor Logic

        protected void OnTransformUpdated()
        {
            onTransformChanged?.Invoke(transform.localPosition, transform.localRotation);
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

        public void OnEventOccured(FlightControllerEventData eventData)
        {
            Debug.Log("Got event update from Fligth controller : " + eventData.EventType);
            switch (eventData.EventType)
            {
                case FlightControllerEventType.OnTakeOffBegin:
                    _flightStatus = FlightStatus.Launching;
                    break;
                case FlightControllerEventType.OnTakeOffEnd:
                    _flightStatus = FlightStatus.Flying;
                    break;
                case FlightControllerEventType.OnLandBegin:
                    _flightStatus = FlightStatus.Landing;
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}