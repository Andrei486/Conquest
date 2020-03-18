using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Objects;

public class SkillMenuCursor : MenuCursor
{
	public Quaternion skillRotation;
	public BoardManager board;
	new GameObject camera;
	public Vector2 boardPosition;
    protected override void Start()
    {
		base.Start();
        camera = GameObject.FindGameObjectsWithTag("MainCamera")[0];
		board = BoardManager.GetBoard();
		this.skillRotation = Quaternion.AngleAxis((int) camera.transform.eulerAngles.y, Vector3.back);
    }

    // Update is called once per frame
    protected override void Update()
    {
		base.Update();
		if (this.menu != null){
			if (Input.GetKeyDown(controls.GetCommand(Command.CAMERA_LEFT))){
				skillRotation *= Quaternion.AngleAxis(90, Vector3.forward);
				HoverItem(currentItem);
			}
			if (Input.GetKeyDown(controls.GetCommand(Command.CAMERA_RIGHT))){
				skillRotation *= Quaternion.AngleAxis(90, Vector3.back);
				HoverItem(currentItem);
			}
			if (Input.GetKeyDown(controls.GetCommand(Command.CONFIRM))){
				cursor.locked = false;
				SelectItem(currentItem);
			}
			
			if (Input.GetKeyDown(controls.GetCommand(Command.TOGGLE_INFO))){
				if (menuController.showInfo){
					ShowSkillCosts(currentItem);
				} else {
					HideSkillCosts();
				}
			}
		}
    }

	public override void LinkMenu(GameObject menu){
		base.LinkMenu(menu);
		this.skillRotation = Quaternion.AngleAxis((int) camera.transform.eulerAngles.y, Vector3.back);
		this.boardPosition = cursor.position;
		cursor.MakeVisible(false);
		cursor.rotationLocked = true;
	}
	
	public override void UnlinkMenu(){
		base.UnlinkMenu();
		cursor.MakeVisible(true);
		cursor.rotationLocked = false;
	}
	
	protected override void HoverItem(GameObject item){
		bool updateCosts = (currentItem != item);
		base.HoverItem(item);
		
		string skillName = this.currentItem.transform.Find("Name").gameObject.GetComponent<Text>().text;
		PlayerController player = board.GetSpace(boardPosition).occupyingUnit.GetComponent<PlayerController>();
		Skill skill = Skill.GetSkillByName(skillName, board.skillData);
		skill.VisualizeTarget(cursor.selectedSpace, player.gameObject, skillRotation);

		if (updateCosts && menuController.showInfo){
			ShowSkillCosts(item);
		}
	}
	
	protected override void SelectItem(GameObject item){
		
		string skillName = this.currentItem.transform.Find("Name").GetComponent<Text>().text;
		PlayerController player = board.GetSpace(boardPosition).occupyingUnit.GetComponent<PlayerController>();
		Skill skill = Skill.GetSkillByName(skillName, board.skillData);
		if (skill.IsValid(board.GetSpace(boardPosition), skillRotation)){
			player.UseSkill(skill, skillRotation);
			player.previousAction = UnitAction.SKILL;
			player.hasActed = true;
		} else {
			Debug.Log("can't use that skill here");
		}
		cursor.Select(cursor.board.GetSpace(cursor.position));
		menuController.ToggleSkills(false);
	}

	private void ShowSkillCosts(GameObject item){
		HideSkillCosts(); //remove any existing skill info before making new ones
		Skill skill = Skill.GetSkillByName(item.transform.Find("Name").GetComponent<Text>().text, board.skillData);
		List<GameObject> skillInfo = new List<GameObject>();

		GameObject actionCost = Instantiate(menuController.attributePrefab, item.transform);
		actionCost.transform.Find("Attribute").GetComponent<Text>().text = "Action Cost";
		actionCost.transform.Find("Value").GetComponent<Text>().text = skill.actionCost.ToString();
		skillInfo.Add(actionCost);

		if (skill.bulletCost > 0){
			GameObject bulletCost = Instantiate(menuController.attributePrefab, item.transform);
			bulletCost.transform.Find("Attribute").GetComponent<Text>().text = "Bullet Cost";
			bulletCost.transform.Find("Value").GetComponent<Text>().text = skill.bulletCost.ToString();
			skillInfo.Add(bulletCost);
		}
		if (skill.moveCost > 0){
			GameObject moveCost = Instantiate(menuController.attributePrefab, item.transform);
			moveCost.transform.Find("Attribute").GetComponent<Text>().text = "Movement Cost";
			moveCost.transform.Find("Value").GetComponent<Text>().text = skill.moveCost.ToString();
			skillInfo.Add(moveCost);
		}
		foreach(GameObject attribute in skillInfo){
			ScaleCorrection(item, attribute);
		}

		StartCoroutine(SetupMenu(item, skillInfo, infoMoveTime));
	}

	private void HideSkillCosts(){
		/**Removes all info items for skills.*/
		foreach (Transform skillItem in menu.transform){
			foreach (Transform skillInfo in skillItem.transform){
				if (skillInfo.name.Contains("Attribute Item")){
					Destroy(skillInfo.gameObject);
				}
			}
		}
	}
}
