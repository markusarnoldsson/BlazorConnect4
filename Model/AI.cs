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
        private CellColor PlayerColor;

        // Konstanter - Belöningar
        //private float InvalidMoveReward = -0.1F;
        private float WinReward = 1;
        private float LossReward = -1;
        private float DrawReward = 0;

        // Game tracking variabler
        public long wins = 0;
        public long losses = 0;
        public long ties = 0;
        public long nrOfGames = 0;

        //QTable
        private Dictionary<String, double[]> qDictionary;

        //QAgent konstruktor
        public QAgent(CellColor playerColor)
        {
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

        //Funktion för att få fram alla kolumner som ger ett giltigt drag
        public int[] GetValidMoves(Cell[,] boardState)
        {
            List<int> validMoves = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                if (GameEngineAi.IsValid(boardState, i))
                {
                    validMoves.Add(i);
                }
            }
            return validMoves.ToArray();
        }

        //QAgent konstruktor som skapar utifrån fil
        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            return temp;
        }

        //Funktion för att välja ett drag
        public override int SelectMove(Cell[,] grid)
        {
            Random rnd = new Random();
            //procent som ska utforskas
            double epsilon = 0.9;
            //Ta fram alla moves och skicka till exploration
            int[] validMoves = GetValidMoves(grid);
            int temp = EGreedyMove(epsilon, grid, validMoves);
            int move = 0;

            for (int i = 0; i < validMoves.Length; i++)
            {
                if (temp == validMoves[i])
                {
                    move = validMoves[i];
                }
                else
                {
                    move = validMoves[rnd.Next(0, validMoves.Length)];
                }
            }
            
            return move;
        }

        //Funktion för att leta bland validMoves efter det bästa draget
        // - väljer kolumn som har högst Q-värde belöning
        private int Exploration(Cell[,] grid, int[] validMoves)
        {
            int bestColumn = 0;
            double qValue = GetReward(grid, bestColumn);

            //Loopa igenom validMoves och spara undan kolumenn med högst belöning
            for (int i = 0; i < validMoves.Length; i++)
            {
                if (GetReward(grid, i) > qValue)
                {
                    bestColumn = i;
                    qValue = GetReward(grid, bestColumn);
                }

            }
            return bestColumn;
        }

        public int EGreedyMove(double epsilon, Cell[,] board, int[] validMoves)
        {
            Random rnd = new Random();
            int move = -1;
            if (rnd.NextDouble() < epsilon)
            {
                move = validMoves[rnd.Next(0, validMoves.Length)];
            }
            else
            {
                move = Exploration(board, validMoves);
            }
            return move;
        }

        //Funktion som returnerar Q-värdes belöning från QTable
        private double GetReward(Cell[,] grid, int move)
        {
            String key = GameBoard.GetHashCodeAsString(grid);
            Random rnd = new Random();

            //Ifall detta state/drag (t.ex. drag 20) finns -> ge Q-värde för kolumn drag
            if (qDictionary.ContainsKey(key))
            {
                return qDictionary[key][move];
            }
            //Ifall inte -> ge ut random Q-värden till detta drag och returnera 0
            else
            {
                double[] moves = 
                { 
                    rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() 
                };

                qDictionary.Add(key, moves);
                return 0;
            }
        }

        //Funktion som sätter Q-värden i QTable
        private void SetReward(Cell[,] grid, int move, double value)
        {
            String key = GameBoard.GetHashCodeAsString(grid);
            Random rnd = new Random();

            //Ifall detta state/drag (t.ex. drag 20) INTE finns -> ge ut random Q-värden till detta drag och returnera 0
            if (!qDictionary.ContainsKey(key))
            {
                double[] moves =
                {
                    rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble()
                };
                qDictionary.Add(key, moves);
            }

            //Sedan sätt Q-värde
            qDictionary[key][move] = value;
        }

        //Funktion som returnerar ifall valet av kolumn är ett giltigt drag
        private bool IsValid(Cell[,] grid, int col)
        {
            return grid[col, 0].Color == CellColor.Blank;
        }

        //Funktion som tränar QAgent mot en "opponent"-AI i antal "iterations" spel
        public void TrainAgent(AI opponent, int iterations)
        {
            //GameEngineAI är en modifierad version av GameEngine som stödjer AI mot AI matcher
            GameEngineAi gameEngineAi = new GameEngineAi();
            int opponentsMove;
            Random rnd = new Random();

            CellColor opponentsColor = GameEngineAi.OtherPlayer(PlayerColor);

            wins = 0;
            ties = 0;
            losses = 0;
            nrOfGames = 0;

            //Loop för antal "iterations" spel omgångar
            for (int i = 0; i < iterations; i++)
            {
                nrOfGames++;

                //Reset GameEngine och gameOver till false -> Nytt spel
                gameEngineAi.Reset();
                bool gameOver = false;

                //Ifall man är gul (spelaren som inte kör först) -> ge röd-AI ett random första move
                if (PlayerColor == CellColor.Yellow)
                {
                    opponentsMove = EGreedyMove(1, gameEngineAi.Board.Grid, GetValidMoves(gameEngineAi.Board.Grid));
                    gameEngineAi.MakeMove(opponentsMove);
                }

                //Gör ett move
                int move = EGreedyMove(0.7F, gameEngineAi.Board.Grid, GetValidMoves(gameEngineAi.Board.Grid));


                //Sålänge spelet är igång:
                while(!gameOver)
                {
                    //Titta ifall en vinst eller draw sker med nya move, isf ge belöningar
                    if (gameEngineAi.IsWin(gameEngineAi.Board, move, PlayerColor))
                    {
                        SetReward(gameEngineAi.Board.Grid, move, WinReward);
                        gameOver = true;
                        wins++;
                    }
                    else if (gameEngineAi.IsDraw(gameEngineAi.Board, move))
                    {
                        SetReward(gameEngineAi.Board.Grid, move, DrawReward);
                        gameOver = true;
                        ties++;
                    }
                    else
                    {
                        //Ta fram (Q(s,a)) för vårt move
                        double saReward = GetReward(gameEngineAi.Board.Grid, move);

                        //Skapa en kopia av boarden och gör agentens move på det
                        // - sedan gör också opponents nästa move
                        GameBoard tempBoard = gameEngineAi.Board.CopyBoard();
                        GameEngineAi.MakeMove(ref tempBoard, PlayerColor, move);
                        opponentsMove = opponent.SelectMove(tempBoard.Grid);

                        //Titta ifall opponent har en vinst eller draw på sitt move -> isf ge ut belöningar
                        if (gameEngineAi.IsWin(tempBoard, opponentsMove, opponentsColor))
                        {
                            SetReward(gameEngineAi.Board.Grid, move, LossReward);
                            losses++;
                            break;
                        }
                        else if(gameEngineAi.IsDraw(tempBoard, opponentsMove))
                        {
                            SetReward(gameEngineAi.Board.Grid, move, DrawReward);
                            ties++;
                            break;
                        }

                        //Ifall det inte är vinst elelr draw, gör opponents move på board och sedan agentens
                        GameEngineAi.MakeMove(ref tempBoard, opponentsColor, opponentsMove);
                        int bestMove = EGreedyMove(2, gameEngineAi.Board.Grid, GetValidMoves(gameEngineAi.Board.Grid));

                        //Här räknar vi ut (genom Q-learning formeln) det nya Q-värdet för denna plats
                            // Q(s',a')
                        double nsReward = GetReward(tempBoard.Grid, bestMove);

                            //  𝑄(𝑠,𝑎) ← 𝑄(𝑠,𝑎) + 𝛼 (𝛾 ∗ max𝑄(s',𝑎′) − 𝑄(𝑠,𝑎))
                        double qCurrentState = saReward + 0.5F * (0.9F * nsReward - saReward);
                        SetReward(gameEngineAi.Board.Grid, move, qCurrentState);

                        //Gör dragen på faktiska gameboard
                        gameEngineAi.MakeMove(move);
                        gameEngineAi.MakeMove(opponentsMove);

                        //Skaffa ett nytt agent move att analysera
                        move = EGreedyMove(0.7F, gameEngineAi.Board.Grid, GetValidMoves(gameEngineAi.Board.Grid));
                    }
                }
            }
            Console.WriteLine("Victories: " + wins + "\n" + "Ties: " + ties + "\n" + "Defeats: " + losses + "\n" + "winrate: " + (((double)wins/nrOfGames)*100) + "%\n" + "Games played: " + nrOfGames + "\n");
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
