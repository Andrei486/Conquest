using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objects;

public class CursorSelector : MonoBehaviour
{
	public Sprite normalSelector;
	public Sprite activeSelector;
	GameObject cursor;
	GameObject activeSelect;
	Cursor cursorScript;
	SpriteRenderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = this.gameObject.GetComponent<SpriteRenderer>();
		cursor = GameObject.FindGameObjectsWithTag("Cursor")[0];
		cursorScript = cursor.GetComponent<Cursor>();
		activeSelect = new GameObject("SelectedTile");
		activeSelect.AddComponent(typeof(SpriteRenderer));
		activeSelect.GetComponent<SpriteRenderer>().sprite = activeSelector;
		activeSelect.GetComponent<SpriteRenderer>().enabled = false;
		activeSelect.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
		
    }
	
	public void Select(BoardSpace space){
		if (cursorScript.selectedSpace == null){
			if (cursorScript.selectedSpace.occupyingUnit != null){
				cursorScript.selectedSpace = space; //select the space if none already selected and not empty
				activeSelect.GetComponent<SpriteRenderer>().enabled = true;
				activeSelect.transform.position = space.anchorPosition;
			} else{
				if (space == cursorScript.selectedSpace){
					Deselect(); //can't double-select a space
				} else {
					cursorScript.board.MoveUnit(cursorScript.selectedSpace, space);
					cursorScript.Move(new Vector2(0, 0)); //update the cursor's height if needed
					Deselect(); //move player if possible
				}
			}
		}
	}
	public void Deselect(){
		cursorScript.selectedSpace = null;
		activeSelect.GetComponent<SpriteRenderer>().enabled = false;
	}
}
