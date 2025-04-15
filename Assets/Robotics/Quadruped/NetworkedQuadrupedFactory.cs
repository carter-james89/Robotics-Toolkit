using System.Collections;
using System.Collections.Generic;
using Toolkit.Networking;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Toolkit.Utilities.Events;


namespace RoboticsToolkit.Robotics.QuadrupedRobot
{
    public class NetworkedQuadrupedFactory : MonoBehaviour, IEventListener<UDPConnectionEventData>
    {
        [SerializeField]
        private UDPConnectionListener _connectionManager;

        [SerializeField]
        private QuadrupedModel _bittleModel;

        [SerializeField] private Quadruped _bittle;

        //List<IQuadruped> _preLoadedQuadrupeds = new List<IQuadruped>();

        private void Awake()
        {
            _connectionManager.SubscribeToEvents(this);
        }
        public void OnEventOccured(UDPConnectionEventData eventData)
        {
    
            Debug.Log("Factor event occured");
            if (eventData.ConnectionData.BoardType.ToLower().Contains("esp32"))
            {
                if (eventData.ConnectionData.Name.ToLower().Contains("bittle"))
                {
                    (_bittle as ESP32Quadruped).EstablishConnection(eventData);
                }
                else{
                    Debug.Log("Not a bittle");
                }
            }
            else
            {
                Debug.Log("Not a ESP32");
            }   
        }

    

        public GameObject GetGameObject()
        {
            throw new System.NotImplementedException();
        }

        public Component GetComponent()
        {
            throw new System.NotImplementedException();
        }
    }
}