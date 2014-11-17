using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    /// <summary>
    /// In equation bar as part of solution - don't do anything
    /// A mostly unneccessary state
    /// </summary>
    public class StuckEquationState : BubbleState
    {
        protected override void Start()
        {
            base.Start();
        }
        public override void Move()
        {
            //do nothing
        }
        public override void Click()
        {
            //do nothing
        }
        public override void Release() { }
        public override void Activate()
        {
            renderer.enabled = false;
            rigidbody2D.isKinematic = true;
        }
        public override void Deactivate()
        {
            renderer.enabled = true;
            Debug.Log("and this too?");
            rigidbody2D.isKinematic = false;
        }
    }
}