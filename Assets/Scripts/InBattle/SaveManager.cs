using System;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Objects;
using JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InBattle{    
    public class SaveManager : MonoBehaviour
    {
        public bool isBattleSaved = false;
        public GameObject playerPrefab;
        public GameObject boardPrefab;
        //file paths
        public string battleSave;
        public string backupFile;
        public string controlsData;
        public TextAsset baseUnits;
        ControlsManager controls;

        public Mission currentMission;
        void Start()
        {
            controls = ControlsManager.GetControls();
            battleSave = Application.persistentDataPath + "/SaveData/saveFile.txt";
            backupFile = Application.persistentDataPath + "/SaveData/backupFile.txt";
            controlsData = Application.persistentDataPath + "/SaveData/controlsData.txt";
            Debug.Log(Application.persistentDataPath);
            //LoadControls();

            InfoObject info = InfoObject.Find();
            if (info != null){
                currentMission = info.missionToLoad;
                string toLoad = currentMission.filename;
                StartCoroutine(StartNewBattle(toLoad));
            } else {
                StartCoroutine(StartNewBattle("mission"));
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(controls.GetCommand(Command.QUICKSAVE))){
                SaveBoard(BoardManager.GetBoard());
            }
            if (Input.GetKeyDown(controls.GetCommand(Command.QUICKLOAD))){
                ResumeSavedBattle();
            }
        }

        public GameObject LoadMission(GameObject boardObject, JObject missionInfo){
            BoardManager board = boardObject.GetComponent<BoardManager>();
            List<GameObject> units = new List<GameObject>();
                foreach (JToken item in missionInfo["units"].Children()){
                    Debug.Log(item);
                    units.Add(JsonConvert.DeserializeObject<GameObject>(item.ToString(), new PlayerConverter()));
                }
                board.SetUnits(units);
                board.currentTurn = missionInfo["currentTurn"].Value<int>();
                board.phase = (UnitAffiliation) Enum.Parse(typeof(UnitAffiliation), missionInfo["currentPhase"].Value<string>());
                board.mapName = missionInfo["map"].Value<string>();
                if (missionInfo.ContainsKey("armyManager")){
                    board.armyManager = JsonConvert.DeserializeObject<ArmyManager>(missionInfo["armyManager"].ToString());
                } else {
                    Debug.Log("using default army manager, none specified");
                    board.armyManager = new ArmyManager();
                    board.armyManager.SetDefault();
                }
                
            return boardObject;
        }

        public IEnumerator StartNewBattle(string missionData, bool fromSave = false){
            /**Loads the battlefield corresponding to the JSON file in missionData.
            If no argument is given, loads the battle save instead, assuming it exists.*/
            RemovePreviousBoard();
            string mapData;
            JObject missionInfo;
            yield return new WaitForEndOfFrame();
            if (!fromSave){
                TextAsset fullMission = Resources.Load<TextAsset>("Missions/" + missionData);
                missionInfo = JObject.Parse(fullMission.text);
            } else {
                missionInfo = JObject.Parse(File.ReadAllText(missionData));
            }
            
            mapData = missionInfo["map"].Value<string>();
            TextAsset boardInfo = Resources.Load<TextAsset>("Maps/" + mapData);
            GameObject boardObject = JsonConvert.DeserializeObject<GameObject>(boardInfo.text, new BoardConverter());
            boardObject = LoadMission(boardObject, missionInfo);
        }

        public void ResumeSavedBattle(){
            StartCoroutine(StartNewBattle(battleSave, true));
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
            SaveControls();

            string json = JsonConvert.SerializeObject(board.gameObject, Formatting.Indented, new BoardConverter());

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
            File.WriteAllText(battleSave, baseUnits.text);
        }

        public void SaveControls(){
            ControlsManager controls = ControlsManager.GetControls();
            string json = JsonConvert.SerializeObject(controls.keyMappings);
            File.WriteAllText(controlsData, json);
        }

        public static SaveManager GetSaveManager(){
            return GameObject.FindWithTag("Global").GetComponent<SaveManager>();
        }

        public void ExitToMissionSelect(bool successful){
            Mission updated = currentMission;
            //if mission was not completed and is now complete, update it.
            updated.completed = (successful || updated.completed);
            InfoObject infoObject = InfoObject.Create();
            infoObject.gameObject.name = "To Update";
            infoObject.missionToUpdate = updated;
            SceneManager.LoadScene("Scenes/battleselect", LoadSceneMode.Single);
        }
    }
}