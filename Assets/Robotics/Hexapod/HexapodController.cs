using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexapodController : MonoBehaviour
{
    [SerializeField]
    private HexapodLeg _legPrefab;

    [SerializeField]
    private Transform _legHolder;

    [SerializeField]
    private Transform _legTargets;

    private List<HexapodLeg> _legs = new List<HexapodLeg>();

    [SerializeField]
    private HexapodLeg _leg1;
    [SerializeField]
    private HexapodLeg _leg2;
    [SerializeField]
    private HexapodLeg _leg3;
    [SerializeField]
    private HexapodLeg _leg4;
    [SerializeField]
    private HexapodLeg _leg5;
    [SerializeField]
    private HexapodLeg _leg6;
    [SerializeField]
    private Transform _gimbal;
    [SerializeField]
    private Transform _hipTargets;
    [SerializeField]
    private bool _positionGaitHeight;


    private void Awake()
    {
        //_legPrefab.gameObject.SetActive(false);
        var legTargets = _legHolder.GetComponentsInChildren<Transform>();

        //for (int i = 1; i < 7; i++)
        //{
        //    var newLeg = Instantiate(_legPrefab.gameObject, legTargets[i].position, legTargets[i].rotation).GetComponent<HexapodLeg>();
        //    newLeg.transform.SetParent(transform);
        //    // newLeg._hip.GetComponent<HingeJoint>().connectedBody = transform.GetComponent<Rigidbody>();
         
        //    newLeg.gameObject.SetActive(true);
        //    // newLeg._hip.transform.localRotation = Quaternion.identity;
        //    newLeg.Initialize();
        //   // newLeg.GetLegTarget().SetParent(_legTargets);
        //    _legs.Add(newLeg);
        //    switch (i)
        //    {
        //        case 0:
        //            _leg1 = newLeg;
        //            break;
        //        case 1:
        //            _leg2 = newLeg;
        //            break;
        //        case 2:
        //            _leg3 = newLeg;
        //            break;
        //        case 3:
        //            _leg4 = newLeg;
        //            break;
        //        case 4:
        //            _leg5 = newLeg;
        //            break;
        //        case 6:
        //            _leg6 = newLeg;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        _legs.Add(_leg1);
        _legs.Add(_leg2);
        _legs.Add(_leg3);
        _legs.Add(_leg4);
        _legs.Add(_leg5);
        _legs.Add(_leg6);


        foreach (var leg in _legs)
        {
            leg.AttachToRigidbody(transform.GetComponent<Rigidbody>());

            var newAnchor = new GameObject("Hip Target").transform;
            leg.SetHipTarget(newAnchor);
            newAnchor.SetParent(_hipTargets);

            leg.Initialize();

        }
    }

    private void Update()
    {
        _gimbal.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
       // _legTargets.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        //_hipTargets.transform.rotation = _gimbal.transform.rotation;

        if (_positionGaitHeight)
        {
            foreach (var leg in _legs)
            {
                leg.SetGaitHeight();
            }
        }
    }
}
