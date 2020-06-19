using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Objects;

namespace InBattle{	
	public class SkillMenuCursor : MenuCursor
	{
		public Quaternion skillRotation;
		private List<Quaternion> validRotations = new List<Quaternion>();
		private List<GameObject> visualizations = new List<GameObject>();
		public BoardManager board;
		new GameObject camera;
		Cursor cursor;
		BattleMenu menuController;
		ControlsManager controls;
		public Vector2 boardPosition;
		protected override void Start()
		{
			base.Start();
			menuController = BattleMenu.GetMenu();
			camera = GameObject.FindGameObjectsWithTag("MainCamera")[0];
			controls = ControlsManager.GetControls();
			board = BoardManager.GetBoard();
			this.skillRotation = Quaternion.identity;
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
				if (Input.GetKeyDown(controls.GetCommand(Command.CAMERA_LEFT))){
					RotateLeft();
				}
				if (Input.GetKeyDown(controls.GetCommand(Command.CAMERA_RIGHT))){
					RotateRight();
				}
				if (Input.GetKeyDown(controls.GetCommand(Command.CONFIRM))){
					cursor.locked = false;
					SelectItem(currentItem);
				}
				if (Input.GetKeyDown(controls.GetCommand(Command.BACK))){
					Debug.Log("no longer showing skills");
					cursor.locked = false;
					cursor.Deselect();
					cursor.Select(board.GetSpace(boardPosition));
				}
				// if (Input.GetKeyDown(controls.GetCommand(Command.TOGGLE_INFO))){
				// 	if (menuController.showInfo){
				// 		ShowSkillCosts(currentItem);
				// 	} else {
				// 		HideSkillCosts();
				// 	}
				// }
			}
		}

		public override void LinkMenu(GameObject menu){
			base.LinkMenu(menu);
			//this.skillRotation = Quaternion.AngleAxis((int) camera.transform.eulerAngles.y, Vector3.back);
			cursor = GameObject.FindGameObjectsWithTag("Cursor")[0].GetComponent<Cursor>();
			this.skillRotation = Quaternion.identity;
			this.boardPosition = cursor.position;
			cursor.MakeVisible(false);
			cursor.rotationLocked = true;
			//ResetRotation();
		}
		
		public override void UnlinkMenu(){
			base.UnlinkMenu();
			cursor.MakeVisible(true);
			cursor.rotationLocked = false;
		}

		
		protected override void HoverItem(GameObject item){
			bool newSelected = (currentItem != item);
			base.HoverItem(item);
			BoardManager.ClearVisualization();
			
			string skillName = this.currentItem.transform.Find("Name").gameObject.GetComponent<Text>().text;
			PlayerController player = board.GetSpace(boardPosition).occupyingUnit.GetComponent<PlayerController>();
			Skill skill = Skill.GetSkillByName(skillName, board.skillData);
			skill.VisualizeTarget(cursor.selectedSpace, player.gameObject, skillRotation);
			
			// if (newSelected && menuController.showInfo){
			// 	ShowSkillCosts(item);
			// }
			if (newSelected){
				ResetRotation();
			}
			VisualizeDirections(cursor.selectedSpace);
		}

		private List<Quaternion> GetDirections(Skill skill, BoardSpace space){
			List<Quaternion> valid = new List<Quaternion>();
			Quaternion stepRotation = Quaternion.AngleAxis(90, Vector3.up);
			Quaternion rotation;
			for (int i=0; i<=3; i++){
				rotation = Quaternion.identity;
				for (int j = 0; j < i; j++){
					rotation *= stepRotation;
				}
				Debug.Log(rotation.eulerAngles);
				if (skill.IsValid(space, rotation)){
					valid.Add(rotation);
				}
			}
			return valid;
		}

		private void VisualizeDirections(BoardSpace space){
			GameObject unit = space.occupyingUnit;
			PlayerController pc = unit.GetComponent<PlayerController>();
			GameObject arrow;
			foreach (GameObject obj in visualizations){
				Destroy(obj.gameObject);
			}
			visualizations = new List<GameObject>();
			foreach (Quaternion direction in validRotations){
				arrow = Instantiate(board.directionArrowPrefab);
				arrow.name = "arrow";
				arrow.transform.position = unit.transform.position;
				arrow.transform.Translate(Vector3.up, Space.World); //arrow should be above terrain
				arrow.transform.Rotate(-direction.eulerAngles, Space.World);
				arrow.transform.Translate(Vector3.up, Space.Self); //move arrow in the direction it points
				if (skillRotation.ApproximatelyEqual(direction)){
					Debug.Log("highlighted");
					arrow.GetComponent<SpriteRenderer>().color = new Color(0.95f, 0.85f, 0.1f); //orange
				}
				visualizations.Add(arrow);
			}
			//HighlightDirection(skillRotation);
		}

		private void HighlightDirection(Quaternion direction){
			if (direction == Quaternion.identity){
				return;
			}
			foreach (GameObject arrow in visualizations){
				arrow.GetComponent<SpriteRenderer>().color = Color.white;
			}
			int index = validRotations.IndexOf(direction);
			GameObject toHighlight = visualizations[index];
			toHighlight.GetComponent<SpriteRenderer>().color = new Color(255, 179, 25); //orange
		}

