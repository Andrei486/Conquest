using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsMenuCursor : MenuCursor
{
    ControlsManager menuController;
    protected override void Start()
    {
        this.gameObject.GetComponent<Image>().enabled = false;
        menuController = ControlsManager.GetControls();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (locked || !menuController.showingControls){
            return;
        }
        if (Input.GetKeyDown(menuController.GetCommand(Command.CONFIRM))){
            SelectItem(currentItem);
        }
    }
    
    public override void SelectItem(GameObject item){
        Command command = (Command) ControlsManager.GetCommandByName(item.transform.Find("Command Name").GetComponent<Text>().text);
        StartCoroutine(menuController.SetCommandPopup(command));
    }

}