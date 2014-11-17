using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class BubbleNumberSpawner : MonoBehaviour
    {
        protected BubbleSpawner spawner;

        protected void Start()
        {
            spawner = gameObject.GetComponent<BubbleSpawner>();
            checkBubblesTimer = new Timer(checkTime);
        }

        public Timer checkBubblesTimer;
        public float checkTime = 2f;

        protected void Update()
        {
            //Only check the number of bubbles on screen every once in a while
            if(checkBubblesTimer.Update(Time.deltaTime))
            {
                checkBubblesTimer.Start(checkTime);
                
                //Check to see if we should spawn bubbles
                //The +2 is because both bubble holders in the equation have tag "bubble" - not the best design but oh well
                if(BubbleParent.Instance.transform.childCount<maxBubbles)
                {
                    SpawnBlock();
                }
            }
        }

        public int maxBubbles = 7;

        public void SpawnBlock()
        {
            spawner.SpawnBlock();
        }
    }
}
