using UnityEngine;
using Objects;
using System.Linq;

public class CursorSelector : MonoBehaviour
{
	public BattleMenu menu;
	GameObject cursor;
	Vector3 offset = new Vector3(0, 0.1f, 0);

	Cursor cursorScript;
	new SpriteRenderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = this.gameObject.GetComponent<SpriteRenderer>();
		cursor = GameObject.FindGameObjectsWithTag("Cursor")[0];
		cursorScript = cursor.GetComponent<Cursor>();
		menu = BattleMenu.GetMenu();
    }

    // Update is called once per frame
    void Update()
    {
		
    }

	public void SetPosition(Vector2 newPosition){
		SetPosition(BoardManager.GetBoard().GetSpace(newPosition));
	}

	public void SetPosition(BoardSpace space){
		LineRenderer renderer = this.GetComponent<LineRenderer>();
		Vector3[] corners = (from corner in space.corners select corner + offset).ToArray();
		renderer.SetPositions(corners);
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
						//menu.ShowSkillList(cursorScript.selectedSpace.occupyingUnit.GetComponent<PlayerController>());
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
		if (space.occupyingUnit != null){
			PlayerController pc = space.occupyingUnit.GetComponent<PlayerController>();
			if (pc.affiliation == BoardManager.GetBoard().phase && !pc.turnEnded){
				//select the space if none already selected and the unit on it is selectable
				cursorScript.selectedSpace = space; 
				menu.ShowActionList(cursorScript.selectedSpace.occupyingUnit.GetComponent<PlayerController>());
			}
			
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
		BoardManager.ClearVisualization();
		BattleMenu.GetMenu().ToggleSkills(false);
		BattleMenu.GetMenu().ToggleActions(false);
	}
}
