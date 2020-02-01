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
	public List<Sprite> defaultSprites;
	public List<Color> tileColors;
	GameObject board;
	public BoardSpace[,] boardSpaces;
	public GameObject cursor;
	public GameObject pawn;
	public Material spriteShader;
	public GameObject boardTilePrefab;
	public GameObject moveTilePrefab;
	public GameObject skillTilePrefab;
	public TextAsset mapData;
    // Start is called before the first frame update
    void Start()
    {
		board = this.gameObject;
		CreateMap();
		CreateUnits();
		Instantiate(cursor);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void showTile(BoardSpace space){
		GameObject tile = Instantiate(boardTilePrefab, board.transform);
		tile.transform.position = space.anchorPosition;
		SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
		tile.transform.Find("Pillar").gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", space.pillarColor);
		renderer.sprite = space.sprite;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		renderer.receiveShadows = true;
		/*
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
		*/
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
		end.occupyingUnit.GetComponent<PlayerController>().boardPosition = end.boardPosition;
		
	}
	
	public void MoveUnit(Vector2 start, Vector2 end){
		MoveUnit(this.boardSpaces[(int) start.x, (int) start.y], this.boardSpaces[(int) start.x, (int) start.y]);
	}
	
	public void CreateUnits(){
		GameObject unit = Instantiate(pawn);
		this.boardSpaces[4,4].occupyingUnit = unit;
		unit.transform.position = this.boardSpaces[4,4].anchorPosition;
		unit.GetComponent<PlayerController>().boardPosition = new Vector2(4, 4);
	}
	
	public void CreateMap(){
		List<BoardSpace> spacesList = JsonConvert.DeserializeObject<List<BoardSpace>>(mapData.text, new BoardConverter());
		boardSpaces = new BoardSpace[columns, rows];
		foreach (BoardSpace space in spacesList){
			int i = (int) space.boardPosition.x;
			int j = (int) space.boardPosition.y;
			boardSpaces[i, j] = space;
			showTile(boardSpaces[i, j]);
		}
	}
	
	internal class BoardConverter : JsonConverter{
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
}
