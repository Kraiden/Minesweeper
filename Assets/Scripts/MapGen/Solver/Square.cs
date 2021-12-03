public class Square{
    public bool mine;
    public bool flag;
    public bool unprobed;

    public Position position;

    public Square(int x, int y){
        position = new Position(x, y);
    }

    public struct Position{
        public int x;
        public int y;

        public Position(int _x, int _y){
            x = _x;
            y = _y;
        }
    }
}