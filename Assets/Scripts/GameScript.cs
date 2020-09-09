using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour
{

    #region Fields

    #region Constants

    private struct LevelDefinition
    {
        internal Vector2Int dimensions;
        internal int mineCount;

        internal LevelDefinition(int x, int y, int m) {
            dimensions = new Vector2Int(x, y);
            mineCount = m;
        }
    }

    private readonly List<LevelDefinition> levelDefinitions = new List<LevelDefinition>()
    {
        new LevelDefinition(10, 8, 5),
        new LevelDefinition(16, 9, 10),
        new LevelDefinition(18, 10, 15)
    };

    private const float tileSize = 0.6f;
    private const float tileSpacing = 0.1f;

    #endregion

    #region External References

    [SerializeField] private GameObject titleScreenUI, levelSelectUI, inGameUI, gameOverUI;
    [SerializeField] private GameObject tileContainer;

    #endregion

    #region Variables

    private GameObject tilePrefab;

    private readonly List<List<MinesweeperTile>> tileMap = new List<List<MinesweeperTile>>();
    private readonly List<MinesweeperTile> mines = new List<MinesweeperTile>();
    private LevelDefinition currentLevelDefinition;
    private bool firstClick;
    private int numSafeLeft;

    public bool gameOver;

    #endregion

    #region Singleton

    public static GameScript instance;

    #endregion

    #endregion

    #region Methods

    #region Unity Built-ins

    void Awake()
    {
        instance = this;

        tilePrefab = Resources.Load<GameObject>("Tile");
    }

    void Start()
    {

    }

    void Update()
    {

    }

    #endregion

    #region Game Flow

    public void Quit()
    {
        Application.Quit();
    }

    public void StartLevel(int level)
    {
        levelSelectUI.SetActive(false);
        inGameUI.SetActive(true);

        currentLevelDefinition = levelDefinitions[level];
        firstClick = true;
        gameOver = false;
        numSafeLeft = (currentLevelDefinition.dimensions.x * currentLevelDefinition.dimensions.y) - currentLevelDefinition.mineCount;

        float tileOffset = tileSize + tileSpacing;
        float xStart = (1f - currentLevelDefinition.dimensions.x) / 2f * tileOffset;
        float yStart = (1f - currentLevelDefinition.dimensions.y) / 2f * tileOffset;

        tileMap.Clear();
        mines.Clear();
        
        for (float x = 0; x < currentLevelDefinition.dimensions.x; x++)
        {
            tileMap.Add(new List<MinesweeperTile>());

            for (float y = 0; y < currentLevelDefinition.dimensions.y; y++)
            {
                float xPos = xStart + tileOffset * x;
                float yPos = yStart + tileOffset * y;

                GameObject tile = Instantiate(tilePrefab);
                tile.transform.localPosition = new Vector3(xPos, yPos, 0);
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1);
                tile.transform.SetParent(tileContainer.transform, false);

                TileBehavior tileBehavior = tile.GetComponent<TileBehavior>();
                tileBehavior.x = (int)x;
                tileBehavior.y = (int)y;

                tileMap[(int)x].Add(new MinesweeperTile(tile));
            }
        }
    }

    private Vector2Int GenerateMinePosition()
    {
        return new Vector2Int(
            Random.Range(0, currentLevelDefinition.dimensions.x),
            Random.Range(0, currentLevelDefinition.dimensions.y)
        );
    }

    private bool CheckDuplicatePositions(List<Vector2Int> positionList, Vector2Int candidatePosition)
    {
        foreach (Vector2Int position in positionList)
        {
            if(position.Equals(candidatePosition))
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < currentLevelDefinition.dimensions.x && y < currentLevelDefinition.dimensions.y;
    }

    public void OnTileLeftClick(int x, int y)
    {

        if(firstClick)
        {
            firstClick = false;

            Vector2Int clickPosition = new Vector2Int(x, y);

            List<Vector2Int> minePositions = new List<Vector2Int>(currentLevelDefinition.mineCount);
            for (int i = 0; i < currentLevelDefinition.mineCount; i++)
            {
                Vector2Int minePositionCandidate = GenerateMinePosition();
                while(CheckDuplicatePositions(minePositions, minePositionCandidate) || minePositionCandidate.Equals(clickPosition))
                {
                    minePositionCandidate = GenerateMinePosition();
                }

                minePositions.Add(minePositionCandidate);
            }

            foreach(Vector2Int minePosition in minePositions) {

                tileMap[minePosition.x][minePosition.y].state.isMine = true;

                for (int px = -1; px < 2; px++)
                {
                    for (int py = -1; py < 2; py++)
                    {
                        if ((px | py) == 0) continue; // this is the mine tile

                        int tx = minePosition.x + px;
                        int ty = minePosition.y + py;

                        if (CheckInBounds(tx, ty))
                        {
                            tileMap[tx][ty].state.minesNearby++;
                        }
                    }
                }
            }
        }

        void SearchTile(int x1, int y1)
        {
            tileMap[x1][y1].Reveal();
            if (tileMap[x1][y1].state.type == MinesweeperTile.TileState.TileStateType.Mine) return;

            numSafeLeft--;
            if (numSafeLeft == 0) return;

            if (tileMap[x1][y1].state.type == MinesweeperTile.TileState.TileStateType.Clear)
            {
                for (int px = -1; px < 2; px++)
                {
                    for (int py = -1; py < 2; py++)
                    {
                        if ((px | py) == 0) continue;
                        int tx = x1 + px;
                        int ty = y1 + py;

                        if (CheckInBounds(tx, ty) && tileMap[tx][ty].state.clickable)
                        {
                            SearchTile(tx, ty);
                        }
                    }
                }
            }
        }

        SearchTile(x, y);
        
        if(numSafeLeft == 0)
        {
            GameOver(true);
        }
    }

    public void OnTileRightClick(int x, int y)
    {
        tileMap[x][y].Mark();
    }

    public void GameOver(bool win)
    {
        if (gameOver) return;
        gameOver = true;
        StartCoroutine(GameOverCoroutine(win));
    }

    private IEnumerator GameOverCoroutine(bool win)
    {
        inGameUI.SetActive(false);

        for (int x = 0; x < currentLevelDefinition.dimensions.x; x++)
        {
            for (int y = 0; y < currentLevelDefinition.dimensions.y; y++)
            {
                tileMap[x][y].ActivateGameOver(win);
            }
        }

        yield return new WaitForSeconds(3f);

        foreach (Transform child in tileContainer.transform)
        {
            Destroy(child.gameObject);
        }

        gameOverUI.SetActive(true);
    }

    public void PlayAgain()
    {
        gameOverUI.SetActive(false);
        levelSelectUI.SetActive(true);
    }

    public void ReturnToTitle()
    {
        gameOverUI.SetActive(false);
        titleScreenUI.SetActive(true);
    }

    #endregion

    #endregion

}
