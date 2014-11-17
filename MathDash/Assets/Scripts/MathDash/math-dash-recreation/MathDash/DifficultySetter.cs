using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class DifficultySetter : MonoBehaviour
    {
        public class SpawningNumberConstants
        {
            //Try not to spawn more than this number for each atom
            public int MAX_ATOM_NUMBER_ON_SCREEN = 2;
            //This number could add to some other number to make solution
            public int ALL_SOLUTIONS_PRIORITY = 2;
            //priority given to numbers in the bubble
            public int CURRENT_SOLUTIONS_PRIORITY = 6;
            //public float ASSISTED_ATOM_GENERATION_DIFFICULTY_SCALAR = 1;
            public int ADJUSTED_GENERAL_SOLUTIONS_PRIORITY = 1;
        }

        public class BlockDifficulty
        {
            public int level;
            public BlockDifficulty(int l)
            {
                level = l;
            }
        }

        protected SmartNumberDecider numberDecider;

        protected string difficultyFile = "spawningDifficulty";

        public EquationDecider equationDecider;
        protected string equationsFile = "equations";

        protected GenericDecider<BlockDifficulty> blockDecider = new GenericDecider<BlockDifficulty>();
        protected string blocksFile = "blockDificulty.json";

        protected void Start()
        {
            numberDecider = BubbleMakerToolbox.Instance.GetComponent<SmartNumberDecider>();
            LoadDifficulty(difficultyFile);

            blockDecider.NoMoreItems = () => { PauseToolbox.Instance.GetComponent<GameOver>().EndGame(); };
            if(!blockDecider.LoadFromFile(blocksFile))
            {
                blockDecider.items = new List<BlockDifficulty>
                {
                    new BlockDifficulty(1),
                    new BlockDifficulty(2),
                    new BlockDifficulty(3),
                    new BlockDifficulty(4)
                };
                blockDecider.SaveToFile(blocksFile);
            }
        }

        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadDifficulty(difficultyFile + "1");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadDifficulty(difficultyFile + "2");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                LoadDifficulty(difficultyFile + "3");
            }
        }

        public void GotoNextLevel()
        {
            blockDecider.GetItem();
        }

        public void LoadLevel(int level)
        {
            //Level 1: equation with a single unknown, answer is likely to be spawned
            //Level 2: equation with a single unknown, random spawning where answer is 
                        //more likely to be produced by combinations
            //Level 3: two unknowns, answer is likely to be spawned
            //Level 4: two unknowns, random spawning
            switch(level)
            {
                case 1:
                    LoadEquations(equationsFile + "1");
                    LoadDifficulty(difficultyFile + "1");
                    break;
                case 2:
                    LoadEquations(equationsFile + "1");
                    LoadDifficulty(difficultyFile + "2");
                    break;
                case 3:
                    LoadEquations(equationsFile + "2");
                    LoadEquations(difficultyFile + "1");
                    break;
                case 4:
                    LoadEquations(equationsFile + "2");
                    LoadDifficulty(difficultyFile + "2");
                    break;
                default:
                    LoadEquations(equationsFile+"1");
                    LoadDifficulty(difficultyFile+"1");
                    break;
            }
        }

        protected void LoadEquations(string fName)
        {
            if(!fName.Contains(".json"))
            {
                fName += ".json";
            }
            equationDecider.LoadFromFile(fName);
        }

        protected void LoadDifficulty(string fName)
        {
            if(!fName.Contains(".json"))
            {
                fName += ".json";
            }
            //read constants from file
            IEnumerable<SpawningNumberConstants> spawningNumbers = FileInput.Instance.ReadAndDeserialize<SpawningNumberConstants>(fName);
            if (spawningNumbers != null && spawningNumbers.Count() != 0)
            {
                SpawningNumberConstants constants = spawningNumbers.First();
                numberDecider.MAX_ATOM_NUMBER_ON_SCREEN = constants.MAX_ATOM_NUMBER_ON_SCREEN;
                numberDecider.ALL_SOLUTIONS_PRIORITY = constants.ALL_SOLUTIONS_PRIORITY;
                numberDecider.CURRENT_SOLUTIONS_PRIORITY = constants.CURRENT_SOLUTIONS_PRIORITY;
                numberDecider.ADJUSTED_GENERAL_SOLUTIONS_PRIORITY = constants.ADJUSTED_GENERAL_SOLUTIONS_PRIORITY;
            }
            else
            {
                //create file off current numbers
                SpawningNumberConstants constants = new SpawningNumberConstants
                {
                    MAX_ATOM_NUMBER_ON_SCREEN = numberDecider.MAX_ATOM_NUMBER_ON_SCREEN,
                    ALL_SOLUTIONS_PRIORITY = numberDecider.ALL_SOLUTIONS_PRIORITY,
                    CURRENT_SOLUTIONS_PRIORITY = numberDecider.CURRENT_SOLUTIONS_PRIORITY,
                    ADJUSTED_GENERAL_SOLUTIONS_PRIORITY = numberDecider.ADJUSTED_GENERAL_SOLUTIONS_PRIORITY
                };
                FileInput.Instance.WriteAndSerialize<SpawningNumberConstants>(new List<SpawningNumberConstants>() { constants }, fName);
            }
        }

    }
}
