using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuCursor : MonoBehaviour
{
	protected GameObject menu;
	protected BattleMenu menuController;
	protected Cursor cursor;
	public GameObject currentItem;
	public int index;
	protected ControlsManager controls = ControlsManager.GetControls();
	protected float infoMoveTime = 0.1f;
	Vector3 offset;
    // Start is called before the first frame update
    virtual protected void Start()
    {
        this.gameObject.GetComponent<Image>().enabled = false;
		menuController = BattleMenu.GetMenu();
		cursor = GameObject.FindGameObjectsWithTag("Cursor")[0].GetComponent<Cursor>();
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        if (this.menu != null){
			if (Input.GetKeyDown(controls.GetCommand(Command.TOGGLE_INFO))){
				menuController.showInfo = !menuController.showInfo;
				HoverItem(currentItem); //allow changes to take effect
			}
		}
    }
	
	virtual public void LinkMenu(GameObject menu){
		this.menu = menu;
		ResetItem();
		offset = new Vector3(this.gameObject.GetComponent<RectTransform>().rect.width * this.gameObject.transform.localScale.x * -1.1f, 0f, 0f);
		this.gameObject.GetComponent<Image>().enabled = true;
	}
	
	virtual public void UnlinkMenu(){
		this.menu = null;
		this.currentItem = null; //select the first element
		this.gameObject.GetComponent<Image>().enabled = false;
	}
	
	virtual protected void HoverItem(GameObject item){
		this.currentItem = item;
		this.transform.position = item.transform.position + offset;
		BoardManager.ClearVisualization();
	}
	
	protected void HoverItem(int childNumber){
		Debug.Log(menu.transform);
		Debug.Log(menu.transform.GetChild(childNumber));
		Debug.Log(menu.transform.GetChild(childNumber).gameObject);
		HoverItem(menu.transform.GetChild(childNumber).gameObject);
	}
	
	virtual protected void SelectItem(GameObject item){
		
	}
	
	public void MoveUp(){
		if (index < menu.transform.childCount - 1){
			index++;
			HoverItem(index);
		}
	}
	
	public void MoveDown(){
		if (index > 0){
			index--;
			HoverItem(index);
		}
	}
	
	protected void ResetItem(){
		StartCoroutine(Reset());
	}
	
	IEnumerator Reset(){
		yield return new WaitForEndOfFrame();
		index = 0;
		HoverItem(index);
	}

	protected IEnumerator SetupMenu(GameObject item, List<GameObject> attributes, float time){
		float elapsedTime = 0;
		float horizontalOffset = item.GetComponent<RectTransform>().rect.size.x * item.transform.localScale.x * 1.1f;
		float horizontalSpeed = horizontalOffset / time;
		float upwardsSpeed = menuController.attributeStackHeight / time;

		foreach (GameObject attribute in attributes){
			attribute.transform.SetAsFirstSibling(); //render below emblem and text
		}
		//first move everything horizontally
		while (elapsedTime < time){
			if (currentItem != item){ //if a different item is selected, stop
				yield break;
			}
			foreach (GameObject attribute in attributes){
				attribute.transform.Translate(new Vector3(horizontalSpeed * Time.deltaTime, 0, 0));
			}
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		//then vertically
		elapsedTime = 0;
		while (elapsedTime < time){
			if (currentItem != item){ //if a different item is selected, stop
				yield break;
			}
			int i = 0;
			foreach (GameObject attribute in attributes){
				attribute.transform.Translate(new Vector3(0, upwardsSpeed * Time.deltaTime * i, 0));
				i++;
			}
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	protected void ScaleCorrection(GameObject item, GameObject attribute){
		float x = (float) 1.0 / item.transform.localScale.x;
		float y = (float) 1.0 / item.transform.localScale.y;
		float z = (float) 1.0 / item.transform.localScale.z;
		Vector3 inverseScale = new Vector3(x, y, z);
		attribute.transform.localScale = Vector3.Scale(inverseScale, attribute.transform.localScale);
	}
}
