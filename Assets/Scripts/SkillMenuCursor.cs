using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Objects;

public class SkillMenuCursor : MenuCursor
{
	public Quaternion skillRotation;
	GameObject camera;
    // Start is called before the first frame update
    protected override void Start()
    {
		base.Start();
		
        camera = GameObject.FindGameObjectsWithTag("MainCamera")[0];
		
		this.skillRotation = Quaternion.AngleAxis((int) camera.transform.eulerAngles.y, Vector3.back);
    }

    // Update is called once per frame
    void Update()
    {
		if (this.menu != null){
			if (Input.GetKeyDown("a")){
				skillRotation *= Quaternion.AngleAxis(90, Vector3.forward);
				Debug.Log(skillRotation.eulerAngles);
				HoverItem(currentItem);
			}
			if (Input.GetKeyDown("d")){
				skillRotation *= Quaternion.AngleAxis(90, Vector3.back);
				Debug.Log(skillRotation.eulerAngles);
				HoverItem(currentItem);
			}
			if (Input.GetKeyDown("space")){
				cursor.locked = false;
				UpdateItem();
				SelectItem(currentItem);
			}
		}
    }
	
	public override void LinkMenu(GameObject menu){
		base.LinkMenu(menu);
		this.skillRotation = Quaternion.AngleAxis((int) camera.transform.eulerAngles.y, Vector3.back);
		cursor.MakeVisible(false);
	}
	
	public override void UnlinkMenu(){
		base.UnlinkMenu();
		cursor.MakeVisible(true);
	}
	
	protected override void HoverItem(GameObject item){
		base.HoverItem(item);
		
		string skillName = this.currentItem.transform.Find("Name").gameObject.GetComponent<Text>().text;
		PlayerController player = cursor.selectedSpace.occupyingUnit.GetComponent<PlayerController>();
		foreach (Skill skill in player.skillList){
			if (skill.name == skillName){
				skill.VisualizeTarget(cursor.temporarySpace, player.gameObject, skillRotation);
			}
		}
	}
	
	protected override void SelectItem(GameObject item){
		cursor.Select(cursor.board.GetSpace(cursor.position));
		menuController.ToggleSkills(false);
	}
}
