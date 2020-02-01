using System.Collections;
using System;
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
	public int maxBullets;
	public int bullets;
	public List<Skill> skillList;
	BoardManager board;
	
    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.FindGameObjectsWithTag("Board")[0].GetComponent<BoardManager>();
		bullets = maxBullets;
		MakeSemiTransparent(false);
    }

    // Update is called once per frame
    void Update()
    {
		
    }
	
	public HashSet<BoardSpace> GetAccessibleSpaces(int movementLeft, int startX, int startY){
		
		BoardSpace currentSpace = board.boardSpaces[startX, startY];
		BoardSpace toCheck;
		HashSet<BoardSpace> accessibleSpaces = new HashSet<BoardSpace>();
		if (movementLeft <= 0){
			return new HashSet<BoardSpace>(); //can't move any further
		}
		
		if (startX > 0){ //check left space if possible
			toCheck = board.boardSpaces[startX - 1, startY];
			if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){
				if (movementLeft >= toCheck.moveCost){
					accessibleSpaces.Add(toCheck);
					accessibleSpaces.UnionWith(GetAccessibleSpaces(movementLeft - toCheck.moveCost, startX - 1, startY));
				}
			}
		}
		if (startY > 0){ //check lower space if possible
			toCheck = board.boardSpaces[startX, startY - 1];
			if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){
				if (movementLeft >= toCheck.moveCost){
					accessibleSpaces.Add(toCheck);
					accessibleSpaces.UnionWith(GetAccessibleSpaces(movementLeft - toCheck.moveCost, startX, startY - 1));
				}
			}
		}
		if (startX < board.columns - 1){ //check right space if possible
			toCheck = board.boardSpaces[startX + 1, startY];
			if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){
				if (movementLeft >= toCheck.moveCost){
					accessibleSpaces.Add(toCheck);
					accessibleSpaces.UnionWith(GetAccessibleSpaces(movementLeft - toCheck.moveCost, startX + 1, startY));
				}
			}
		}
		if (startY < board.rows - 1){ //check top space if possible
			toCheck = board.boardSpaces[startX, startY + 1];
			if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){
				if (movementLeft >= toCheck.moveCost){
					accessibleSpaces.Add(toCheck);
					accessibleSpaces.UnionWith(GetAccessibleSpaces(movementLeft - toCheck.moveCost, startX, startY + 1));
				}
			}
		}
		return accessibleSpaces;
	}
	
	public void ShowAccessibleSpaces(){
		GameObject moveTilePrefab = board.moveTilePrefab;
		GameObject visualization = new GameObject("Accessible Spaces");
		visualization.tag = "Visualization";
		HashSet<BoardSpace> accessibleSpaces = GetAccessibleSpaces(moveRange, (int) this.boardPosition.x, (int) this.boardPosition.y);
		foreach(BoardSpace space in accessibleSpaces){
			GameObject moveTile = Instantiate(moveTilePrefab, visualization.transform);
			moveTile.transform.position = space.anchorPosition + new Vector3(0f, 0.02f, 0f);
		}
	}
	
	public void MakeSemiTransparent(bool enabled){
		Color color = this.gameObject.GetComponent<MeshRenderer>().material.color;
		if (enabled){
			color.a = 0.5f;
		} else {
			color.a = 1.0f;
		}
		this.gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
	}
}
