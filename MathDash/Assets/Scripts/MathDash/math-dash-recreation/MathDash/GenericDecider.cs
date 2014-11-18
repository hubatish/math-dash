using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class GenericDecider<T>
    {
        public List<T> items = new List<T>();
        protected int curItem = 0;

        public virtual T GetItem()
        {
            curItem = (curItem + 1);
            if(curItem>=items.Count)
            {
                //keep looping so we don't break
                curItem = curItem % items.Count;
                if(NoMoreItems!=null)
                {
                    //do whatever when no more
                    NoMoreItems();
                }
            }
            return items[curItem];
        }

        //What do I do if there are no more items in list
        public Action NoMoreItems = null;

        /// <summary>
        /// Read entries from specified fileName
        /// Loads those entries into items list
        ///     Side effect: resets curItem counter so can go through all of the new items
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>True if an error occurred/file didn't exist or had no items in it</returns>
        public bool LoadFromFile(string fileName)
        {
            items = FileInput.Instance.ReadAndDeserialize<T>(fileName).ToList();
            bool gotSomething = (items.Count != 0);
            if(gotSomething)
            {
                ResetPosition();
            }
            return gotSomething;
        }

        protected virtual void ResetPosition()
        {
            curItem = 0;
        }

        /// <summary>
        /// Write the current items List to a file
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveToFile(string fileName)
        {
            FileInput.Instance.WriteAndSerialize<T>(items, fileName);
        }
    }
}
