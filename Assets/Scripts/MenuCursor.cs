using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuCursor : MonoBehaviour
{
	protected GameObject menu;
	protected Menu menuController;
	protected Cursor cursor;
	public GameObject currentItem;
	public int index;
	
	Vector3 offset;
    // Start is called before the first frame update
    virtual protected void Start()
    {
        this.gameObject.GetComponent<Image>().enabled = false;
		menuController = GameObject.FindGameObjectsWithTag("MenuController")[0].GetComponent<Menu>();
		cursor = GameObject.FindGameObjectsWithTag("Cursor")[0].GetComponent<Cursor>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
