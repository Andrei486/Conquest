using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static StateManager stateManager;
    public Context state;
    void Start()
    {
        
    }
    void Update()
    {
        
    }

    public static StateManager GetStateManager(){
        if (stateManager == null){
            stateManager = GameObject.FindWithTag("Global").GetComponent<StateManager>();
        }
        return stateManager;
    }

    public void SetState(Context newState){
        switch (newState){
            case Context.BATTLE:
                break;
        }
    }
}

public enum Context{
    BATTLE, ACTION_MENU, SKILL_MENU,
    CONTROLS_MENU, UNIT_INFO
}