using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Objects;
using JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
public class SaveManager : MonoBehaviour
{
    public bool isBattleSaved = false;
    public GameObject playerPrefab;
    public GameObject boardPrefab;
    //file paths
    public static string battleSave;
    public static string backupFile;
    public static string baseUnits;
    ControlsManager controls;
    void Start()
    {
        controls = ControlsManager.GetControls();
        battleSave = Application.persistentDataPath + "/SaveData/saveFile.txt";
        backupFile = Application.persistentDataPath + "/SaveData/backupFile.txt";
        baseUnits = Application.persistentDataPath + "/SaveData/baseUnits.txt";
        Debug.Log(Application.persistentDataPath);
        StartCoroutine(StartNewBattle("mapData"));
    }

    void Update()
    {
        if (Input.GetKeyDown(controls.GetCommand(Command.QUICKSAVE))){
            SaveBoard(BoardManager.GetBoard());
        }
        if (Input.GetKeyDown(controls.GetCommand(Command.QUICKLOAD))){
            StartCoroutine(LoadBattle(battleSave));
        }
    }

    public IEnumerator LoadBattle(string mapData){
        /**Loads the battlefield corresponding to the JSON file in mapData.
        If no argument is given, loads the battle save instead, assuming it exists.*/
        RemovePreviousBoard();
        yield return new WaitForEndOfFrame();
        string fullInfo = File.ReadAllText(mapData);
        GameObject boardObject = JsonConvert.DeserializeObject<GameObject>(fullInfo, new BoardConverter());
        Debug.Log("loaded");
    }

    public IEnumerator StartNewBattle(string mapData){
        /**Loads the battlefield corresponding to the JSON file in mapData.
        If no argument is given, loads the battle save instead, assuming it exists.*/
        RemovePreviousBoard();
        yield return new WaitForEndOfFrame();
        TextAsset fullInfo = Resources.Load<TextAsset>("Maps/" + mapData);
        GameObject boardObject = JsonConvert.DeserializeObject<GameObject>(fullInfo.text, new BoardConverter());
    }

    public void RemovePreviousBoard(){
        /**Destroys the previous board GameObject.
        If there is none, does nothing.*/
        GameObject board = GameObject.FindWithTag("Board");
        if (board != null){
            Destroy(board);
        }
    }

    public List<GameObject> LoadUnits(JArray unitsInfo){
        /**Loads and returns the units corresponding to the JSON object passed as argument.
        If no argument is given, loads the battle save instead, assuming it exists.*/

        List<GameObject> units = new List<GameObject>();
        foreach (JToken unit in unitsInfo){
            units.Add(LoadUnit((JObject) unit));
        }
        return units;
    }

    public GameObject LoadUnit(JObject playerInfo){
        /**Loads and returns one unit, instantiating the prefab and creating its components
        according to the JSON data in playerInfo.*/

        GameObject player = Instantiate(playerPrefab);
        JsonUtility.FromJsonOverwrite(playerInfo["playerController"].ToString(), player.GetComponent<PlayerController>());
        JsonUtility.FromJsonOverwrite(playerInfo["health"].ToString(), player.GetComponent<Health>());
        return player;
    }

    public void SaveBoard(BoardManager board){
        /**Saves the specified board as a battle save into the battle save file.*/
        
        //back up the save
        BackupSave();
        

        string json = JsonConvert.SerializeObject(board.gameObject, new BoardConverter());

        //write the info
        File.WriteAllText(battleSave, json);

        isBattleSaved = true;
        Debug.Log("saved!");
    }

    public void BackupSave(){
        /**Backs up the current battle save.*/
        if (!File.Exists(battleSave)){
            File.Create(battleSave); //create a battle save if it doesn't exist
        }
        if (!File.Exists(backupFile)){
            File.Create(backupFile); //create a backup file if it doesn't exist
        }
        File.WriteAllText(backupFile, File.ReadAllText(battleSave));
    }

    public JArray GetUnitsInfo(BoardManager board){
        /**Returns the JArray of player info objects for the units on the given board.*/

        //get all players' info
        List<PlayerInfo> players = new List<PlayerInfo>();
        for (int x = 0; x < board.columns; x++){
			for (int y = 0; y < board.rows; y++){
				BoardSpace space = board.GetSpace(new Vector2(x, y));
				if (space.occupyingUnit != null){
					players.Add(space.occupyingUnit.GetComponent<PlayerController>().GetPlayerInfo());
				}
			}
		}

        //make the JArray
        JArray units = new JArray(
            from player in players
            select JObject.Parse(JsonUtility.ToJson(player))
        );
        return units;
    }

    public void CreateDefaultFile(){
        /**Creates a basic save file.*/
        File.Copy(baseUnits, battleSave);
    }

    public static SaveManager GetSaveManager(){
        return GameObject.FindWithTag("Global").GetComponent<SaveManager>();
    }
}
