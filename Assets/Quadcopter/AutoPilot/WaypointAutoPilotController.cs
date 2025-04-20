using System;
using Toolkit.Utilities.Events;
using UnityEngine;


namespace FlightControllers.Quadcopters
{
    public class WaypointAutoPilotController : MonoBehaviour, IEventListener<AutoPilotEventData>
    {
        private IAutoPilot _autopilot;

        /// <summary>
        /// How should the target drone move towardes its endpoint
        /// </summary>
        public enum TranslationStyle
        {
            /// <summary>
            /// When <see cref="SetNewTarget(Transform)"/> is called, the start point will be <see cref="_quadToControl"/>'s position,
            /// and the end point will be the provided <see cref="Transforms"/> position.
            /// This objects's transform will interpolate between those two points in a linear fashion according to <see cref="_linearSpeed"/>
            /// </summary>
            Linear,
            /// <summary>
            /// When <see cref="SetNewTarget(Transform)"/> is called, the start point will be <see cref="_quadToControl"/>'s position,
            /// and the end point will be the provided <see cref="Transforms"/> position.
            /// This objects's transform will interpolate between those two points in a non-linear fashion which will slow itself down
            /// as it approaches its final position. This is controlled by <see cref="_nonLinearSpeed"/>
            /// </summary>
            NonLinear,
            /// <summary>
            /// When <see cref="SetNewTarget(Transform)"/> is called, this objects transform will jump to that position, with no interpolation
            /// between a start and end point
            Instant,
        }
        /// <summary>
        /// The <see cref="TranslationStyle"/> that will be used to move this objects transform, can be updated on the fly via <see cref="SetTransitionSytle(TranslationStyle)"/>
        /// </summary>
        [SerializeField]
        public TranslationStyle translationStyle = TranslationStyle.Linear;
        // Add this field near the top of the class
        private float _atWaypointDuration = 0f;
    

        public bool atWaypoint { get; private set; }

        /// <summary>
        /// The speed at which this transform will interpolate between <see cref="_originalQuadPos"/> and <see cref="currentWaypoint"/>
        /// when in <see cref="TranslationStyle.Linear"/>
        /// </summary>
        [SerializeField]
        private float _linearSpeed = .5f;
        /// <summary>
        /// The speed at which this transform will interpolate between <see cref="_originalQuadPos"/> and <see cref="currentWaypoint"/>
        /// when in <see cref="TranslationStyle.NonLinear"/>
        /// </summary>
        [SerializeField]
        private float _nonLinearSpeed = .5f;

        /// <summary>
        /// The distance between the <see cref="_quadToControl"/> and <see cref="currentWaypoint"/> when <see cref="SetNewTarget(Transform)"/>
        /// </summary>
        private float _originalDistToTarget;
        /// <summary>
        /// The original position of <see cref="_quadToControl"/> when <see cref="SetNewTarget(Transform)"/>, used as the starting point for interpolation
        /// </summary>
        private Vector3 _originalQuadPos;

        /// <summary>
        /// The distance need to consider <see cref="_quadToControl"/> at <see cref="currentWaypoint"/>
        /// </summary>
        [SerializeField]
        private float _achieveTargetDist = .15f;
        /// <summary>
        /// Event to be raised when <see cref="_achieveTargetDist"/> is reached
        /// </summary>
        public Action<Waypoint> onWaypointAchieved;

        /// <summary>
        /// Event to be raised when new <see cref="_achieveTargetDist"/> is set
        /// </summary>
        public Action<Waypoint> onWaypointSet;

        /// <summary>
        /// The current target <see cref="transform"/> is heading towardes 
        /// </summary>
        public Waypoint currentWaypoint { get; private set; }


        public Component GetComponent()
        {
            return this;
        }

        public GameObject GetGameObject()
        {
           return this == null ? null : this.gameObject;
        }

        public void OnEventOccured(AutoPilotEventData eventData)
        {
            switch (eventData.EventType)
            {
                case AutoPilotEventType.OnAutoPilotInitialized:
                    break;
                case AutoPilotEventType.OnAutoPilotEngaged:
                 //   SetTransitionSytle(translationStyle);
                    if (currentWaypoint)
                    {
                        SetNewWaypoint(currentWaypoint);
                    }
                    break;
                case AutoPilotEventType.OnAutoPilotDisEngaged:
                    EndMission();
                    break;
                default:
                    break;
            }
        }
        private IQuadcopter _targetQuad;
        public void Initialize(IAutoPilot autoPilotToControl, IQuadcopter quadcopter)//not guaranteed to be the same
        {
            Debug.Log("Initializing WaypointAutoPilotController with Autopilot : " + autoPilotToControl.GetGameObject().name);
            _autopilot = autoPilotToControl;
            _autopilot.SubscribeToEvents(this);
            _targetQuad = quadcopter;
        }

