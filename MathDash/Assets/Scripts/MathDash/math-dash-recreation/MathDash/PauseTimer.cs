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
        protected Timer scoreTimer;

        public float gameTime = 40f;
        public float plusTime = 5f;
        public float scoreTime = 2f;

        //did the game just start?
        protected float timeSoFar = 0f;

        //handle the GUI
        public GameObject plusSign;
        public GUITimer gui;

        public TextMesh scorePrefab;
        protected TextMesh spawnedPrefab;

        //Set Unity timeScale to 0 to stop other scripts, but keep going using real time
        protected float lastRealTime;
        protected float prevTimeScale;

        protected PeriodManager periodManager;

        protected void Start()
        {
            //initialize timers
            gameTimer = new Timer(gameTime);
            gameTimer.Done = DoneGame;
            //gui.StartTimer(gameTime);
            plusTimer = new Timer();
            plusTimer.Done = DonePause;
            scoreTimer = new Timer();
            scoreTimer.Started = delegate()
            {
                //also display the score for a second
                spawnedPrefab = (TextMesh)GameObject.Instantiate(scorePrefab);
                spawnedPrefab.text = "Your Score: " + UserRecorder.Instance.score.score;
            };
            scoreTimer.Done = delegate()
            {
                GameObject.Destroy(spawnedPrefab.gameObject);
            };

            lastRealTime = Time.realtimeSinceStartup;

            periodManager = gameObject.GetComponent<PeriodManager>();
            
            //pause to start with
            DoneGame();
            //Invoke("DoneGame", 0.03f);
        }


        protected void Update()
        {
            //Update the timers with fake delta time since pause sets timeScale to 0
            float deltaTime = Time.realtimeSinceStartup - lastRealTime;
            gameTimer.Update(deltaTime);
            plusTimer.Update(deltaTime);
            scoreTimer.Update(deltaTime);
            lastRealTime = Time.realtimeSinceStartup;

            timeSoFar += deltaTime;
        }

        public void DoneGame()
        {
            //Done with game, so start pause timer and pause game
            plusTimer.Start(plusTime);
            Pause();
            plusSign.SetActive(true);

            //We're starting a new period!  Handled by another script
            periodManager.StartNewPeriod();

            if(timeSoFar>0.5)
            {
                //also display score
                scoreTimer.Start(scoreTime);
            }
        }

        public void DonePause()
        {
            //done with pause, so start game timer and unpause
            gameTimer.Start(gameTime);
            plusSign.SetActive(false);
            Resume();
            gui.StartTimer(gameTime);
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
    }
}
