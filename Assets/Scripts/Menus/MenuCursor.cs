using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Menus{
	public class MenuCursor : MonoBehaviour
	{
		public Menu root;
		public MenuElement activeElement;
		public MenuElement hoveredElement{
			get{
				return hoveredElement;
			}
			set{
				hoveredElement.OnUnhover(this);
				hoveredElement = value;
				if (hoveredElement != null){
					hoveredElement.OnHover(this); //when hovered element changes, automatically call OnHover
				}
			}
		}
		public bool locked = false;
		[SerializeField] Vector3 offset;
		private ControlsManager controls;
		virtual protected void Start()
		{
			controls = ControlsManager.GetControls();
		}

		// Update is called once per frame
		virtual protected void Update()
		{
			if (this.locked){
				return;
			} else {
				//move the cursor if the correct keys are pressed
				if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_UP))){
					HoverItem(hoveredElement.up);
				}
				if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_DOWN))){
					HoverItem(hoveredElement.down);
				}
				if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_LEFT))){
					HoverItem(hoveredElement.left);
				}
				if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_RIGHT))){
					HoverItem(hoveredElement.right);
				}
			}
		}
		
		virtual public void Setup(Menu root){
			this.root = root;
			this.activeElement = root.GetComponent<MenuElement>();
			HoverItem(0);
		}

		virtual public void Destroy(){
			/**Called when menu is destroyed to free up or unlock anything that was locked by the menu.!--*/
		}
		
		virtual protected void HoverItem(MenuElement element){
			/**Moves the cursor to hover over element, unless element is null.!--*/
			if (element == null){
				return;
			}
			this.hoveredElement = element;
			this.transform.position = element.transform.position + offset;
		}
		
		protected void HoverItem(int childNumber){
			MenuElement firstValid = null;
			foreach (Transform child in activeElement.transform){
				if (child.GetComponent<MenuElement>() != null){
					firstValid = child.GetComponent<MenuElement>();
					break;
				}
			}
			HoverItem(firstValid);
		}
		
		virtual public void SelectItem(GameObject item){
			
		}
		
		protected void ResetItem(){
			StartCoroutine(Reset());
		}
		
		IEnumerator Reset(){
			Setup(root);
			yield return new WaitForEndOfFrame();
		}

		// protected IEnumerator SetupMenu(GameObject item, List<GameObject> attributes, float time){
		// 	float elapsedTime = 0;
		// 	float horizontalOffset = item.GetComponent<RectTransform>().rect.size.x * item.transform.localScale.x * 1.1f;
		// 	float horizontalSpeed = horizontalOffset / time;
		// 	float upwardsSpeed = menuController.attributeStackHeight / time;

		// 	foreach (GameObject attribute in attributes){
		// 		attribute.transform.SetAsFirstSibling(); //render below emblem and text
		// 	}
		// 	//first move everything horizontally
		// 	while (elapsedTime < time){
		// 		if (currentItem != item){ //if a different item is selected, stop
		// 			yield break;
		// 		}
		// 		foreach (GameObject attribute in attributes){
		// 			attribute.transform.Translate(new Vector3(horizontalSpeed * Time.deltaTime, 0, 0));
		// 		}
		// 		elapsedTime += Time.deltaTime;
		// 		yield return new WaitForEndOfFrame();
		// 	}
		// 	//then vertically
		// 	elapsedTime = 0;
		// 	while (elapsedTime < time){
		// 		if (currentItem != item){ //if a different item is selected, stop
		// 			yield break;
		// 		}
		// 		int i = 0;
		// 		foreach (GameObject attribute in attributes){
		// 			attribute.transform.Translate(new Vector3(0, upwardsSpeed * Time.deltaTime * i, 0));
		// 			i++;
		// 		}
		// 		elapsedTime += Time.deltaTime;
		// 		yield return new WaitForEndOfFrame();
		// 	}
		// }

		// protected void ScaleCorrection(GameObject item, GameObject attribute){
		// 	float x = (float) 1.0 / item.transform.localScale.x;
		// 	float y = (float) 1.0 / item.transform.localScale.y;
		// 	float z = (float) 1.0 / item.transform.localScale.z;
		// 	Vector3 inverseScale = new Vector3(x, y, z);
		// 	attribute.transform.localScale = Vector3.Scale(inverseScale, attribute.transform.localScale);
		// }
	}
}