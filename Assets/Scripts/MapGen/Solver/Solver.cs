using System.Diagnostics;
using UnityEngine;

//An implementation of solver by 
// Single Point Algorithm and Constraint Satisfication 
// Problem strategy
//
// Adapted from: https://github.com/purindaisuki/Minesweeper
// Adapted by: Someone who doesn't know C# very well... Sooo use at your own risk.
public class Solver{

    private static Solver instance;  

    private Solver() {}

    public static Solver getSolver() {
        if(instance == null){
            instance = new Solver();
        }
        return instance;
    }

    public Game[,] GenerateSolvableMap(int x, int y, int bombCount, Coord firstClick){
        GameBoard board = new GameBoard(x, y, bombCount);
        Square clickedSquare = board.squares[firstClick.x, firstClick.y];

        Algorithm alg = new DSSP();

        // int genAttempts = 0;
        // Stopwatch s = Stopwatch.StartNew();

        bool isSolvable = false;
        while (!isSolvable){
            //genAttempts ++;
            int placedMineNum = 0;
            int randRow;
            int randCol;

            board.reInit();
            board.probedCount = 1;

            while (placedMineNum < board.mineNumber){
                randRow = Random.Range(0, board.gridRow);
                randCol = Random.Range(0, board.gridCol);

                //Don't place mines near start position
                 if(!(randRow >= clickedSquare.position.x - 1 && randRow <= clickedSquare.position.x + 1 
                 && randCol >= clickedSquare.position.y - 1 && randCol <= clickedSquare.position.y + 1) 
                 && !board.squares[randRow, randCol].mine) {
                     board.squares[randRow, randCol].mine = true;
                     placedMineNum++;
                 }
            }

            isSolvable = alg.IsSolvable(board, clickedSquare.position.x * board.gridCol + clickedSquare.position.y);
            //Debug.Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n" + 
            //" is solveable loop");
        }
        // s.Stop();

        // UnityEngine.Debug.Log("Generated " + genAttempts + " in " + (s.ElapsedMilliseconds));

        Game[,] game = new Game[x,y];
        for(int i = 0; i < x; i++){
            for(int j = 0; j < y; j++){
                Square s = board.squares[i,j];
                Game g = new Game(board.squares[i,j].mine, board.countNeighbors(s, GameBoard.COUNT_MINE));
                game[i, j] = g;
            }
        }

        return game;
    }

    public struct Game{
        public bool IsMine;
        public int adjCount;

        public Game(bool mine, int count){
            this.IsMine = mine;
            this.adjCount = count;
        }
    }

    
}