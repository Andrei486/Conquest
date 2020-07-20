using System.Collections;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Objects;
using Newtonsoft.Json;
using JsonConverters;

namespace InBattle{	
	[Serializable]
	public class PlayerController : MonoBehaviour
	{
		public const float MOVE_TILE_TIME = 0.5f;
		public const float SHORT_ROTATION_TIME = 0.1f;
		public const float LONG_ROTATION_TIME = 0.5f;
		public const float LONG_ROTATION_THRESHOLD = 60f; //in degrees
		public const float BASE_JUMP_HEIGHT = 0.5f;
		public const float FAST_FORWARD_SPEED = 2.5f;
		public static List<Vector2Int> HORIZONTAL_DIRECTIONS = new List<Vector2Int>{Vector2Int.left, Vector2Int.right};
		public static List<Vector2Int> VERTICAL_DIRECTIONS = new List<Vector2Int>{Vector2Int.up, Vector2Int.down};
		public Vector2 boardPosition;
		public float jumpHeight;
		public int moveRange;
		public int maxActions;
		public int maxBullets;
		public List<Skill> skillList = new List<Skill>();
		BoardManager board;
		public int[,] moveGrid;
		public int remainingMove;
		public int remainingActions;
		public int bullets;
		public UnitAffiliation affiliation;
		public bool isCommander = false;
		public bool turnEnded = false;
		public bool hasActed = false;
		public Health health;
		public UnitAction previousAction;
		public AutoMoveInfo autoMove;
		public string classTitle = "";
		public Sprite unitSprite;
		public Sprite armySprite;
		public string modelName;
		public Vector3 defaultEulerAngles;
		public bool saveAfterBattle = false; //if true, save the unit's info to units file after battle.
		public bool rotating = false;
		public bool moving = false;
		public Renderer playerRenderer;
		private Animator animator;
		
		// Start is called before the first frame update
		void Start()
		{
			board = BoardManager.GetBoard();
			health = this.gameObject.GetComponent<Health>();
			// bullets = maxBullets;
			// remainingMove = moveRange;
			// remainingActions = maxActions;
			moveGrid = new int[board.columns, board.rows];
			defaultEulerAngles = gameObject.transform.eulerAngles;
			UpdateMoveGrid((int) boardPosition.x, (int) boardPosition.y);

			if (GetComponentInChildren<Renderer>() != null){
				playerRenderer = GetComponentInChildren<Renderer>();
			}

			animator = GetComponentInChildren<Animator>();
			animator.runtimeAnimatorController = Instantiate(PrefabManager.GetPrefabs().animatorController); //set the animator controller
			MakeCollider();
		}

		// Update is called once per frame
		void Update()
		{
			if (Input.GetKey(ControlsManager.GetControls().GetCommand(Command.CONFIRM))){
				animator.speed = FAST_FORWARD_SPEED;
			} else {
				animator.speed = 1;
			}
		}

		private void MakeCollider(){
			/**Adds a CapsuleCollider to the current player, adjusted to the player's renderer.!--*/
			CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
			float radius = playerRenderer.bounds.size.y * 0.1f;
			float height = playerRenderer.bounds.size.y * 0.8f;
			Vector3 centerPos = playerRenderer.bounds.extents;
			collider.radius = radius;
			collider.height = height;
			collider.center = centerPos;
			collider.direction = 1; //capsule points upwards (along y axis)
		}
		
