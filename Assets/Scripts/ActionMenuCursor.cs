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
			if (Input.GetKeyDown("space")){
				cursor.locked = false;
				UpdateItem();
				SelectItem(currentItem);
			}
		}
    }
	
	protected override void HoverItem(GameObject item){
		base.HoverItem(item);
		UnitAction action = (UnitAction) Enum.Parse(typeof(UnitAction), item.transform.Find("Name").gameObject.GetComponent<Text>().text);
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
	
	protected override void SelectItem(GameObject item){
		UnitAction action = (UnitAction) Enum.Parse(typeof(UnitAction), item.transform.Find("Name").gameObject.GetComponent<Text>().text);
		switch (action){
			case UnitAction.MOVE:
				Instantiate(cursor.moveCursorPrefab);
				break;
			case UnitAction.SKILL:
				menuController.ShowSkillList(cursor.selectedSpace.occupyingUnit.GetComponent<PlayerController>());
				break;
			case UnitAction.WAIT:
				cursor.selectedSpace.occupyingUnit.GetComponent<PlayerController>().EndTurn();
				break;
			default:
				break;
		}
		menuController.ToggleActions(false);
	}
}
