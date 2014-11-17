using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class GUITimer : MonoBehaviour
    {
        protected Vector3 startPos;
        protected Vector3 startScale;

        protected void Start()
        {
            startPos = transform.position - new Vector3(transform.collider2D.bounds.extents.x,0f,0f);
            startScale = transform.localScale;
        }

        public float curTime = -1f;
        public float maxTime = 1f;

        public void StartTimer(float time)
        {
            maxTime = time;
            curTime = maxTime;
        }

        protected void Update()
        {
            if(curTime>=0)
            {
                transform.localScale = new Vector3(startScale.x, startScale.y * curTime / maxTime, startScale.z);
                transform.position = startPos + new Vector3(transform.collider2D.bounds.extents.x, 0f, 0f);
                curTime -= Time.deltaTime;
            }
        }
    }
}
