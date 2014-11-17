using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public enum Op { Add, Subtract, Multiply, Divide };

    /// <summary>
    /// Ties together the equation with references to left and right numbers
    /// Displays solution & operation to screen
    /// Determines if equation is correct
    /// </summary>
    public class EquationManager : MonoBehaviour
    {
        //The two placeholders that contain bubbles
        public EquationHolder leftNum;
        public EquationHolder rightNum;
        public EquationHolder solutionNum;

        public int? leftNumber 
        {
            get
            {
                return leftNum.number;
            }
        }

        public int? rightNumber
        {
            get
            {
                return rightNum.number;
            }
        }

        //Solution and operator variables and GUI display elements
        public TextMesh textSoln;
        private int? solution
        {
            get 
            {
                return solutionNum.number;
                //return equation.solution; 
            }
        }
        public TextMesh textOperator;
        private Op curOperator
        {
            get { return equation.operation; }
        }

        public Equation equation = new Equation();

        protected EquationDecider setter;

/*        public void SetSolution(int sol)
        {
            equation.solution = sol;
            textSoln.text = equation.solution.ToString();
        }

        public void SetOperator(Op op)
        {
            equation.operation = op;
            textOperator.text = Equation.OpToSymbol(equation.operation);
        }*/

        /*public void SetPermanentNumber(EquationHolder)
        {
            equation.solution = sol;
            solutionNum.SetPermanentNumber(sol);
//            textSoln.text = equation.solution.ToString();
        }*/

        public void SetEquation(Equation equation)
        {
            this.equation = equation;
            textOperator.text = Equation.OpToSymbol(equation.operation);
            Action<EquationHolder,int?> setPermNum = delegate(EquationHolder holder, int? num)
            {
                if(num!=null)
                {
                    holder.SetPermanentNumber(num ?? 0);
                }
            };
            setPermNum(solutionNum, equation.solution);
            setPermNum(leftNum, equation.leftNum);
            setPermNum(rightNum, equation.rightNum);
        }

        public void GetNextEquation()
        {
            //clear equation and get new one
            leftNum.Clear();
            rightNum.Clear();
            solutionNum.Clear();
            setter.SetNewEquation();
        }

        protected void Start()
        {
            //textSoln.text = solution.ToString();
            equation.operation = Op.Add;
            textOperator.text = Equation.OpToSymbol(curOperator);
            setter = gameObject.GetComponent<EquationDecider>();
            setter.SetNewEquation();
        }

        public GameObject correctAnswer;
        public GameObject wrongAnswer;
        public Transform indicatorPos;

        public void AttemptSolve()
        {
            //Don't do anything if only one bubble in equation
            if (leftNumber != null && rightNumber != null && solution!=null)
            {
                //Check if equation was correct
                bool correct = CheckEquation();
                UserRecorder.Instance.SolveEquation(correct,equation,leftNum.number ?? -1,rightNum.number ?? -1);
                IndicateAnswer(correct);

                GetNextEquation();
            }
        }

        //Display correct or incorrect indicator
        protected void IndicateAnswer(bool correct)
        {
            if (indicatorPos != null)
            {
                if (correct && correctAnswer != null)
                {
                    GameObject.Instantiate(correctAnswer, indicatorPos.position, Quaternion.identity);
                }
                else if (wrongAnswer != null)
                {
                    GameObject.Instantiate(wrongAnswer, indicatorPos.position, Quaternion.identity);
                }
            }
        }

        protected bool CheckEquation()
        {
            //Debug.Log("left:" + leftNum.number + "right: " + rightNum.number);
            return (Equation.ComputeResult(leftNum.number ?? 100, rightNum.number ?? 100, curOperator) == solution);
        }
    }
}
