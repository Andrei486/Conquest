using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Objects;

namespace MissionSelect{
    public class MissionManager : MonoBehaviour
    {
        string missionsFile; //the file in which mission completion data is stored
        List<Mission> allMissions;
        List<Mission> selectableMissions;

        GameObject menu;

        void Start()
        {
            missionsFile = Application.persistentDataPath + "/SaveData/missionsFile.txt";
            LoadMissions(missionsFile);
            UpdateMissions(); //if any missions are updated from InfoObject, do it before creating menu
            SaveMissions(missionsFile);
            menu = MissionSelectMenu(selectableMissions);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void LoadMissions(string filename){
            /**Sets selectableMissions to the list of currently playable missions, from the list in the specified file.!--*/
            allMissions = JsonConvert.DeserializeObject<List<Mission>>(File.ReadAllText(filename));
            selectableMissions = (from mission in allMissions where IsSelectable(mission, allMissions) select mission).ToList();
            Debug.Log(selectableMissions.Count);
        }

        void SaveMissions(string filename){
            string json = JsonConvert.SerializeObject(allMissions, Formatting.Indented);
            File.WriteAllText(filename, json);
        }

        void UpdateMissions(){
            InfoObject info = InfoObject.Find();
            if (info != null){
                //do stuff
                Mission updatedMission = info.missionToUpdate;
                int index;
                foreach (Mission mission in allMissions){
                    if (mission.name == updatedMission.name){
                        //replace the old mission with the same name
                        index = allMissions.IndexOf(mission);
                        allMissions.Remove(mission);
                        allMissions.Insert(index, updatedMission);
                        break;
                    }
                }
                Destroy(info);
            }
            selectableMissions = (from mission in allMissions where IsSelectable(mission, allMissions) select mission).ToList();
            Debug.Log(selectableMissions.Count);
        }

        public void StartMission(Mission mission){
            InfoObject toLoad = InfoObject.Create();
            toLoad.gameObject.name = "To Load";
            toLoad.missionToLoad = mission;
            SceneManager.LoadScene("Scenes/battlefield", LoadSceneMode.Single);
        }

        GameObject MissionSelectMenu(List<Mission> missions){
            /**Creates a menu for mission selection.!--*/
            GameObject menu = Menu.Create(PrefabManager.GetPrefabs().menuItemPrefab, typeof(MissionCursor), 5);
            menu.GetComponent<Menu>().Generate(from mission in missions select mission.name);
            return menu;
        }

        bool IsSelectable(Mission toSelect, List<Mission> missions){
            
            //if the mission has been played and cannot be replayed, can't select it.
            if (toSelect.completed && !toSelect.replayable){
                return false;
            }
            
            //if some prerequisites are missing, cannot select it.
            List<Mission> prereqs = (from mission in missions where toSelect.required.Contains(mission.name) select mission).ToList();
            foreach (Mission prereq in prereqs){
                if (!prereq.completed){
                    return false;
                }
            }

            //otherwise, the mission can be played.
            return true;
        }

        public Mission? FindByName(string missionName){
            /**Returns the mission with name missionName (assumes names are unique);
            if there is no mission with that name returns null.*/
            foreach (Mission mission in selectableMissions){
                if (mission.name == missionName){
                    return mission;
                }
            }
            return null;
        }

        public static MissionManager GetManager(){
            return GameObject.FindWithTag("Global").GetComponent<MissionManager>();
        }

        // void CreateMissions(string filename){
        //     Mission mission1 = new Mission();
        //     mission1.completed = false;
        //     mission1.replayable = false;
        //     mission1.filename = "mission";
        //     mission1.name = "Mission 1";
        //     mission1.required = new List<string>();

        //     Mission mission2 = new Mission();
        //     mission2.completed = true;
        //     mission2.replayable = false;
        //     mission2.filename = "mission";
        //     mission2.name = "Mission 2";
        //     mission2.required = new List<string>();
            

        //     Mission mission3 = new Mission();
        //     mission3.completed = true;
        //     mission3.replayable = true;
        //     mission3.filename = "mission";
        //     mission3.name = "Mission 3";
        //     mission3.required = new List<string>();
        //     mission3.required.Add("Mission 3");

        //     List<Mission> missions = new List<Mission>();
        //     missions.Add(mission1);
        //     missions.Add(mission2);
        //     missions.Add(mission3);
            
        //     string json = JsonConvert.SerializeObject(missions);

        //     //write the info
        //     File.WriteAllText(filename, json);
        // }
    }
}
