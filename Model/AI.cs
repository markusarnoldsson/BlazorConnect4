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
        private Random rnd = new Random();
        private CellColor PlayerColor;
        private int turn = 0;

        //rewards
        //private float InvalidMoveReward = -0.1F;
        private float WinReward = 1F;
        private float LossReward = -1F;
        private float DrawReward = 0F;

        // game tracking
        private long wins = 0;
        private long losses = 0;
        private long ties = 0;
        private long nrOfGames = 0;

        private double[,] qTable;
        public double[,] QTable { get => qTable; }

        public QAgent(GameEngine gameEngine, CellColor playerColor)
        {
            this.gameEngine = gameEngine;
            if (playerColor == CellColor.Red)
            {
                PlayerColor = CellColor.Red;
            }
            else if (playerColor == CellColor.Yellow)
            {
                PlayerColor = CellColor.Yellow;
            }
            qTable = new double[21,7];
            turn = 0;
            for (int i = 0; i < qTable.GetLength(0); i++)
            {
                for (int j = 0; j < qTable.GetLength(1); j++)
                {
                    qTable[i, j] = rnd.NextDouble();
                }
            }
        }


        public int[] GetValidActions(Cell[,] boardState)
        {
            List<int> validActions = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                if (gameEngine.IsValid(i))
                {
                    validActions.Add(i);
                }
            }
            return validActions.ToArray();
        }


        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            return temp;
        }
        public override int SelectMove(Cell[,] grid)
        {
            int move = InitializeEpisode(grid);
            turn++;
            return move;
        }

        private int TakeAction(Cell[,] currentBoard)
        {
            int[] validActions = GetValidActions(currentBoard);
            int action = validActions[Exploration(validActions)];
            double gamma = 0.9;

            double saReward = GetReward(action);
            double nsReward = nextMaxValue(action);
            double qCurrentState = saReward + (gamma * nsReward);
            qTable[turn, action] = qCurrentState;
            int placement = action;
            return placement;
        }

        private int Exploration(int[] validActions)
        {
            int bestColumn = 0;
            double qValue = qTable[turn, bestColumn];
            for (int i = 0; i < validActions.Length; i++)
            {
               if (qTable[turn, validActions[i]] > qValue)
                {
                    bestColumn = i;
                }

            }
            return bestColumn;
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

        public void TrainAgent(AI opponent, int nrOfGames)
        {
            GameEngineAi gameEngineAi = new GameEngineAi();
            int opponentsAction;
            CellColor opponentsColor = GameEngineAi.OtherPlayer(PlayerColor);

            for (int i = 0; i < nrOfGames; i++)
            {
                nrOfGames++;

                //new game
                gameEngineAi.Reset();
                bool gameOver = false;

                if (PlayerColor == CellColor.Yellow)
                {
                    opponentsAction = rnd.Next(0, 7);
                    gameEngineAi.MakeMove(opponentsAction);
                }

                int action = SelectMove(gameEngine.Board.Grid);

                while(!gameOver)
                {
                    if (gameEngineAi.IsWin(gameEngine.Board, action, gameEngineAi.PlayerTurn))
                    {
                        qTable[turn, action] += WinReward;
                        gameOver = true;
                        wins++;
                    }
                    else if (gameEngineAi.IsDraw(gameEngine.Board, action))
                    {
                        qTable[turn, action] += DrawReward;
                        gameOver = true;
                        ties++;
                    }
                    else
                    {
                        GameBoard tempBoard = gameEngine.Board.CopyBoard();

                        GameEngineAi.MakeMove(ref tempBoard, PlayerColor, action);

                        opponentsAction = opponent.SelectMove(tempBoard.Grid);

                        if (gameEngineAi.IsWin(tempBoard, opponentsAction, opponentsColor))
                        {
                            qTable[turn, action] = LossReward;
                            losses++;
                            break;
                        }
                        else if(gameEngineAi.IsDraw(tempBoard, opponentsAction))
                        {
                            qTable[turn, action] = DrawReward;
                            ties++;
                            break;
                        }

                        gameEngineAi.MakeMove(action);
                        gameEngineAi.MakeMove(opponentsAction);

                        action = SelectMove(gameEngine.Board.Grid);

                    }
                }
            }
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
