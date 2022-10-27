using RoboticToolkit.Robotics.Limbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbHeightPositioner : MonoBehaviour
{
    [SerializeField]
    private ThreeJointRoboticLimb m_limbToSet;
    // Start is called before the first frame update
    void Start()
    {
        //  m_limbToSet = GetComponentInChildren<IRoboticLimb>();
    }

    // Update is called once per frame
    void Update()
    {
        m_limbToSet.SetLimbHeight(transform.position.y);
    }
}
