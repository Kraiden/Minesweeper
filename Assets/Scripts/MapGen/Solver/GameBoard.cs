using System.Collections;
using System.Collections.Generic;

public class GameBoard{

    public static int COUNT_UNPROBED = 0;
    public static int COUNT_MINE = 1;
    public static int COUNT_FLAG = 2;

    public int gridRow;
    public int gridCol;
    public int mineNumber;

    public int probedCount;

    public Square[,] squares;

    public GameBoard(int x, int y, int mineCount){
        gridRow = x;
        gridCol = y;
        mineNumber = mineCount;

        squares = new Square[gridRow, gridCol];
        for (int row = 0; row < gridRow; row++) {
            for (int col = 0; col < gridCol; col++) {
                squares[row,col] = new Square(row, col);
            }
        }
    }

    public void reInit(){
        probedCount = 0;
        for (int row = 0; row < gridRow; row++) {
            for (int col = 0; col < gridCol; col++) {
                Square s = squares[row,col];
                s.mine = false;
                s.flag = false;
                s.unprobed = true;
            }
        }
    }

    public List<Square> getNeighbors(Square square){
        List<Square> list = new List<Square>();
        int row;
        int col;
        for (int i = -1; i < 2; i++) {
            row = square.position.x + i;
            if (row >= 0 && row < gridRow) {
                for (int j = -1; j < 2; j++) {
                    col = square.position.y + j;
                    if (col >= 0 && col < gridCol && !(i == 0 && j == 0)) {
                        list.Add(squares[row,col]);
                    }
                }
            }
        }

        return list;
    }

    public byte countNeighbors(Square square, int key){
        byte count = 0;
        foreach (Square sqrCount in getNeighbors(square)){
            if((key == COUNT_UNPROBED && sqrCount.unprobed)
            || (key == COUNT_MINE && sqrCount.mine) 
            || (key == COUNT_FLAG && sqrCount.flag) ) {
                count++;
            }
        }
        return count;
    }
}