using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objects;

public class Cursor : MonoBehaviour
{
	public Vector2 position;
	public BoardSpace selectedSpace = null;
	public GameObject cursorSelectPrefab;
	public float moveTime = 0.05f; //it takes the cursor 0.1s to move one space
	public bool moving = false;
	public bool locked = false;
	public GameObject camera;
	public BoardManager board;
	GameObject selector;
	
    // Start is called before the first frame update
    void Start()
    {
		board = GameObject.FindGameObjectsWithTag("Board")[0].GetComponent<BoardManager>(); //find board object and script
		this.gameObject.transform.parent = GameObject.FindGameObjectsWithTag("Board")[0].transform; //set board as parent for convenience
		position = new Vector2(0, 0);
		this.gameObject.transform.position = board.boardSpaces[0, 0].anchorPosition + new Vector3 (0f, 4f, 0f);
		if (board.boardSpaces[0, 0].occupyingUnit != null){
			this.gameObject.transform.position += new Vector3 (0f, 1f, 0f);
		}
		selector = Instantiate(cursorSelectPrefab);
		selector.transform.position = board.boardSpaces[0, 0].anchorPosition + new Vector3 (0f, 0.01f, 0f);
		camera = GameObject.FindGameObjectsWithTag("MainCamera")[0]; //find camera object
    }

    // Update is called once per frame
    void Update()
    {
		if (camera.GetComponent<Camera>().rotating || this.moving || this.locked){ //to prevent camera going off-center, do not move if already moving or turning
			return;
		}
		Quaternion rotation = Quaternion.AngleAxis((int) camera.transform.eulerAngles.y, Vector3.back);
        if (Input.GetKeyDown("down")){
			Move(rotation * Vector3.down);
		}
		if (Input.GetKeyDown("up")){
			Move(rotation * Vector3.up);
		}
		if (Input.GetKeyDown("left")){
			Move(rotation * Vector3.left);
		}
		if (Input.GetKeyDown("right")){
			Move(rotation * Vector3.right);
		}
		
		if (Input.GetKeyDown("space")){
			selector.GetComponent<CursorSelector>().Select(board.boardSpaces[(int) position.x, (int) position.y]);
		}
		if (Input.GetKeyDown("backspace")){
			selector.GetComponent<CursorSelector>().Deselect();
		}
    }
	
	public void Move(Vector2 movement){	
		Vector2 newPosition = position + movement;
		if (board.IsWithinBounds(newPosition)){
			UpdatePosition(newPosition);
		}
	}
	
	public void Move(Vector3 movement){
		Move(new Vector2((int) Math.Round(movement.x, 0), (int) Math.Round(movement.y, 0)));
	}
	
	void UpdatePosition(Vector2 newPosition){
		
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
		moving = true;
		float startTime = Time.time;
		float fractionTime = 0;
		
		//make the object "move"
		while (fractionTime < 1.0f){
			fractionTime = (Time.time - startTime)/moveTime;
			this.gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, fractionTime);
			yield return new WaitForEndOfFrame();
		}
		
		this.gameObject.transform.position = endPosition;
		selector.transform.position = selectorEnd;
		moving = false;
	}
}
