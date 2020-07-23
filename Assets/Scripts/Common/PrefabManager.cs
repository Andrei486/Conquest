using System.Collections;
using System.Collections.Generic;
using UnityEngine;
   
public class PrefabManager : MonoBehaviour
{
    public GameObject menuCursorPrefab;
    public GameObject menuItemPrefab;
    public RuntimeAnimatorController animatorController;
    public TextAsset skillData;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static PrefabManager GetPrefabs(){
        return GameObject.FindWithTag("Global").GetComponent<PrefabManager>();
    }
}
