using UnityEngine;
using UnityEngine.UI;

public class MinesweeperTile
{

    private readonly Color safeColor = new Color(0.6f, 0.6f, 0.6f);
    private readonly Color winColor = new Color(0f, 0.6f, 0f);
    private readonly Color loseColor = new Color(0.8f, 0f, 0f);
    private readonly Color loseBlameColor = new Color(1f, 0.5f, 0.5f);

    public GameObject gameObject;
    public TileState state;

    private GameObject mineSprite, flagSprite, unknownSprite, textCanvas;
    private Text numText;

    public MinesweeperTile(GameObject tile)
    {
        gameObject = tile;
        state = new TileState();

        mineSprite = tile.transform.Find("Mine").gameObject;
        flagSprite = tile.transform.Find("Flag").gameObject;
        unknownSprite = tile.transform.Find("Unknown").gameObject;
        textCanvas = tile.transform.Find("Canvas").gameObject;
        numText = textCanvas.transform.GetChild(0).GetComponent<Text>();
    }

    private void UpdateGraphics()
    {
        mineSprite.SetActive(false);
        flagSprite.SetActive(false);
        unknownSprite.SetActive(false);

        switch (state.type)
        {
            case TileState.TileStateType.FlagMark:
                flagSprite.SetActive(true);
                break;
            case TileState.TileStateType.UnknownMark:
                unknownSprite.SetActive(true);
                break;
            case TileState.TileStateType.Clear:
                gameObject.GetComponent<SpriteRenderer>().color = safeColor;
                break;
            case TileState.TileStateType.Number:
                gameObject.GetComponent<SpriteRenderer>().color = safeColor;
                textCanvas.SetActive(true);
                numText.text = state.minesNearby.ToString();
                break;
            case TileState.TileStateType.Mine:
                mineSprite.SetActive(true);
                break;
        }
    }

    public void ActivateGameOver(bool win)
    {
        bool blame = state.isMine && !state.clickable;
        Reveal();
        gameObject.GetComponent<SpriteRenderer>().color = win ? winColor : (blame ? loseBlameColor : loseColor);
    }

    public void Reveal()
    {
        if (!state.clickable) return;
        state.clickable = false;

        if (state.isMine)
        {
            state.type = TileState.TileStateType.Mine;
            GameScript.instance.GameOver(false);
        }
        else if (state.minesNearby > 0)
        {
            state.type = TileState.TileStateType.Number;
        }
        else
        {
            state.type = TileState.TileStateType.Clear;
        }

        UpdateGraphics();
    }

    public void Mark()
    {
        if (!state.clickable) return;

        switch (state.type)
        {
            case TileState.TileStateType.Untouched:
                state.type = TileState.TileStateType.FlagMark;
                break;
            case TileState.TileStateType.FlagMark:
                state.type = TileState.TileStateType.UnknownMark;
                break;
            case TileState.TileStateType.UnknownMark:
                state.type = TileState.TileStateType.Untouched;
                break;
        }

        UpdateGraphics();
    }

    public class TileState
    {
        public TileStateType type = TileStateType.Untouched;
        public bool clickable = true;
        public bool isMine = false;
        public int minesNearby = 0;

        public enum TileStateType
        {
            Untouched,
            FlagMark,
            UnknownMark,

            Clear,
            Number,
            Mine
        }
    }

}
