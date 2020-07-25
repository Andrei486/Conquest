using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Menus{
    public class Menu : MonoBehaviour
    {
        public GameObject menuItemPrefab;
        public Type cursorType;
        public int columnHeight;
        private Vector3 offset;
        private GameObject cursorPrefab;
        private GameObject cursor;
        private MenuCursor menuCursor;
        private float menuItemStackHeight;
        private float menuItemRowWidth;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Setup(GameObject menuItemPrefab, Type cursorType, int columnHeight){

            this.menuItemPrefab = menuItemPrefab;
            this.cursorType = cursorType;
            this.columnHeight = columnHeight;
            this.cursorPrefab = PrefabManager.GetPrefabs().menuCursorPrefab;

            menuItemStackHeight = menuItemPrefab.GetComponent<RectTransform>().rect.height * menuItemPrefab.transform.localScale.y * 1.1f;
            menuItemRowWidth = menuItemPrefab.GetComponent<RectTransform>().rect.width * menuItemPrefab.transform.localScale.x * 1.1f;

            //set up cursor for showing controls
            cursor = Instantiate(cursorPrefab, GameObject.FindWithTag("MainCanvas").transform);
            offset = new Vector3(cursor.GetComponent<RectTransform>().rect.width * this.gameObject.transform.localScale.x * -1.1f, 0f, 0f);
            menuCursor = (MenuCursor) cursor.AddComponent(cursorType);
        }

        public void Generate(IEnumerable<string> names){
            int row = 0;
            int column = 0;
            this.GetComponent<RectTransform>().position -= offset;
            foreach (string name in names){
                GameObject item = Instantiate(menuItemPrefab, gameObject.transform);
                item.name = name;
                item.transform.Translate(new Vector3(menuItemRowWidth * column, menuItemStackHeight * row, 0f)); //stack items properly
                item.transform.Find("Name").GetComponent<Text>().text = name;
                row++;
                //start a new column if at max number of items
                if (columnHeight != -1 && row == columnHeight){
                    row = 0;
                    column++;
                }
            }
            menuCursor.LinkMenu(this.gameObject);
        }

        public void Toggle(bool enabled){
            foreach (Transform child in transform){
                child.gameObject.SetActive(enabled);
            }
            GetComponent<Renderer>().enabled = enabled;
            menuCursor.locked = enabled;
            if (enabled){
                menuCursor.LinkMenu(this.gameObject);
            } else {
                menuCursor.UnlinkMenu();
            }
        }

        public static GameObject Create(GameObject menuItemPrefab, Type cursorType, int columnHeight = -1){
            GameObject obj = new GameObject("Menu");
            obj.transform.parent = GameObject.FindWithTag("MainCanvas").transform;
            obj.AddComponent<RectTransform>().pivot = Vector2.zero;
            Menu menu = obj.AddComponent<Menu>();
            menu.Setup(menuItemPrefab, cursorType, columnHeight);
            return obj;
        }

        public void Destroy(){
            menuCursor.UnlinkMenu();
            Destroy(cursor);
            Destroy(this.gameObject);
        }
    }
}