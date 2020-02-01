using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Objects{
	
	/**Represents a single space on the game board.*/
	public class BoardSpace {
		
		public Vector2 boardPosition;
		public Vector3 anchorPosition;
		public GameObject occupyingUnit;
		public Sprite sprite;
		public Color pillarColor;
		public int moveCost = 1;
		public const float BOARD_SIZE = 2.0f;
		public bool impassable = false;
		
		public float GetHeight(){
			return this.anchorPosition.y;
		}
	}
	
	public class Attack{
		
		public float basePower;
		public float accuracy;
		//relative to user, in board space, when user faces upwards.
		public Vector2 targetPosition;
		public Vector2 knockbackPosition;
	}
	
	public enum SkillType{
		Melee,
		Ranged,
		Support
	}
	public class Skill{
		
		public string name;
		public SkillType skillType;
		public bool reusable = false;
		public List<Attack> attacks;
		public int actionCost;
		public int bulletCost;
		public int moveCost;
		//relative to user, in board space, when user faces upwards.
		public Vector2 movePosition; 
		
		public void VisualizeTarget(BoardSpace space){
			PlayerController pc = space.occupyingUnit.GetComponent<PlayerController>();
			Health h = space.occupyingUnit.GetComponent<Health>();
			
			GameObject skillVisual = new GameObject(); //empty object to child tiles to, and destroy later
			BoardManager board = GameObject.FindGameObjectsWithTag("Board")[0].GetComponent<BoardManager>();
			GameObject skillTile = board.skillTilePrefab;
			GameObject tile;
			
			foreach (Attack attack in attacks){
				tile = Object.Instantiate(skillTile, skillVisual.transform);
				tile.transform.position = board.boardSpaces[(int) (space.boardPosition.x + attack.targetPosition.x), (int) (space.boardPosition.y + attack.targetPosition.y)].anchorPosition;
				if (attack.knockbackPosition != null){
					tile = Object.Instantiate(skillTile, skillVisual.transform);
					tile.transform.position = board.boardSpaces[(int) (space.boardPosition.x + attack.knockbackPosition.x), (int) (space.boardPosition.y + attack.knockbackPosition.y)].anchorPosition;
					tile.GetComponent<MeshRenderer>().materials[2] = tile.GetComponent<MeshRenderer>().materials[1];
				}
			}
			
			if(this.movePosition != null){
				tile = Object.Instantiate(skillTile, skillVisual.transform);
				tile.transform.position = board.boardSpaces[(int) (space.boardPosition.x + movePosition.x), (int) (space.boardPosition.y + movePosition.y)].anchorPosition;
				tile.GetComponent<MeshRenderer>().materials[2] = tile.GetComponent<MeshRenderer>().materials[0];
			}
		}
	}
}
