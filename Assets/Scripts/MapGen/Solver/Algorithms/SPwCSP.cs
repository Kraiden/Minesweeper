using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SPwCSP : Algorithm
{
    /**
     * Check whether the board is solvable by Single Point method and CSP Strategy
     * @param mineBoard the board to be solve
     * @param clickedSquareIndex the index of first clicked square
     * @return whether the board is solvable
     */
     // x = gridRow, y = gridCol
    public bool IsSolvable(GameBoard board, int clickedIndex){
        int gridRow = board.gridRow;
        int gridCol = board.gridCol;
        int mineNum = board.mineNumber;

        bool mapUpdated = false;
        int totalCount = gridCol * gridRow;
        int totalFlagCount = 0;
        int totalProbedSquaresCount = 1; // Including the first one

        HashSet<int> frontierSquares = new HashSet<int>();
        frontierSquares.Add(clickedIndex);

        bool forceBreak = false;
        while(! (mineNum == totalFlagCount 
        || totalCount - totalProbedSquaresCount == mineNum) && !forceBreak){
            mapUpdated = false;

            //used in both SP and CSP
            int[] keyList = new int[frontierSquares.Count];
            frontierSquares.CopyTo(keyList); 

            //Try SP
            foreach(int key in keyList){
                int row = key / gridCol;
                int col = key % gridCol;

                Square square = board.squares[row, col];
                byte unprobedCount = board.countNeighbors(square, GameBoard.COUNT_UNPROBED);
                byte mineCount = board.countNeighbors(square, GameBoard.COUNT_MINE);
                byte flagCount = board.countNeighbors(square, GameBoard.COUNT_FLAG);

                if(mineCount == unprobedCount + flagCount || mineCount == flagCount) {
                    frontierSquares.Remove(key);

                    foreach (Square neb in board.getNeighbors(square)){
                        if(neb.unprobed) {
                            neb.unprobed = false;
                            if(mineCount != flagCount) {
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

            if(mapUpdated) {
                // if SP succeeds, keep using it since it's faster                
                continue;
            } 

            //Debug.Log("=============================================\n" 
            //+ "SP Failed - Trying CSP");
            //return false; //Or dont

            // //if SP fails, use CSP
            HashSet<Constraints> constraintsSet = new HashSet<Constraints>();

            foreach(int key in keyList){
                int row = key / gridCol;
                int col = key % gridCol;

                Square square = board.squares[row, col];

                byte mineCount = board.countNeighbors(square, GameBoard.COUNT_MINE);
                byte flagCount = board.countNeighbors(square, GameBoard.COUNT_FLAG);

                Constraints squareConstraints = new Constraints((byte) (mineCount - flagCount));
                
                foreach (Square neb in board.getNeighbors(square)){
                    if(neb.unprobed){
                        int index = neb.position.x * gridCol + neb.position.y;
                        squareConstraints.add(index);
                    }
                }

                if(!squareConstraints.isEmpty()){
                    constraintsSet.Add(squareConstraints);
                }
            }

            // decompose constraints according to their overlaps and differences
            bool constraintsSetUpdated = true;
            while(constraintsSetUpdated) {
                constraintsSetUpdated = false;

                Constraints[] constraintsList = new Constraints[constraintsSet.Count] ;
                constraintsSet.CopyTo(constraintsList);
                
                foreach(Constraints constraints1 in constraintsList){
                    foreach (Constraints constraints2 in constraintsList){
                        if( !constraints1.Equals(constraints2)){
                            //If Constraints1 is included in Constraints2
                            if(constraints1.IsSubsetOf(constraints2)){
                                Constraints diffConstraints = new Constraints();
                                foreach(int entry2 in constraints2){
                                    if(!constraints1.contains(entry2)) {
                                        diffConstraints.add(entry2);
                                    }
                                }

                                //decompose the constraint
                                byte diffMineNumber = (byte) (constraints2.mineNumber - constraints1.mineNumber);
                                diffConstraints.mineNumber = diffMineNumber;

                                constraintsSetUpdated = constraintsSet.Add(diffConstraints);
                                constraintsSet.Remove(constraints2);
                            }
                        }
                    }
                }

                Debug.Log("-----------------------------------------------------------------\n" 
                + "Constraint decomp loop");
            }

            Debug.Log("______________________________________________________________________\n" 
                + "Constraint decomp loop broken");

            // solve variables if All-Free-Neighbor or All-Mine-Neighbor
            foreach(Constraints constraints in constraintsSet) {
                byte mines = constraints.mineNumber;

                if(mines == 0 || mines == constraints.size()) {
                    foreach(int sqrIndex in constraints){
                        int row = sqrIndex / gridCol;
                        int col = sqrIndex % gridCol;

                        Square square = board.squares[row,col];

                        if(square.unprobed){
                            square.unprobed = false;

                            if(mines == 0){
                                frontierSquares.Add(sqrIndex);
                                totalProbedSquaresCount++;
                            } else {
                                square.flag = true;
                                totalFlagCount++;
                            }
                        }
                    }
                    mapUpdated = true;
                }
            }

            if(!mapUpdated){
                Debug.Log("Unsolvable...");
                //if both methods fail, call it unsolvable
                forceBreak = true;
                return false;
            }
        }

        return true;
    }

    /**
     * A class for representing a set of constraints
     * on the number of mines a set of squares contain
     */
    private class Constraints {
        // mineNumber is the sum of mines in the squares (represented as their indices)
        public byte mineNumber;
        private HashSet<int> squaresIndices;

        public Constraints() : this((byte) 0) {}

        public Constraints(byte number) {
            mineNumber = number;
            squaresIndices = new HashSet<int>();
        }

        public int size() {
            return squaresIndices.Count;
        }

        public bool isEmpty(){
            return squaresIndices.Count == 0;
        }

        public bool IsSubsetOf(Constraints set2){
            return squaresIndices.IsSubsetOf(set2.squaresIndices);
        }

        public bool contains(int i){
            return squaresIndices.Contains(i);
        }

        public IEnumerator<int> GetEnumerator(){
            return squaresIndices.GetEnumerator();
        }

        public int[] toArray(){
            return squaresIndices.ToArray();
        }

        public void add(int e) {
            squaresIndices.Add(e);
        }
    
        public void remove(int o) {
            squaresIndices.Remove(o);
        }

        public bool containsAll(ICollection<int> c){
            return c.All(i => squaresIndices.Contains(i) );
        }

        public void clear(){
            squaresIndices.Clear();
        }

        public override int GetHashCode()
        {
            int hashCode = -404001401;
            hashCode = hashCode * -1521134295 + mineNumber.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<int>>.Default.GetHashCode(squaresIndices);
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is Constraints constraints &&
                   mineNumber == constraints.mineNumber &&
                   EqualityComparer<HashSet<int>>.Default.Equals(squaresIndices, constraints.squaresIndices);
        }
    }
}
