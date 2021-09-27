using System;
using System.IO;
using BlazorConnect4.Model;
using System.Threading.Tasks;

namespace BlazorConnect4.AIModels
{
    [Serializable]
    public abstract class AI
    {
        // Funktion för att bestämma vilken handling som ska genomföras.
        public abstract int SelectMove(Cell[,] grid);

        // Funktion för att skriva till fil.
        public virtual void ToFile(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, this);
            }
        }

        // Funktion för att att läsa från fil.
        protected static AI FromFile(string fileName)
        {
            AI returnAI;
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                returnAI = (AI)bformatter.Deserialize(stream);
            }
            return returnAI;

        }

    }

    [Serializable]
    public class QAgent : AI
    {
        GameEngine gameEngine;
        double[][] rewards = new double[7][]
        {
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0}
        };
        double[][] validActions = new double[7][]
        {
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0},
            new double[]{0, 0, 0, 0, 0, 0}
        };
        private struct bestAction
        {
            public int col;
            public int row;
            public int value;
            public bestAction(int col, int row, int value)
            {
                this.col = col;
                this.row = row;
                this.value = value;
            }
        }
        public QAgent(GameEngine gameEngine)
        {
            this.gameEngine = gameEngine;
        }
        public QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            return temp;
        }
        public override int SelectMove(Cell[,] grid)
        {
            //need to reset validactions after everymove
            GetValidActions(grid);

            return 0;
        }

        public static void TrainAgents(int numberOfIterations)
        {
            for (int i = 0; i < numberOfIterations; i++)
            {
                
            }
        }

        private bestAction EvaluateOptions()
        {
            bestAction bestOption = new bestAction(0, 0, 0);

             for (int i = 0; i < 7; i++)
            {
                for (int j = 5; j >= 0; j--)
                {
                    if (validActions[i][j] == 1)
                    {
                        if (gameEngine.IsWin(i, j))
                        {
                            bestOption.col = i;
                            bestOption.row = j;
                            bestOption.value = 1;
                        }
                        else
                        {
                            if (bestOption.value != 1)
                            {
                                bestOption.col = i;
                                bestOption.row = j;
                                bestOption.value = 0;
                            }
                        }
                        //check rewards matrix
                    }
                }
            }
            return bestOption;
        }
        private void GetValidActions(Cell[,] grid)
        {
            for (int i = 0; i < 7; i++)
            {
                for (int j = 5; j >= 0; j--)
                {
                    if (IsValid(grid, i, j))
                    {
                        validActions[i][j] = 1;
                    }
                    else if (j == 0)
                    {
                        validActions[i][j] = 0;
                    }
                }
            }
        }
        private bool IsValid(Cell[,] grid, int col, int row)
        {
            return grid[col, row].Color == CellColor.Blank;
        }
    }


    [Serializable]
    public class RandomAI : AI
    {
        [NonSerialized] Random generator;

        public RandomAI()
        {
            generator = new Random();
        }

        public override int SelectMove(Cell[,] grid)
        {
            return generator.Next(7);
        }

        public static RandomAI ConstructFromFile(string fileName)
        {
            RandomAI temp = (RandomAI)(AI.FromFile(fileName));
            // Eftersom generatorn inte var serialiserad.
            temp.generator = new Random();
            return temp;
        }
    }
}
