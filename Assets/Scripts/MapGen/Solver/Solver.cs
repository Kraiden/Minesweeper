using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Threading;
using UnityEngine;

//An implementation of solver by 
// Double Set Single Point Algorithm
//
// Adapted from: https://github.com/purindaisuki/Minesweeper
// Adapted by: Someone who doesn't know C# very well... Sooo use at your own risk.
public class Solver{

    private static Solver instance;

    private int x;
    private int y;
    private int bombCount;
    private Coord firstClick;
    private System.Action<Game[,]> callback;

    private bool noGuess;
    private bool noGuessOverride;

    private Game[,] result;

    private Solver() {}

    public static Solver getSolver() {
        if(instance == null){
            instance = new Solver();
        }
        return instance;
    }

    public IEnumerator GenerateSolvableMap(int x, int y, int bombCount, Coord firstClick, System.Action<Game[,]> callback){
        this.x = x;
        this.y = y;
        this.bombCount = bombCount;
        this.firstClick = firstClick;
        this.callback = callback;

        this.noGuess = PlayerPrefs.GetInt(PrefsConstants.SET_NO_GUESS_ENABLED, 1) == 1;
        this.noGuessOverride = PlayerPrefs.GetInt(PrefsConstants.CUST_NO_GUESS_OVERRIDE, 0)  == 1;

        Thread genThread = new Thread(DoMapGen);
        genThread.Priority = System.Threading.ThreadPriority.Highest;
        genThread.Start();

        while(genThread.IsAlive){
            yield return null;
        }

        callback(result);

    }

    private void DoMapGen(){
        GameBoard board = new GameBoard(x, y, bombCount);
        Square clickedSquare = board.squares[firstClick.x, firstClick.y];

        Algorithm alg; 

        if(noGuess && !noGuessOverride){
            alg = new DSSP();
        } else {
            alg = new None();
        }

        int[] allCoords = GenAllCoords(x, y);

        bool isSolvable = false;
        while (!isSolvable){
            int placedMineNum = 0;
            int randRow;
            int randCol;

            board.reInit();
            board.probedCount = 1;

            Queue<int> shuffledCoord = new Queue<int>(Arrays.Shuffle(allCoords));

            while (placedMineNum < board.mineNumber){
                int index = shuffledCoord.Dequeue();
                randRow = index / y;
                randCol = index % y;

                //Don't place mines near start position
                 if(!(randRow >= clickedSquare.position.x - 1 && randRow <= clickedSquare.position.x + 1 
                 && randCol >= clickedSquare.position.y - 1 && randCol <= clickedSquare.position.y + 1) 
                 && !board.squares[randRow, randCol].mine) {
                     board.squares[randRow, randCol].mine = true;
                     placedMineNum++;
                 }
            }

            isSolvable = alg.IsSolvable(board, clickedSquare.position.x * board.gridCol + clickedSquare.position.y);
        }
        
        Game[,] game = new Game[x,y];
        for(int i = 0; i < x; i++){
            for(int j = 0; j < y; j++){
                Square s = board.squares[i,j];
                Game g = new Game(board.squares[i,j].mine, board.countNeighbors(s, GameBoard.COUNT_MINE));
                game[i, j] = g;
            }
        }

        result = game;
    }

    private int[] GenAllCoords(int x, int y){
        int[] all = new int[x * y];

        int idx = -1;
        for(int i = 0; i < x; i++){
            for(int j = 0; j < y; j++){
                all[++idx] = (i * y) + j;
            }
        }

        return all;
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