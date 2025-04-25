using FlightControllers.Quadcopters;
using System;
using UnityEngine;
using UnityEngine.UI;

public class FlightControllerUI : MonoBehaviour
{
    [SerializeField] private Toggle _takeoffLandToggle;
    [SerializeField] private Toggle _autopilotToggle;
    [SerializeField] private Toggle _lidarModel;
    [SerializeField] private Button _beginWaypointMisson;

    [SerializeField] private MeshRenderer _lidarScan;

    [SerializeField] private Text _launchText;
    [SerializeField] private Text _autopilotText;

    [SerializeField] private Dropdown _quadOptions;
    [SerializeField] private Dropdown _autoPilotOptions;

    [SerializeField] private QuadcopterGroundControl _quadcopterGroundControl;

    void Start()
    {
        _takeoffLandToggle.onValueChanged.AddListener(OnTakeoffLandToggleChanged);
        _autopilotToggle.onValueChanged.AddListener(OnAutopilotToggleChanged);
        _lidarModel.onValueChanged.AddListener(OnLidarModelToggleChanged);
        _beginWaypointMisson.onClick.AddListener(OnBeginWaypointMissionClicked);

        // Init quad dropdown
        switch (_quadcopterGroundControl.GetGroundControlMode())
        {
            case QuadcopterGroundControl.GroundControlMode.RemoteQuadcopter:
                _quadOptions.value = (int)QuadcopterGroundControl.GroundControlMode.RemoteQuadcopter;
                break;
            case QuadcopterGroundControl.GroundControlMode.SiimulatedQuadcopter:
                _quadOptions.value = (int)QuadcopterGroundControl.GroundControlMode.SiimulatedQuadcopter;
                break;
            case QuadcopterGroundControl.GroundControlMode.Both:
                _quadOptions.value = (int)QuadcopterGroundControl.GroundControlMode.Both;
                break;
        }
        _quadOptions.onValueChanged.AddListener(OnQuadOptionsChanged);

        // Init autopilot dropdown
        switch (_quadcopterGroundControl.GetAutoPilotMode())
        {
            case QuadcopterGroundControl.AutoPilotMode.PID:
                _autoPilotOptions.value = (int)QuadcopterGroundControl.AutoPilotMode.PID;
                break;
            case QuadcopterGroundControl.AutoPilotMode.MLAgent:
                _autoPilotOptions.value = (int)QuadcopterGroundControl.AutoPilotMode.MLAgent;
                break;
        }
        _autoPilotOptions.onValueChanged.AddListener(OnAutoPilotOptionsChanged);
    }

    private void OnQuadOptionsChanged(int arg0)
    {
        _quadcopterGroundControl.SetGroundControlMode((QuadcopterGroundControl.GroundControlMode)arg0);
    }

    private void OnAutoPilotOptionsChanged(int arg0)
    {
        _quadcopterGroundControl.SetAutoPilotMode((QuadcopterGroundControl.AutoPilotMode)arg0);
    }

    private void OnBeginWaypointMissionClicked()
    {
        _quadcopterGroundControl.BeginWaypointMission();
    }

    private void OnLidarModelToggleChanged(bool arg0)
    {
        _lidarScan.enabled = arg0;
    }

    private void OnAutopilotToggleChanged(bool arg0)
    {
        _quadcopterGroundControl.ToggleAutoPilot();
    }

    private void OnTakeoffLandToggleChanged(bool arg0)
    {
        if (arg0)
        {
            _quadcopterGroundControl.TakeOff();
        }
        else
        {
            _quadcopterGroundControl.Land();
        }
    }

    private void Update()
    {
        _launchText.text = _quadcopterGroundControl.IsFlying() ? "LAND" : "LAUNCH";
        _autopilotText.text = _quadcopterGroundControl.IsAutoPilotActive() ? "DISABLE AUTOPILOT" : "ENABLE AUTOPILOT";
    }
}