		private void ResetRotation(){
			string skillName = this.currentItem.transform.Find("Name").gameObject.GetComponent<Text>().text;
			PlayerController player = board.GetSpace(boardPosition).occupyingUnit.GetComponent<PlayerController>();
			Skill skill = Skill.GetSkillByName(skillName, board.skillData);

			player.gameObject.transform.eulerAngles = player.defaultEulerAngles;
			validRotations = GetDirections(skill, cursor.selectedSpace);
			skillRotation = validRotations.Count == 0 ? Quaternion.identity : validRotations[0];
			player.gameObject.transform.Rotate(-skillRotation.eulerAngles, Space.World);
			RotateRight();
			RotateLeft();
			Debug.Log(validRotations.Count);
		}
		
		public override void SelectItem(GameObject item){
			
			PlayerController player = board.GetSpace(boardPosition).occupyingUnit.GetComponent<PlayerController>();
			string skillName = this.currentItem.transform.Find("Name").GetComponent<Text>().text;
			
			Skill skill = Skill.GetSkillByName(skillName, board.skillData);
			if (skill.IsValid(board.GetSpace(boardPosition), skillRotation)){
				player.UseSkill(skill, skillRotation);
				player.previousAction = UnitAction.SKILL;
				player.hasActed = true;
			} else {
				Debug.Log("can't use that skill here");
			}
			cursor.Select(cursor.board.GetSpace(cursor.position));
			menuController.HideSkills();
		}

		// private void ShowSkillCosts(GameObject item){
		// 	HideSkillCosts(); //remove any existing skill info before making new ones
		// 	Skill skill = Skill.GetSkillByName(item.transform.Find("Name").GetComponent<Text>().text, board.skillData);
		// 	List<GameObject> skillInfo = new List<GameObject>();

		// 	GameObject actionCost = Instantiate(menuController.attributePrefab, item.transform);
		// 	actionCost.transform.Find("Attribute").GetComponent<Text>().text = "Action Cost";
		// 	actionCost.transform.Find("Value").GetComponent<Text>().text = skill.actionCost.ToString();
		// 	skillInfo.Add(actionCost);

		// 	if (skill.bulletCost > 0){
		// 		GameObject bulletCost = Instantiate(menuController.attributePrefab, item.transform);
		// 		bulletCost.transform.Find("Attribute").GetComponent<Text>().text = "Bullet Cost";
		// 		bulletCost.transform.Find("Value").GetComponent<Text>().text = skill.bulletCost.ToString();
		// 		skillInfo.Add(bulletCost);
		// 	}
		// 	if (skill.moveCost > 0){
		// 		GameObject moveCost = Instantiate(menuController.attributePrefab, item.transform);
		// 		moveCost.transform.Find("Attribute").GetComponent<Text>().text = "Movement Cost";
		// 		moveCost.transform.Find("Value").GetComponent<Text>().text = skill.moveCost.ToString();
		// 		skillInfo.Add(moveCost);
		// 	}
		// 	foreach(GameObject attribute in skillInfo){
		// 		ScaleCorrection(item, attribute);
		// 	}

		// 	StartCoroutine(SetupMenu(item, skillInfo, infoMoveTime));
		// }

		// private void HideSkillCosts(){
		// 	/**Removes all info items for skills.*/
		// 	foreach (Transform skillItem in menu.transform){
		// 		foreach (Transform skillInfo in skillItem.transform){
		// 			if (skillInfo.name.Contains("Attribute Item")){
		// 				Destroy(skillInfo.gameObject);
		// 			}
		// 		}
		// 	}
		// }

		public override void MoveUp(){
			base.MoveUp();
			ResetRotation();
			VisualizeDirections(cursor.selectedSpace);
		}

		public override void MoveDown(){
			base.MoveDown();
			ResetRotation();
			VisualizeDirections(cursor.selectedSpace);
		}

		void RotateLeft(){
			if (validRotations.Count == 0){
				return;	
			}
			GameObject unit = cursor.selectedSpace.occupyingUnit;
			unit.transform.eulerAngles = unit.GetComponent<PlayerController>().defaultEulerAngles;
			skillRotation = validRotations.NextOf(skillRotation);
			unit.transform.Rotate(-skillRotation.eulerAngles, Space.World);
			//HighlightDirection(skillRotation);
			HoverItem(currentItem);
		}
		void RotateRight(){
			if (validRotations.Count == 0){
				return;	
			}
			GameObject unit = cursor.selectedSpace.occupyingUnit;
			unit.transform.eulerAngles = unit.GetComponent<PlayerController>().defaultEulerAngles;
			skillRotation = validRotations.PreviousOf(skillRotation);
			unit.transform.Rotate(-skillRotation.eulerAngles, Space.World);
			//HighlightDirection(skillRotation);
			HoverItem(currentItem);
		}
	}
}