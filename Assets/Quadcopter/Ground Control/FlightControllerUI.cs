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

    // [SerializeField] private QuadcopterGroundControl _quadcopterGroundControl;
    [SerializeField] private Transform _lidarScan;

    [SerializeField] private Text _launchText;
    [SerializeField] private Text _autopilotText;


    [SerializeField] private Dropdown _quadOptions;


    [SerializeField] private QuadcopterGroundControl _quadcopterGroundControl;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _takeoffLandToggle.onValueChanged.AddListener(OnTakeoffLandToggleChanged);
        _autopilotToggle.onValueChanged.AddListener(OnAutopilotToggleChanged);
        _lidarModel.onValueChanged.AddListener(OnLidarModelToggleChanged);
        _beginWaypointMisson.onClick.AddListener(OnBeginWaypointMissionClicked);

        switch (_quadcopterGroundControl.GetGroundControlMode())
        {
            case QuadcopterGroundControl.GroundControlMode.Tello:
                _quadOptions.value = (int)QuadcopterGroundControl.GroundControlMode.Tello;
                break;
            case QuadcopterGroundControl.GroundControlMode.TelloSim:
                _quadOptions.value = (int)QuadcopterGroundControl.GroundControlMode.TelloSim;
                break;
            case QuadcopterGroundControl.GroundControlMode.TelloSimBoth:
                _quadOptions.value = (int)QuadcopterGroundControl.GroundControlMode.TelloSimBoth;
                break;
            case QuadcopterGroundControl.GroundControlMode.Picopter:
                break;
            case QuadcopterGroundControl.GroundControlMode.PicopterSim:
                break;
            case QuadcopterGroundControl.GroundControlMode.Ardupilot:
                break;
            case QuadcopterGroundControl.GroundControlMode.ArdupilotSim:
                break;
            default:
                break;
        }
        _quadOptions.onValueChanged.AddListener(OnQuadOptionsChanged);
    }

    private void OnQuadOptionsChanged(int arg0)
    {
        _quadcopterGroundControl.SetGroundControlMode((QuadcopterGroundControl.GroundControlMode)arg0);
    }

    private void OnBeginWaypointMissionClicked()
    {
        _quadcopterGroundControl.BeginWaypointMission();
    }

    private void OnLidarModelToggleChanged(bool arg0)
    {
        _lidarScan.gameObject.SetActive(arg0);
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
