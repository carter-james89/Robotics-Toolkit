using QuadcopterUtilities;
using System;
using UnityEngine;

public class SimulatedOnboardFlightController : MonoBehaviour, IFlightController
{
    private IQuadcopter _quadToControl;
    /// <summary>
    /// Rigidbody to control the physics of the simulator
    /// </summary>
    private Rigidbody rigidBody;
    /// <summary>
    /// "Aerodynamic" drag when the user is inputing control values
    /// </summary>
    [SerializeField]
    private float inputDrag;
    /// <summary>
    /// "Aerodynamic" drag when the user is not inputing control values
    /// </summary>
    [SerializeField]
    private float drag;

    public float timeSpeed = 1;

    private Action<IQuadcopter.FlightStatus> _onFlightStatusChanged;


    private bool _isInitialized;
    public bool IsInitialized()
    {
        return _isInitialized;
    }
    public bool IsReadyToFly()
    {
        return true;
    }

    public void Initialize(IQuadcopter quadToControl, Action<IQuadcopter.FlightStatus> onFlightStatusChanged)
    {
       // Debug.Log("initialize flight controller");
        _quadToControl = quadToControl;
        var physicsCalculator = new GameObject("Simulation Physics Simulation");
        physicsCalculator.transform.SetParent(_quadToControl.GetLocalTrackingSpace());
        physicsCalculator.transform.position = quadToControl.GetGameObject().transform.position;
        rigidBody = physicsCalculator.AddComponent<Rigidbody>();
        rigidBody.mass = quadToControl.GetGameObject().GetComponent<Rigidbody>().mass;
        rigidBody.useGravity = false;

        var boxCollider = rigidBody.gameObject.AddComponent<BoxCollider>();
        boxCollider.size = quadToControl.GetGameObject().GetComponent<BoxCollider>().size;
        boxCollider.center = quadToControl.GetGameObject().GetComponent<BoxCollider>().center;

        _onFlightStatusChanged = onFlightStatusChanged;

        Time.timeScale = timeSpeed;
        _isInitialized = true;
    }

    public Quaternion GetGyroRotation()
    {
        throw new System.NotImplementedException();
    }

    public IQuadcopter.QuadcopterData GetSensorData()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        //if (Physics.Raycast(rigidBody.transform.localPosition, rigidBody.transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        //{
        //    Debug.DrawRay(rigidBody.transform.localPosition, Vector3.down * hit.distance, Color.yellow);
        //}
      //  new Vector3(rigidBody.transform.localPosition.x, rigidBody.transform.localPosition.z), hit.distance, rigidBody.transform.rotation, rigidBody.velocity.y);

        var data = new IQuadcopter.QuadcopterData();

        data.posX = rigidBody.transform.localPosition.x;
        data.posY = rigidBody.transform.localPosition.y;
        data.posZ = rigidBody.transform.localPosition.z;

        data.gyroYaw = rigidBody.transform.localEulerAngles.y;
        data.gyroPitch = rigidBody.transform.localEulerAngles.x;
        data.gyroRoll = rigidBody.transform.localEulerAngles.z;

        // data.height = hit.distance;
        data.height = data.posY;
        data.VelocityVector = rigidBody.angularVelocity;

        return data;
    }

    /// <summary>
    /// All the physics for the simulator
    /// </summary>
    /// <remarks>
    /// Tried my best to tune the simulator to match real life Tello, but dont expect PID tunings for simulator to work for Tello
    /// </remarks>
    public void RunFixedUpdate()
    {
        if (_flightStatus != IQuadcopter.FlightStatus.PreLaunch)
        {
            rigidBody.AddForce(rigidBody.transform.up * 9.81f);
     
            bool receivingInput = false;
            var pitchInput = _craftInputs.pitch;
            rigidBody.AddForce(rigidBody.transform.forward * pitchInput);
            if (System.Math.Abs(pitchInput) > 0)
            {
                receivingInput = true;
            }
            var elvInput = _craftInputs.throttle;
            rigidBody.AddForce(rigidBody.transform.up * elvInput);
            if (System.Math.Abs(elvInput) > 0)
            {
                receivingInput = true;
            }
            var rollInput = _craftInputs.roll;
            rigidBody.AddForce(rigidBody.transform.right * rollInput);
            if (System.Math.Abs(rollInput) > 0)
            {

                receivingInput = true;
            }

            var yawInput = _craftInputs.yaw;
            rigidBody.AddTorque(rigidBody.transform.up * yawInput);
            if (System.Math.Abs(yawInput) > 0)
            {

                receivingInput = true;
            }

            if (receivingInput & rigidBody.drag != inputDrag)
            {
                rigidBody.drag = inputDrag;
                rigidBody.angularDrag = inputDrag;
            }
            else if (!receivingInput & rigidBody.drag != drag)
            {
                rigidBody.drag = drag;
                rigidBody.angularDrag = drag * .9f;
            }
        }
    }


    public bool IsSimulator()
    {
        return true;
    }

    public void Land()
    {
        rigidBody.transform.position = _quadToControl.GetGameObject().transform.position;

        _onFlightStatusChanged.Invoke(IQuadcopter.FlightStatus.Landing);
        _onFlightStatusChanged.Invoke(IQuadcopter.FlightStatus.PreLaunch);
    }

    private IInputs.FlightControlValues _craftInputs;
    private IQuadcopter.FlightStatus _flightStatus;
    public void Run(IQuadcopter.FlightStatus flightStatus, IInputs.FlightControlValues craftInputs)
    {
        _craftInputs = craftInputs;
        _flightStatus = flightStatus;
        RunFixedUpdate();
       // SetVirtualPosition(new Vector3(rigidBody.transform.localPosition.x, rigidBody.transform.localPosition.z), hit.distance, rigidBody.transform.localRotation, rigidBody.velocity.y);
    }

    public void Takeoff()
    {
        rigidBody.Move(rigidBody.transform.position + new Vector3(0, .8f, 0), transform.rotation);
        //.localPosition = rigidBody.transform.localPosition;
        rigidBody.transform.position = _quadToControl.GetGameObject().transform.position;
        rigidBody.useGravity = true;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        _onFlightStatusChanged.Invoke(IQuadcopter.FlightStatus.Launching);
        _onFlightStatusChanged.Invoke(IQuadcopter.FlightStatus.Flying);
    }
}
