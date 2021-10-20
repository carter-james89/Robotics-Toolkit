using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrottleDisplay : MonoBehaviour
{
    [SerializeField]
    private LineRenderer _fullBar;
    [SerializeField]
    private LineRenderer _throttleBar;

    [SerializeField]
    private Motor _motorToDisplay;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _throttleBar.SetPosition(1, new Vector3(0,_motorToDisplay.motorSpeed * 2,0));
    }
}
