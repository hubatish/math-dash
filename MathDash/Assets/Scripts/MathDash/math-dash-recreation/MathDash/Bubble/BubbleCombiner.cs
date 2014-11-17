using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class BubbleCombiner : Combinable
    {
        protected Bubble bubble;

        protected void Start()
        {
            bubble = gameObject.GetComponent<Bubble>();
        }

        public Vector3 spawnOffset = new Vector3(0.2f, 0, 0);

        //Combine this bubble with another, by adding
        public override void CombineWith(Bubble other)
        {
            //Add their results together
            int result = other.number + bubble.number;
            if (result <= 9)
            {
                //Spawn a bubble with number = result
                BubbleFactory.Instance.SpawnBubble(result, transform.position);
            }
            else
            {
                //if result two digit number, spawn one bubble for each digit
                int digitOne = result % 10;
                int digitTwo = result / 10;
                BubbleFactory.Instance.SpawnBubble(digitOne, transform.position + spawnOffset);
                BubbleFactory.Instance.SpawnBubble(digitTwo, transform.position - spawnOffset);
            }
            UserRecorder.Instance.CombineBubbles(bubble.number, other.number);
            //Destroy both bubbles
            GameObject.Destroy(other.gameObject);
            GameObject.Destroy(gameObject);
        }

    }
}
