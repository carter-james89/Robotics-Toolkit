using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoboticsToolkit.Robotics
{
    public interface IRoboticController
    {
        public GameObject GetGameObject();
        public bool Initialize(IRobot robot);
        public bool SetTransformValues();

        public bool IsSimulator();

        public bool SendCommands(QuadrupedGroundStationData groundStationData);
    }

    public interface IRoboticControllerEventListener
    {
      
    }

    public class QuadrupedSensorData
    {
        public float Y;
        public float P;
        public float R;
        public float H;
        public float W;
        public float X;
        public float QX;
        public float Z;
        public int C;

        // public int FL_0;
        // public int FL_1;
        //public int FL_2;

        //public int FR_0;
        //public int FR_1;
        //public int FR_2;

        //public int BL_0;
        //public int BL_1;
        //public int BL_2;

        //public int BR_0;
        //public int BR_1;
        //public int BR_2;

    }
    public class QuadrupedGroundStationData
    {
        //public int[] Motors;
        // public int[] MotorPositions;


        public int FL0;
        public int FL1;
        public int FL2;

        public int FR0;
        public int FR1;
        public int FR2;

        public int BL0;
        public int BL1;
        public int BL2;

        public int BR0;
        public int BR1;
        public int BR2;
    }
}
