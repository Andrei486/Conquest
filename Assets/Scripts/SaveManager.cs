using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
public class SaveManager : MonoBehaviour
{
    //file paths
    public const string saveFile = "/Assets/Miscellaneous/saveFile.txt";
    public const string backupFile = "/Assets/Miscellaneous/backupFile.txt";
    public const string baseUnits = "/Assets/Miscellaneous/baseUnits.txt";
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void LoadFile(){

    }

    public void SaveFile(){
        File.Copy(saveFile, backupFile);
        File.WriteAllText(saveFile, string.Empty);
    }

    public void CreateDefaultFile(){
        /**Creates a basic save file.*/
        File.Copy(baseUnits, saveFile);
    }
}
