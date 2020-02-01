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
		if (cursorScript.selectedSpace == null){ //no space selected
			if (space.occupyingUnit != null){
				cursorScript.selectedSpace = space; //select the space if none already selected and not empty
				activeSelect.GetComponent<SpriteRenderer>().enabled = true;
				cursorScript.selectedSpace.occupyingUnit.GetComponent<PlayerController>().ShowAccessibleSpaces();
				activeSelect.transform.position = space.anchorPosition + new Vector3(0f, 0.02f, 0f);
			}
		} else { //something already selected
			PlayerController pc = cursorScript.selectedSpace.occupyingUnit.GetComponent<PlayerController>();
			if (space == cursorScript.selectedSpace){
					Deselect(); //can't double-select a space
				} else {
					if (pc.GetAccessibleSpaces(pc.moveRange, (int) pc.boardPosition.x, (int) pc.boardPosition.y).Contains(space)){
						cursorScript.board.MoveUnit(cursorScript.selectedSpace, space); //move player if possible
						cursorScript.Move(new Vector2(0, 0)); //update the cursor's height if needed
						Deselect();
					}
				}
		}
	}
	public void Deselect(){
		//clear all visualizations for unit's move range
		foreach (GameObject vis in GameObject.FindGameObjectsWithTag("Visualization")){
			Destroy(vis);
		}
		
		cursorScript.selectedSpace = null;
		activeSelect.GetComponent<SpriteRenderer>().enabled = false;
	}
}
