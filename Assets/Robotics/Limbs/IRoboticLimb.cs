using UnityEngine;

namespace RoboticsToolkit.Robotics.Limbs
{
    public interface IRoboticLimb
    {
        public GameObject GetGameObject();
        public ILimbPositioner GetPositioner();
        public Transform GetEndPoint();
        public Transform GetTargetBasePosition();
        //public IServoController[] GetServoControllers();

        public IRoboticLimbSegment[] GetSegments();
        public void RunLimb(bool positionImmediate, bool adjustHeight = false);
        public void ResetLimb();
        public void ResetLimbTargetPosition();

        public void SetIKTargetPos(Vector3 globalPos);
        public Vector3 GetIKTargetPos();

        public bool LimbAtTarget();

        public bool BaseAtTarget();

    }}