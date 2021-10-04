using System;
using System.IO;
using BlazorConnect4.AIModels;

namespace BlazorConnect4.Model
{
    public enum CellColor
    {
        Red,
        Yellow,
        Blank
    }


    public class Cell
    {
        public CellColor Color {get; set;}

        public Cell(CellColor color)
        {
            Color = color;
        }

    }

    public class GameBoard
    {
        public Cell[,] Grid { get; set; }

        public GameBoard()
        {
            Grid = new Cell[7, 6];

            //Populate the Board with blank pieces
            for (int i = 0; i <= 6; i++)
            {
                for (int j = 0; j <= 5; j++)
                {
                    Grid[i, j] = new Cell(CellColor.Blank);
                }
            }
        }

        public static String GetHashCode(Cell[,] grid)
        {
            //https://docs.microsoft.com/en-us/dotnet/csharp/how-to/concatenate-multiple-strings
            var hashCode = new System.Text.StringBuilder(); 
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    hashCode.Append(grid[i, j]);
                }
            }
            return hashCode.ToString();
        }
        public GameBoard CopyBoard()
        {
            GameBoard copy = new GameBoard();

            for (int i = 0; i <= 6; i++)
            {
                for (int j = 0; j <= 5; j++)
                {
                    switch (this.Grid[i, j].Color)
                    {
                        case CellColor.Blank:
                            copy.Grid[i, j].Color = CellColor.Blank;
                            break;
                        case CellColor.Red:
                            copy.Grid[i, j].Color = CellColor.Red;
                            break;
                        case CellColor.Yellow:
                            copy.Grid[i, j].Color = CellColor.Yellow;
                            break;

                    }
                }
            }
            return copy;

        }
    }



    public class GameEngine
    {
        public GameBoard Board { get; set; }
        public CellColor Player { get; set;}
        public bool active;
        public String message;
        private AI ai;


        public GameEngine()
        {
            Reset("Human");
        }



        // Reset the game and creats the opponent.
        // TODO change the code so new RL agents are created.
        public void Reset(String playAgainst)
        {
            Board = new GameBoard();
            Player = CellColor.Red;
            active = true;
            message = "Starting new game";

            if (playAgainst == "Human")
            {
                ai = null;
            }
            else if (playAgainst == "Random")
            {
                if (File.Exists("Data/Random.bin"))
                {
                    ai = RandomAI.ConstructFromFile("Data/Random.bin");
                }
                else
                {
                    ai = new RandomAI();
                    ai.ToFile("Data/Random.bin");
                }
                
            }
            else if (playAgainst == "Q1")
            {
                ai = new QAgent(this, Player);
            }
            else if (playAgainst == "Q2")
            {
                ai = new RandomAI();
            }
            else if (playAgainst == "Q3")
            {
                ai = new RandomAI();
            }

        }




        public bool IsValid(int col)
        {
            return Board.Grid[col, 0].Color == CellColor.Blank;
        }


        public bool IsDraw()
        {
            for (int i = 0; i < 7; i++)
            {
                if (Board.Grid[i,0].Color == CellColor.Blank)
                {
                    return false;
                }
            }
            return true;
        }

        public CellColor OtherPlayer(CellColor player)
        {
            return player == CellColor.Red ? CellColor.Yellow : CellColor.Red;
        }

        public bool IsWin(CellColor player)
        {
            int height = 6;
            int width = 7;
            bool isWin = false;
            // horizontalCheck 
            for (int j = 0; j < height - 3; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    if (Board.Grid[i, j].Color == player && Board.Grid[i, j + 1].Color == player && Board.Grid[i, j + 2].Color == player && Board.Grid[i, j + 3].Color == player)
                    {
                        isWin = true;
                    }
                }
            }
            if (isWin == false)
            {
                // verticalCheck
                for (int i = 0; i < width - 3; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (Board.Grid[i, j].Color == player && Board.Grid[i + 1, j].Color == player && Board.Grid[i + 2, j].Color == player && Board.Grid[i + 3, j].Color == player)
                        {
                            isWin = true;
                        }
                    }
                }
            }
            if (isWin == false)
            {
                // ascendingDiagonalCheck 
                for (int i = 3; i < width; i++)
                {
                    for (int j = 0; j < height - 3; j++)
                    {
                        if (Board.Grid[i, j].Color == player && Board.Grid[i - 1, j + 1].Color == player && Board.Grid[i - 2, j + 2].Color == player && Board.Grid[i - 3, j + 3].Color == player)
                            isWin = true;
                    }
                }
            }

            if (isWin == false)
            {
                // descendingDiagonalCheck
                for (int i = 3; i < width; i++)
                {
                    for (int j = 3; j < height; j++)
                    {
                        if (Board.Grid[i, j].Color == player && Board.Grid[i - 1, j - 1].Color == player && Board.Grid[i - 2, j - 2].Color == player && Board.Grid[i - 3, j - 3].Color == player)
                            isWin = true;
                    }
                }
            }
            return isWin;
        }




        public bool Play(int col)
        {
            if (IsValid(col) && active){

                for (int i = 5; i >= 0; i--)
                {
                    if (Board.Grid[col, i].Color == CellColor.Blank)
                    {
                        Board.Grid[col, i].Color = Player;

                        if (IsWin(Player))
                        {
                            message = Player.ToString() + " Wins";
                            active = false;
                            return true;
                        }

                        if (IsDraw())
                        {
                            message = "Draw";
                            active = false;
                            return true;
                        }
                        break;
                    }
                }
                return PlayNext();
            }

            return false;
        }


        private bool PlayNext()
        {

            if (Player == CellColor.Red)
            {
                Player = CellColor.Yellow;
            }
            else
            {
                Player = CellColor.Red;
            }

            if (ai != null && Player == CellColor.Yellow)
            {
                int move = ai.SelectMove(Board.Grid);

                while (! IsValid(move))
                {
                    move = ai.SelectMove(Board.Grid);
                }

                return Play(move);
            }

            return false;
        }
    }

    public class GameEngineAi
    {
        public GameBoard Board { get; set; }
        public CellColor PlayerTurn { get; set; }

        public GameEngineAi()
        {
            Board = new GameBoard();
            PlayerTurn = CellColor.Red;
        }
        public static CellColor OtherPlayer(CellColor player)
        {
            return player == CellColor.Red ? CellColor.Yellow : CellColor.Red;
        }
        public bool IsValid(int col)
        {
            return Board.Grid[col, 0].Color == CellColor.Blank;
        }
        public void Reset()
        {
            Board = new GameBoard();
            PlayerTurn = CellColor.Red;
        }
        public bool IsDraw(GameBoard gameBoard, int action)
        {
            GameBoard tempBoard = gameBoard.CopyBoard();
            GameEngineAi.MakeMove(ref tempBoard, CellColor.Yellow, action);

            for (int i = 0; i < 7; i++)
            {
                if (tempBoard.Grid[i, 0].Color == CellColor.Blank)
                {
                    return false;
                }
            }
            return true;
        }
        public bool IsWin(GameBoard gameBoard, int action, CellColor player)
        {
            int height = 6;
            int width = 7;
            bool Win = false;
            GameBoard tempBoard = gameBoard.CopyBoard();
            GameEngineAi.MakeMove(ref tempBoard, player, action);

            // Check horizontal
            for (int j = 0; j < height - 3; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    if (tempBoard.Grid[i, j].Color == player && tempBoard.Grid[i, j + 1].Color == player && tempBoard.Grid[i, j + 2].Color == player && tempBoard.Grid[i, j + 3].Color == player)
                    {
                        Win = true;
                    }
                }
            }
            // Check down
            for (int i = 0; i < width - 3; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (tempBoard.Grid[i, j].Color == player && tempBoard.Grid[i + 1, j].Color == player && tempBoard.Grid[i + 2, j].Color == player && tempBoard.Grid[i + 3, j].Color == player)
                    {
                        Win = true;
                    }
                }
            }
            // Check left up diagonal
            for (int i = 3; i < width; i++)
            {
                for (int j = 0; j < height - 3; j++)
                {
                    if (tempBoard.Grid[i, j].Color == player && tempBoard.Grid[i - 1, j + 1].Color == player && tempBoard.Grid[i - 2, j + 2].Color == player && tempBoard.Grid[i - 3, j + 3].Color == player)
                        Win = true;
                }
            }
        

            // Check left down diagonal
            for (int i = 3; i < width; i++)
            {
                for (int j = 3; j < height; j++)
                {
                    if (tempBoard.Grid[i, j].Color == player && tempBoard.Grid[i - 1, j - 1].Color == player && tempBoard.Grid[i - 2, j - 2].Color == player && tempBoard.Grid[i - 3, j - 3].Color == player)
                        Win = true;
                }
            }
            return Win;
        }
        public bool MakeMove(int action)
        {
            for (int i = 5; i >= 0; i -= 1)
            {
                if (Board.Grid[action, i].Color == CellColor.Blank)
                {
                    Board.Grid[action, i].Color = PlayerTurn; 
                    PlayerTurn = OtherPlayer(PlayerTurn);
                    return true;
                }
            }
            return false;
        }
            public static void MakeMove(ref GameBoard board, CellColor playerColor, int action)
        {
            for (int i = 5; i >= 0; i -= 1)
            {
                if (board.Grid[action, i].Color == CellColor.Blank)
                {
                    board.Grid[action, i].Color = playerColor; 

                }
            }  
        }
    }
}
