using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MathDash
{

    /// <summary>
    /// Decide which number should be spawned next
    /// "Intelligent" in that it takes current numbers on screen and in equation bar into account
    /// Bulk of code is from original MathDash project
    /// </summary>
	public class SmartNumberDecider : INumberDecider
	{
        public class SolutionSet
        {
            public int numberOne { get; set; }
            public int numberTwo { get; set; }
            public SolutionSet(int one, int two)
            {
                numberOne = one;
                numberTwo = two;
            }
        }

        //Try not to spawn more than this number for each atom
        public int MAX_ATOM_NUMBER_ON_SCREEN = 2;
        //This number could add to some other number to make solution
        public int ALL_SOLUTIONS_PRIORITY = 2;
        //priority given to numbers in the bubble
        public int CURRENT_SOLUTIONS_PRIORITY = 6;
        //public float ASSISTED_ATOM_GENERATION_DIFFICULTY_SCALAR = 1;
        public int ADJUSTED_GENERAL_SOLUTIONS_PRIORITY = 1;

        protected int last_spawned_atom = -1;

        protected Transform bubbleParent;

        protected void Start()
        {
            bubbleParent = BubbleParent.Instance.transform;
        }

        /// <summary>
        /// Find all the bubbles on the screen
        /// </summary>
        /// <returns></returns>
		private List<Bubble> GetAllAtoms()
		{
			List<Bubble> bubbles = new List<Bubble>();
			foreach(Transform t in bubbleParent)
			{
                Bubble bubbleScript = t.gameObject.GetComponent<Bubble>();
				bubbles.Add(bubbleScript);
			}
			return bubbles;
		}

        public EquationManager equationManager;

        public override int GetNumber()
        {
            return GetAtomNumber(equationManager.equation.solution, equationManager, GetAllAtoms());
        }

        /// <summary>
        /// Creates a newly generated atom based on a weighted table
        /// </summary>
        /// <param name="solution">The answer number of the currently shown solution</param>
        /// <param name="eq">A new equation</param>
        /// <param name="atom_list">The list of current atoms from AtomHandler, for collision checking</param>
        /// <returns>A new atom, psuedo-randomly generated</returns>
        private int GetAtomNumber(int? solution, EquationManager eq, List<Bubble> atom_list)
        {
            //getting weights is only really valid when there is a solution available
            List<SolutionSet> all_solutions = new List<SolutionSet>();
            List<int> current_solutions = new List<int>();

            if (solution != null)
            {
                int sol = solution ?? 0;
                all_solutions = getAllSolutionSets(sol, eq.equation.operation);
                current_solutions = getCurrentSolutionNumbers(eq, sol);
            }

            int[] atom_count = getScreenAtomCount(atom_list);
            List<int> weight_table = getAtomWeightTable(all_solutions, current_solutions, atom_count);

            int NumberIndex = UnityEngine.Random.Range(0, weight_table.Count);//Get next Number within the weight table
            int NextAtom = weight_table[NumberIndex];

            last_spawned_atom = NextAtom;

            return NextAtom;
        }

        /// <summary>
        /// Converts the current atoms on the screen into a integer hashtable of sorts
        /// </summary>
        /// <param name="atom_list">The current atom list from AtomHandler, for counting</param>
        /// <returns>An integer hashtable of the current atoms on the screen</returns>
        private int[] getScreenAtomCount(List<Bubble> atom_list)
        {
            int[] atom_count = new int[10];//0 through 9
            List<int> int_atom_list = new List<int>();

            // Convert the atom list into a number list
            foreach (Bubble a in atom_list)
            {
                int atom_number = a.number;
                int_atom_list.Add(atom_number);
            }

            // Populates atom_count with the current list of atom numbers
            for (int i = 0; i < atom_count.Length; i++)
            {
                for (int j = 0; j < int_atom_list.Count; j++)
                {
                    if (int_atom_list[j] == i)
                    {
                        atom_count[i]++;
                    }
                }
            }

            return atom_count;
        }

        /// <summary>
        /// Gets a weighted array/table that calculates percentages for what numbers to spawn
        /// </summary>
        /// <param name="all_solutions">All possible solutions</param>
        /// <param name="current_solutions">All current solutions with the numbers that are in the operand list</param>
        /// <param name="atom_list">List of atoms on the screen</param>
        /// <returns>The weighted atom generation table</returns>
        private List<int> getAtomWeightTable(List<SolutionSet> all_solutions, List<int> current_solutions, int[] atom_count)
        {
            List<int> weight_table = new List<int>();
            
            // All solutions
            foreach (SolutionSet ss in all_solutions)
            {
                // Declare ahead of time because we cross refferrence more
                int num1 = ss.numberOne;
                int num2 = ss.numberTwo;
                List<int> add_list = new List<int>();//Making an add list because it will increase performance due to its dynamic nature

                if (atom_count[num1] < MAX_ATOM_NUMBER_ON_SCREEN && num1 != last_spawned_atom)//if the atom doesn't already exist on the screen
                    add_list.Add(num1);
                if (atom_count[num2] < MAX_ATOM_NUMBER_ON_SCREEN && num1 != last_spawned_atom)//If the atom doesn't already exist on the screen
                    add_list.Add(num2);

                //Add a ticket for each number in the weighted table so that it's X times more likely to be picked
                for(int i=0;i<ALL_SOLUTIONS_PRIORITY;i++)
                {
                    //Add each item in the add list but only if its not already on the screen (calculated above)
                    foreach (int num in add_list)
                        weight_table.Add(num);
                }
            }

            //Current solutions
            foreach (int num in current_solutions)
            {
                if (atom_count[num] < MAX_ATOM_NUMBER_ON_SCREEN && num != last_spawned_atom)//If the atom isn't already on the screen
                {
                    //Add a ticket for each number in the weighted table so that it's X times more likely to be picked
                    for (int i = 0; i < CURRENT_SOLUTIONS_PRIORITY; i++)
                        weight_table.Add(num);
                }
            }

            //Make sure that at least 1 ticket for every number exists within the weight table (even if it already exists on the screen)
            for (int i = 1; i <= 9; i++)
            {
                if (!weight_table.Contains(i))
                {
                    for(int j=0;j<ADJUSTED_GENERAL_SOLUTIONS_PRIORITY;j++)//Used to adjust general solutions
                        weight_table.Add(i);
                }
            }

            return weight_table;
        }

        /// <summary>
        /// Returns every solution to the provided answer with the given Op
        /// </summary>
        /// <param name="solution">The current "AnswerAtom" solution proposed</param>
        /// <param name="op">The current opreator</param>
        /// <returns>A list of all possible solutions to the given solution with the Op</returns>
        private List<SolutionSet> getAllSolutionSets(int solution, Op op)
        {
            List<SolutionSet> ss = new List<SolutionSet>();

            for (int i = 1; i <= 9; i++)
            {
                for (int j = 1; j <= 9; j++)
                {
                    switch (op)
                    {
                        case Op.Add:
                            if(i+j == solution)
                                ss.Add(new SolutionSet(i, j));
                            break;
                        case Op.Subtract:
                            if(i-j == solution)
                                ss.Add(new SolutionSet(i, j));
                            break;
                        case Op.Multiply:
                            if(i*j == solution)
                                ss.Add(new SolutionSet(i, j));
                            break;
                        case Op.Divide:
                            if (i / j == solution)
                                ss.Add(new SolutionSet(i, j));
                            break;
                    }
                }
            }

            return ss;
        }

        /// <summary>
        /// Gets all the solution sets based on the locked in numbers/numbers in equation
        /// </summary>
        /// <param name="eq">Current equation populated with the locked in numbers</param>
        /// <param name="solution">Current Solution</param>
        /// <returns>List of all possible solution numbers to what's provided</returns>
        private List<int> getCurrentSolutionNumbers(EquationManager eq, int solution)
        {
            List<int> ss = new List<int>();

            if (eq.leftNumber!=null || eq.rightNumber!=null)//At least one of them has a number
            {
                Op op = eq.equation.operation;

                if (eq.leftNumber==null)
                {
                    int num2 = eq.rightNumber ?? 0;

                    for (int i = 1; i <= 9; i++)
                    {
                        switch (op)
                        {
                            case Op.Add:
                                if (i + num2 == solution)
                                    ss.Add(i);
                                break;
                            case Op.Subtract:
                                if (i - num2 == solution)
                                    ss.Add(i);
                                break;
                            case Op.Multiply:
                                if (i * num2 == solution)
                                    ss.Add(i);
                                break;
                            case Op.Divide:
                                if(i/num2==solution)
                                    ss.Add(i);
                                break;
                        }
                    }
                }
                else//Num2 is null
                {
                    int num1 = eq.leftNumber ?? 0;

                    for (int i = 1; i <= 9; i++)
                    {
                        switch (op)
                        {
                            case Op.Add:
                                if (num1 + i == solution)
                                    ss.Add(i);
                                break;
                            case Op.Subtract:
                                if (num1 - i == solution)
                                    ss.Add(i);
                                break;
                            case Op.Multiply:
                                if (num1 * i == solution)
                                    ss.Add(i);
                                break;
                            case Op.Divide:
                                if (num1 / i == solution)
                                    ss.Add(i);
                                break;
                        }
                    }
                }
            }
            return ss;
        }
	}
}