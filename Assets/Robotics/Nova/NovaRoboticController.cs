using RoboticToolkit.Robotics.Limbs;
using RoboticToolKit.Robotics.Servos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovaRoboticController : MonoBehaviour
{
    [SerializeField]
    private ThreeJointRoboticLimb m_frLimb;
    [SerializeField]
    private ThreeJointRoboticLimb m_flLimb;
    [SerializeField]
    private ThreeJointRoboticLimb m_brLimb;
    [SerializeField]
    private ThreeJointRoboticLimb m_blLimb;

    private List<ThreeJointRoboticLimb> m_limbs = new List<ThreeJointRoboticLimb>();

    private int m_stridePosition = 0;
    [SerializeField]
    private float m_strideLength = .05f;
    [SerializeField]
    private float m_strideHeight = .1f;
    [SerializeField]
    private Transform m_gaits;
    [SerializeField]
    private Transform m_baseTargets;
    [SerializeField]
    private float m_gaitTranslateSpeed = .1f;
    [SerializeField]
    private float m_mGaitRotationSpeed = 25;

    [SerializeField]
    private float m_pidMax = 10;
    [SerializeField]
    private float m_pidMin = -10;
    [SerializeField]
    private float m_pidP = .1f;
    [SerializeField]
    private float m_pidI = 0;
    [SerializeField]
    private float m_pidD = 0;

    private ArticulationBody m_articulationBody;

    // Start is called before the first frame update
    void Start()
    {
        m_articulationBody = GetComponent<ArticulationBody>();
        m_limbs.Add(m_blLimb);
        m_limbs.Add(m_brLimb);
        m_limbs.Add(m_flLimb);
        m_limbs.Add(m_frLimb);

        // m_gaits.transform.position = transform.position;
        m_gaits.transform.SetParent(null);
        m_baseTargets.transform.SetParent(null);
        foreach (var limb in m_limbs)
        {
            limb.GetGait().gameObject.name += "("+limb.name+")";
            //limb.GetGait().transform.SetParent(m_gaits);
            //var tempPos = limb.GetGait().transform.localPosition;
            //tempPos.y = 0;
            //limb.GetGait().transform.localPosition = tempPos;

            limb.GetBaseTarget().SetParent(m_baseTargets);
           var tempPos = limb.GetBaseTarget().localPosition;
            tempPos.y = 0;
            limb.GetBaseTarget().localPosition = tempPos;

            (limb.ShoulderServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
            (limb.ElbowServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
            (limb.WristServoController as PIDServoController).ResetPid(m_pidD, m_pidI, m_pidD, m_pidMax, m_pidMin);
        }


    }
   
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            m_articulationBody.immovable = false;
        }
        PositionGimble();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MoveToNextStride();
            //  m_stridePosition = 1;
            //   m_frLimb.GetGait().MoveToPosition(new Vector3(0, 0, .05f), 25f, RoboticToolKit.Robotics.Limbs.IGait.MovementStyle.Rotate);
            //   m_flLimb.GetGait().MoveToPosition(new Vector3(0, 0, -.05f), .1f, RoboticToolKit.Robotics.Limbs.IGait.MovementStyle.Translate);
            //   m_rrLimb.GetGait().MoveToPosition(new Vector3(0, 0, -.05f), .1f, RoboticToolKit.Robotics.Limbs.IGait.MovementStyle.Translate);
            //   m_rlLimb.GetGait().MoveToPosition(new Vector3(0, 0, -.05f), .1f, RoboticToolKit.Robotics.Limbs.IGait.MovementStyle.Translate);
        }

      
        if (m_stridePosition != 0)
        {
            bool strideComplete = true;
            foreach (var limb in m_limbs)
            {
                if (limb.LimbAtTarget() == false)
                {
                    strideComplete = false;
                }
            }
            if (strideComplete)
            {
                MoveToNextStride();
            }
        }

    }

    private void MoveToNextStride()
    {
        m_stridePosition++;
        if (m_stridePosition == 3)
        {
            m_stridePosition = 1;
        }
        switch (m_stridePosition)
        {
            case 1:
                m_frLimb.GetGait().RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed,m_strideHeight);
                m_blLimb.GetGait().RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);

                m_flLimb.GetGait().TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
                m_brLimb.GetGait().TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
                break;
            case 2:
                m_flLimb.GetGait().RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);
                m_brLimb.GetGait().RotateToPosition(new Vector3(0, 0, m_strideLength), m_mGaitRotationSpeed, m_strideHeight);

                m_frLimb.GetGait().TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
                m_blLimb.GetGait().TranslateToPosition(new Vector3(0, 0, -m_strideLength), m_gaitTranslateSpeed);
                break;
            default:
                break;
        }
    }
    private void FixedUpdate()
    {
        PositionGimble();
    }

    private void PositionGimble()
    {
        var tempPos = m_gaits.transform.position;
        tempPos.x = transform.position.x;
        tempPos.y = 0;
        tempPos.z = transform.position.z;
        m_gaits.transform.position = tempPos;

        var tempEuler = m_gaits.transform.eulerAngles;
        tempEuler.y = transform.eulerAngles.y;
        m_gaits.eulerAngles = tempEuler;

        tempPos = m_baseTargets.transform.position;
        tempPos.x = transform.position.x;
        tempPos.z = transform.position.z;
        m_baseTargets.transform.position = tempPos;
        //  m_baseTargets.transform.rotation = m_gaits.rotation;
        // m_baseTargets.transform.rotation = Quaternion.LookRotation(transform.forward);
       tempEuler = m_baseTargets.eulerAngles;
        tempEuler.y = transform.eulerAngles.y;
        m_baseTargets.eulerAngles = tempEuler;
    }
}
