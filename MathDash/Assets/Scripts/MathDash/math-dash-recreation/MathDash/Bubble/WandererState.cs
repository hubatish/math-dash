using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class WandererState : BubbleState
    {
        protected HeldState holder;
        protected Timer destroyTimer = new Timer();
        public static float destroyTime = 19f;

        protected override void Start()
        {
            holder = gameObject.GetComponent<HeldState>();
            toMove = UnityEngine.Random.insideUnitCircle * speed;
            destroyTimer.Done = delegate { GameObject.Destroy(gameObject); };
            base.Start();
        }

        protected Vector3 toMove;
        public float speed = 0.4f;

        public override void Move()
        {
            //Move around randomly
            transform.position += toMove * Time.deltaTime;

            //Keep destroying self
            destroyTimer.Update();
        }

        public override void Click() 
        {
            master.ChangeState(holder);
        }
        public override void Release() { }
        public override void Activate() 
        {
            destroyTimer.Start(destroyTime);
        }
        public override void Deactivate() 
        {
            destroyTimer.on = false;
        }

        //
        /*protected void OnTriggerEnter2D(Collider2D col)
        {
            Debug.Log("other bubble's trigger entered");
            if (col.collider2D.CompareTag("Bubble"))
            {
                //col.gameObject.GetComponent<HeldState>().OnTriggerEnterCol(col);
            }
        }
        protected void OnTriggerExit2D(Collider2D col)
        {
            if (col.collider2D.CompareTag("Bubble"))
            {

                //col.gameObject.GetComponent<HeldState>().OnTriggerExitCol(col);
            }
        }*/

        public void OnCollisionEnter2D(Collision2D col)
        {
            //if(col!=null && col.contacts.Count()!=0)
            {
                //Vector3 diff = - col.transform.position + transform.position;
                //rigidbody2D.AddForce(diff.normalized * UnityEngine.Random.Range(1f, 6f));
                float force = UnityEngine.Random.Range(12f, 20f);
                rigidbody2D.AddForce(col.contacts[0].normal * force);
            }
        }

    }
}
