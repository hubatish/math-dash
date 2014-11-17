using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public abstract class Combinable : MonoBehaviour
    {
        public abstract void CombineWith(Bubble bubble);
    }
}
