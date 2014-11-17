using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class Bubble : MonoBehaviour
    {
        public BubbleState state;
        public int number = 1;
        public TextMesh bubbleText;

        protected void Start()
        {
            if(state==null)
            {
                //Start in wanderer state by default
                state = gameObject.GetComponent<WandererState>();
                ChangeState(state);
            }
            bubbleText.text = number.ToString();
        }
        
        //Change current state and enable/disable appriopriate components?
        public void ChangeState(BubbleState newState)
        {
            if(state!=null)
            {
                state.Deactivate();
                state.enabled = false;
            }
            state = newState;
            state.enabled = true;
            newState.Activate();
        }

        protected void Update()
        {
            state.Move();

            //really, only the held state needs a release button
            if (Input.GetMouseButtonUp(0))
            {
                state.Release();
            }
        }

        protected void OnMouseOver()
        {
            if(Input.GetMouseButtonDown(0))
            {
                state.Click();
            }
        }
    }
}
