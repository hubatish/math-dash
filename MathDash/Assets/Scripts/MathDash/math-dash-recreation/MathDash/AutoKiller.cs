using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class AutoKiller : MonoBehaviour
    {
        public float destroyTime = 0.7f;
        void Awake()
        {
            GameObject.Destroy(gameObject, destroyTime);
        }
    }
}
