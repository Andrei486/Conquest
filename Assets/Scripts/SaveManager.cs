using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
public class SaveManager : MonoBehaviour
{
    public bool isBattleSaved = false;
    //file paths
    public const string battleSave = "/Assets/Miscellaneous/saveFile.txt";
    public const string backupFile = "/Assets/Miscellaneous/backupFile.txt";
    public const string baseUnits = "/Assets/Miscellaneous/baseUnits.txt";
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void LoadBattle(string mapData = battleSave){
        /**Loads the battlefield and units corresponding to the JSON file in mapData.
        If no argument is given, loads the battle save instead, assuming it exists.*/

        //get relevant JSON objects

        //create the map by calling CreateMap()
        //map must be in the Resources folder

    }

    public void BattleSave(){
        File.Copy(battleSave, backupFile); //back up the save
        File.WriteAllText(battleSave, string.Empty); //empty the save

        //write map name (and other info: event flags?) in a map object

        //write units and positions (this ignores and overwrites the information in the map data)

        isBattleSaved = true;
    }

    public void CreateDefaultFile(){
        /**Creates a basic save file.*/
        File.Copy(baseUnits, battleSave);
    }
}
