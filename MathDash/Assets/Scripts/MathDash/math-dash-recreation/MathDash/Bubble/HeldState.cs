using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class HeldState : BubbleState
    {
        protected WandererState wanderer;
        protected override void Start()
        {
            wanderer = gameObject.GetComponent<WandererState>();
            base.Start();
        }

        public override void Move()
        {
            //Follow mouse
            transform.position = InputMouse.worldPosition;
        }
        public override void Click() { }
        public override void Release() 
        {
            //Debug.Log("bubble?"+onBubble+" other null?"+(otherBubble==null));
			if(onBubble)
            {
                //master.Combine(otherBubble);
                var script = otherBubble.GetComponent<Combinable>();
				//Debug.Log("does script==null? "+(script==null) + " ");
                if(script!=null)
                {
                    script.CombineWith(master);
                }
                else
                {
					//Debug.Log("we ran into "+otherBubble.name);
                }
            }
            else
            {
                master.ChangeState(wanderer);
            }
        }
        public override void Activate() 
        {
            collider2D.isTrigger = true;
            onBubble = false;
            otherBubble = null;
        }
        public override void Deactivate()
        {
            collider2D.isTrigger = false;
        }

        //HeldState specific code
        //we call CombineWith on the other bubble when release click
        public bool onBubble = false;
        public GameObject otherBubble = null;
        protected void OnTriggerStay2D(Collider2D col)
        {
           // Debug.Log("this trigger entered another");
            //Is the other object a bubble, and not the bubble we currently have?
            if (col.collider2D.CompareTag("Bubble") && col.gameObject!=otherBubble)
            {
                Bubble bubble = col.gameObject.GetComponent<Bubble>();
                if(bubble==null || !(bubble.state is InEquationState))
                {
                    onBubble = true;
                    otherBubble = col.gameObject;
                }
            }
        }
        protected void OnTriggerExit2D(Collider2D col)
        {
           // Debug.Log("releasing other");
            if (col.collider2D.CompareTag("Bubble"))
            {
                onBubble = false;
                otherBubble = null;
            }
        }
    }
}