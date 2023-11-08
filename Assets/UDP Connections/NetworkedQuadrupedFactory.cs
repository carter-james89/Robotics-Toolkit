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




        public void OnConnectionEventOccured(IUDPConnectionEventListener.EventData eventData)
        {
            Debug.Log("Factor event occured");
            //DynamicRoboticController dynamicRoboticController = new GameObject(eventData.ConnectionData.Name + " Robotic Controller").AddComponent<DynamicRoboticController>();
            //  ESP32Quadruped newRobot = new GameObject(eventData.Name + " Quadruped").AddComponent<ESP32Quadruped>();
            Debug.Log("what the fuck");
          //  dynamicRoboticController.transform.SetParent(transform, false);
            // newRobot.transform.SetParent(transform, false);
            Debug.Log("what the fuck2");
            if (eventData.ConnectionData.BoardType.ToLower().Contains("esp32"))
            {
                if (eventData.ConnectionData.Name.ToLower().Contains("bittle"))
                {
                    //var udpClient = new UdpClient(eventData.BroadcastPort);
                    //var remoteEndPoint = new IPEndPoint(eventData.BroadcastIP, eventData.BroadcastPort);
                    //byte[] sendBytes = Encoding.UTF8.GetBytes("Digital Twin Connection Established");
                    // udpClient.Send(sendBytes, sendBytes.Length, remoteEndPoint);
                    //udpClient.Close();
                    //var newConstructionData = new

                    //newRobot.BuildQuadruped(eventData, null);
                    //  dynamicRoboticController.ConstructQuadrupedTwin(_bittle);

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
            Debug.Log("what the fuck 3");
        }

        private void Awake()
        {
            _connectionManager.SubscribeToConnectionEvents(this);
        }
    }

}