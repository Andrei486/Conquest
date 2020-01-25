using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Objects;

public class BoardManager : MonoBehaviour
{
	public int rows;
	public int columns;
	public Sprite defaultSprite;
	GameObject board;
	public BoardSpace[,] boardSpaces;
	public GameObject cursor;
	public GameObject pawn;
	public Material spriteShader;
	public GameObject skillTilePrefab;
    // Start is called before the first frame update
    void Start()
    {
		BoardSpace newSpace;
		board = this.gameObject;
		boardSpaces = new BoardSpace[columns, rows];
		for (int i = 0; i < columns; i++){
			for (int j = 0; j < columns; j++){
				newSpace = new BoardSpace();
				newSpace.boardPosition = new Vector2(i, j);
				newSpace.anchorPosition = new Vector3(i * 2, i * 0.5f, j * 2);
				newSpace.sprite = defaultSprite;
				boardSpaces[i,j] = newSpace;
				showTile(boardSpaces[i,j]);
			}
		}
		CreateUnits();
		Instantiate(cursor);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void showTile(BoardSpace space){
		GameObject tile = new GameObject("Tile");
		tile.transform.parent = board.transform; //set as child to board, so that hierarchy is simplified
		tile.transform.position = space.anchorPosition;
		Quaternion rotation = new Quaternion();
		rotation.eulerAngles = new Vector3(90, 0, 0);
		tile.transform.rotation = rotation;
		SpriteRenderer renderer = tile.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
		renderer.sprite = space.sprite;
		renderer.material = spriteShader;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		renderer.receiveShadows = true;
	}
	
	public bool IsWithinBounds(Vector2 position){
		if (position.x < 0 || position.x > columns - 1 || position.y < 0 || position.y > rows - 1){
			return false;
		} else {
			return true;
		}
		return false;
	}
	
	public void MoveUnit(BoardSpace start, BoardSpace end){
		if (start.occupyingUnit == null){
			Debug.Log("no unit to move");
			return;
		}
		if (end.occupyingUnit != null){
			Debug.Log("cannot move to a used space");
			return;
		}
		end.occupyingUnit = start.occupyingUnit;
		start.occupyingUnit = null;
		end.occupyingUnit.transform.position = end.anchorPosition; //lerp here
	}
	
	public void MoveUnit(Vector2 start, Vector2 end){
		MoveUnit(this.boardSpaces[(int) start.x, (int) start.y], this.boardSpaces[(int) start.x, (int) start.y]);
	}
	
	public void CreateUnits(){
		GameObject unit = Instantiate(pawn);
		this.boardSpaces[4,4].occupyingUnit = unit;
		unit.transform.position = this.boardSpaces[4,4].anchorPosition;
	}
}
