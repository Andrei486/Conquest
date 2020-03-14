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
	public float infoMoveTime = 0.05f;
    protected override void Start()
    {
		base.Start();
        camera = GameObject.FindGameObjectsWithTag("MainCamera")[0];
		board = GameObject.FindGameObjectsWithTag("Board")[0].GetComponent<BoardManager>();
		this.skillRotation = Quaternion.AngleAxis((int) camera.transform.eulerAngles.y, Vector3.back);
    }

    // Update is called once per frame
    void Update()
    {
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
			if (Input.GetKeyDown(controls.GetCommand(Command.BACK))){
				cursor.locked = false;
				cursor.selectedSpace = null;
				cursor.Select(cursor.board.GetSpace(boardPosition));
				menuController.ToggleSkills(false);
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

		if (updateCosts){
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

	private void ScaleCorrection(GameObject item, GameObject attribute){
		float x = (float) 1.0 / item.transform.localScale.x;
		float y = (float) 1.0 / item.transform.localScale.y;
		float z = (float) 1.0 / item.transform.localScale.z;
		Vector3 inverseScale = new Vector3(x, y, z);
		attribute.transform.localScale = Vector3.Scale(inverseScale, attribute.transform.localScale);
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

	private IEnumerator SetupMenu(GameObject item, List<GameObject> skillInfo, float time){
		float startTime = Time.time;
		float elapsedTime = 0;
		float horizontalOffset = item.GetComponent<RectTransform>().rect.size.x * item.transform.localScale.x * 1.1f;
		float baseSpeed = menuController.attributeStackHeight / time;
		//move all menu items to their starting position right of the menu
		foreach (GameObject attribute in skillInfo){
			attribute.transform.Translate(new Vector3(horizontalOffset, 0, 0));
		}
		while (elapsedTime < time){
			if (currentItem != item){ //if a different item is selected, stop
				yield break;
			}
			int i = 0;
			foreach (GameObject attribute in skillInfo){
				attribute.transform.Translate(new Vector3(0, baseSpeed * Time.deltaTime * i, 0));
				i++;
			}
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}
}
