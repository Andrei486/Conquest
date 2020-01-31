using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Objects;

public class PlayerController : MonoBehaviour
{
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
        board = GameObject.FindGameObjectsWithTag("BoardManager")[0].GetComponent<BoardManager>();
		MakeSemiTransparent(false);
    }

    // Update is called once per frame
    void Update()
    {
		
    }
	
	public HashSet<Vector2> GetAccessibleSpaces(int movementLeft, int startX, int startY){
		if (movementLeft == 0){
			return new HashSet<Vector2>(); //can't move any further
		}
		
		BoardSpace currentSpace = board.boardSpaces[startX, startY];
		BoardSpace toCheck;
		List<BoardSpace> accessibleSpaces = new List<BoardSpace>();
		
		if (startX > 0){ //check left space if possible
			toCheck = board.boardSpaces[startX - 1, startY];
			if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){
				accessibleSpaces.Add(space);
				accessibleSpaces.UnionWith(GetAccessibleSpaces(movementLeft - 1, startX - 1, startY));
			}
		}
		if (startY > 0){ //check lower space if possible
			toCheck = board.boardSpaces[startX, startY - 1];
			if (Math.Abs(toCheck.GetHeight() - currentSpace.GetHeight()) <= jumpHeight){
				accessibleSpaces.Add(space);
				accessibleSpaces.UnionWith(GetAccessibleSpaces(movementLeft - 1, startX, startY - 1));
			}
		}
		return null;
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
