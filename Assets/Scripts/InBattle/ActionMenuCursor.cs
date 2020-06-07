using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Objects;

namespace InBattle{
	public class ActionMenuCursor : MenuCursor
	{
		public PlayerController playerController;
		BoardManager board;
		Cursor cursor;
		BattleMenu menuController;
		ControlsManager controls;
		
		protected override void Start()
		{
			base.Start();
			board = BoardManager.GetBoard();
			controls = ControlsManager.GetControls();
			cursor = GameObject.FindGameObjectsWithTag("Cursor")[0].GetComponent<Cursor>();
			menuController = BattleMenu.GetMenu();
		}

		// Update is called once per frame
		protected override void Update()
		{
			base.Update();
			if (this.menu != null){
				if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_DOWN))){
					MoveDown();
				}
				if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_UP))){
					MoveUp();
				}
				if (Input.GetKeyDown(controls.GetCommand(Command.CONFIRM))){
					cursor.locked = false;
					SelectItem(currentItem);
				}
				// if (Input.GetKeyDown(controls.GetCommand(Command.TOGGLE_INFO))){
				// 	if (menuController.showInfo){
				// 		ShowActionInfo(currentItem);
				// 	} else {
				// 		HideActionInfo();
				// 	}
				// }
				if (Input.GetKeyDown(controls.GetCommand(Command.BACK)) && !playerController.hasActed){
					Debug.Log("no longer showing actions");
					cursor.Deselect();
					cursor.locked = false;
				}
			}
		}

		protected override void HoverItem(GameObject item){
			base.HoverItem(item);
			BoardManager.ClearVisualization();
			string itemName = item.transform.Find("Name").gameObject.GetComponent<Text>().text;
			UnitAction action = (UnitAction) Enum.Parse(typeof(UnitAction), itemName);
			switch (action){
				case UnitAction.MOVE:
					playerController.ShowAccessibleSpaces();
					break;
				case UnitAction.SKILL:
					break;
				case UnitAction.WAIT:
					break;
				default:
					break;
			}
			if (menuController.showInfo){
				// ShowActionInfo(item);
			}
		}

		// private void ShowActionInfo(GameObject item){
		// 	HideActionInfo();
		// 	string itemName = item.transform.Find("Name").gameObject.GetComponent<Text>().text;
		// 	UnitAction action = (UnitAction) Enum.Parse(typeof(UnitAction), itemName);
		// 	List<GameObject> actionInfo = new List<GameObject>();
		// 	GameObject actionsLeft;
		// 	GameObject moveLeft;
		// 	GameObject bulletsLeft;
		// 	switch (action){
		// 		case UnitAction.MOVE:
		// 			moveLeft = Instantiate(menuController.attributePrefab, item.transform);
		// 			ScaleCorrection(item, moveLeft);
		// 			moveLeft.transform.Find("Attribute").GetComponent<Text>().text = "Movement Left";
		// 			moveLeft.transform.Find("Value").GetComponent<Text>().text = playerController.remainingMove.ToString();
		// 			actionInfo.Add(moveLeft);
		// 			break;
		// 		case UnitAction.SKILL:
		// 			actionsLeft = Instantiate(menuController.attributePrefab, item.transform);
		// 			ScaleCorrection(item, actionsLeft);
		// 			actionsLeft.transform.Find("Attribute").GetComponent<Text>().text = "Actions Left";
		// 			actionsLeft.transform.Find("Value").GetComponent<Text>().text = string.Format(
		// 				"{0}/{1}",
		// 				playerController.remainingActions.ToString(),
		// 				playerController.maxActions.ToString());
		// 			actionInfo.Add(actionsLeft);

		// 			bulletsLeft = Instantiate(menuController.attributePrefab, item.transform);
		// 			ScaleCorrection(item, bulletsLeft);
		// 			bulletsLeft.transform.Find("Attribute").GetComponent<Text>().text = "Bullets Left";
		// 			bulletsLeft.transform.Find("Value").GetComponent<Text>().text = string.Format(
		// 				"{0}/{1}",
		// 				playerController.bullets.ToString(),
		// 				playerController.maxBullets.ToString());
		// 			actionInfo.Add(bulletsLeft);
		// 			break;
		// 		case UnitAction.WAIT:
		// 			break;
		// 	}
		// 	StartCoroutine(SetupMenu(item, actionInfo, infoMoveTime));
		// }

		// private void HideActionInfo(){
		// 	/**Removes all info items for skills.*/
		// 	foreach (Transform actionItem in menu.transform){
		// 		foreach (Transform actionInfo in actionItem.transform){
		// 			if (actionInfo.name.Contains("Attribute Item")){
		// 				Destroy(actionInfo.gameObject);
		// 			}
		// 		}
		// 	}
		// }
		
		public override void LinkMenu(GameObject menu){
			base.LinkMenu(menu);
			cursor = cursor = GameObject.FindGameObjectsWithTag("Cursor")[0].GetComponent<Cursor>();
			playerController = cursor.selectedSpace.occupyingUnit.GetComponent<PlayerController>();
		}
		
		public override void SelectItem(GameObject item){
			string itemName = item.transform.Find("Name").gameObject.GetComponent<Text>().text;
			UnitAction action = (UnitAction) Enum.Parse(typeof(UnitAction), itemName);
			switch (action){
				case UnitAction.MOVE:
					Instantiate(cursor.moveCursorPrefab, BoardManager.GetBoard().transform);
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
			menuController.HideActions();
		}
	}
}