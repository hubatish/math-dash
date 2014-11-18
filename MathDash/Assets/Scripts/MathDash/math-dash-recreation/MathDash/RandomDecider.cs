using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class RandomDecider<T> : GenericDecider<T>
    {

        //a 0 or 1 for every item - 1 if we've seen it
        protected HashSet<int> itemsRetrieved = new HashSet<int>();

        public T GetItem(int i)
        {
            itemsRetrieved.Add(i);
            return items[i];
        }

        public override T GetItem()
        {
            T item;
            int r = 0;
            if (itemsRetrieved.Count <= items.Count * 3f/4f)
            {
                do
                {
                    r = UnityEngine.Random.Range(0, items.Count);
                    item = items[r];
                }
                while (itemsRetrieved.Contains(r));
                return GetItem(r);
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (!itemsRetrieved.Contains(i))
                    {
                        return GetItem(i);
                    }
                }
                //we've visited all the things?
                if (NoMoreItems != null)
                {
                    //do whatever when no more
                    NoMoreItems();
                }
                return GetItem(0);
            }
        }

        protected override void ResetPosition()
        {
            itemsRetrieved = new HashSet<int>();
        }
    }
}
