using System;
using System.Collections;
using System.Collections.Generic;
using  QuadcopterUtilities;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField] private float _loiterTime = 1;

    internal float GetLoiterTime()
    {
        return _loiterTime;
    }
}
