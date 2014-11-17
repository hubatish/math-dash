using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class Score : MonoBehaviour
    {
        //Also handle the score
        public void AddToScore(int score)
        {
            this.score += score;
            scoreText.text = score.ToString();
        }
        public int score = 0;
        public TextMesh scoreText;

        protected void Start()
        {
            scoreText.text = "0";
        }

        public int correctScoreBonus = 20;
        public int incorrectScoreBonus = -20;
        public int combinationBonus = 10;
        public int firstCombinationBonus = 10;

        public void SolveEquation(bool correct, int numCombinations)
        {
            if(correct)
            {
                AddToScore(correctScoreBonus);
                if(numCombinations==1)
                {
                    AddToScore(firstCombinationBonus);
                }
                else
                {
                    AddToScore(combinationBonus * numCombinations);
                }
            }
            else
            {
                AddToScore(incorrectScoreBonus);
            }
        }

    }
}
