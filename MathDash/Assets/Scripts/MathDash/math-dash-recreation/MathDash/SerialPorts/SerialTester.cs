using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class SerialTester : MonoBehaviour
    {
        protected SerialPort port;

        byte incoming;

        protected void Start()
        {
      //      BtConnector.isBluetoothEnabled();

            setup();
//            Serial.println("hey this is bluetooth?");
        }

        void setup() 
        { 
 //           Serial.begin(9600); 
        }

        void loop()
        {
   //         if (Serial.available() > 0)
            {
     ///           incoming = Serial.read(); // do whatever you want with (incoming) } }
            }
        }
    }
}
