using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class ChangeLevelButton : MonoBehaviour
    {
        public int level;

        protected void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Application.loadedLevel == 1) //game level loaded currently
                {
                    PauseToolbox.Instance.GetComponent<PauseTimer>().Resume();
                }
                Application.LoadLevel(level);
            }
        }
    }
}
