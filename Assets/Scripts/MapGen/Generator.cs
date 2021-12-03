using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AndroidNativeCore;

public class Generator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform underTilePrefab;
    public Transform bombPrefab;

    public ParticleSystem popEffect;
    public ParticleSystem flagEffect;
    public ParticleSystem revealEffect;

    //TODO: Ugh, sorry about the inconsistency in spelling here. I wanted to stick with the american spelling 
    // to be consistent with the SDK, but my brain still wants to spell it the british way.
    public Color[] countColours;
    public Color flagColor;
    
    public Coord mapSize = new Coord (16, 30);
    public int bombCount = 99;

    public System.Action OnNewGame;
    public System.Action<bool> OnGameOver;
    public System.Action<bool> OnFlagChange;

    private Dictionary<Coord, Tile> allTiles;
    private Queue<Tile> bombTiles;

    private Controller controller;

    public CameraLocation startCamera {private set; get;}

    private bool gameStarted = false;
    private bool gameOver = false;

    private Transform mapHolder;

    void Start(){
        int i = 1;
        if(1 - i == 1){ // Workaround to force Unity to add vibration permissions
            Handheld.Vibrate();
        }

        controller = GetComponent<Controller>();
        controller.OnTouchEvent += HandleTileReveal;
        controller.OnLongTouchEvent += HandleTileFlag;

        Tile.OnBombHit = HandleBombHit;
        Tile.OnCheckWin = CheckWinState;
        Tile.OnFlagChange = OnFlagChange;
        Tile.playEffect = playEffect;

        Tile.popEffect = popEffect;
        Tile.flagEffect = flagEffect;
        Tile.revealEffect = revealEffect;

        Tile.overTileStartColor = tilePrefab.GetComponent<Renderer>().sharedMaterial.color;
        Tile.flagColor = flagColor;
        Tile.countColours = countColours;

        int x = PlayerPrefs.GetInt("map-x", 16);
        int y = PlayerPrefs.GetInt("map-y", 30);
        int bombs = PlayerPrefs.GetInt("map-bombs", 99);

        mapSize = new Coord(x, y);
        bombCount = bombs;

        InitBoard();
        GenerateMap();
    }

    private void InitBoard(){
        string holderName = "Generated Map";
        string holderTileName = "Map Tiles";
        string holderBombName = "Map Bombs";

        if(transform.Find(holderName)) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        Transform tileHolder = new GameObject(holderTileName).transform;
        tileHolder.parent = mapHolder;

        Transform bombHolder = new GameObject(holderBombName).transform;
        bombHolder.parent = mapHolder;

        allTiles = new Dictionary<Coord, Tile>();

        for(int x = 0; x < mapSize.x ; x++){
            for (int y = 0; y < mapSize.y; y++) {
                Coord c = new Coord(x, y);

                Transform overTile = SpawnTile(c, tileHolder);
                Transform underTile = SpawnUnderTile(c, tileHolder);
                Transform bomb = SpawnBomb(c, bombHolder);

                Tile tile = new Tile(c, overTile, underTile, bomb);

                allTiles.Add(c, tile);
            }
        }
    }

    public void GenerateMap(){
        gameStarted = false;

        foreach(Tile tile in allTiles.Values){
            tile.Reset();
        }

        MoveCam();
    }

    private void startGame(Coord firstClick){
        gameOver = false;
        gameStarted = true;

        bombTiles = new Queue<Tile>();

        Solver.Game[,] game = Solver.getSolver().GenerateSolvableMap(mapSize.x, mapSize.y, bombCount, firstClick);

        for(int i = 0; i < mapSize.x; i++){
            for(int j = 0; j < mapSize.y; j++){
                Solver.Game g = game[i,j];
                Coord c = new Coord(i, j);

                Tile t = allTiles[c];
                t.SetBomb(g.IsMine, g.adjCount);

                if(t.isBomb){
                    bombTiles.Enqueue(t);
                }
            }
        }

        if(OnNewGame != null){
            OnNewGame();
        }
    }

    private Transform SpawnTile(Coord coord, Transform parent){
        Vector3 tilePosition = CoordToPosition(coord);
        Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
        newTile.localScale = Vector3.one * 0.95f ;
        newTile.parent = parent;

        return newTile;
    }

    private Transform SpawnUnderTile(Coord coord, Transform parent){
        Vector3 tilePosition = CoordToPosition(coord.x,coord.y,1);
        Transform newTile = Instantiate(underTilePrefab, tilePosition, Quaternion.identity);
        newTile.localScale = Vector3.one * 0.95f ;
        newTile.parent = parent;

        return newTile;
    }

    private Transform SpawnBomb(Coord coord, Transform parent){
        Vector3 bombPosition = CoordToPosition(coord.x,coord.y,1);
        Transform newBomb = Instantiate(bombPrefab, bombPosition, Quaternion.identity);
        newBomb.localScale = Vector3.one * 0.5f ;
        newBomb.parent = parent;

        return newBomb;
    }

    private int CalcAdjFlags(Coord c){
        int i = 0;
        List<Coord> adjacent = new List<Coord>();
        adjacent.Add(new Coord(c.x -1, c.y - 1));
        adjacent.Add(new Coord(c.x -1, c.y));
        adjacent.Add(new Coord(c.x -1, c.y + 1));
        adjacent.Add(new Coord(c.x, c.y - 1));
        adjacent.Add(new Coord(c.x, c.y + 1));
        adjacent.Add(new Coord(c.x + 1, c.y - 1));
        adjacent.Add(new Coord(c.x + 1, c.y));
        adjacent.Add(new Coord(c.x + 1, c.y + 1));

        foreach(Coord adj in adjacent){
            if(adj.x >= 0 && adj.x < mapSize.x &&
            adj.y >= 0 && adj.y < mapSize.y){
                if(allTiles[adj].isFlagged){
                    i ++;
                }
            }
        }

        return i;
    }

    private void RecurseReveal(Coord coord){
        if(allTiles.ContainsKey(coord)){
            Tile tile = allTiles[coord];
            if(!tile.isRevealed){
                tile.Reveal();

                if(tile.adjBombs == 0){
                    RecurseReveal(new Coord(coord.x -1, coord.y - 1));
                    RecurseReveal(new Coord(coord.x -1, coord.y));
                    RecurseReveal(new Coord(coord.x -1, coord.y +1));
                    RecurseReveal(new Coord(coord.x, coord.y - 1));
                    RecurseReveal(new Coord(coord.x, coord.y + 1));
                    RecurseReveal(new Coord(coord.x + 1, coord.y - 1));
                    RecurseReveal(new Coord(coord.x + 1, coord.y));
                    RecurseReveal(new Coord(coord.x + 1, coord.y + 1));
                }
            }
        }
    }

    private int RevealAdj(Coord c){
        int revealedThisItt = 0;

        List<Coord> adjacent = new List<Coord>();
        adjacent.Add(new Coord(c.x -1, c.y - 1));
        adjacent.Add(new Coord(c.x -1, c.y));
        adjacent.Add(new Coord(c.x -1, c.y + 1));
        adjacent.Add(new Coord(c.x, c.y - 1));
        adjacent.Add(new Coord(c.x, c.y + 1));
        adjacent.Add(new Coord(c.x + 1, c.y - 1));
        adjacent.Add(new Coord(c.x + 1, c.y));
        adjacent.Add(new Coord(c.x + 1, c.y + 1));

        foreach(Coord adj in adjacent){
            if(adj.x >= 0 && adj.x < mapSize.x &&
            adj.y >= 0 && adj.y < mapSize.y){
                if(allTiles.ContainsKey(adj)){
                    Tile tile = allTiles[adj];
                    
                    if((!tile.isBomb && tile.adjBombs == 0)){
                        RecurseReveal(adj);
                    } else {
                        if(tile.Reveal()){
                            revealedThisItt++;
                        }
                    }
                }
            }
        }

        return revealedThisItt;
    }

    private void HandleTileReveal(Ray touchRay){
        Coord hitCoord = PositionToCoord(touchRay.GetPoint(0));
        if(hitCoord != null){
            if(!gameStarted){
                startGame(hitCoord);
            }
            
            if(!gameOver){
                if(allTiles.ContainsKey(hitCoord)){
                    Tile tile = allTiles[hitCoord];
                    bool isAlreadyRevealed = tile.isRevealed;
                    int adjReveals = 0;

                    if((!tile.isBomb && tile.adjBombs == 0)){
                        RecurseReveal(hitCoord);
                    } else if (!tile.isBomb && tile.isRevealed && CalcAdjFlags(hitCoord) == tile.adjBombs) {
                        adjReveals = RevealAdj(hitCoord);
                    } else {
                        tile.Reveal();
                    }
                    
                    if((!isAlreadyRevealed && tile.isRevealed) || adjReveals != 0){
                        if(PlayerPrefs.GetInt("settings-vib", 1) == 1){
                            Vibrator.Vibrate(50);
                        }
                    }
                }
            }
        }
    }

    private void HandleTileFlag(Ray touchRay){
        if(!gameOver){
            Coord hitCoord = PositionToCoord(touchRay.GetPoint(0));
            if(hitCoord != null){
                if(allTiles.ContainsKey(hitCoord)){
                    allTiles[hitCoord].Flag();
                }
            }
        }
    }

    private void HandleBombHit(Tile tile){
        gameOver = true;
        if(OnGameOver != null){
            OnGameOver(false);
        }
        StartCoroutine("PopTiles", tile);
    }

    private void playEffect(ParticleSystem effect, Vector3 position){
        Destroy(Instantiate(effect.gameObject, position, Quaternion.identity), effect.main.startLifetime.constant);
    }

    private IEnumerator PopTiles(Tile tile){
        float duration = 1f;
        float popWait = duration / bombTiles.Count;

        Queue<Tile> tiles = new Queue<Tile>(bombTiles.Where(x => x != tile));
        tile.Pop(1f);

        yield return new WaitForSeconds(0.5f);

        foreach(Tile t in tiles){
            t.Pop(.5f);
            yield return new WaitForSeconds(popWait);
        }
    }

    private void CheckWinState(){
        int unrevealed = 0;
        bool bombHit = false; 
        foreach(Tile t in allTiles.Values){
            if(!t.isRevealed) unrevealed++;
            if(t.isBomb && t.isRevealed) bombHit = true;
        }

        if(unrevealed == bombCount && !bombHit){
            gameOver = true;
            if(OnGameOver != null){
                OnGameOver(true);
            }
        }
    }

    public Vector3 MinPosition(){
        return CoordToPosition(0,0);
    }

    public Vector3 MaxPosition(){
        return CoordToPosition(mapSize.x, mapSize.y);
    }

    private Vector3 CoordToPosition(Coord coord){
        return CoordToPosition(coord.x, coord.y);
    }

    private Vector3 CoordToPosition(int x, int y, float z = 0){
        return new Vector3(-mapSize.x/2f + x,
                            -mapSize.y/2f + y,
                            z);
    }

    private Coord PositionToCoord(Vector3 position){
        int x = Mathf.RoundToInt(position.x + (mapSize.x / 2f) );
        int y = Mathf.RoundToInt(position.y + (mapSize.y / 2f) );


        if(x < 0 || x > mapSize.x || y < 0 || y > mapSize.y){
            return null;
        } else {
            return new Coord(x,y);
        }
    }

    private void MoveCam(){
        Vector3 pos = CoordToPosition(mapSize.x + 2, mapSize.y + 2) * 2f;
        float sizeX = pos.x;
        float sizeY = pos.y;

        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = sizeX / sizeY;

        float zoom;

        if(screenRatio >= targetRatio){
            zoom = sizeY / 2f;
        }else{
            float differenceInSize = targetRatio / screenRatio;
            zoom = sizeY / 2f * differenceInSize;
        }

        Camera.main.orthographicSize = zoom;

        //Reset Pan
        //TODO: pull out the magic number
        Vector3 camPosition = CoordToPosition((mapSize.x / 2), mapSize.y / 2, Camera.main.transform.position.z);
        camPosition.x -= 0.5f; // account for tile size
        camPosition.y -= 0.5f;
        Camera.main.transform.position = camPosition;

        startCamera = new CameraLocation(camPosition, zoom);
    }
}
