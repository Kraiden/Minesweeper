using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using AndroidNativeCore;

public class Generator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform underTilePrefab;
    public Transform bombPrefab;

    public ParticleSystem popEffect;
    public ParticleSystem flagEffect;
    public ParticleSystem revealEffect;

    public Color[] countColours;
    
    public Coord mapSize = new Coord (16, 30);
    public int bombCount = 99;

    public System.Action OnNewGame;
    public System.Action<bool> OnGameOver;
    public System.Action<bool> OnFlagChange;

    private Dictionary<Coord, Tile> allTiles;
    private Queue<Tile> bombTiles;

    private Controller controller;

    private static Color flagColor;
    private static Color overTileStartColor;

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
        Tile.popEffect = popEffect;
        Tile.flagEffect = flagEffect;
        Tile.revealEffect = revealEffect;

        overTileStartColor = tilePrefab.GetComponent<Renderer>().sharedMaterial.color;
        flagColor = countColours[8];

        int x = PlayerPrefs.GetInt("map-x", 16);
        int y = PlayerPrefs.GetInt("map-y", 30);
        int bombs = PlayerPrefs.GetInt("map-bombs", 99);

        mapSize = new Coord(x, y);
        bombCount = bombs;

        GenerateMap();
    }

    public void GenerateMap(){
        gameStarted = false;

        string holderName = "Generated Map";
        string holderTileName = "Map Tiles";

        if(transform.Find(holderName)) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        Transform tileHolder = new GameObject(holderTileName).transform;
        tileHolder.parent = mapHolder;

        allTiles = new Dictionary<Coord, Tile>();

        for(int x = 0; x < mapSize.x ; x++){
            for (int y = 0; y < mapSize.y; y++) {
                Coord c = new Coord(x, y);
                Tile tile = new Tile(c);

                allTiles.Add(c, tile);

                SpawnTile(c, tileHolder, tile);
            }
        }

        MoveCam();
    }

    private void startGame(Coord firstClick){
        gameOver = false;
        gameStarted = true;

        string holderBombName = "Map Bombs";

        Transform bombHolder = new GameObject(holderBombName).transform;
        bombHolder.parent = mapHolder;

        bombTiles = new Queue<Tile>();

        bool[,] game = Solver.getSolver().GenerateSolvableMap(mapSize.x, mapSize.y, bombCount, firstClick);

        for(int i = 0; i < mapSize.x; i++){
            for(int j = 0; j < mapSize.y; j++){
                bool isBomb = game[i,j];
                Coord c = new Coord(i, j);
                allTiles[c].isBomb = isBomb;
            }
        }

        foreach(Tile t in allTiles.Values){
            Coord c = t.coord;
            if(t.isBomb){
                SpawnBomb(c, bombHolder);
            } else {
                SpawnUnderTile(c, bombHolder, CalculateBombs(c));
            }
        }

        if(OnNewGame != null){
            OnNewGame();
        }
    }

    private void SpawnTile(Coord coord, Transform parent, Tile tile){
        Vector3 tilePosition = CoordToPosition(coord);
        Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
        newTile.localScale = Vector3.one * 0.95f ;
        newTile.parent = parent;

        tile.overTile = newTile;
    }

    private void SpawnUnderTile(Coord coord, Transform parent, int count){
        Vector3 tilePosition = CoordToPosition(coord.x,coord.y,1);
        Transform newTile = Instantiate(underTilePrefab, tilePosition, Quaternion.identity);
        newTile.localScale = Vector3.one * 0.95f ;
        newTile.parent = parent;

        Tile tile = allTiles[coord];
        tile.adjBombs = count;
        tile.underTile = newTile;

        TextMeshPro tmp = newTile.gameObject.GetComponentInChildren<TextMeshPro>();
        if(count > 0){
            tmp.text = count.ToString();
            tmp.color = countColours[count -1];

        } else {
            tmp.text = "";
        }
    }

    private void SpawnBomb(Coord coord, Transform parent){
        Vector3 bombPosition = CoordToPosition(coord.x,coord.y,1);
        Transform newBomb = Instantiate(bombPrefab, bombPosition, Quaternion.identity);
        newBomb.localScale = Vector3.one * 0.5f ;
        newBomb.parent = parent;

        Tile tile = allTiles[coord];
        tile.isBomb = true;
        tile.underTile = newBomb;

        bombTiles.Enqueue(tile);
    }

    private bool CoordIsValidBombCoord(Coord startCoord, Coord test){
        if(test == null) {
            return false;
        }

        int x = startCoord.x;
        int y = startCoord.y;

        bool isValid = (test.x > x + 2 || test.x < x -2) ||
                (test.y > y + 2 || test.y < y -2);

        return isValid;
    }

    private int CalculateBombs(Coord c){
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
                if(allTiles[adj].isBomb){
                    i ++;
                }
            }
        }

        return i;
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
        RaycastHit hit;

        if(Physics.Raycast(touchRay, out hit, 100f)) {
            Transform it = hit.collider.transform;
            if(it != null){
                Coord hitCoord = PositionToCoord(it.position);

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
    }

    private void HandleTileFlag(Ray touchRay){
        if(!gameOver){
            RaycastHit hit;

            if(Physics.Raycast(touchRay, out hit, 100f)) {
                Transform it = hit.collider.transform;
                if(it != null){
                    Coord hitCoord = PositionToCoord(it.position);
                    if(allTiles.ContainsKey(hitCoord)){
                        allTiles[hitCoord].Flag();
                    }
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
        x = Mathf.Clamp(x, 0, mapSize.x);
        y = Mathf.Clamp(y, 0, mapSize.y);

        return new Coord(x,y);
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

    public class Tile{
        public static System.Action<Tile> OnBombHit;
        public static System.Action<bool> OnFlagChange;
        public static System.Action OnCheckWin;
        public static ParticleSystem popEffect;
        public static ParticleSystem flagEffect;
        public static ParticleSystem revealEffect;

        public Coord coord;
        public Transform overTile;
        public Transform underTile;

        public bool isRevealed = false;
        public bool isBomb = false;
        public bool isFlagged = false;

        public int adjBombs;

        public Tile(Coord _coord){
            this.coord = _coord;
        }

        public bool Reveal(){
            bool revealed = false;

            if(!isFlagged && !isRevealed){
                isRevealed = true;
                revealed = true;
                AudioManager.instance.PlaySound2d("reveal",.5f);
                Destroy(overTile.gameObject);
                Vector3 position = overTile.transform.position;
                Destroy(Instantiate(revealEffect.gameObject, position, Quaternion.identity), revealEffect.main.startLifetime.constant);
                OnCheckWin();
            }

            if(isBomb && isRevealed){
                if(OnBombHit != null){
                    OnBombHit(this);
                    if(PlayerPrefs.GetInt("settings-vib", 1) == 1){
                        Vibrator.Vibrate(200);
                    }
                }
            }

            return revealed;
        }

        public void Pop(float vol){
            if(!isRevealed){
                isRevealed = true;
                AudioManager.instance.PlaySound2d("pop", vol);
                Destroy(overTile.gameObject);
                Transform t = underTile.transform;
                t.position += Vector3.back * 6f;
                Destroy(Instantiate(popEffect.gameObject, t), popEffect.main.startLifetime.constant);
                //Destroy(underTile.gameObject, popEffect.main.startLifetime.constant / 2f);
            }
        }

        public void Flag(){
            if(!isRevealed){
                isFlagged = !isFlagged;
                if(isFlagged){
                    overTile.GetComponent<Renderer>().material.color = flagColor;
                } else {
                    overTile.GetComponent<Renderer>().material.color = overTileStartColor;
                }

                if(OnFlagChange != null){
                    OnFlagChange(isFlagged);
                }

                AudioManager.instance.PlaySound2d("flag");

                Vector3 position = overTile.transform.position;
                Destroy(Instantiate(flagEffect.gameObject, position, Quaternion.identity), flagEffect.main.startLifetime.constant);

                if(PlayerPrefs.GetInt("settings-vib", 1) == 1){
                    Vibrator.Vibrate(100);
                }
            }
        }
    }

}
