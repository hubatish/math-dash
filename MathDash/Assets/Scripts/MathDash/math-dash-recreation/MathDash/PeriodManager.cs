using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class PeriodManager : MonoBehaviour
    {
        public EquationManager equationManager;

        protected int period = 0;
        public int numPeriods = 3;

        Transform bubbleParent;

        protected void Awake()
        {
            bubbleParent = GameObject.Find("AllBubbles").transform;
        }

        public void StartNewPeriod()
        {
            if(period>=numPeriods)
            {
                PauseToolbox.Instance.GetComponent<GameOver>().EndGame();
            }
            UserRecorder.Instance.SetPeriod(period);
            period += 1;

            //delete all the current bubbles
            foreach(Transform bubble in bubbleParent)
            {
                GameObject.Destroy(bubble.gameObject);
            }

            equationManager.GetNextEquation();
        }
    }
}
