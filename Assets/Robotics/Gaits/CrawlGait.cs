using RoboticsToolkit.Gimbal;
using RoboticsToolkit.Robotics;
using RoboticsToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics.Gaits
{
    public struct GaitCycleInfo
    {
        public int[] RotatingLimbs;
        public int[] TranslatingLimbs;
    }
    public class CrawlGait : Gait
    {
        private int _rotatingLimb = 0;

        private GaitCycleInfo BuildCycleInfo(int rotatingLimb)
        {
            var returnInfo = new GaitCycleInfo();
            returnInfo.RotatingLimbs = new int[1] { rotatingLimb };

            List<int> translatingLimbs = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                if (i != rotatingLimb)
                {
                    translatingLimbs.Add(i);
                }
            }
            returnInfo.TranslatingLimbs = translatingLimbs.ToArray();
            return returnInfo;
        }

        public override GaitCycleInfo GetGaitCycleInfo()
        {
            Debug.Log("Set stride : " + _currentStrideCount);
            switch (_currentStrideCount)
            {
                case 0:
                    return BuildCycleInfo(0);
                    break;
                case 1:
                    return BuildCycleInfo(2);
                    break;
                case 2:
                    return BuildCycleInfo(1);
                    break;
                case 3:
                    return BuildCycleInfo(3);
                    break;
                case 4://loops back here
                    return BuildCycleInfo(0);
                    break;
                case 5:
                    return BuildCycleInfo(0);
                    break;
                case 6:
                    return BuildCycleInfo(0);
                    break;
                case 7:
                    return BuildCycleInfo(0);
                    break;
                default:
                    break;
            }
            return new GaitCycleInfo();
        }


        public override bool CheckLimbPositions(ILimbPositioner[] limbPositioners)
        {
            foreach (var positioner in limbPositioners)
            {
                if(positioner != limbPositioners[_rotatingLimb] && positioner.LimbAtTarget())
                {
                    Debug.LogWarning("Translating limb is waiting to rotate");
                }
            }
            if (limbPositioners[_rotatingLimb].LimbAtTarget())
            {
                NotifyListeners(EventType.OnGaitPointHit);
                _rotatingLimb++;
                _currentStride++;
                if (_rotatingLimb == 4)
                {
                    _currentStride = 0;
                    _rotatingLimb = 0;
                    NotifyListeners(EventType.OnGaitCycleComplete);

                }

                _currentStrideCount++;
                if (_currentStrideCount == 8)
                {
                    _currentStrideCount = 4;
                }
                return true;
            }
            return false;
        }

        public override void Reset()
        {
            base.Reset();
            _currentStride = 0;
        }

        public override float GetRotationSpeedMultiplier()
        {
            return 1;
        }
        int[] _strideOrder = new int[4] { 0, 1, 2, 3 };
        int _currentStride;
        public override void Translate(ILimbPositioner[] limbPositioners, float speed, float strideLength, float strideHeight)
        {
            var rotatingLimb = limbPositioners[_strideOrder[_currentStride]] as AdvancedLimbPositioner;
            List<ILimbPositioner> translatingLimbs = new List<ILimbPositioner>();    
            for (int i = 0; i < limbPositioners.Length; i++)
            {
                if(limbPositioners[i] as AdvancedLimbPositioner != rotatingLimb)
                {
                    translatingLimbs.Add(limbPositioners[i]);
                }
            }

            //float closestDistance = 1000;
            //foreach (var limb in translatingLimbs)
            //{
            //    var distanceToTarget = Vector3.Distance((limb as AdvancedLimbPositioner).GetTargetGlobalPosition(), limb.GetGameObject().transform.position - limb.GetGameObject().transform.forward * strideLength / 2);
            //if(distanceToTarget < closestDistance)
            //    {
            //        closestDistance = distanceToTarget;
            //    }
            //}

            //float timeToCompleteStride = closestDistance / speed;

            //float closestDistance = 1000;
            //foreach (var limb in translatingLimbs)
            //{
            //    AdvancedLimbPositioner positioner = limb as AdvancedLimbPositioner;
            //    if (positioner != null)
            //    {
            //        Vector3 targetPosition = positioner.GetTargetGlobalPosition();
            //        Vector3 limbPosition = limb.GetGameObject().transform.position;
            //        Vector3 strideTarget = limbPosition - limb.GetGameObject().transform.forward * strideLength / 2;
            //        float distanceToTarget = Vector3.Distance(targetPosition, strideTarget);

            //        if (distanceToTarget < closestDistance)
            //        {
            //            closestDistance = distanceToTarget;
            //        }
            //    }
            //}

            //float timeToCompleteStride = closestDistance / speed;

          //  var strideSegment = strideLength / 2 / 3;

            float timeToCompleteStride = (strideLength/2/3) / speed;

            if (_currentStrideCount < 4)
            {
                switch (_currentStrideCount)
                {
                    case 0:
                        rotatingLimb.RotateToPositionViaTime(rotatingLimb.GetGameObject().transform.position + rotatingLimb.GetGameObject().transform.forward * strideLength / 2 / 3, strideHeight,timeToCompleteStride);
                        break;
                    case 1:
                        rotatingLimb.RotateToPositionViaTime(rotatingLimb.GetGameObject().transform.position + rotatingLimb.GetGameObject().transform.forward * strideLength / 2 / 3, strideHeight, timeToCompleteStride);
                        //nextInLine.TranslateToPosition(nextInLine.GetGameObject().transform.position - nextInLine.GetGameObject().transform.forward * strideLength / 2 / 3,speed);
                        //nextAfterThat.TranslateToPosition(nextAfterThat.GetGameObject().transform.position - nextAfterThat.GetGameObject().transform.forward * strideLength / 2 / 3, speed);
                        //lastStride.TranslateToPosition(lastStride.GetGameObject().transform.position - lastStride.GetGameObject().transform.forward * strideLength / 2 / 3, speed);

                        foreach (var limb in translatingLimbs)
                        {
                            (limb as AdvancedLimbPositioner).TranslateToPosition(limb.GetGameObject().transform.position - limb.GetGameObject().transform.forward * strideLength / 2, speed);
                        }
                        break;
                    case 2:
                        rotatingLimb.RotateToPositionViaTime(rotatingLimb.GetGameObject().transform.position + rotatingLimb.GetGameObject().transform.forward * strideLength / 2 / 3, strideHeight, timeToCompleteStride);
                        //nextInLine.TranslateToPosition(nextInLine.GetGameObject().transform.position - nextInLine.GetGameObject().transform.forward * strideLength / 2 / 3,speed);
                        //nextAfterThat.TranslateToPosition(nextAfterThat.GetGameObject().transform.position - nextAfterThat.GetGameObject().transform.forward * strideLength / 2 / 3, speed);
                        //lastStride.TranslateToPosition(lastStride.GetGameObject().transform.position - lastStride.GetGameObject().transform.forward * strideLength / 2 / 3, speed);

                        foreach (var limb in translatingLimbs)
                        {
                            (limb as AdvancedLimbPositioner).TranslateToPosition(limb.GetGameObject().transform.position - limb.GetGameObject().transform.forward * strideLength / 2, speed);
                        }
                        break;
                    case 3:
                        rotatingLimb.RotateToPositionViaTime(rotatingLimb.GetGameObject().transform.position + rotatingLimb.GetGameObject().transform.forward * strideLength / 2 / 3, strideHeight, timeToCompleteStride);
                        //nextInLine.TranslateToPosition(nextInLine.GetGameObject().transform.position - nextInLine.GetGameObject().transform.forward * strideLength / 2 / 3,speed);
                        //nextAfterThat.TranslateToPosition(nextAfterThat.GetGameObject().transform.position - nextAfterThat.GetGameObject().transform.forward * strideLength / 2 / 3, speed);
                        //lastStride.TranslateToPosition(lastStride.GetGameObject().transform.position - lastStride.GetGameObject().transform.forward * strideLength / 2 / 3, speed);

                        foreach (var limb in translatingLimbs)
                        {
                            (limb as AdvancedLimbPositioner).TranslateToPosition(limb.GetGameObject().transform.position - limb.GetGameObject().transform.forward * strideLength / 2, speed);
                        }
                        break;

                }
            }
            else
            {
                rotatingLimb.RotateToPositionViaTime(rotatingLimb.GetGameObject().transform.position + rotatingLimb.GetGameObject().transform.forward * strideLength / 2 / 3, strideHeight, timeToCompleteStride);
                //nextInLine.TranslateToPosition(nextInLine.GetGameObject().transform.position - nextInLine.GetGameObject().transform.forward * strideLength / 2 / 3,speed);
                //nextAfterThat.TranslateToPosition(nextAfterThat.GetGameObject().transform.position - nextAfterThat.GetGameObject().transform.forward * strideLength / 2 / 3, speed);
                //lastStride.TranslateToPosition(lastStride.GetGameObject().transform.position - lastStride.GetGameObject().transform.forward * strideLength / 2 / 3, speed);

                foreach (var limb in translatingLimbs)
                {
                    (limb as AdvancedLimbPositioner).TranslateToPosition(limb.GetGameObject().transform.position - limb.GetGameObject().transform.forward * strideLength / 2, speed);
                }
            }

       
        }
    }
}
