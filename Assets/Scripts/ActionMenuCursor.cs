using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Objects;

public class ActionMenuCursor : MenuCursor
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (this.menu != null){
			if (Input.GetKeyDown(controls.GetCommand(Command.CONFIRM))){
				cursor.locked = false;
				SelectItem(currentItem);
			}
		}
    }

	protected override void HoverItem(GameObject item){
		base.HoverItem(item);
		string itemName = item.transform.Find("Name").gameObject.GetComponent<Text>().text;
		UnitAction action = (UnitAction) Enum.Parse(typeof(UnitAction), itemName);
		switch (action){
			case UnitAction.MOVE:
				cursor.selectedSpace.occupyingUnit.GetComponent<PlayerController>().ShowAccessibleSpaces();
				break;
			case UnitAction.SKILL:
				break;
			case UnitAction.WAIT:
				break;
			default:
				break;
		}
	}
	
	public override void LinkMenu(GameObject menu){
		base.LinkMenu(menu);
	}
	
	protected override void SelectItem(GameObject item){
		string itemName = item.transform.Find("Name").gameObject.GetComponent<Text>().text;
		if (itemName == "BACK"){
			; //logic for canceling or ending turn
		}
		UnitAction action = (UnitAction) Enum.Parse(typeof(UnitAction), itemName);
		switch (action){
			case UnitAction.MOVE:
				Instantiate(cursor.moveCursorPrefab);
				break;
			case UnitAction.SKILL:
				menuController.ShowSkillList(cursor.selectedSpace.occupyingUnit.GetComponent<PlayerController>());
				break;
			case UnitAction.WAIT:
				cursor.selectedSpace.occupyingUnit.GetComponent<PlayerController>().EndTurn();
				cursor.selectedSpace.occupyingUnit.GetComponent<PlayerController>().previousAction = UnitAction.WAIT;
				break;
			default:
				break;
		}
		menuController.ToggleActions(false);
	}
}
