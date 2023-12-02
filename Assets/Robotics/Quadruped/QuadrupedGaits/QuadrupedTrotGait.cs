using RoboticsToolkit.Robotics;
using RoboticToolkit.Robotics.Limbs;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticToolkit.Robotics.Gaits
{
    public interface IGaitEventListener
    {
        public enum EventType
        {
            OnGaitCycleBegin,
            OnGaitCycleComplete,
            OnGaitReturnedHome
        }
        public struct GaitEventData
        {
            public EventType EventType;
            public IRobot Robot;
            public IGait Gait;
            public GaitEventData(EventType eventType, IRobot robot, IGait gait)
            {
                EventType = eventType;
                Robot = robot;
                Gait = gait;
            }
        }
        public void OnGaitEventOccured(GaitEventData eventData);
    }
    public interface IGait
    {
        public enum Direction
        {
            NONE,
            Forward,
            Backward,
            RotatingClockwise,
            RotatingCounterClockwise,
            StrafeLeft,
            StrafeRight,
        }
        public void Initialize(IRobot robot);
        public void Begin();
        public void ReturnHome();
        public void Stop();
        public void RunGait();
        public void SetNextCycle(Vector3 direction, ILimbPositioner[] limbPositioners, bool rotate);
        public bool IsRunning();
        public void SubscribeToEvents(IGaitEventListener listener);
        public void UnubscribeFromEvents(IGaitEventListener listener);
    }

    public class QuadrupedTrotGait : MonoBehaviour, IGait
    {
        private int m_stridePosition = 1;

        private bool m_postStrideCooldown = false;
        private float m_postStrideCooldownTime = 0;
    

        private bool m_halfStride = false;
        private bool m_running = false;

        private IRoboticLimb[] m_limbs;

        private IRoboticLimb[] m_rotatingLimbs = new IRoboticLimb[2];
        private IRoboticLimb[] m_translatingLimbs = new IRoboticLimb[2];

        private float m_strideDistance = 0;
        private float m_strideTime = 0;
        private float m_postStrideCooldownTargetTime = .2f;

        public void SetStrideValues(float strideDistance, float strideTime, float strideCoolDownTime)
        {
            m_strideDistance = strideDistance;
            m_strideTime = strideTime;
            m_postStrideCooldownTargetTime = strideCoolDownTime;
        }

        private IRobot m_robot;

        public enum Direction
        {
            NONE,
            Forward,
            Backward,
            RotatingClockwise,
            RotatingCounterClockwise,
            StrafeLeft,
            StrafeRight,
        }
        private Direction m_direction = Direction.NONE;

        public void Initialize(IRobot robot)
        {
            m_robot = robot;
            m_limbs = robot.GetLimbs();
        }

        public void SetNextCycle()
        {
            switch (m_stridePosition)
            {
                case 0:
                    m_rotatingLimbs[0] = m_limbs[0];
                    m_rotatingLimbs[1] = m_limbs[2];
                    m_translatingLimbs[0] = m_limbs[1];
                    m_translatingLimbs[1] = m_limbs[3];
                    break;
                case 1:
                    m_rotatingLimbs[0] = m_limbs[1];
                    m_rotatingLimbs[1] = m_limbs[3];
                    m_translatingLimbs[0] = m_limbs[0];
                    m_translatingLimbs[1] = m_limbs[2];
                    break;
                default:
                    break;
            }

            if(m_direction == Direction.Forward || m_direction == Direction.Backward || m_direction == Direction.StrafeLeft || m_direction == Direction.StrafeRight)
            {
                float distance = m_strideDistance;
                float time = m_strideTime;
                if (m_halfStride)
                {
                    distance /= 2;
                    time /= 2;
                    m_halfStride = false;
                }
                foreach (var limb in m_rotatingLimbs)
                {
                    Vector3 direction = Vector3.zero;
                    switch (m_direction)
                    {
                        case Direction.NONE:
                            break;
                        case Direction.Forward:
                            direction = m_robot.GetGimbal().GetGameObject().transform.forward;
                            break;
                        case Direction.Backward:
                            direction = -m_robot.GetGimbal().GetGameObject().transform.forward;
                            break;
                        case Direction.StrafeLeft:
                            direction = -m_robot.GetGimbal().GetGameObject().transform.right;
                            break;
                        case Direction.StrafeRight:
                            direction = m_robot.GetGimbal().GetGameObject().transform.right;
                            break;
                        default:
                            break;
                    }
                    limb.GetPositioner().RotateToPosition(direction, m_robot.GetGimbal().GetGameObject().transform.up, distance, time - (time * .25f));
                }
                foreach (var limb in m_translatingLimbs)
                {
                    Vector3 direction = Vector3.zero;
                    switch (m_direction)
                    {
                        case Direction.NONE:
                            break;
                        case Direction.Forward:
                            direction = -m_robot.GetGimbal().GetGameObject().transform.forward;
                            break;
                        case Direction.Backward:
                            direction = m_robot.GetGimbal().GetGameObject().transform.forward;
                            break;
                        case Direction.StrafeLeft:
                            direction = m_robot.GetGimbal().GetGameObject().transform.right;
                            break;
                        case Direction.StrafeRight:
                            direction = -m_robot.GetGimbal().GetGameObject().transform.right;
                            break;
                        default:
                            break;
                    }
                    limb.GetPositioner().TranslateToPosition(direction, m_robot.GetGimbal().GetGameObject().transform.up, distance, time);
                }
            }
            else if(m_direction == Direction.RotatingClockwise || m_direction == Direction.RotatingCounterClockwise)
            {
                var distance = .04f;
                var time = .2f;
                if (m_halfStride)
                {
                    distance /= 2;
                    time /= 2;
                    m_halfStride = false;
                }
                if(m_direction == Direction.RotatingCounterClockwise)
                {
                    m_rotatingLimbs[0].GetPositioner().RotateToPosition(-m_robot.GetGimbal().GetGameObject().transform.right, Vector3.up, distance, time);
                    m_rotatingLimbs[1].GetPositioner().RotateToPosition(m_robot.GetGimbal().GetGameObject().transform.right, Vector3.up, distance, time);

                    m_translatingLimbs[0].GetPositioner().TranslateToPosition(m_robot.GetGimbal().GetGameObject().transform.right, m_robot.GetGimbal().GetGameObject().transform.up, distance, time);
                    m_translatingLimbs[1].GetPositioner().TranslateToPosition(-m_robot.GetGimbal().GetGameObject().transform.right, m_robot.GetGimbal().GetGameObject().transform.up, distance, time);
                }
                else
                {
                    m_rotatingLimbs[0].GetPositioner().RotateToPosition(m_robot.GetGimbal().GetGameObject().transform.right, Vector3.up, distance, time);
                    m_rotatingLimbs[1].GetPositioner().RotateToPosition(-m_robot.GetGimbal().GetGameObject().transform.right, Vector3.up, distance, time);

                    m_translatingLimbs[0].GetPositioner().TranslateToPosition(-m_robot.GetGimbal().GetGameObject().transform.right, m_robot.GetGimbal().GetGameObject().transform.up, distance, time);
                    m_translatingLimbs[1].GetPositioner().TranslateToPosition(m_robot.GetGimbal().GetGameObject().transform.right, m_robot.GetGimbal().GetGameObject().transform.up, distance, time);
                }
            }

            m_stridePosition++;
            if (m_stridePosition == 2)
            {
                m_stridePosition = 0;
            }
        }

        public void RunGait()
        {
            if (m_running)
            {
                List<ILimbPositioner> m_limbsAtTarget = new List<ILimbPositioner>();
                foreach (var limb in m_limbs)
                {
                    //limb.GetPositioner().Run();
                    //if (m_rotatingLimbs[0] == limb || m_rotatingLimbs[1] == limb)
                    //{
                    //    limb.RunLimb(true, true);
                    //}
                    //else
                    //{
                    //    limb.RunLimb(true, true);
                    //}
                    if (limb.GetPositioner().StrideComplete() == true)
                    {
                        //   Debug.Log(limb.GetPositioner().cu);
                        m_limbsAtTarget.Add(limb.GetPositioner());
                    }
                    else
                    {
                        // Debug.Log("Waiting for : " + limb.GetGameObject().name);
                    }
                }
                //Debug.Log(m_limbsAtTarget.Count);
                if (m_limbsAtTarget.Count >= 3)
                {
                    m_postStrideCooldown = true;
                }
            }

            if (m_postStrideCooldown)
            {
                m_postStrideCooldownTime += Time.deltaTime;
                if (m_postStrideCooldownTime >= m_postStrideCooldownTargetTime)
                {
                    m_postStrideCooldown = false;
                    m_postStrideCooldownTime = 0;
                    NotifyListeners(IGaitEventListener.EventType.OnGaitCycleComplete);
                    bool atHome = true;
                    foreach (var item in m_limbs)
                    {
                        if (item.GetIKTargetPos() != item.GetPositioner().GetGameObject().transform.position)
                        {
                            atHome = false;
                        }
                    }
                    if (atHome)
                    {
                        NotifyListeners(IGaitEventListener.EventType.OnGaitReturnedHome);
                    }
                }
            }
            //if (m_postStrideCooldown)
            //{
            //    // Debug.Log("cooldown");
            //    foreach (var limb in m_limbs)
            //    {
            //        limb.RunLimb(true, true);
            //    }
            //    return;
            //}
           
        }

        public bool IsRunning()
        {
            return m_running;
        }
        public void SetDirection(QuadrupedTrotGait.Direction direction)
        {
            m_direction = direction;
        }
        public void Begin(ILimbPositioner[] limbPositioners)
        {
            Debug.Log("Begin " + Time.frameCount);
            m_running = true;
            m_halfStride = true;
            SetNextCycle();
            NotifyListeners(IGaitEventListener.EventType.OnGaitCycleBegin);
        }
        public void Stop()
        {
            Debug.Log("Stop trot " + Time.frameCount);
            m_running = false;
        }
        public void ReturnHome()
        {
            Debug.Log("Trot return home " + Time.frameCount);
            m_halfStride = true;
        }
        private void NotifyListeners(IGaitEventListener.EventType eventType)
        {
            foreach (var item in m_listeners.ToArray())
            {
                item.OnGaitEventOccured(new IGaitEventListener.GaitEventData(eventType, m_robot, this));
            }
        }

        private List<IGaitEventListener> m_listeners = new List<IGaitEventListener>();
        public void SubscribeToEvents(IGaitEventListener listener)
        {
            if (listener != null && !m_listeners.Contains(listener))
            {
                m_listeners.Add(listener);
            }
        }

        public void UnubscribeFromEvents(IGaitEventListener listener)
        {
            if (listener != null && m_listeners.Contains(listener))
            {
                m_listeners.Remove(listener);
            }
        }

        public void SetNextCycle(ILimbPositioner[] limbPositioners)
        {
            
        }

        public void Begin()
        {
            throw new System.NotImplementedException();
        }

        public void SetNextCycle(Vector3 direction, ILimbPositioner[] limbPositioners, bool rotate)
        {
            throw new System.NotImplementedException();
        }
    }
}


