/*
* ==========================================
*  Bolt Class (Fastener Implementation)
* ------------------------------------------
*  Description:
* Moves the HMD to the provided position and rotation
*  
*  Author: Carter Egan  
*  Date: 3-10-25
*  ==========================================
*/

using UnityEngine;
using UnityEngine.XR;

public class HMDAlligner : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform rigOrigin; // The transform you move
    [SerializeField] private Transform hmd;       // The tracked HMD (child of rigOrigin)

    [SerializeField] private Transform _objectToMove;
    [SerializeField] private Transform _handOrigin;


    private void Start()
    {
        if (startPoint != null)
        {
            AlignRigToHMD(startPoint.position, startPoint.rotation);
        }
    }
    private void Update()
    {
        var rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        // If the device is valid, check for the primary button press
        if (rightHandDevice.isValid)
        {
            bool primaryButtonPressed;
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPressed) && primaryButtonPressed)
            {
                AlignRigToHMD(startPoint.position, startPoint.rotation);
                //_objectToMove.position = _handOrigin.position;
                //_objectToMove.rotation = _handOrigin.rotation;

                //var tempRot = _objectToMove.eulerAngles;
                //tempRot.z = 0;
                //tempRot.x = 0;
                //_objectToMove.eulerAngles = tempRot;
            }
        }
    }
    /// <summary>
    /// Moves the rigOrigin so that the HMD reaches the target position and rotation.
    /// </summary>
    public void AlignRigToHMD(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (rigOrigin == null || hmd == null)
        {
            Debug.LogError("Rig Origin or HMD reference is missing!");
            return;
        }

        // Calculate the offset: Where does the rig need to move to align the HMD?
        Vector3 positionOffset = targetPosition - hmd.position;

        // Move the rig to correct the offset
        rigOrigin.position += positionOffset;

        // Calculate rotational difference
        Quaternion rotationOffset = targetRotation * Quaternion.Inverse(hmd.rotation);

        // Apply rotation correction to the rig
        rigOrigin.rotation = rotationOffset * rigOrigin.rotation;

        var tempeuler = rigOrigin.eulerAngles;
        tempeuler.x = 0;
        tempeuler.z = 0;
        rigOrigin.eulerAngles = tempeuler;


    }
}
