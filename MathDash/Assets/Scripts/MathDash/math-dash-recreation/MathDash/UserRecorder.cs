using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    /// <summary>
    /// Record equations the user solves and bubbles they combine
    /// And writes that info to a file
    /// </summary>
    public class UserRecorder : Singleton<UserRecorder>
    {
        //The list of all solved equations
        protected IList<SolvedEquation> stats = new List<SolvedEquation>();
        public int curPeriod = 0;

        public Score score;

        //Info about the current equation
        protected SolvedEquation curStat;
        protected IList<Combination> combinations = new List<Combination>();

        public float timeOnEquation = 0f;

        public string fileName = "output";

        //Record combinations of bubbles
        public void CombineBubbles(int num1, int num2)
        {
            combinations.Add(new Combination
            {
                num1 = num1,
                num2 = num2
            });
        }

        //record the equations solved
        public void SolveEquation(bool correct, Equation equation, int leftSide, int rightSide)
        {
            //add to score
            if (correct)
            {
                score.SolveEquation(correct, combinations.Count);
            }

            //Record and write the data
            SolvedEquation eqn = new SolvedEquation
            {
                combinations = combinations,
                timeSpent = timeOnEquation,
                correct = correct,
                givenEquation = equation,
                leftSide = leftSide,
                rightSide = rightSide,
                period = curPeriod,
                score = score.score
            };
            stats.Add(eqn);
            WriteProgress();

            //reset variables for this equation
            combinations = new List<Combination>();
            timeOnEquation = 0f;
        }

        protected void Awake()
        {
            score = gameObject.GetComponent<Score>();
            fileName += DateTime.Now.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture) + ".json";
        }

        protected void Update()
        {
            timeOnEquation += Time.deltaTime;
        }

        //Start recording a new 30 second period
        public void SetPeriod(int period)
        {
            timeOnEquation = 0;
            curPeriod = period;

            //WriteProgress();
        }

        protected void OnApplicationQuit()
        {
            //WriteProgress();
        }

        public void WriteProgress()
        {
            FileInput.Instance.WriteAndSerialize<SolvedEquation>(stats, fileName);
        }
    }

    public class UserStats
    {
        //public int score = 0;
        public int equationsSolved = 0;
        public int equationsWrong = 0;
        public int period;
        public UserStats(int period)
        {
            this.period = period;
        }
    }

    public class SolvedEquation
    {
        public IList<Combination> combinations;
        public int period;
        public float timeSpent;
        public int leftSide;
        public int rightSide;
        public Equation givenEquation;
        public bool correct;
        public int score;
    }

    public class Combination
    {
        public int num1;
        public int num2;
    }
}
