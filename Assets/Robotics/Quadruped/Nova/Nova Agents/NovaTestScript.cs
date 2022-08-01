using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NovaTestScript : MonoBehaviour
{
    public Transform COM;
    public ArticulationBody ab;

    public Transform gait0;
    public Transform gait1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(COM != null)
        COM.localPosition = ab.centerOfMass;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            gait0.transform.localPosition = new Vector3(0, .05f, 0);
            gait1.transform.localPosition = new Vector3(0, .05f, 0);
        }
      //  ab.ba
        
    }
}
