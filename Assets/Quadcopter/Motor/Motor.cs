using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motor : MonoBehaviour
{

    public bool useRandomness;
    [SerializeField]
    private Rigidbody _motorShaft;

    [SerializeField]
    private Rigidbody _quadBody;

    [SerializeField]
    private bool _counterMotor;

    public float motorSpeed = 1;

    private float _maxSpeed = 3f;

    private float _maxTorque = .003f;

    public void SetQuadBody(Rigidbody quadBody)
    {
        _quadBody = quadBody;
    }

    private void RunFixedUpdate()
    {


        // _motorShaft.AddTorque(_motorShaft.transform.up * .1f, ForceMode.Force);

        // _motorShaft.AddForce(Vector3.up);

        var force = motorSpeed * _maxSpeed;
        //  GetComponent<Rigidbody>().AddForce(transform.up * force, ForceMode.Force);

        float randomness = 0;
        if (useRandomness)
        {
            randomness = UnityEngine.Random.Range(-.01f, .01f);
        }

        _quadBody.AddForceAtPosition(transform.up * (force + randomness), transform.position);

        var torque = motorSpeed * _maxTorque;

        if (_counterMotor)
        {
            torque = -torque;
        }



        _quadBody.AddTorque(transform.up * torque, ForceMode.Impulse);
        // _quadBody.tor

    }

    internal void SetThrottle(float newThrottle)
    {
        newThrottle = Mathf.Clamp(newThrottle, 0, 1);
          motorSpeed = newThrottle;
        RunFixedUpdate();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
