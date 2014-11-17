using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public static class InputMouse
    {
        public static Vector3 worldPosition
        {
            get
            {
                Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPoint.z = 0; //Camera is in different plane than rest of game - it will return a position with same z value as camera
                return worldPoint;
            }
        }
    }
}
