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
		SelectUnit(space);
		
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
