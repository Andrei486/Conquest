using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Objects;

public class BattleMenu : MonoBehaviour
{
	bool showingSkills = false;
	bool showingActions = false;
	public bool showInfo = false;
	public GameObject skillItemPrefab;
	public GameObject actionItemPrefab;
	public GameObject attributePrefab;
	public GameObject cursorPrefab;
	public SkillTypeInfo[] skillTypes;
	public ActionInfo[] actionItems;
	float skillStackHeight;
	float actionStackHeight;
	public float attributeStackHeight;
	Quaternion skillRotation;
	public Cursor cursor;
	BoardManager board;
	ControlsManager controls;
	public GameObject skillCursor;
	public GameObject actionCursor;
    // Start is called before the first frame update
    void Start()
    {
		board = BoardManager.GetBoard();
		controls = ControlsManager.GetControls();
		skillStackHeight = skillItemPrefab.GetComponent<RectTransform>().rect.height * skillItemPrefab.transform.localScale.y * 1.1f;
		actionStackHeight = actionItemPrefab.GetComponent<RectTransform>().rect.height * actionItemPrefab.transform.localScale.y * 1.1f;
		attributeStackHeight = attributePrefab.GetComponent<RectTransform>().rect.height * attributePrefab.transform.localScale.y * 1.1f;
		
		skillCursor = Instantiate(cursorPrefab, this.gameObject.transform.Find("Cursors"));
		skillCursor.AddComponent(typeof(SkillMenuCursor));
		actionCursor = Instantiate(cursorPrefab, this.gameObject.transform.Find("Cursors"));
		actionCursor.AddComponent(typeof(ActionMenuCursor));
    }

    // Update is called once per frame
    void Update()
    {
		if (showingSkills){
			if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_DOWN))){
				skillCursor.GetComponent<MenuCursor>().MoveDown();
			}
			if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_UP))){
				skillCursor.GetComponent<MenuCursor>().MoveUp();
			}
			if (Input.GetKeyDown(controls.GetCommand(Command.BACK))){
				cursor.locked = false;
				cursor.Deselect();
				cursor.Select(cursor.board.GetSpace(skillCursor.GetComponent<SkillMenuCursor>().boardPosition));
			}
		}
		else if (showingActions){
			PlayerController playerController = actionCursor.GetComponent<ActionMenuCursor>().playerController;
			if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_DOWN))){
				actionCursor.GetComponent<MenuCursor>().MoveDown();
			}
			if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_UP))){
				actionCursor.GetComponent<MenuCursor>().MoveUp();
			}
			//cannot cancel if unit has already acted or moved
			if (Input.GetKeyDown(controls.GetCommand(Command.BACK)) && !playerController.hasActed){
				cursor.locked = false;
				cursor.Deselect();
				ToggleActions(false);
			}
		}
    }
	
	public void ShowSkillList(PlayerController pc){
		List<Skill> skillList = pc.GetUsableSkills();
		cursor.locked = true;
		
		foreach (Transform child in transform.Find("Skills")){
			Destroy(child.gameObject);
		}
		ToggleActions(false);
		
		showingSkills = true;
		GameObject menuItem;
		Text text;
		Image emblem;
		int skillCount = 0;
		foreach (Skill toShow in skillList){
			menuItem = Instantiate(skillItemPrefab, this.transform.Find("Skills")); //for each skill create a menu item as child of menu
			menuItem.transform.Translate(new Vector3(0f, skillStackHeight * skillCount, 0f));
			
			text = menuItem.transform.Find("Name").GetComponent<Text>();
			text.text = toShow.name;
			
			emblem = menuItem.transform.Find("Emblem").gameObject.GetComponent<Image>();
			foreach (SkillTypeInfo t in skillTypes){
				if (t.type == toShow.skillType){
					emblem.sprite = t.emblemSprite;
					break;
				}
			}
			
			skillCount++;
		}
		
		ToggleSkills(true);
		//skillCursor.GetComponent<MenuCursor>().LinkMenu(this.gameObject.transform.Find("Skills").gameObject);
	}
	
	public void ShowActionList(PlayerController pc){
		cursor.locked = true;
		List<UnitAction> actions = pc.GetUsableActions();
		
		if (actions.Count == 1){ //the only action is WAIT, must end turn
			pc.turnEnded = true;
			//free the cursor
			cursor.locked = false;
			cursor.rotationLocked = false;
			cursor.Deselect();
			pc.EndTurn();
			return;
		}
		
		foreach (Transform child in transform.Find("Actions")){
			//child.SetAsLastSibling();
			Destroy(child.gameObject);
		}
		ToggleSkills(false);
		showingActions = true;
		GameObject menuItem;
		Text text;
		Image emblem;
		int actionCount = 0;
		foreach (UnitAction action in actions){
			menuItem = Instantiate(actionItemPrefab, this.transform.Find("Actions"));
			menuItem.name = action.ToString();
			menuItem.transform.Translate(new Vector3(0f, actionStackHeight * actionCount, 0f));
			text = menuItem.transform.Find("Name").gameObject.GetComponent<Text>();
			text.text = action.ToString();
			
			emblem = menuItem.transform.Find("Emblem").gameObject.GetComponent<Image>();
			foreach (ActionInfo a in actionItems){
				if (a.action == action){
					emblem.sprite = a.emblemSprite;
					break;
				}
			}
			
			actionCount++;
		}
		ToggleActions(true);
		//actionCursor.GetComponent<MenuCursor>().LinkMenu(this.gameObject.transform.Find("Actions").gameObject);
	}
	
	public void ToggleSkills(bool enabled){
		foreach (Transform child in transform.Find("Skills")){
			child.gameObject.SetActive(enabled);
		}
		showingSkills = enabled;
		if (enabled){
			skillCursor.GetComponent<MenuCursor>().LinkMenu(this.gameObject.transform.Find("Skills").gameObject);
		} else {
			skillCursor.GetComponent<MenuCursor>().UnlinkMenu();
		}
	}
	public void ToggleActions(bool enabled){
		foreach (Transform child in transform.Find("Actions")){
			child.gameObject.SetActive(enabled);
		}
		showingActions = enabled;
		if (enabled){
			actionCursor.GetComponent<MenuCursor>().LinkMenu(this.gameObject.transform.Find("Actions").gameObject);
		} else {
			actionCursor.GetComponent<MenuCursor>().UnlinkMenu();
		}
	}

	public static BattleMenu GetMenu(){
		return GameObject.FindWithTag("MenuController").GetComponent<BattleMenu>();
	}
}
