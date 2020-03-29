using System.Collections.Generic;
using System;
using UnityEngine;
using Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverters;

public class BoardManager : MonoBehaviour
{
	public int rows;
	public int columns;
	[Tooltip("A list of sprites to use for tile defaults. In order, tile and grass.")]
	public List<Sprite> defaultSprites;
	[Tooltip("A list of colors to use for board tile pillars. In order, tile and grass.")]
	GameObject board;
	public string modelName;
	BattleMenu menu;
	public bool locked = false;
	Cursor cursor;
	public Cursor moveCursor;
	public UnitAffiliation phase;
	public BoardSpace[,] boardSpaces;
	public GameObject[,] boardTiles;
	public HighlightColor[] highlightColors;
	public GameObject cursorPrefab;
	public GameObject pawn;
	public Material spriteShader;
	public GameObject boardTilePrefab;
	public GameObject moveTilePrefab;
	public GameObject skillTilePrefab;
	public GameObject skillHitPrefab;
	public TextAsset mapData;
	public TextAsset skillData;
	private List<GameObject> players;
	private List<BoardSpace> spaces;
    // Start is called before the first frame update
    void Start()
    {
		board = this.gameObject;
		menu = BattleMenu.GetMenu();
		InitializeMap(spaces);
		InitializeUnits(players);
		menu.cursor = Instantiate(cursorPrefab, this.transform).GetComponent<Cursor>();
		cursor = menu.cursor;
		ControlsManager.GetControls().board = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetSpaces(List<BoardSpace> spaces){
		this.spaces = spaces;
	}
	public void SetPlayers(List<GameObject> players){
		this.players = players;
	}
	
	public BoardSpace GetSpace(Vector2 position){
		/**Returns the BoardSpace at the (x, y) coordinates specified by position.
		If the coordinates are out of bounds, returns null instead.*/
		if (IsWithinBounds(position)){
			return this.boardSpaces[(int) Math.Round(position.x), (int) Math.Round(position.y)];
		} else {
			return null;
		}
	}

	public GameObject GetTile(BoardSpace space){
		/**Returns the tile of the BoardSpace at the (x, y) coordinates specified by position.
		If the coordinates are out of bounds, returns null instead.*/
		Vector2 position = space.boardPosition;
		if (IsWithinBounds(position)){
			return this.boardTiles[(int) Math.Round(position.x), (int) Math.Round(position.y)];
		} else {
			return null;
		}
	}
	
	void AddTile(BoardSpace space){
		/**Shows a board tile corresponding to a BoardSpace.*/
		
		GameObject tile = Instantiate(boardTilePrefab, this.transform);
		//move the tile up a bit so it looks better
		tile.transform.Translate(Vector3.up * 0.1f * BoardSpace.BOARD_SIZE);
		LineRenderer lrenderer = tile.GetComponent<LineRenderer>();
		MeshRenderer mrenderer = tile.GetComponent<MeshRenderer>();
		lrenderer.positionCount = 4;
		lrenderer.SetPositions(space.corners);
		Mesh mesh = new Mesh();
		mesh.vertices = space.corners;
		mesh.triangles = new int[]{2, 1, 0, 0, 3, 2};
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		tile.GetComponent<MeshFilter>().mesh = mesh;
		lrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		mrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		mrenderer.receiveShadows = false;
		mrenderer.enabled = false;
		boardTiles[(int) space.boardPosition.x, (int) space.boardPosition.y] = tile;
	}

	void ToggleOutlines(bool enabled){
		foreach (GameObject tile in boardTiles){
			tile.GetComponent<LineRenderer>().enabled = enabled;
		}
	}

	public void HighlightSpaces(HashSet<BoardSpace> spaces){
		foreach (BoardSpace space in boardSpaces){
			GetTile(space).GetComponent<MeshRenderer>().enabled = false;
		}
		if (spaces != null){
			foreach (BoardSpace space in spaces){
				GetTile(space).GetComponent<MeshRenderer>().enabled = true;
			}
		}
	}
	
	public bool IsWithinBounds(Vector2 position){
		/**Returns true if the (x, y) coordinates specified by position are within bounds for the grid,
		ie there is a space at that position. Otherwise returns false.*/
		int x = (int) Math.Round(position.x);
		int y = (int) Math.Round(position.y);
		if (x < 0 || x > columns - 1 || y < 0 || y > rows - 1){
			return false;
		} else {
			return true;
		}
	}
	
	public void MoveUnit(BoardSpace start, BoardSpace end){
		/**Moves the unit on the BoardSpace start to the BoardSpace end, if any.*/
		if (start.occupyingUnit == null){
			Debug.Log("no unit to move");
			return;
		}
		if (end.occupyingUnit != null){
			Debug.Log("cannot move to a used space");
			return;
		}
		PlayerController pc = start.occupyingUnit.GetComponent<PlayerController>();
		if (!pc.CanMove()){
			return;
		}
		end.occupyingUnit = start.occupyingUnit;
		start.occupyingUnit = null;
		end.occupyingUnit.transform.position = end.anchorPosition; //lerp here
		pc.boardPosition = end.boardPosition;
		pc.remainingMove = pc.moveGrid[(int) end.boardPosition.x, (int) end.boardPosition.y];
		pc.previousAction = UnitAction.MOVE;
		pc.hasActed = true;
		// if there was a temporary move player then remove it
		if (end.occupyingUnit.transform.Find("Temporary") != null){
			Destroy(end.occupyingUnit.transform.Find("Temporary").gameObject);
		}
		cursor.selectedSpace = end;
		BattleLog.GetLog().Log(pc.name + " moved.");
	}
	
	public void MoveUnit(Vector2 start, Vector2 end){
		/**Moves the unit on the BoardSpace at coordinates start to the BoardSpace at coordinates end, if any.*/
		MoveUnit(this.GetSpace(start), this.GetSpace(end));
	}
	
	public void TempMoveUnit(BoardSpace start, BoardSpace end){
		/**Creates a temporary visualization of the unit at start and moves it to the end position.*/
		if (start.occupyingUnit == null){
			Debug.Log("no unit to move");
			return;
		}
		if (end.occupyingUnit != null){
			Debug.Log("cannot move to a used space");
			return;
		}
		GameObject tempPlayer = Instantiate(start.occupyingUnit, start.occupyingUnit.transform);
		tempPlayer.transform.localEulerAngles = new Vector3(0, 0, 0);
		tempPlayer.transform.localScale = new Vector3(1, 1, 1);
		tempPlayer.name = "Temporary";
		tempPlayer.GetComponent<PlayerController>().MakeSemiTransparent(true);
		tempPlayer.transform.position = end.anchorPosition;
	}
	
	public void TempMoveUnit(Vector2 start, Vector2 end){
		/**Creates a temporary visualization of the unit at start and moves it to the end position.*/
		TempMoveUnit(this.GetSpace(start), this.GetSpace(end));
	}
	
	public void KnockbackMoveUnit(BoardSpace start, BoardSpace end, bool throughUnits){
		/**Moves a unit from start to end, going through units if specified.
		Used to move a unit due to a skill from applied knockback.*/
		BoardManager board = BoardManager.GetBoard();
		PlayerController pc = start.occupyingUnit.GetComponent<PlayerController>();
		float startPos;
		float endPos;
		float currentHeight = start.GetHeight();
		BoardSpace nextSpace;
		BoardSpace lastAttainable = start;
		
		if (start.boardPosition.x == end.boardPosition.x){
			startPos = start.boardPosition.y;
			endPos = end.boardPosition.y;
			for (float y = startPos + 1; y < endPos; y++){
				nextSpace = board.GetSpace(new Vector2(start.boardPosition.x, y));
				if (nextSpace.impassable || Math.Abs(currentHeight - nextSpace.GetHeight()) > pc.jumpHeight){
					break; //there is something preventing movement through it, stop there
				} else {
					if (nextSpace.occupyingUnit != null && throughUnits == false){
						break; //if cannot move through units, stop
					}
					currentHeight = nextSpace.GetHeight();
					lastAttainable = nextSpace; //keep going
				}
			}
			
		} else if (start.boardPosition.y == end.boardPosition.y){ 
			startPos = start.boardPosition.x;
			endPos = end.boardPosition.x;
			for (float x = startPos + 1; x < endPos; x++){
				nextSpace = board.GetSpace(new Vector2(x, start.boardPosition.y));
				if (nextSpace.impassable || Math.Abs(currentHeight - nextSpace.GetHeight()) > pc.jumpHeight){
					break; //there is something preventing movement through it, stop there
				} else {
					if (nextSpace.occupyingUnit != null && throughUnits == false){
						break; //if cannot move through units, stop
					}
					currentHeight = nextSpace.GetHeight();
					lastAttainable = nextSpace; //keep going
				}
			}
		}
		
		//move to last attainable space along the path
		lastAttainable.occupyingUnit = start.occupyingUnit;
		start.occupyingUnit = null;
		lastAttainable.occupyingUnit.transform.position = lastAttainable.anchorPosition; //lerp here
		pc.boardPosition = lastAttainable.boardPosition;
	}
	
	public void SkillMoveUnit(Skill skill, BoardSpace start, Quaternion direction){
		/**Moves the unit from space start according to the movement of skill used in direction.
		Assumes that the movement is possible.*/
		if (skill.movePosition == Vector2.zero){
			return; //don't need to move the unit
		}
		BoardSpace end = GetSpace(start.boardPosition + (Vector2) (direction * (Vector3) skill.movePosition));
		PlayerController pc = start.occupyingUnit.GetComponent<PlayerController>();
		end.occupyingUnit = start.occupyingUnit;
		start.occupyingUnit = null;
		end.occupyingUnit.transform.position = end.anchorPosition; //lerp here
		pc.boardPosition = end.boardPosition;
		cursor.selectedSpace = end;
	}
	
	public void RefreshUnits(){
		/**Called at the end of turn to allow all units a new turn.*/
		for (int i = 0; i < columns; i++){
			for (int j = 0; j < rows; j++){
				BoardSpace space = GetSpace(new Vector2(i, j));
				if (space.occupyingUnit != null){
					PlayerController pc = space.occupyingUnit.GetComponent<PlayerController>();
					RefreshUnit(pc);
				}
			}
		}
	}

	public void RefreshUnit(PlayerController pc){
		pc.turnEnded = false;
		pc.previousAction = UnitAction.WAIT;
		pc.hasActed = false;
	}

	void CheckEndPhase(){

	}

	public static BoardManager GetBoard(){
		return GameObject.FindWithTag("Board").GetComponent<BoardManager>();
	}

	public void InitializeUnits(List<GameObject> units){
		/**Places the specified units at their respective positions.*/
		foreach (GameObject unit in units){
			PlayerController pc = unit.GetComponent<PlayerController>();
			Vector2 position = pc.boardPosition;
			GetSpace(position).occupyingUnit = unit;
			unit.transform.position = GetSpace(position).anchorPosition;
		}
	}

	public void InitializeMap(List<BoardSpace> spaces){
		GameObject modelObject = (GameObject) Resources.Load("MapModels/" + modelName);
		Instantiate(modelObject);
		boardSpaces = new BoardSpace[columns, rows];
		boardTiles = new GameObject[columns, rows];
		foreach (BoardSpace space in spaces){
			int i = (int) space.boardPosition.x;
			int j = (int) space.boardPosition.y;
			boardSpaces[i, j] = space;
			AddTile(boardSpaces[i, j]);
		}
		ToggleOutlines(true);
	}
	
	public static void ClearVisualization(){
		/**Clears all visualizations for skills, movement, etc.*/
		foreach (GameObject vis in GameObject.FindGameObjectsWithTag("Visualization")){
			Destroy(vis);
		}
		BoardManager.GetBoard().HighlightSpaces(null);
	}
}
