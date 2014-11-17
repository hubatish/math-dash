using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class BubbleSpawner : MonoBehaviour
    {
        protected BubbleFactory factory;

        protected void Start()
        {
            factory = BubbleFactory.Instance;
            whichPosition = gameObject.GetComponent<PositionDecider>();
            whichNumber = gameObject.GetComponent<INumberDecider>();
        }

        public void SpawnBlock()
        {
            factory.SpawnBubble(whichNumber.GetNumber(), whichPosition.GetPosition());
        }

        protected INumberDecider whichNumber;
        protected PositionDecider whichPosition;
    }
}
