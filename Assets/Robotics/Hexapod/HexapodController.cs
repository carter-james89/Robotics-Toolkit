using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexapodController : MonoBehaviour
{
    [SerializeField]
    private HexapodLeg _legPrefab;

    [SerializeField]
    private Transform _legHolder;

    private void Awake()
    {
        _legPrefab.gameObject.SetActive(false);
        foreach (var item in _legHolder.GetComponentsInChildren<Transform>())
        {
            if (item != _legHolder)
            {
                var newLeg = Instantiate(_legPrefab.gameObject, item.position, item.rotation).GetComponent<HexapodLeg>();
                newLeg.transform.SetParent(transform);
                newLeg.hip.GetComponent<HingeJoint>().connectedBody = transform.GetComponent<Rigidbody>();
                newLeg.gameObject.SetActive(true);

                newLeg.hip.transform.localRotation = Quaternion.identity;
            }


        }

    }
}
