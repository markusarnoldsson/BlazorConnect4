using System;
using System.IO;
using BlazorConnect4.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        private Dictionary<String, double[]> qDictionary;

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

            qDictionary = new Dictionary<string, double[]>();
        }


        public int[] GetValidMoves(Cell[,] boardState)
        {
            List<int> validMoves = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                if (gameEngine.IsValid(i))
                {
                    validMoves.Add(i);
                }
            }
            return validMoves.ToArray();
        }


        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            return temp;
        }
        public override int SelectMove(Cell[,] grid)
        {
            int[] validMoves = GetValidMoves(grid);
            int move = validMoves[Exploration(grid, validMoves)];

            return move;
        }

        private int Exploration(Cell[,] grid, int[] validMoves)
        {
            int bestColumn = 0;
            double qValue = GetReward(grid, bestColumn);

            for (int i = 0; i < validMoves.Length; i++)
            {
                if (GetReward(grid, i) > qValue)
                {
                    bestColumn = i;
                }

            }
            return bestColumn;
        }


        private double GetReward(Cell[,] grid, int move)
        {
            String key = GameBoard.GetHashCodeAsString(grid);
            if (qDictionary.ContainsKey(key))
            {
                return qDictionary[key][move];
            }
            else
            {
                double[] moves = { rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() };
                qDictionary.Add(key, moves);
            }
            return 0;
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
                        //qTable[turn, action] += WinReward;
                        gameOver = true;
                        wins++;
                    }
                    else if (gameEngineAi.IsDraw(gameEngine.Board, action))
                    {
                        //qTable[turn, action] += DrawReward;
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
                            //qTable[turn, action] = LossReward;
                            losses++;
                            break;
                        }
                        else if(gameEngineAi.IsDraw(tempBoard, opponentsAction))
                        {
                            //qTable[turn, action] = DrawReward;
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
