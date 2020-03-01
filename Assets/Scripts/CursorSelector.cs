using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objects;

public class CursorSelector : MonoBehaviour
{
	public Sprite normalSelector;
	public Sprite activeSelector;
	public Menu menu;
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
		menu = GameObject.FindGameObjectsWithTag("MenuController")[0].GetComponent<Menu>();
    }

    // Update is called once per frame
    void Update()
    {
		
    }
	
	public void Select(BoardSpace space){
		/**Selects a space with a unit.*/
		if (cursorScript.selectedSpace == null){ //no space selected
			SelectUnit(space);
		} else { //something already selected
		
			PlayerController pc = cursorScript.selectedSpace.occupyingUnit.GetComponent<PlayerController>();
			if (!cursorScript.movedTemporary){
				if (space == cursorScript.selectedSpace){
					Deselect(); //can't double-select a space
				} else {
					if (pc.GetAccessibleSpaces((int) pc.boardPosition.x, (int) pc.boardPosition.y).Contains(space)){
						cursorScript.board.TempMoveUnit(cursorScript.selectedSpace, space); //move player if possible
						cursorScript.Move(new Vector2(0, 0)); //update the cursor's height if needed
						BoardManager.ClearVisualization();
						cursorScript.movedTemporary = true;
						cursorScript.temporarySpace = space;
						menu.ShowSkillList(cursorScript.selectedSpace.occupyingUnit.GetComponent<PlayerController>());
					}
				}
			} else {
				if (pc.GetAccessibleSpaces((int) pc.boardPosition.x, (int) pc.boardPosition.y).Contains(space)){
					cursorScript.board.MoveUnit(cursorScript.selectedSpace, space); //move player if possible
					cursorScript.Move(new Vector2(0, 0)); //update the cursor's height if needed
					Deselect();
					cursorScript.movedTemporary = false;
					cursorScript.temporarySpace = null;
				}
			}
		}
		
	}
	
	private void SelectUnit(BoardSpace space){
		/**Selects the unit on the specified space, if it is selectable.*/
		if (space.occupyingUnit != null && space.occupyingUnit.GetComponent<PlayerController>().alreadyActed == false){
			cursorScript.selectedSpace = space; //select the space if none already selected and not empty
			activeSelect.GetComponent<SpriteRenderer>().enabled = true;
			//cursorScript.selectedSpace.occupyingUnit.GetComponent<PlayerController>().ShowAccessibleSpaces();
			menu.ShowActionList(cursorScript.selectedSpace.occupyingUnit.GetComponent<PlayerController>());
			activeSelect.transform.position = space.anchorPosition + new Vector3(0f, 0.02f, 0f);
		}
	}
	
	public void Deselect(){
		/**Deselects the currently selected unit.*/
		if (cursorScript.selectedSpace == null){
			return;
		}
		
		// if there was a temporary move player then remove it
		if (cursorScript.selectedSpace.occupyingUnit != null && cursorScript.selectedSpace.occupyingUnit.transform.Find("Temporary") != null){
			Destroy(cursorScript.selectedSpace.occupyingUnit.transform.Find("Temporary").gameObject);
		}
		cursorScript.movedTemporary = false;
		cursorScript.selectedSpace = null;
		activeSelect.GetComponent<SpriteRenderer>().enabled = false;
		BoardManager.ClearVisualization();
	}
}
