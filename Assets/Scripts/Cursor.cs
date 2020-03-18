using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objects;

public class Cursor : MonoBehaviour
{
	public Vector2 position;
	public BoardSpace selectedSpace = null;
	public BoardSpace temporarySpace = null;
	public bool movedTemporary = false;
	public float moveTime = 0.03f; //it takes the cursor 0.1s to move one space
	public bool moving = false;
	public bool lockSelection = false;
	public bool locked = false;
	public bool rotationLocked = false;
	
	public new GameObject camera;
	public BoardManager board;
	public GameObject selector;
	
	public GameObject cursorSelectPrefab;
	public GameObject moveCursorPrefab;
	protected ControlsManager controls = ControlsManager.GetControls();
    // Start is called before the first frame update
    void Start()
    {
		board = BoardManager.GetBoard(); //find board object and script
		position = new Vector2(0, 0);
		this.gameObject.transform.position = board.boardSpaces[0, 0].anchorPosition + new Vector3 (0f, 4f, 0f);
		if (board.boardSpaces[0, 0].occupyingUnit != null){
			this.gameObject.transform.position += new Vector3 (0f, 1f, 0f);
		}
		selector = Instantiate(cursorSelectPrefab, this.gameObject.transform);
		selector.transform.position = board.boardSpaces[0, 0].anchorPosition + new Vector3 (0f, 0.01f, 0f);
		camera = GameObject.FindGameObjectsWithTag("MainCamera")[0]; //find camera object
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
		if (Input.GetKeyDown(controls.GetCommand(Command.CONFIRM))){
			Select(board.boardSpaces[(int) position.x, (int) position.y]);
		}
		if (Input.GetKeyDown(controls.GetCommand(Command.BACK))){
			Deselect();
		}
    }
	
	public void Move(Vector2 movement){
		/**Moves the cursor on the grid by movement, specified in grid units.*/
		Vector2 newPosition = position + movement;
		if (board.IsWithinBounds(newPosition)){
			UpdatePosition(newPosition);
		}
	}
	
	public void Move(Vector3 movement){
		/**Moves the cursor on the grid by movement, specified in grid units. Convenience method.*/
		Move(new Vector2((int) Math.Round(movement.x, 0), (int) Math.Round(movement.y, 0)));
	}
	
	virtual public void Select(BoardSpace space){
		/**Forwards the call to the attached cursor selector.*/
		selector.GetComponent<CursorSelector>().Select(space);
	}
	
	virtual public void Deselect(){
		/**Forwards the call to the attached cursor selector.*/
		selector.GetComponent<CursorSelector>().Deselect();
	}
	
	void UpdatePosition(Vector2 newPosition){
		/**Updates position of cursor to match the new position.*/
		Vector3 startPosition = this.gameObject.transform.position;
		BoardSpace endSpace = board.boardSpaces[(int) newPosition.x, (int) newPosition.y];
		Vector3 endPosition = endSpace.anchorPosition + new Vector3 (0f, 4f, 0f);
		if (endSpace.occupyingUnit != null){
			endPosition += new Vector3 (0f, 1f, 0f);
		}
		Vector3 selectorEnd = board.boardSpaces[(int) newPosition.x, (int) newPosition.y].anchorPosition + new Vector3 (0f, 0.01f, 0f);
		StartCoroutine(MoveForSeconds(startPosition, endPosition, selectorEnd));
		//actually set the position
		this.position = newPosition;
	}
	
	public IEnumerator MoveForSeconds(Vector3 startPosition, Vector3 endPosition, Vector3 selectorEnd){
		/**Moves the cursor from startPosition to endPosition and the selector is moved accordingly.*/
		moving = true;
		float startTime = Time.time;
		float fractionTime = 0;
		
		//make the object "move"
		while (fractionTime < 1.0f){
			fractionTime = (Time.time - startTime) / moveTime;
			this.gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, fractionTime);
			yield return new WaitForEndOfFrame();
		}
		
		this.gameObject.transform.position = endPosition;
		selector.transform.position = selectorEnd;
		moving = false;
	}
	
	public void MakeVisible(bool enabled){
		/**Makes the cursor visible or invisible.*/
		this.gameObject.GetComponent<MeshRenderer>().enabled = enabled;
	}
}
