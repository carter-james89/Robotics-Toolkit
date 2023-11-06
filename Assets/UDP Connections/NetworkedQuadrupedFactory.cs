using System.Collections;
using System.Collections.Generic;
using Toolkit.NetworkUtilites;
using UnityEngine;
using System.Net;

namespace Toolkit.Robotics.Quadruped
{
    public class NetworkedQuadrupedFactory : MonoBehaviour, IUDPConnectionEventListener
    {
        [SerializeField]
        private UDPConnectionListener _connectionManager;

        [SerializeField]
        private QuadrupedModel _bittleModel;

        [SerializeField] private Quadruped _bittle;


        public void OnConnectionEventOccured(IUDPConnectionEventListener.EventData eventData)
        {
            DynamicRoboticController dynamicRoboticController = new GameObject(eventData.Name + " Robotic Controller").AddComponent<DynamicRoboticController>();
         //  ESP32Quadruped newRobot = new GameObject(eventData.Name + " Quadruped").AddComponent<ESP32Quadruped>();

            dynamicRoboticController.transform.SetParent(transform, false);
            // newRobot.transform.SetParent(transform, false);

            if (eventData.Name.ToLower().Contains("esp32"))
            {
                if (eventData.Name.ToLower().Contains("bittle"))
                {
                    //var newConstructionData = new

                    //newRobot.BuildQuadruped(eventData, null);
                    dynamicRoboticController.ConstructQuadrupedTwin(_bittle);

                    (_bittle as ESP32Quadruped).EstablishConnection(eventData);
                }
            }

    

            

        }

        private void Awake()
        {
            _connectionManager.SubscribeToConnectionEvents(this);
        }
    }

}