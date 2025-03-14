using RoboticsToolkit.Robotics.RoboticControllers;
using UnityEngine;

namespace RoboticsToolkit.Robotics.QuadrupedRobot
{
    public class SimulatedQuadruped : Quadruped
    {
        float hipAngle = 70;
        float kneeAngle = -130;

        protected enum SubStatus
        {
            NotReady,
            WaitingForInitialLimbPlacement,
            WaitingForPhysics,
            Ready,
        }
        protected SubStatus _subStatus = SubStatus.NotReady;

        private float _physicsInitializedTime = -1;

        protected override void Awake()
        {
            base.Awake();
            ToggleColliders(false);
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnBootup()
        {
            base.OnBootup();

            var limbValues = new LimbValues(Vector3.zero, new float[3] { 0, hipAngle, kneeAngle });
            SetLimbs(new LimbValues[4] { limbValues, limbValues, limbValues, limbValues });
            _subStatus = SubStatus.WaitingForInitialLimbPlacement;
        }

        void Update()
        {
            switch (_subStatus)
            {
                case SubStatus.NotReady:
                    break;
                case SubStatus.WaitingForInitialLimbPlacement:
                    bool allServosReady = true;
                    foreach (var limb in m_limbs)
                    {
                        if (!(limb as QuadrupedLeg).SegmentsAtTarget(.3f))
                        {
                            allServosReady = false;
                        }
                    }
                    if (allServosReady)
                    {
                        var height = transform.position.y - GetLowestFoot().y;

                        GetComponent<ArticulationBody>().TeleportRoot(new Vector3(transform.position.x, transform.parent.position.y + height + .05f, transform.position.z), transform.rotation);
                        ToggleColliders(true);
                        GetComponent<ArticulationBody>().immovable = false;

                        _physicsInitializedTime = Time.timeSinceLevelLoad;
                        _subStatus = SubStatus.WaitingForPhysics;
                    }
                    break;
                case SubStatus.WaitingForPhysics:
                    if (Time.timeSinceLevelLoad > _physicsInitializedTime + 2)
                    {
                        CompleteBootup();
                        _subStatus = SubStatus.Ready;
                    }
                    break;
                default:
                    break;
            }
        }

        public override bool IsSimulation()
        {
            return true;
        }
    }
}
