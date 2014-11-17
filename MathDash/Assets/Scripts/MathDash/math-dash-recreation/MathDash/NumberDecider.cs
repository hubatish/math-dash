using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class NumberDecider : INumberDecider
    {
        public List<int> numbers = new List<int>();
        protected int curNumber = 0;

        protected void Start()
        {
            if(numbers.Count ==0)
            {
                numbers.Add(5);
                numbers.Add(2);
                numbers.Add(8);
                numbers.Add(4);
                numbers.Add(7);
                numbers.Add(3);
                numbers.Add(2);
                numbers.Add(9);
                numbers.Add(1);
                numbers.Add(6);
                numbers.Add(8);
                numbers.Add(5);
                numbers.Add(1);
                numbers.Add(7);
            }
        }

        public override int GetNumber()
        {
            curNumber = (curNumber+1)%numbers.Count;
            return numbers[curNumber];
        }
    }
}