		void FillMoveGrid(Vector2Int pos, bool preferHorizontal){
			/**Recursive function to fill the move grid.*/
			if (moveGrid[pos.x, pos.y] == 0){
				return;
			}
			
			BoardSpace currentSpace = board.GetSpace(pos);
			BoardSpace toCheck;
			Vector2Int newPos;
			int movementLeft = moveGrid[pos.x, pos.y];
			List<Vector2Int> directions = new List<Vector2Int>();
			if (preferHorizontal){
				directions.AddRange(HORIZONTAL_DIRECTIONS);
				directions.AddRange(VERTICAL_DIRECTIONS);
			} else {
				directions.AddRange(VERTICAL_DIRECTIONS);
				directions.AddRange(HORIZONTAL_DIRECTIONS);
			}
			foreach (Vector2Int direction in directions){
				newPos = pos + direction;
				if (board.IsWithinBounds(newPos)){ //check if space exists
					toCheck = board.GetSpace(newPos);
					if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){
						if (movementLeft >= toCheck.moveCost 
							&& (toCheck.occupyingUnit == null
                            || board.armyManager.IsFriendly(toCheck.occupyingUnit.GetComponent<PlayerController>().affiliation, affiliation))){
							if (moveGrid[newPos.x, newPos.y] < moveGrid[pos.x, pos.y] - toCheck.moveCost){
								moveGrid[newPos.x, newPos.y] = moveGrid[pos.x, pos.y] - toCheck.moveCost;
								FillMoveGrid(newPos, !preferHorizontal); //prefer zigzag patterns
							}
						}
					}
				}
			}
		}

