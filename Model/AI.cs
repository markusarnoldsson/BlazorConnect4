using System;
using System.IO;
using BlazorConnect4.Model;
using System.Collections.Generic;
using System.Linq;

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
        private GameEngine gameEngine;
        private int reward = 0;
        private Random rnd = new Random();
        private int turn = 0;

        private double[,] qTable;
        public double[,] QTable { get => qTable; }

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
        public QAgent(GameEngine gameEngine)
        {
            this.gameEngine = gameEngine;
            qTable = new double[21,7];
            turn = 0;
            for (int i = 0; i < qTable.GetLength(0); i++)
            {
                for (int j = 0; j < qTable.GetLength(1); j++)
                {
                    qTable[i, j] = 0;
                }
            }
        }


        public int[] GetValidActions(Cell[,] boardState)
        {
            List<int> validActions = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                if (IsValid(boardState, i))
                {
                    validActions.Add(i);
                }
            }
            return validActions.ToArray();
        }


        public QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            return temp;
        }
        public override int SelectMove(Cell[,] grid)
        {
            turn++;
            InitializeEpisode(grid);
            return 0;
        }

        private int TakeAction(Cell[,] currentBoard)
        {
            int[] validActions = GetValidActions(currentBoard);
            int randomIndex = rnd.Next(0, validActions.Length);
            int action = validActions[randomIndex];
            double gamma = 0.9;

            double saReward = GetReward(action);
            double nsReward = nextMaxValue(action);
            double qCurrentState = saReward + (gamma * nsReward);
            qTable[turn, action] = qCurrentState;
            int placement = action;
            return placement;
        }

        private double nextMaxValue(int action)
        {
            var nextTurn = turn + 1;
            double maxValue = 0;
            for (int i = 0; i < 7; i++)
            {
                if (maxValue < qTable[nextTurn, i])
                {
                    maxValue = qTable[nextTurn, i];
                }
            }
            return maxValue;
        }
        private double GetReward(int action)
        {
            return qTable[turn, action];
        }

        public int InitializeEpisode(Cell[,] initialBoard)
        {

            int recomendedMove = TakeAction(initialBoard);
            return recomendedMove;
        }
        private bool IsValid(Cell[,] grid, int col)
        {
            return grid[col, 0].Color == CellColor.Blank;
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
