using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class AssortedInput : MonoBehaviour
    {
        public string fileName = "timeConfig.json";

        protected PauseTimer pauser;
        protected PeriodManager periodManager;
        protected BubbleNumberSpawner bubbleSpawner;

        protected void Start()
        {
            pauser = PauseToolbox.Instance.GetComponent<PauseTimer>();
            periodManager = PauseToolbox.Instance.GetComponent<PeriodManager>();
            bubbleSpawner = BubbleMakerToolbox.Instance.GetComponent<BubbleNumberSpawner>();
            LoadTimes();
        }

        protected void LoadTimes()
        {
            IEnumerable<MiscTimes> times = FileInput.Instance.ReadAndDeserialize<MiscTimes>(fileName);
            if(times!=null && times.Count() > 0)
            {
                MiscTimes time = times.First();
                pauser.gameTime = time.gameTime;
                pauser.plusTime = time.plusTime;
                WandererState.destroyTime = time.bubbleDestroyTime;
                periodManager.numPeriods = time.numPeriods;
                bubbleSpawner.checkTime = time.bubbleSpawnTime;
                bubbleSpawner.maxBubbles = time.maxBubbles;
            }
            else
            {
                //nothing in the file, so write a new file with current values
                MiscTimes newTimes = new MiscTimes
                {
                    gameTime = pauser.gameTime,
                    plusTime = pauser.plusTime,
                    bubbleDestroyTime = WandererState.destroyTime,
                    numPeriods = periodManager.numPeriods,
                    bubbleSpawnTime = bubbleSpawner.checkTime,
                    maxBubbles = bubbleSpawner.maxBubbles
                };
                FileInput.Instance.WriteAndSerialize<MiscTimes>(new List<MiscTimes>(){newTimes},fileName);
            }
        }
    }

    public class MiscTimes
    {
        public float gameTime;
        public float plusTime;
        public float bubbleDestroyTime;
        public int numPeriods;
        public float bubbleSpawnTime;
        public int maxBubbles;
    }
}
