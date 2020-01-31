using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Objects;

public class Menu : MonoBehaviour
{
	bool showingMenu;
	public GameObject menuItemPrefab;
	float menuItemStackHeight;
    // Start is called before the first frame update
    void Start()
    {
		menuItemStackHeight = menuItemPrefab.GetComponent<RectTransform>().rect.height * menuItemPrefab.transform.localScale.y * 1.1f;
		
        Skill skill = new Skill();
		skill.name = "sdfkjhalskdfhbl1";
		List<Skill> skills = new List<Skill>();
		skills.Add(skill);
		skills.Add(skill);
		skills.Add(skill);
		skills.Add(skill);
		ShowSkillList(skills);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("q")){
			ClearMenu();
		}
    }
	
	public void ShowSkillList(List<Skill> skillList){
		GameObject menuItem;
		Text text;
		Image emblem;
		int skillCount = 0;
		foreach (Skill toShow in skillList){
			menuItem = Instantiate(menuItemPrefab, this.transform); //for each skill create a menu item as child of menu
			menuItem.transform.Translate(new Vector3(0f, menuItemStackHeight * skillCount, 0f));
			
			text = menuItem.transform.Find("Skill Name").gameObject.GetComponent<Text>();
			text.text = toShow.name;
			
			emblem = menuItem.transform.Find("Emblem").gameObject.GetComponent<Image>();
			
			skillCount++;
		}
	}
	
	public void ClearMenu(){
		foreach (Transform child in transform){
			Destroy(child.gameObject);
		}
	}
}
