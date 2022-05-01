using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAngle : MonoBehaviour
{
    public Transform t1;
    public Transform t2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var yawError = t1.localEulerAngles.y - t2.localEulerAngles.y;
        if (yawError < -180)
            yawError = 360 - System.Math.Abs(yawError);
        else if (yawError > 180)
            yawError = -(360 - yawError);
        Debug.Log(yawError);
    }
}
