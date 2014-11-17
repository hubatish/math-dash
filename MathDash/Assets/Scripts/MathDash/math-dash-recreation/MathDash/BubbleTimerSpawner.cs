using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class BubbleTimerSpawner : MonoBehaviour
    {
        protected BubbleSpawner spawner;
        public float spawnTime = 2f;

        protected void Start()
        {
            Invoke("SpawnBlock", 0.1f);
            spawner = gameObject.GetComponent<BubbleSpawner>();
        }

        public void SpawnBlock()
        {
            spawner.SpawnBlock();
            Invoke("SpawnBlock", spawnTime);
        }
    }
}
