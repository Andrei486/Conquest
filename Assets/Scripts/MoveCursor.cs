using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objects;

public class MoveCursor : Cursor
{
	public Cursor mainCursor;
	public BattleMenu menu;
	BoardSpace startSpace;
	PlayerController pc;
	bool firstFrame = true;
    // Start is called before the first frame update
    void Start()
    {
		mainCursor = GameObject.FindGameObjectsWithTag("Cursor")[0].GetComponent<Cursor>();
		mainCursor.locked = true;
		mainCursor.MakeVisible(false);
		this.locked = false;
		board = BoardManager.GetBoard();
		uI = UIController.GetUI();
		board.moveCursor = this;
		position = mainCursor.position;
		startSpace = board.GetSpace(position);
		pc = startSpace.occupyingUnit.GetComponent<PlayerController>();
		// pc.ShowAccessibleSpaces();
		this.gameObject.transform.position = startSpace.anchorPosition + new Vector3 (0f, 5f, 0f);
		this.camera = mainCursor.camera;
		selector = mainCursor.selector;
		menu = mainCursor.selector.GetComponent<CursorSelector>().menu;
		controls = ControlsManager.GetControls();
    }

    // Update is called once per frame
    void LateUpdate()
    {
		if (this.firstFrame){
			this.firstFrame = false;
			return;
		}
        if (this.locked || board.locked){
			return;
		}
		movedFrame = false;
		if (!(camera.GetComponent<Camera>().rotating || this.moving || this.movedTemporary)){ //to prevent camera going off-center, do not move if already moving or turning
			//cursor movement
			Quaternion rotation = Quaternion.AngleAxis((int) camera.transform.eulerAngles.y, Vector3.back);
			if (Input.GetKey(controls.GetCommand(Command.MOVE_DOWN))){
				Move(rotation * Vector3.down);
			}
			if (Input.GetKey(controls.GetCommand(Command.MOVE_UP))){
				Move(rotation * Vector3.up);
			}
			if (Input.GetKey(controls.GetCommand(Command.MOVE_LEFT))){
				Move(rotation * Vector3.left);
			}
			if (Input.GetKey(controls.GetCommand(Command.MOVE_RIGHT))){
				Move(rotation * Vector3.right);
			}
		}
		
		//select and deselect commands work regardless
		if (!movedFrame && Input.GetKeyDown(controls.GetCommand(Command.CONFIRM))){
			Select(board.boardSpaces[(int) position.x, (int) position.y]);
		}
		if (!movedFrame && Input.GetKeyDown(controls.GetCommand(Command.BACK))){
			Deselect();
		}
    }
	
	public override void Select(BoardSpace space){
		if (firstFrame){
			return;
		}
		if (!pc.GetAccessibleSpaces((int) pc.boardPosition.x, (int) pc.boardPosition.y).Contains(space)){
			return; //can't move to an unaccessible space
		}
		if (startSpace == space){
			Deselect();
		}
		
		if (movedTemporary){
			board.MoveUnit(startSpace, space);
			pc.hasActed = true; //moving counts as an action
			pc.EndTurnIfNeeded();
			mainCursor.locked = false;
			mainCursor.Move(space.boardPosition - startSpace.boardPosition);
			mainCursor.MakeVisible(true);
			if (!pc.turnEnded){
				menu.ShowActionList(pc);
			} else {
				mainCursor.Deselect();
			}
			Destroy(this.gameObject);
		} else {
			board.TempMoveUnit(startSpace, space);
			movedTemporary = true;
			mainCursor.temporarySpace = space;
		}
		BoardManager.ClearVisualization(); //clear the accessible-area visualization
	}
	
	public override void Deselect(){
		mainCursor.locked = false;
		mainCursor.MakeVisible(true);
		selector.GetComponent<CursorSelector>().SetPosition(startSpace);
		mainCursor.selectedSpace = startSpace;
		menu.ShowActionList(pc);
		BoardManager.ClearVisualization();
		if (startSpace.occupyingUnit.transform.Find("Temporary") != null){
			Destroy(startSpace.occupyingUnit.transform.Find("Temporary").gameObject);
		}
		Destroy(this.gameObject);
	}
}
