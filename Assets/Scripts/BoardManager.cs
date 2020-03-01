using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class BoardManager : MonoBehaviour
{
	public int rows;
	public int columns;
	[Tooltip("A list of sprites to use for tile defaults. In order, tile and grass.")]
	public List<Sprite> defaultSprites;
	[Tooltip("A list of colors to use for board tile pillars. In order, tile and grass.")]
	public List<Color> tileColors;
	GameObject board;
	Menu menu;
	Cursor cursor;
	public BoardSpace[,] boardSpaces;
	public GameObject cursorPrefab;
	public GameObject pawn;
	public Material spriteShader;
	public GameObject boardTilePrefab;
	public GameObject moveTilePrefab;
	public GameObject skillTilePrefab;
	public TextAsset mapData;
	public TextAsset skillData;
    // Start is called before the first frame update
    void Start()
    {
		board = this.gameObject;
		menu = GameObject.FindGameObjectsWithTag("MenuController")[0].GetComponent<Menu>();
		CreateMap();
		CreateUnits();
		menu.cursor = Instantiate(cursorPrefab).GetComponent<Cursor>();
		cursor = menu.cursor;
    }

    // Update is called once per frame
    void Update()
    {
        
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
	
	void showTile(BoardSpace space){
		/**Shows a board tile corresponding to a BoardSpace.*/
		GameObject tile = Instantiate(boardTilePrefab, board.transform);
		tile.transform.position = space.anchorPosition;
		SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
		tile.transform.Find("Pillar").gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", space.pillarColor);
		renderer.sprite = space.sprite;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		renderer.receiveShadows = true;
	}
	
	public bool IsWithinBounds(Vector2 position){
		/**Returns true if the (x, y) coordinates specified by position are within bounds for the grid,
		ie there is a space at that position. Otherwise returns false.*/
		if (position.x < 0 || position.x > columns - 1 || position.y < 0 || position.y > rows - 1){
			return false;
		} else {
			return true;
		}
		return false;
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
		// if there was a temporary move player then remove it
		if (end.occupyingUnit.transform.Find("Temporary") != null){
			Destroy(end.occupyingUnit.transform.Find("Temporary").gameObject);
		}
		cursor.selectedSpace = end;
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
	
	public void RefreshUnits(){
		/**Called at the end of turn to allow all units a new turn.*/
		for (int i = 0; i < columns; i++){
			for (int j = 0; j < rows; j++){
				BoardSpace space = GetSpace(new Vector2(i, j));
				if (space.occupyingUnit != null){
					PlayerController pc = space.occupyingUnit.GetComponent<PlayerController>();
					pc.alreadyActed = false;
				}
			}
		}
	}
	
	void CreateUnits(){
		/**Creates units from the JSON file of map data. Must be called after CreateMap.*/
		JToken array = JObject.Parse(mapData.text)["players"];
		List<GameObject> players = JsonConvert.DeserializeObject<List<GameObject>>(array.ToString(), new PlayerConverter());
		foreach (GameObject player in players){
			Vector2 position = player.GetComponent<PlayerController>().boardPosition;
			boardSpaces[(int) position.x, (int) position.y].occupyingUnit = player;
			player.transform.position = boardSpaces[(int) position.x, (int) position.y].anchorPosition;
		}
	}
	
	void CreateMap(){
		/**Creates a board of BoardSpaces from the JSON file of map data.*/
		List<BoardSpace> spacesList = JsonConvert.DeserializeObject<List<BoardSpace>>(mapData.text, new BoardConverter());
		boardSpaces = new BoardSpace[columns, rows];
		foreach (BoardSpace space in spacesList){
			int i = (int) space.boardPosition.x;
			int j = (int) space.boardPosition.y;
			boardSpaces[i, j] = space;
			showTile(boardSpaces[i, j]);
		}
	}
	
	public static void ClearVisualization(){
		/**Clears all visualizations for skills, movement, etc.*/
		foreach (GameObject vis in GameObject.FindGameObjectsWithTag("Visualization")){
			Destroy(vis);
		}
	}
	
	internal class BoardConverter : JsonConverter{
		/**A class which converts JSON representations of BoardSpaces to BoardSpace objects.*/
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer){
			throw new System.NotImplementedException(); //can't use to serialize json
		}
		
		public override bool CanConvert(Type type){
			if (type == typeof(BoardSpace) || type == typeof(List<BoardSpace>)){
				return true;
			} else {
				return false;
			}
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer){
			BoardManager board = GameObject.FindGameObjectsWithTag("Board")[0].GetComponent<BoardManager>();
			if (objectType == typeof(BoardSpace)){ //if it is a single space
				JObject item = JObject.Load(reader);
				BoardSpace space = new BoardSpace();
				space.boardPosition = new Vector2(item["boardPosition"][0].Value<int>(), item["boardPosition"][1].Value<int>());
				space.anchorPosition = new Vector3((float) space.boardPosition.x * BoardSpace.BOARD_SIZE,
													item["anchorHeight"].Value<float>(),
													(float) space.boardPosition.y * BoardSpace.BOARD_SIZE);
													
				List<Sprite> defaultSprites = board.defaultSprites;
				List<Color> tileColors = board.tileColors;
				switch (item["spriteName"].Value<string>()){
					case "tile":
						space.sprite = defaultSprites[0];
						space.pillarColor = tileColors[0];
						break;
					case "grass":
						space.sprite = defaultSprites[1];
						space.pillarColor = tileColors[1];
						break;
					default:
						space.sprite = defaultSprites[1]; //make an actual default sprite?
						space.pillarColor = tileColors[1];
						break;
				}
				return space;
			} else { //if it is the entire map
				JObject boardInfo = JObject.Load(reader);
				//set board size
				JToken rows = boardInfo["rows"];
				JToken columns = boardInfo["columns"];
				board.rows = rows.Value<int>();
				board.columns = columns.Value<int>();
				//get list of spaces, arrange them later
				JArray array = (JArray) boardInfo["spaces"];
				List<BoardSpace> spaces = new List<BoardSpace>();
				foreach (JToken token in array){
					JsonTextReader newReader = new JsonTextReader(new StringReader(token.ToString()));
					spaces.Add(this.ReadJson(newReader, typeof(BoardSpace), existingValue, serializer) as BoardSpace); 
				}
				return spaces;
			}
		}
	}
	internal class PlayerConverter : JsonConverter{
		/**A class which converts JSON representations of players to player GameObjects.*/
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer){
			throw new System.NotImplementedException(); //can't use to serialize json
		}
		
		public override bool CanConvert(Type type){
			if (type == typeof(GameObject) || type == typeof(List<GameObject>)){
				return true;
			} else {
				return false;
			}
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer){
			BoardManager board = GameObject.FindGameObjectsWithTag("Board")[0].GetComponent<BoardManager>();
			if (objectType == typeof(GameObject)){ //if it is a single player
				JObject playerInfo = JObject.Load(reader);
				GameObject player = Instantiate(board.pawn, board.transform);
				PlayerController pc = player.GetComponent<PlayerController>();
				pc.name = playerInfo["name"].Value<string>();
				pc.attackPower = playerInfo["attackPower"].Value<float>();
				pc.defense = playerInfo["defense"].Value<float>();
				pc.jumpHeight = playerInfo["jumpHeight"].Value<float>();
				pc.moveRange = playerInfo["moveRange"].Value<int>();
				pc.maxActions = playerInfo["moveRange"].Value<int>();
				pc.boardPosition = new Vector2(playerInfo["boardPosition"][0].Value<int>(), playerInfo["boardPosition"][1].Value<int>());
				pc.maxBullets = playerInfo["maxBullets"].Value<int>();
				foreach (JToken skillName in playerInfo["skillNames"]){
					pc.skillList.Add(Skill.GetSkillByName(skillName.ToString(), board.skillData));
				}
				return player;
			} else {
				JArray array = JArray.Load(reader);
				List<GameObject> players = new List<GameObject>();
				foreach (JToken token in array){
					JsonTextReader newReader = new JsonTextReader(new StringReader(token.ToString()));
					players.Add(this.ReadJson(newReader, typeof(GameObject), existingValue, serializer) as GameObject); 
				}
				return players;
			}
		}
	}
}
