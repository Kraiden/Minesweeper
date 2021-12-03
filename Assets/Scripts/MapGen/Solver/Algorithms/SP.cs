using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SP : Algorithm
{
    private int gridRow = 0;
    private int gridCol = 0;
    private int mineNumber = 0;

    public bool IsSolvable(GameBoard board, int clickedIndex){
        gridRow = board.gridRow;
        gridCol = board.gridCol;
        mineNumber = board.mineNumber;

        bool mapUpdated = false;
        int totalCount = gridCol * gridRow;
        int totalFlagCount = 0;
        int totalProbedSquaresCount = 1;

        HashSet<int> frontierSquares = new HashSet<int>();
        frontierSquares.Add(clickedIndex);

        while( !(mineNumber == totalFlagCount || totalCount - totalProbedSquaresCount == mineNumber)){
            mapUpdated = false;

            int[] keyList = new int[frontierSquares.Count];
            frontierSquares.CopyTo(keyList);
            foreach(int key in keyList){
                int row = key / gridCol;
                int col = key % gridCol;

                Square square = board.squares[row, col];
                byte unprobedCount = board.countNeighbors(square, GameBoard.COUNT_UNPROBED);
                byte mineCount = board.countNeighbors(square, GameBoard.COUNT_MINE);
                byte flagCount = board.countNeighbors(square, GameBoard.COUNT_FLAG);

                if(mineCount == unprobedCount + flagCount || mineCount == flagCount){
                    frontierSquares.Remove(key);

                    foreach( Square neb in board.getNeighbors(square)){
                        if(neb.unprobed) {
                            neb.unprobed = false;

                            if(mineCount != flagCount){
                                neb.flag = true;
                                totalFlagCount++;
                            } else {
                                frontierSquares.Add(neb.position.x * gridCol + neb.position.y);
                                totalProbedSquaresCount++;
                            }
                        }
                    }

                    mapUpdated = true;
                    break;
                }
            }

            if(!mapUpdated){
                return false;
            }
        }

        return true;
    }
}