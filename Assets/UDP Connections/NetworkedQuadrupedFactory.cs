using System.Collections;
using System.Collections.Generic;
using Toolkit.NetworkUtilites;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Toolkit.Robotics.Quadruped
{
    public class NetworkedQuadrupedFactory : MonoBehaviour, IUDPConnectionEventListener
    {
        [SerializeField]
        private UDPConnectionListener _connectionManager;

        [SerializeField]
        private QuadrupedModel _bittleModel;

        [SerializeField] private Quadruped _bittle;

        //List<IQuadruped> _preLoadedQuadrupeds = new List<IQuadruped>();

        private void Awake()
        {
            _connectionManager.SubscribeToConnectionEvents(this);
        }

        public void OnConnectionEventOccured(IUDPConnectionEventListener.EventData eventData)
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

     
    }
}