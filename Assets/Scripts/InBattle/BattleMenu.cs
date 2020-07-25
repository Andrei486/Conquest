using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Objects;
using Menus;

namespace InBattle{
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
		public GameObject skillMenu;
		public GameObject actionMenu;
		// Start is called before the first frame update
		void Start()
		{
			board = BoardManager.GetBoard();
			controls = ControlsManager.GetControls();
			// skillStackHeight = skillItemPrefab.GetComponent<RectTransform>().rect.height * skillItemPrefab.transform.localScale.y * 1.1f;
			// actionStackHeight = actionItemPrefab.GetComponent<RectTransform>().rect.height * actionItemPrefab.transform.localScale.y * 1.1f;
			attributeStackHeight = attributePrefab.GetComponent<RectTransform>().rect.height * attributePrefab.transform.localScale.y * 1.1f;
			
			// skillCursor = Instantiate(cursorPrefab, this.gameObject.transform.Find("Cursors"));
			// skillCursor.AddComponent(typeof(SkillMenuCursor));
			// actionCursor = Instantiate(cursorPrefab, this.gameObject.transform.Find("Cursors"));
			// actionCursor.AddComponent(typeof(ActionMenuCursor));
		}

		// Update is called once per frame
		void Update()
		{
			// if (showingSkills){
			// 	if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_DOWN))){
			// 		skillCursor.GetComponent<MenuCursor>().MoveDown();
			// 	}
			// 	if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_UP))){
			// 		skillCursor.GetComponent<MenuCursor>().MoveUp();
			// 	}
			// 	if (Input.GetKeyDown(controls.GetCommand(Command.BACK))){
			// 		Debug.Log("no longer showing skills");
			// 		cursor.locked = false;
			// 		cursor.Deselect();
			// 		cursor.Select(cursor.board.GetSpace(skillCursor.GetComponent<SkillMenuCursor>().boardPosition));
			// 	}
			// }
			// else if (showingActions){
			// 	PlayerController playerController = actionCursor.GetComponent<ActionMenuCursor>().playerController;
			// 	if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_DOWN))){
			// 		actionCursor.GetComponent<MenuCursor>().MoveDown();
			// 	}
			// 	if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_UP))){
			// 		actionCursor.GetComponent<MenuCursor>().MoveUp();
			// 	}
			// 	//cannot cancel if unit has already acted or moved
			// 	if (Input.GetKeyDown(controls.GetCommand(Command.BACK)) && !playerController.hasActed){
			// 		Debug.Log("no longer showing actions");
			// 		cursor.Deselect();
			// 		cursor.locked = false;
			// 	}
			// }
		}
		
		public void ShowSkillList(PlayerController pc){
			List<Skill> skillList = pc.GetUsableSkills();
			cursor.locked = true;
			
			// foreach (Transform child in transform.Find("Skills")){
			// 	Destroy(child.gameObject);
			// }
			HideSkills();
			HideActions();

			showingSkills = true;

			skillMenu = Menu.Create(skillItemPrefab, typeof(SkillMenuCursor), -1);
			skillMenu.GetComponent<Menu>().Generate(from skill in skillList select skill.name);
			
			GameObject menuItem;
			//Text text;
			Image emblem;

			foreach (Skill toShow in skillList){
				//for each skill add its emblem and set its theme
				menuItem = skillMenu.transform.Find(toShow.name).gameObject; 
				// menuItem.transform.Translate(new Vector3(0f, skillStackHeight * skillCount, 0f));
				UIController.SetTheme(menuItem.transform, pc.affiliation);
				
				// text = menuItem.transform.Find("Name").GetComponent<Text>();
				// text.text = toShow.name;
				
				emblem = menuItem.transform.Find("Emblem").gameObject.GetComponent<Image>();
				foreach (SkillTypeInfo t in skillTypes){
					if (t.type == toShow.skillType){
						emblem.sprite = t.emblemSprite;
						break;
					}
				}
				
			}
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
			
			// foreach (Transform child in transform.Find("Actions")){
			// 	//child.SetAsLastSibling();
			// 	Destroy(child.gameObject);
			// }
			HideSkills();
			HideActions();

			showingActions = true;

			actionMenu = Menu.Create(actionItemPrefab, typeof(ActionMenuCursor), -1);
			actionMenu.GetComponent<Menu>().Generate(from action in actions select action.ToString());

			GameObject menuItem;
			// Text text;
			Image emblem;
			foreach (UnitAction action in actions){
				menuItem = actionMenu.transform.Find(action.ToString()).gameObject;
				// menuItem.name = action.ToString();
				// menuItem.transform.Translate(new Vector3(0f, actionStackHeight * actionCount, 0f));
				UIController.SetTheme(menuItem.transform, pc.affiliation);
				// text = menuItem.transform.Find("Name").gameObject.GetComponent<Text>();
				// text.text = action.ToString();
				
				emblem = menuItem.transform.Find("Emblem").gameObject.GetComponent<Image>();
				foreach (ActionInfo a in actionItems){
					if (a.action == action){
						emblem.sprite = a.emblemSprite;
						break;
					}
				}
				
			}
			// ToggleActions(true);
			//actionCursor.GetComponent<MenuCursor>().LinkMenu(this.gameObject.transform.Find("Actions").gameObject);
		}
		
		public void HideSkills(){
			if (skillMenu != null){
				skillMenu.GetComponent<Menu>().Destroy();
			}
			showingSkills = false;
		}
		public void HideActions(){
			if (actionMenu != null){
				actionMenu.GetComponent<Menu>().Destroy();
			}
			showingActions = false;
		}

		public static BattleMenu GetMenu(){
			return GameObject.FindWithTag("MenuController").GetComponent<BattleMenu>();
		}
	}
}