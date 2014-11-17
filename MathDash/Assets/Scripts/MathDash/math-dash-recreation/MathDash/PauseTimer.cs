using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    /// <summary>
    /// Handles all aspects of Pausing and unpausing the game
    ///     Uses timers
    ///     Handles GUI bar/cross appearance
    ///     Sends out Serial port 1 or 2 based on entering or exiting game
    /// </summary>
    public class PauseTimer : MonoBehaviour
    {
        //timers
        protected Timer gameTimer;
        protected Timer plusTimer;

        public float gameTime = 40f;
        public float plusTime = 5f;

        //handle the GUI
        public GameObject plusSign;
        public GUITimer gui;

        //Set Unity timeScale to 0 to stop other scripts, but keep going using real time
        protected float lastRealTime;
        protected float prevTimeScale;

        //For writing to serial ports
        protected SerialPort port;
        protected byte[] pauseByte;
        protected byte[] startByte;

        protected PeriodManager periodManager;

        protected void Start()
        {
            //initialize timers
            gameTimer = new Timer(gameTime);
            gameTimer.Done = DoneGame;
            gui.StartTimer(gameTime);
            plusTimer = new Timer();
            plusTimer.Done = DonePause;

            lastRealTime = Time.realtimeSinceStartup;

            periodManager = gameObject.GetComponent<PeriodManager>();

            //Initialize port data
            string[] ports = SerialPort.GetPortNames();
            string portName = "COM3";
            if(ports.Length>0)
            {
                portName = ports[0];
            }
            port = new SerialPort(portName,9600,Parity.None,8,StopBits.One);
            try
            {
                port.Open();
            }
            catch(Exception ex)
            {
                Debug.Log("Error opening port: "+ ex.Message);
            }
            pauseByte = new byte[1];
            startByte = new byte[1];
            pauseByte[0] = 1;
            startByte[0] = 2;
            
            //pause to start with
            DoneGame();
        }


        protected void Update()
        {
            //Update the timers with fake delta time since pause sets timeScale to 0
            float deltaTime = Time.realtimeSinceStartup - lastRealTime;
            gameTimer.Update(deltaTime);
            plusTimer.Update(deltaTime);
            lastRealTime = Time.realtimeSinceStartup;
        }

        public void DoneGame()
        {
            //Done with game, so start pause timer and pause game
            plusTimer.Start(plusTime);
            Pause();
            plusSign.SetActive(true);
            //We're starting a new period!  Handled by another script
            if(Time.realtimeSinceStartup>0.5)
            {
                periodManager.StartNewPeriod();
                WriteToPort("2\n");
            }
        }

        public void DonePause()
        {
            //done with pause, so start game timer and unpause
            gameTimer.Start(gameTime);
            plusSign.SetActive(false);
            Resume();
            gui.StartTimer(gameTime);

            //let the serial port know we're starting again
            WriteToPort("1\n");
        }

        public void Pause()
        {
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0;
        }

        public void GameOver()
        {
            Pause();
            plusSign.SetActive(false);
        }

        public void Resume()
        {
            Time.timeScale = prevTimeScale;
        }

        protected void WriteToPort(byte[] bytes)
        {
            if(port.IsOpen)
            {
                port.Write(bytes, 0, 1);
            }
        }

        protected void WriteToPort(string info)
        {
            if (port.IsOpen)
            {
                port.Write(info);
            }
        }
    }
}
