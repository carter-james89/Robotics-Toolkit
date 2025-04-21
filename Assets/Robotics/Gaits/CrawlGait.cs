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
        private int[] _strideOrder = new int[4] { 0, 2, 1, 3 };
        private int _currentStride;

        private GaitCycleInfo BuildCycleInfo(int rotatingLimb)
        {
            var returnInfo = new GaitCycleInfo();
            returnInfo.RotatingLimbs = new int[] { rotatingLimb };

            var translatingLimbs = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                if (i != rotatingLimb)
                    translatingLimbs.Add(i);
            }

            returnInfo.TranslatingLimbs = translatingLimbs.ToArray();
            return returnInfo;
        }

        public override GaitCycleInfo GetGaitCycleInfo()
        {
            Debug.Log("Set stride : " + _currentStrideCount);
            return _currentStrideCount switch
            {
                0 => BuildCycleInfo(0),
                1 => BuildCycleInfo(2),
                2 => BuildCycleInfo(1),
                3 => BuildCycleInfo(3),
                4 => BuildCycleInfo(0),
                5 => BuildCycleInfo(0),
                6 => BuildCycleInfo(0),
                7 => BuildCycleInfo(0),
                _ => new GaitCycleInfo(),
            };
        }

        public override bool CheckLimbPositions(ILimbPositioner[] limbPositioners)
        {
            foreach (var positioner in limbPositioners)
            {
                if (positioner != limbPositioners[_rotatingLimb] && positioner.LimbAtTarget())
                    Debug.LogWarning("Translating limb is waiting to rotate");
            }

            var rotatingLimb = limbPositioners[_strideOrder[_currentStride]] as AdvancedLimbPositioner;
            if (rotatingLimb.LimbAtTarget())
            {
                Debug.Log("Rotating limb at target");

                NotifyListeners(GaitEventType.OnGaitPointHit);
                _rotatingLimb++;
                _currentStride++;

                if (_rotatingLimb == 4)
                {
                    _currentStride = 0;
                    _rotatingLimb = 0;
                    NotifyListeners(GaitEventType.OnGaitCycleComplete);
                }

                _currentStrideCount++;
                if (_currentStrideCount == 8)
                    _currentStrideCount = 4;

                return true;
            }

            return false;
        }

        public override void Reset()
        {
            base.Reset();
            _currentStride = 0;
        }

        public override float GetRotationSpeedMultiplier() => 1f;

        public override void Translate(ILimbPositioner[] limbPositioners, float speed, float strideLength, float strideHeight)
        {
            if (strideLength == 0)
            {
                var stationaryRotatingLimb = limbPositioners[_strideOrder[_currentStride]] as AdvancedLimbPositioner;
                stationaryRotatingLimb.RotateToPosition(stationaryRotatingLimb.GetGameObject().transform.position, speed, strideHeight);
                return;
            }

            var rotatingLimbIndex = _strideOrder[_currentStride];
            var rotatingLimb = limbPositioners[rotatingLimbIndex] as AdvancedLimbPositioner;

            float rotateDistance = strideLength / 2f;
            float translateDistance = strideLength / 2f;

            // Match time between arc and linear motion
            float timeToCompleteStride = (rotateDistance / 3f) / speed;

            // Move the rotating limb forward in an arc
            Vector3 rotateTarget = rotatingLimb.GetGameObject().transform.position +
                                   rotatingLimb.GetGameObject().transform.forward * rotateDistance;

            rotatingLimb.RotateToPositionViaTime(rotateTarget, strideHeight, timeToCompleteStride);

            // Translate all other limbs backward
            for (int i = 0; i < limbPositioners.Length; i++)
            {
                if (i == rotatingLimbIndex) continue;

                var translatingLimb = limbPositioners[i] as AdvancedLimbPositioner;
                Vector3 translateTarget = translatingLimb.GetGameObject().transform.position -
                                          translatingLimb.GetGameObject().transform.forward * translateDistance;

                translatingLimb.TranslateToPosition(translateTarget, timeToCompleteStride);
            }
        }

    }
}
