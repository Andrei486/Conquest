using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objects;

public class MoveCursor : Cursor
{
	public Cursor mainCursor;
	public Menu menu;
	BoardSpace startSpace;
	PlayerController pc;
    // Start is called before the first frame update
    void Start()
    {
		mainCursor = GameObject.FindGameObjectsWithTag("Cursor")[0].GetComponent<Cursor>();
		mainCursor.locked = true;
		mainCursor.MakeVisible(false);
		board = mainCursor.board;
		position = mainCursor.position;
		startSpace = board.GetSpace(position);
		pc = startSpace.occupyingUnit.GetComponent<PlayerController>();
		pc.ShowAccessibleSpaces();
		this.gameObject.transform.position = startSpace.anchorPosition + new Vector3 (0f, 5f, 0f);
		this.camera = mainCursor.camera;
		selector = mainCursor.selector;
		menu = mainCursor.selector.GetComponent<CursorSelector>().menu;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.locked){
			return;
		}
		if (!(camera.GetComponent<Camera>().rotating || this.moving || this.movedTemporary)){ //to prevent camera going off-center, do not move if already moving or turning
			//cursor movement
			Quaternion rotation = Quaternion.AngleAxis((int) camera.transform.eulerAngles.y, Vector3.back);
			if (Input.GetKey("down")){
				Move(rotation * Vector3.down);
			}
			if (Input.GetKey("up")){
				Move(rotation * Vector3.up);
			}
			if (Input.GetKey("left")){
				Move(rotation * Vector3.left);
			}
			if (Input.GetKey("right")){
				Move(rotation * Vector3.right);
			}
		}
		
		//select and deselect commands work regardless
		if (Input.GetKeyDown("space")){
			Select(board.boardSpaces[(int) position.x, (int) position.y]);
		}
		if (Input.GetKeyDown("backspace")){
			Deselect();
		}
    }
	
	public override void Select(BoardSpace space){
		if (!pc.GetAccessibleSpaces((int) pc.boardPosition.x, (int) pc.boardPosition.y).Contains(space)){
			return; //can't move to an unaccessible space
		}
		if (startSpace == space){
			return;
		}
		
		if (movedTemporary){
			board.MoveUnit(startSpace, space);
			pc.EndTurnIfNeeded();
			mainCursor.locked = false;
			mainCursor.Move(space.boardPosition - startSpace.boardPosition);
			mainCursor.MakeVisible(true);
			menu.ShowActionList(pc);
			mainCursor.temporarySpace = space;
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
		menu.ShowActionList(pc);
		Destroy(this.gameObject);
	}
}
