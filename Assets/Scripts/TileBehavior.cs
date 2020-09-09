using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileBehavior : MonoBehaviour, IPointerClickHandler
{

    public int x, y;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            GameScript.instance.OnTileLeftClick(x, y);
        else if (eventData.button == PointerEventData.InputButton.Right)
            GameScript.instance.OnTileRightClick(x, y);
    }

}
