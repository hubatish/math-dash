using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class EquationHolder : Combinable
    {
        public Bubble bubble;
        public int? number 
        {
            get
            {
                if (bubble == null)
                    return null;
                return bubble.number;
            }
        }

        //The current number cannot be changed until it is deleted
        protected bool isPermanent;

        protected EquationManager equation;
        protected void Start()
        {
            equation = transform.parent.gameObject.GetComponent<EquationManager>();
        }

        public void Clear()
        {
            if(bubble!=null)
            {
                GameObject.Destroy(bubble.gameObject);
                bubble = null;
            }
            isPermanent = false;
            renderer.enabled = true;
        }

        public void SetPermanentNumber(int num)
        {
            //Add new permanent bubble
            GameObject bubbleGO = BubbleFactory.Instance.SpawnBubble(num, transform.position);
            Bubble bubble = bubbleGO.GetComponent<Bubble>();
            CombineWith(bubble);
            //make sure they can't move the bubble
            StuckEquationState stuckState = bubbleGO.AddComponent<StuckEquationState>();
            bubble.ChangeState(stuckState);
            isPermanent = true;
            if(bubble.number>9)
            {
                renderer.enabled = false;
            }
        }

        public void ReleaseBubble()
        {
            //release current bubble
            if (bubble != null)
            {
                bubble.ChangeState(bubble.GetComponent<HeldState>());
                bubble = null;
            }
        }

        //Put other bubble script into this equation
        public override void CombineWith(Bubble other)
        {
            //Don't switch if we have a permanent bubble already
            if(isPermanent && bubble!=null)
            {
                return;
            }

            //Debug.Log("holder combo ith bubble?"+other.number);
            //release current bubble
            ReleaseBubble();

            //Hold the new bubble
            bubble = other;
            InEquationState equationState = other.GetComponent<InEquationState>();
            other.ChangeState(equationState);
            equationState.holder = this;
            other.transform.position = transform.position;

            //attempt to solve the equation with the new bubble
            equation.AttemptSolve();
        }
    }
}
