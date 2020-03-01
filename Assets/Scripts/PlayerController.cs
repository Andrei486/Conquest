using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Objects;

public class PlayerController : MonoBehaviour
{
	public const float MOVE_ANIMATION_TIME = 0.5f;
	public Vector2 boardPosition;
	public float attackPower;
	public float defense;
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
	public bool alreadyActed = false;
	public UnitAction previousAction;
	
    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.FindGameObjectsWithTag("Board")[0].GetComponent<BoardManager>();
		bullets = maxBullets;
		remainingMove = moveRange;
		remainingActions = maxActions;
		moveGrid = new int[board.columns, board.rows];
		UpdateMoveGrid((int) boardPosition.x, (int) boardPosition.y);
    }

    // Update is called once per frame
    void Update()
    {
		
    }
	
	void FillMoveGrid(int startX, int startY){
		/**Recursive function to fill the move grid.*/
		if (moveGrid[startX, startY] == 0){
			return;
		}
		
		BoardSpace currentSpace = board.boardSpaces[startX, startY];
		BoardSpace toCheck;
		int movementLeft = moveGrid[startX, startY];
		
		if (startX > 0){ //check left space if possible
			toCheck = board.boardSpaces[startX - 1, startY];
			if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){
				if (movementLeft >= toCheck.moveCost && toCheck.occupyingUnit == null){
					if (moveGrid[startX - 1, startY] < moveGrid[startX, startY] - toCheck.moveCost){
						moveGrid[startX - 1, startY] = moveGrid[startX, startY] - toCheck.moveCost;
						FillMoveGrid(startX - 1, startY);
					}
				}
			}
		}
		if (startY > 0){ //check lower space if possible
			toCheck = board.boardSpaces[startX, startY - 1];
			if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){
				if (movementLeft >= toCheck.moveCost && toCheck.occupyingUnit == null){
					if (moveGrid[startX, startY - 1] < moveGrid[startX, startY] - toCheck.moveCost){
						moveGrid[startX, startY - 1] = moveGrid[startX, startY] - toCheck.moveCost;
						FillMoveGrid(startX, startY - 1);
					}
				}
			}
		}
		if (startX < board.columns - 1){ //check right space if possible
			toCheck = board.boardSpaces[startX + 1, startY];
			if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){
				if (movementLeft >= toCheck.moveCost && toCheck.occupyingUnit == null){
					if (moveGrid[startX + 1, startY] < moveGrid[startX, startY] - toCheck.moveCost){
						moveGrid[startX + 1, startY] = moveGrid[startX, startY] - toCheck.moveCost;
						FillMoveGrid(startX + 1, startY);
					}
				}
			}
		}
		if (startY < board.rows - 1){ //check top space if possible
			toCheck = board.boardSpaces[startX, startY + 1];
			if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){
				if (movementLeft >= toCheck.moveCost && toCheck.occupyingUnit == null){
					if (moveGrid[startX, startY + 1] < moveGrid[startX, startY] - toCheck.moveCost){
						moveGrid[startX, startY + 1] = moveGrid[startX, startY] - toCheck.moveCost;
						FillMoveGrid(startX, startY + 1);
					}
				}
			}
		}
	}
	
	public void UpdateMoveGrid(int startX, int startY){
		/**Fills the move grid with the appropriate values.*/
		for (int x = 0; x < board.columns; x++){
			for (int y = 0; y < board.rows; y++){
				moveGrid[x, y] = -1;
			}
		}
		moveGrid[startX, startY] = remainingMove;
		FillMoveGrid(startX, startY);
	}
	
	public List<BoardSpace> GetShortestPath(int startX, int startY){
		/**Returns a list of BoardSpaces corresponding to the shortest path from the current space to the target space.
		Assumes that the target space can be reached given the unit's movement.*/
		int targetX = (int) this.boardPosition.x;
		int targetY = (int) this.boardPosition.y;
		int currentMove;
		int[] surroundings = new int[4];
		List<BoardSpace> path = new List<BoardSpace>();
		path.Add(board.GetSpace(new Vector2(startX, startY)));
		while (targetX != startX || targetY != startY){
			currentMove = moveGrid[startX, startY];
			surroundings = new int[4]{moveGrid[startX - 1, startY], moveGrid[startX, startY - 1], moveGrid[startX + 1, startY], moveGrid[startX, startY + 1]};
			switch(Array.IndexOf(surroundings, surroundings.Max())){
				case 0:
					startX -= 1;
					path.Add(board.GetSpace(new Vector2(startX, startY)));
					break;
				case 1:
					startY -= 1;
					path.Add(board.GetSpace(new Vector2(startX, startY)));
					break;
				case 2:
					startX += 1;
					path.Add(board.GetSpace(new Vector2(startX, startY)));
					break;
				case 3:
					startY += 1;
					path.Add(board.GetSpace(new Vector2(startX, startY)));
					break;
			}
		}
		path.Reverse();
		return path;
	}
	
	public void EndTurn(){
		/**Ends the unit's turn and resets its movement and actions.*/
		this.alreadyActed = true;
		this.remainingActions = maxActions;
		this.remainingMove = moveRange;
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
		GameObject moveTilePrefab = board.moveTilePrefab;
		GameObject visualization = new GameObject("Accessible Spaces");
		visualization.tag = "Visualization";
		HashSet<BoardSpace> accessibleSpaces = GetAccessibleSpaces((int) this.boardPosition.x, (int) this.boardPosition.y);
		foreach(BoardSpace space in accessibleSpaces){
			GameObject moveTile = Instantiate(moveTilePrefab, visualization.transform);
			moveTile.transform.position = space.anchorPosition + new Vector3(0f, 0.02f, 0f);
		}
	}
	
	public void UseSkill(){
		
	}
	
	public bool CanMove(){
		/**Returns true if the user can still move, even if there is no space to move to due to obstacles; false otherwise.*/
		return (remainingMove > 0 && previousAction != UnitAction.MOVE && !alreadyActed); //cannot move if the last action taken was a move.
	}
	
	public bool CanAct(){
		/**Returns true if the unit has actions left to use, false otherwise.*/
		Debug.Log((remainingActions > 0 && !alreadyActed));
		return (remainingActions > 0 && !alreadyActed);
	}
	
	public bool CanUse(Skill skill){
		/**Returns true if the unit can use the specified skill, false otherwise.*/
		Debug.Log(CanAct());
		if (!CanAct()){
			return false;
		}
		Debug.Log(remainingActions < skill.actionCost);
		Debug.Log(bullets < skill.bulletCost);
		Debug.Log(remainingMove < skill.moveCost);
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
				Debug.Log(skill);
			}
		}
		return usable;
	}
	
	public HashSet<UnitAction> GetUsableActions(){
		HashSet<UnitAction> actions = new HashSet<UnitAction>();
		actions.Add(UnitAction.WAIT);
		if (CanMove()){
			actions.Add(UnitAction.MOVE);
		}
		if (CanAct()){
			if (GetUsableSkills().Count > 0){
				actions.Add(UnitAction.SKILL);
			}
		}
		return actions;
	}
	
	public void EndTurnIfNeeded(){
		/**If the player cannot move or act, ends the player's turn.*/
		if (!CanMove() && !CanAct()){
			EndTurn();
		}
	}
	
	public void MakeSemiTransparent(bool enabled){
		Color color = this.gameObject.GetComponent<MeshRenderer>().material.color;
		if (enabled){
			this.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Metallic", 0f); //make nonmetallic and semitransparent
			color.a = 0.5f;
		} else {
			this.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Metallic", 0.4f); //make metallic and opaque
			color.a = 1.0f;
		}
		this.gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
	}
}
