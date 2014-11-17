using UnityEngine;
using System;
using System.Collections;

namespace MathDash
{
    public class Timer
    {

        public bool on = false;
        public float maxSteps;
        private float stepsLeft = 0;

        public Action Started;
        public Action Restarted;
        public Action Done;

        public Timer()
        {
            stepsLeft = 0;
            on = false;
        }

        public Timer(float steps)
        {
            //Debug.Log("Timer Started");
            maxSteps = steps;
            stepsLeft = maxSteps;
            on = true;
        }

        // Use this for initialization
        public void Start(float steps)
        {
            //	Debug.Log("Timer Started");
            maxSteps = steps;
            stepsLeft = maxSteps;
            on = true;

            if (Started != null)
                Started();
        }

        public void ReStart()
        {
            //Debug.Log("Timer ReStarted");
            on = true;
            stepsLeft = maxSteps;

            if (Restarted != null)
                Restarted();
        }

        // Update is called once per frame
        public bool Update()
        {
            return Update(Time.deltaTime);
        }

        public bool Update(float deltaTime)
        {
            if (on == true)
            {
                stepsLeft -= deltaTime;
                if (stepsLeft <= 0)
                {
                    //Debug.Log("Timer Done");
                    on = false;
                    if (Done != null)
                    {
                        Done();
                    }
                    return true;
                }
            }
            return false;
        }
    }

}

