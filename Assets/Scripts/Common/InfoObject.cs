using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objects;

public class InfoObject : MonoBehaviour
{
    public Mission missionToLoad;
    public Mission missionToUpdate;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static InfoObject Create(){
        GameObject info;
        //remove previous info object if any
        info = GameObject.FindWithTag("InfoObject");
        if (info != null){
            Destroy(info);
        }
        //create and return the new one
        info = new GameObject("Info Object");
        info.tag = "InfoObject";
        DontDestroyOnLoad(info);
        return info.AddComponent<InfoObject>();
    }
}
