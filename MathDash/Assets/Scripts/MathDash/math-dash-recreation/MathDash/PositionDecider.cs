using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class PositionDecider : MonoBehaviour
    {
        public List<Vector3> positions = new List<Vector3>();
        protected int curPosition = 0;

        protected void Start()
        {
            if (positions.Count == 0)
            {
                for (int i = 0; i < 10;i++ )
                {
                    positions.Add(UnityEngine.Random.insideUnitCircle.normalized*UnityEngine.Random.Range(0,maxDist));
                }
            }
        }

        public float maxDist = 3f;

        public Vector3 GetPosition()
        {
            curPosition = (curPosition + 1) % positions.Count;
            return positions[curPosition];
        }
    }
}