		public int[,] FillDistanceGrid(){
			/**Update and return the distance grid based on this unit's current position.
			A distance grid shows the minimum distance to a unit for all spaces on the grid.!--*/
			int[,] distanceGrid = new int[board.columns, board.rows];
			for (int i=0; i < board.columns; i++){
				for (int j=0; j < board.rows; j++){
					distanceGrid[i, j] = Int32.MaxValue;
				}
			}
			Queue<Vector2Int> queue = new Queue<Vector2Int>();
			queue.Enqueue(Vector2Int.RoundToInt(boardPosition));
			BoardSpace currentSpace;
			BoardSpace toCheck;
			Vector2Int currentPos;
			Vector2Int newPos;
			distanceGrid[(int) boardPosition.x, (int) boardPosition.y] = 0;
			while (queue.Count > 0){
				currentPos = Vector2Int.RoundToInt(queue.Dequeue());
				currentSpace = board.GetSpace(currentPos);
				foreach (Vector2Int direction in HORIZONTAL_DIRECTIONS.Concat(VERTICAL_DIRECTIONS)){
					newPos = currentPos + direction;
					toCheck = board.GetSpace(newPos);
					if (!board.IsWithinBounds(newPos)){
						continue;
					}
					//if an enemy occupies the space, set the distance for that space but don't enter it
					if (toCheck.occupyingUnit != null && !board.armyManager.IsFriendly(toCheck.occupyingUnit.GetComponent<PlayerController>().affiliation, affiliation)){
						distanceGrid[newPos.x, newPos.y] = distanceGrid[currentPos.x, currentPos.y]; //no move cost, don't need to move in
					}
					if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight && (toCheck.occupyingUnit == null
                        || board.armyManager.IsFriendly(toCheck.occupyingUnit.GetComponent<PlayerController>().affiliation, affiliation))){
						if (distanceGrid[newPos.x, newPos.y] > distanceGrid[currentPos.x, currentPos.y] + toCheck.moveCost){ //found a closer route
							distanceGrid[newPos.x, newPos.y] = distanceGrid[currentPos.x, currentPos.y] + toCheck.moveCost;
							queue.Enqueue(newPos);
						}
					}
				}
			}
			return distanceGrid;
		}
		
		public void UpdateMoveGrid(int startX, int startY){
			/**Fills the move grid with the appropriate values.*/
			for (int x = 0; x < board.columns; x++){
				for (int y = 0; y < board.rows; y++){
					moveGrid[x, y] = -1;
				}
			}
			moveGrid[startX, startY] = remainingMove;
			FillMoveGrid(new Vector2Int(startX, startY), true);
		}
		
		public List<BoardSpace> GetShortestPath(Vector2Int target){
			/**Returns a list of BoardSpaces corresponding to the shortest path from the current space to the target space.
			Assumes that the target space can be reached given the unit's movement.*/
			Vector2Int pos = target;
			Vector2Int newPos;
			BoardSpace currentSpace;
			BoardSpace newSpace;
			List<BoardSpace> path = new List<BoardSpace>(){board.GetSpace(pos)};
			bool preferHorizontal = false;
			List<Vector2Int> directions;
			while (pos != this.boardPosition){
				currentSpace = board.GetSpace(pos);
				if (preferHorizontal){
					directions = new List<Vector2Int>();
					directions.AddRange(HORIZONTAL_DIRECTIONS);
					directions.AddRange(VERTICAL_DIRECTIONS);
				} else {
					directions = new List<Vector2Int>();
					directions.AddRange(VERTICAL_DIRECTIONS);
					directions.AddRange(HORIZONTAL_DIRECTIONS);
				}
				foreach (Vector2Int direction in directions){
					newPos = pos + direction;
					newSpace = board.GetSpace(newPos);
					if (board.IsWithinBounds(newPos)){ //if space is within bounds
						if (Math.Abs(newSpace.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){  //if it can be moved to directly
							if (moveGrid[pos.x, pos.y] + currentSpace.moveCost == moveGrid[newPos.x, newPos.y]){ //may not be necessary
								path.Add(newSpace);
								pos = newPos;
								preferHorizontal = (direction.x == 0); //make zigzags: if was going horizontal, stop, and vice versa
								break;
							}
						}
					}
				}
			}
			path.Reverse();
			return path;
		}
		
		public void EndTurn(){
			/**Ends the unit's turn and resets its movement and actions.*/
			this.turnEnded = true;
			Debug.Log("turn ended");
			while (this.board.AdvancePhase()){
				; //advance phases until the next available one
			}
		}

		public PlayerInfo GetPlayerInfo(){
			PlayerInfo info = new PlayerInfo();
			info.playerController = this;
			info.health = this.health;
			return info;
		}
		
		public HashSet<BoardSpace> GetAccessibleSpaces(int startX, int startY){
			/**Returns the set of BoardSpaces that the unit can move to with its remaining movement.*/
			UpdateMoveGrid(startX, startY);
			HashSet<BoardSpace> accessibleSpaces = new HashSet<BoardSpace>();
			for (int x = 0; x < board.columns; x++){
				for (int y = 0; y < board.rows; y++){
					if (moveGrid[x, y] >= 0){
						accessibleSpaces.Add(board.GetSpace(new Vector2(x, y)));
					}
				}
			}
			return accessibleSpaces;
		}
		
		public void ShowAccessibleSpaces(){
			/**Shows a visualization of the area that the unit can move to with its remaining movement.*/
			HashSet<BoardSpace> accessibleSpaces = GetAccessibleSpaces((int) this.boardPosition.x, (int) this.boardPosition.y);
			BoardManager board = BoardManager.GetBoard();
			foreach (BoardSpace space in accessibleSpaces){
				space.SetHighlightMaterial(board, HighlightType.MOVE);
			}
			BoardManager.GetBoard().HighlightSpaces(accessibleSpaces);
		}
		
		public void UseSkill(Skill skill, Quaternion direction){
			/**Uses the Skill skill. Assumes that using it is possible.*/
			BattleLog.GetLog().Log(this.name + " used " + skill.name);
			health.UseSkill(skill, direction);
			this.remainingMove -= skill.moveCost;
			this.remainingActions -= skill.actionCost;
			this.bullets -= skill.bulletCost;
			this.previousAction = UnitAction.SKILL;
		}
		
		public bool CanMove(){
			/**Returns true if the user can still move, even if there is no space to move to due to obstacles; false otherwise.*/
			return (remainingMove > 0 && !turnEnded); //cannot move if the last action taken was a move.
		}
		
		public bool CanAct(){
			/**Returns true if the unit has actions left to use, false otherwise.*/
			return (remainingActions > 0 && !turnEnded);
		}
		
		public bool CanUse(Skill skill){
			/**Returns true if the unit can use the specified skill, false otherwise.*/
			if (!CanAct()){
				return false;
			}
			if (remainingActions < skill.actionCost || bullets < skill.bulletCost || remainingMove < skill.moveCost){
				return false;
			}
			return true;
		}
		
		public List<Skill> GetUsableSkills(){
			List<Skill> usable = new List<Skill>();
			foreach (Skill skill in skillList){
				if (CanUse(skill)){
					usable.Add(skill);
				}
			}
			return usable;
		}
		
		public List<UnitAction> GetUsableActions(){
			List<UnitAction> actions = new List<UnitAction>();
			
			if (CanMove()){
				actions.Add(UnitAction.MOVE);
			}
			if (CanAct()){
				if (GetUsableSkills().Count > 0){
					actions.Add(UnitAction.SKILL);
				}
			}
			actions.Add(UnitAction.WAIT);
			return actions;
		}
		
		public bool EndTurnIfNeeded(){
			/**If the player cannot move or act, ends the player's turn.*/
			if (!CanMove() && !CanAct()){
				EndTurn();
				return true;
			}
			return false;
		}
		
		public void MakeSemiTransparent(bool enabled){
			Color color = playerRenderer.material.color;
			if (enabled){
				playerRenderer.material.SetFloat("_Metallic", 0f); //make nonmetallic and semitransparent
				color.a = 0.5f;
			} else {
				playerRenderer.material.SetFloat("_Metallic", 0.4f); //make metallic and opaque
				color.a = 1.0f;
			}
			playerRenderer.material.SetColor("_Color", color);
		}

		private IEnumerator Rotate(Vector2 previous, Vector2 final){
			/**Rotates the player by the angle between the two vectors.!--*/
			float angle = Vector2.SignedAngle(final, previous);
			float timeToRotate;
			bool wasMoving = true;
			if (Math.Abs(angle) > 1.0f){ //do not rotate if the path is straight, to avoid random stops
				this.rotating = true;
				if (Math.Abs(angle) >= LONG_ROTATION_THRESHOLD){
					timeToRotate = LONG_ROTATION_TIME;
					wasMoving = animator.GetBool("moving");
					animator.SetBool("moving", false);
				} else {
					timeToRotate = SHORT_ROTATION_TIME;
				}
				
				float rotationPerFrame = angle / timeToRotate * Time.deltaTime;
				float totalRotation = 0f;
				
				while (totalRotation < Math.Abs(angle) - 0.5f){ //it is better to undershoot so error correction is less noticeable
					totalRotation += Math.Abs(rotationPerFrame * animator.speed);
					this.gameObject.transform.Rotate(Vector3.up * rotationPerFrame * animator.speed, Space.World);
					yield return new WaitForEndOfFrame();
				}
				
				this.gameObject.transform.Rotate(Vector3.up * (angle - totalRotation * Math.Sign(angle)), Space.World); //account for errors produced
				
				this.rotating = false;
				if (Math.Abs(angle) >= LONG_ROTATION_THRESHOLD){
					timeToRotate = LONG_ROTATION_TIME;
					animator.SetBool("moving", wasMoving);
				}
			}
			yield return null;
		}

		public IEnumerator RotateTo(Vector2 final){
			/**Rotates the player to the angle specified by the vector.!--*/
			Vector2 initialRotation = Vector2.up.Rotate(-transform.eulerAngles.y);
			StartCoroutine(Rotate(initialRotation, final));
			yield return null;
		}

		public IEnumerator Move(BoardSpace start, BoardSpace end){
			/**Moves the player from start to end.!--*/
			this.moving = true;
			Vector3 startPos = start.anchorPosition;
			Vector3 endPos = end.anchorPosition;
			float totalMoveTime = MOVE_TILE_TIME * Vector2.Distance(start.boardPosition, end.boardPosition);
			float t = 0f;
			while (t < 1){
				t += Time.deltaTime * animator.speed / totalMoveTime;
				transform.position = Vector3.Lerp(startPos, endPos, t);
				yield return new WaitForEndOfFrame();
			}
			transform.position = endPos;
			this.moving = false;
			yield return null;
		}

		public IEnumerator FollowPath(List<BoardSpace> path){
			/**Moves this unit along the path of BoardSpaces.!--*/
			board.movingUnit = true;
			Vector2 currentPosition = this.boardPosition;
			Vector2 nextDirection;
			animator.SetBool("moving", true);
			foreach (BoardSpace next in path){
				nextDirection = next.boardPosition - currentPosition;
				StartCoroutine(RotateTo(nextDirection)); //rotate to face the next space
				yield return new WaitUntil(() => !this.rotating);
				StartCoroutine(Move(board.GetSpace(currentPosition), next)); //move to it
				yield return new WaitUntil(() => !this.moving);
				currentPosition = next.boardPosition;
			}
			animator.SetBool("moving", false);
			board.movingUnit = false;
			yield return null;
		}
	}
}