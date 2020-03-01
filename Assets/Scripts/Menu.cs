using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Objects;

public class Menu : MonoBehaviour
{
	bool showingSkills = false;
	bool showingActions = false;
	public GameObject skillItemPrefab;
	public GameObject actionItemPrefab;
	public GameObject cursorPrefab;
	public SkillTypeInfo[] skillTypes;
	public ActionInfo[] actionItems;
	float skillStackHeight;
	float actionStackHeight;
	Quaternion skillRotation;
	public Cursor cursor;
	BoardManager board;
	public GameObject skillCursor;
	public GameObject actionCursor;
    // Start is called before the first frame update
    void Start()
    {
		board = GameObject.FindGameObjectsWithTag("Board")[0].GetComponent<BoardManager>();
		skillStackHeight = skillItemPrefab.GetComponent<RectTransform>().rect.height * skillItemPrefab.transform.localScale.y * 1.1f;
		actionStackHeight = actionItemPrefab.GetComponent<RectTransform>().rect.height * actionItemPrefab.transform.localScale.y * 1.1f;
		
		skillCursor = Instantiate(cursorPrefab, this.gameObject.transform.Find("Cursors"));
		skillCursor.AddComponent(typeof(SkillMenuCursor));
		actionCursor = Instantiate(cursorPrefab, this.gameObject.transform.Find("Cursors"));
		actionCursor.AddComponent(typeof(ActionMenuCursor));
    }

    // Update is called once per frame
    void Update()
    {
		if (showingSkills){
			if (Input.GetKeyDown("down")){
				skillCursor.GetComponent<MenuCursor>().MoveDown();
			}
			if (Input.GetKeyDown("up")){
				skillCursor.GetComponent<MenuCursor>().MoveUp();
			}
		}
		if (showingActions){
			if (Input.GetKeyDown("down")){
				actionCursor.GetComponent<MenuCursor>().MoveDown();
			}
			if (Input.GetKeyDown("up")){
				actionCursor.GetComponent<MenuCursor>().MoveUp();
			}
		}
    }
	
	public void ShowSkillList(PlayerController pc){
		List<Skill> skillList = pc.skillList;
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
			
			text = menuItem.transform.Find("Name").gameObject.GetComponent<Text>();
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
		HashSet<UnitAction> actions = pc.GetUsableActions();
		
		if (actions.Count == 0){
			pc.alreadyActed = true;
			return;
		}
		
		foreach (Transform child in transform.Find("Actions")){
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
}
