using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class GameOver : MonoBehaviour
    {
        protected PauseTimer pause;
        protected void Start()
        {
            pause = PauseToolbox.Instance.GetComponent<PauseTimer>();
        }

        public GameObject endScreen;

        public void EndGame()
        {
            pause.GameOver();
            endScreen.SetActive(true);
        }
    }
}