        /// <summary>
        /// Set a new Target for <see cref="_quadToControl"/> to try and achieve
        /// </summary>
        /// <param name="newWaypoint">The target to match position and rotation</param>
        public void SetNewWaypoint(Waypoint newWaypoint)
        {
            if (enabled)
            {
                Debug.Log("Set new target point : " + newWaypoint.gameObject.name);
                atWaypoint = false;
             //   MatchQuadTransform();
                currentWaypoint = newWaypoint;
                _originalQuadPos = transform.position;
                _originalDistToTarget = Vector3.Distance(_originalQuadPos, currentWaypoint.transform.position);
             // _autopilot.PositionAutoPilot(t  (currentWaypoint.transform.rotation);
                onWaypointSet?.Invoke(newWaypoint);
            }
            else
            {
                Debug.LogWarning("Cannot set autopilot target before autopilot activatd, activate AutoPilot with 'P'");
            }
        }

        public WaypointMission currentMission { get; private set; }
        public void BeginMission(WaypointMission newMission)
        {
            newMission.OnMissionBegun(this);
            currentMission = newMission;
        }
        public void EndMission()
        {
            if (currentMission)
            {
                currentWaypoint = null;
                currentMission.EndMission();
                currentMission = null;
            }
        }

        void Update()
        {
            if (currentWaypoint)
            {
                Vector3 pos = _autopilot.GetGameObject().transform.position;
                switch (translationStyle)
                {        
                    case TranslationStyle.Linear:
                        var currentDist = Vector3.Distance(pos, currentWaypoint.transform.position);
                        var distTraveled = _originalDistToTarget - currentDist;
                        var fractTraveled = distTraveled / _originalDistToTarget;
                       pos  = Vector3.Lerp(_originalQuadPos, currentWaypoint.transform.position, fractTraveled + (Time.deltaTime * _linearSpeed));
                     
                        break;
                    case TranslationStyle.NonLinear:
                        //  transform.position = Vector3.Lerp(transform.position, currentWaypoint.transform.position, Time.deltaTime * _nonLinearSpeed);
                        float distToTarget = Vector3.Distance(pos, currentWaypoint.transform.position);

                        // Full speed until 0.5m away, then begin to slow
                        float slowdownStartDistance = 0.5f;
                        float slowdownFactor = Mathf.Clamp01(distToTarget / slowdownStartDistance); // 1 when far, 0 when very close

                        float adjustedSpeed = _nonLinearSpeed * slowdownFactor;
                        pos = Vector3.MoveTowards(
                            pos,
                            currentWaypoint.transform.position,
                            adjustedSpeed * Time.deltaTime
                        );
                        break;
                    case TranslationStyle.Instant:
                        pos = currentWaypoint.transform.position;
                        break;
                    default:
                        break;
             
                }
                _autopilot.PositionAutoPilot(pos, currentWaypoint.transform.rotation);

                //var distToFinalTarget = Vector3.Distance(quadToControl.GetGameObject().transform.position, currentWaypoint.transform.position);
                //if (distToFinalTarget < _achieveTargetDist && !atWaypoint)
                //{
                //   // (quadToControl as Quadcopter).ResetOffset();
                //  //  (quadToControl as Quadcopter).ResetKnownOffset();
                //    if (currentMission.IsFinalWaypoint(currentWaypoint))
                //    {
                //      //  (quadToControl as Quadcopter).DestroySensorPoints();
                //    }
                //    atWaypoint = true;
                //    onWaypointAchieved?.Invoke(currentWaypoint);
                //}
                float quadToWaypointDist = Vector3.Distance(_targetQuad.GetGameObject().transform.position, currentWaypoint.transform.position);

                if (quadToWaypointDist < _achieveTargetDist)
                {
                    _atWaypointDuration += Time.deltaTime;

                    if (_atWaypointDuration >= currentWaypoint.GetLoiterTime() && !atWaypoint)
                    {
                        atWaypoint = true;
                        onWaypointAchieved?.Invoke(currentWaypoint);

                        if (currentMission?.IsFinalWaypoint(currentWaypoint) == true)
                        {
                            // Optionally handle final waypoint logic here
                        }
                    }
                }
                else
                {
                    _atWaypointDuration = 0f; // reset if it leaves the radius
                }
            }
        
        }
    }
    
}
