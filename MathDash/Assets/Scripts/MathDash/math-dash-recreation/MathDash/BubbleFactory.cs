using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class BubbleFactory : Singleton<BubbleFactory>
    {
        public GameObject bubblePrefab;
        public Transform bubbleParent;

        public GameObject SpawnBubble(int number, Vector3 position)
        {
            GameObject g = GameObject.Instantiate(bubblePrefab, position, Quaternion.identity) as GameObject;
            g.GetComponent<Bubble>().number = number;
            if(bubbleParent!=null)
            {
                g.transform.parent = bubbleParent;
            }
            return g;
        }
    }
}
