using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class InEquationState : BubbleState
    {
        protected HeldState heldState;
        public EquationHolder holder;
        protected override void Start()
        {
            heldState = gameObject.GetComponent<HeldState>();
            base.Start();
        }
        public override void Move()
        {
            //do nothing
        }
        public override void Click()
        {
            master.ChangeState(heldState);
            holder.ReleaseBubble();
        }
        public override void Release() { }
        public override void Activate()
        {
            rigidbody2D.isKinematic = true;
        }
        public override void Deactivate()
        {
            rigidbody2D.isKinematic = false;
        }
    }
}