using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DSSP : Algorithm
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
        int totalProbedCount = 0;

        HashSet<int> squaresToProbe = new HashSet<int>();
        HashSet<int> frontierSquares = new HashSet<int>();

        squaresToProbe.Add(clickedIndex);

        while( !(mineNumber == totalFlagCount || totalCount - totalProbedCount == mineNumber)){
            mapUpdated = false;

            while(squaresToProbe.Count > 0){
                mapUpdated = true;
                int[] iKeyList = new int[squaresToProbe.Count];
                squaresToProbe.CopyTo(iKeyList);

                foreach(int key in iKeyList){
                    int row = key / gridCol;
                    int col = key % gridCol;

                    Square square = board.squares[row, col];
                    byte mineCount = board.countNeighbors(square, GameBoard.COUNT_MINE);
                    byte flagCount = board.countNeighbors(square, GameBoard.COUNT_FLAG);
                    
                    squaresToProbe.Remove(key);
                    square.unprobed = false;
                    totalProbedCount++;

                    if(mineCount == flagCount) {
                        foreach (Square neb in board.getNeighbors(square)){
                            if(neb.unprobed){
                                squaresToProbe.Add(neb.position.x * gridCol + neb.position.y);
                            }
                        }
                    } else {
                        frontierSquares.Add(key);
                    }

                    if(squaresToProbe.Count == 0) break;
                }
            }

            int[] keyList = new int[frontierSquares.Count];
            frontierSquares.CopyTo(keyList);

            foreach(int key in keyList){
                int row = key / gridCol;
                int col = key % gridCol;

                Square square = board.squares[row,col];
                byte mineCount = board.countNeighbors(square, GameBoard.COUNT_MINE);
                byte flagCount = board.countNeighbors(square, GameBoard.COUNT_FLAG);
                byte unprobedCount = board.countNeighbors(square, GameBoard.COUNT_UNPROBED);

                if(mineCount == unprobedCount + flagCount) {
                    mapUpdated = true;
                    frontierSquares.Remove(key);

                    foreach (Square neb in board.getNeighbors(square)){
                        if(neb.unprobed) {
                            neb.unprobed = false;
                            neb.flag = true;
                            totalFlagCount++;
                        }
                    }
                }
            }

            keyList = new int[frontierSquares.Count];
            frontierSquares.CopyTo(keyList);

            foreach(int key in keyList){
                int row = key / gridCol;
                int col = key % gridCol;

                Square square = board.squares[row,col];
                byte mineCount = board.countNeighbors(square, GameBoard.COUNT_MINE);
                byte flagCount = board.countNeighbors(square, GameBoard.COUNT_FLAG);
                
                if(mineCount == flagCount){
                    mapUpdated = true;
                    frontierSquares.Remove(key);

                    foreach (Square neb in board.getNeighbors(square)){
                        if(neb.unprobed){
                            squaresToProbe.Add(neb.position.x * gridCol + neb.position.y);
                        }
                    }
                }
            }

            if(!mapUpdated){
                return false;
            }
        }

        return true;
    }
}