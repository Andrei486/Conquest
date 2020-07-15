using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objects;

namespace InBattle{	
	public class Cursor : MonoBehaviour
	{
		public Vector2 position;
		public BoardSpace selectedSpace = null;
		public BoardSpace temporarySpace = null;
		public bool movedTemporary = false;
		public float moveTime = 0.03f; //it takes the cursor 0.1s to move one space
		public bool moving = false;
		public bool locked = false;
		public bool rotationLocked = false;
		Vector3 offset = new Vector3 (0f, 2.5f, 0f);
		Vector3 playerOffset = new Vector3 (0f, 1f, 0f);
		
		public new GameObject camera;
		public BoardManager board;
		public GameObject selector;
		
		public GameObject cursorSelectPrefab;
		public GameObject moveCursorPrefab;
		protected ControlsManager controls;
		protected UIController uI;
		protected bool movedFrame = false;

		private float EMPTY_SPACE_HEIGHT = 2.0f;
		private float ABOVE_UNIT_HEIGHT = 1.0f;
		// Start is called before the first frame update
		void Start()
		{
			board = BoardManager.GetBoard(); //find board object and script
			controls = ControlsManager.GetControls();
			uI = UIController.GetUI();
			selectedSpace = null;
			position = new Vector2(0, 0);
			this.gameObject.transform.position = board.boardSpaces[0, 0].anchorPosition + offset;
			if (board.boardSpaces[0, 0].occupyingUnit != null){
				this.gameObject.transform.position += playerOffset;
			}
			selector = Instantiate(cursorSelectPrefab, this.gameObject.transform);
			selector.GetComponent<CursorSelector>().SetPosition(new Vector2(0, 0));
			camera = GameObject.FindGameObjectsWithTag("MainCamera")[0]; //find camera object
			UpdatePosition(Vector2.zero);
		}

		// Update is called once per frame
		void LateUpdate()
		{
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
			// if (Input.GetKeyDown(controls.GetCommand(Command.BACK))){
			// 	Deselect();
			// }
		}
		
		public void Move(Vector2 movement){
			/**Moves the cursor on the grid by movement, specified in grid units.*/
			Vector2 newPosition = position + movement;
			if (board.IsWithinBounds(newPosition)){
				UpdatePosition(newPosition);
			}
			movedFrame = true;
		}
		
		public void Move(Vector3 movement){
			/**Moves the cursor on the grid by movement, specified in grid units. Convenience method.*/
			Move(new Vector2((int) Math.Round(movement.x, 0), (int) Math.Round(movement.y, 0)));
		}
		
		virtual public void Select(BoardSpace space){
			/**Forwards the call to the attached cursor selector.*/
			selector.GetComponent<CursorSelector>().Select(space);
			uI.ShowUnitSummary(space);
		}
		
		virtual public void Deselect(){
			/**Forwards the call to the attached cursor selector.*/
			selector.GetComponent<CursorSelector>().Deselect();
			uI.ShowUnitSummary(board.GetSpace(position));
		}
		
		void UpdatePosition(Vector2 newPosition){
			/**Updates position of cursor to match the new position.*/
			Vector3 startPosition = this.gameObject.transform.position;
			BoardSpace endSpace = board.GetSpace(newPosition);
			Vector3 endPosition = GetFloatPosition(endSpace);
			selector.GetComponent<CursorSelector>().SetPosition(newPosition);
			//actually set the position
			this.position = newPosition;
			//move the cursor
			StartCoroutine(MoveForSeconds(startPosition, endPosition));
			uI.ShowUnitSummary(board.GetSpace(newPosition));
		}
		
		public IEnumerator MoveForSeconds(Vector3 startPosition, Vector3 endPosition){
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
			moving = false;
		}
		
		public void MakeVisible(bool enabled){
			/**Makes the cursor visible or invisible.*/
			this.gameObject.GetComponent<MeshRenderer>().enabled = enabled;
		}

		public Vector3 GetFloatPosition(BoardSpace space){
			/**Returns the transform position that this cursor should have if hovered
			on the given BoardSpace.!--*/
			Vector3 basePos = space.anchorPosition;
			if (space.occupyingUnit == null){
				return basePos + Vector3.up * EMPTY_SPACE_HEIGHT;
			} else {
				float unitHeight = space.occupyingUnit.GetComponent<PlayerController>().playerRenderer.bounds.size.y * space.occupyingUnit.transform.localScale.y;
				return basePos + Vector3.up * (unitHeight + ABOVE_UNIT_HEIGHT);
			}
		}
	}
}