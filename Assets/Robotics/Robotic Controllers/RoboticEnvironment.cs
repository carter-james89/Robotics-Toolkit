using RoboticsToolkit.Robotics.RoboticControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{

    public class RoboticEnvironment : MonoBehaviour
    {

        private IRobot _myRobot;
        public IRobot GetRobot()
        {
            if(_myRobot == null)
            {
                _myRobot = GetComponentInChildren<IRobot>();
            }
            return _myRobot;    
        }


        private IRoboticController _myController;
        public IRoboticController GetController()
        {
            Debug.Log("Get Robotot from environment : " + name);
            if (_myController == null)
            {
                _myController = GetComponentInChildren<IRoboticController>();
            }
            return _myController;
        }
    }

}