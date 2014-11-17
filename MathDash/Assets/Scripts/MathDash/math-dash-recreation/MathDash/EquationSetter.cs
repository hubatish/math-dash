using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class EquationSetter : MonoBehaviour
    {
        protected EquationManager equation;

        protected RandomDecider<Equation> decider;
        public string fileName = "equations2.json";

        protected void Awake()
        {
            equation = gameObject.GetComponent<EquationManager>();
        }

        protected void Start()
        {
            decider = new RandomDecider<Equation>();
            //What to do when run out of equations in list
            decider.NoMoreItems = () => { PauseToolbox.Instance.GetComponent<GameOver>().EndGame(); };
            //Load equations from file
            if(!decider.LoadFromFile(fileName))
            {
                //error occurred
                SetDefaultList();
            }
        }

        //Use default list of equations
        //And save that list to file
        public void SetDefaultList()
        {
            decider.items = new List<Equation>
            {
                new Equation{
                    operation = Op.Add,
                    solution = 8
                },
                new Equation{
                    operation = Op.Subtract,
                    solution = 3
                },
                new Equation{
                    operation = Op.Add,
                    solution = 14
                },
                new Equation{
                    operation = Op.Divide,
                    solution = 3
                },
                new Equation{
                    operation = Op.Multiply,
                    solution = 24
                }
            };
            decider.SaveToFile(fileName);
        }

        public void SetNewEquation()
        {
            if(decider==null)
            {
                Invoke("SetNewEquation", 0.03f);
            }
            else
            {
                equation.SetEquation(decider.GetItem());
            }
        }
    }
}
