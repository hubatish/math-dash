using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class LayerSetter : MonoBehaviour
    {
        public int layerId;

        protected void Start()
        {
            renderer.sortingLayerID = layerId;
        }
    }
}
