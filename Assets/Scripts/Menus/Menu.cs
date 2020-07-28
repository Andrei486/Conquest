using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Menus{
    public class Menu : MonoBehaviour
    {
        public Type cursorType;
        public Vector3 offset;
        private GameObject cursorPrefab;
        private GameObject cursorObj;
        private MenuCursor menuCursor;
        public MenuElement root;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Setup(MenuElement root, GameObject menuCursorPrefab, Type cursorType){

            this.cursorType = cursorType;
            this.cursorPrefab = menuCursorPrefab;

            //set up cursor for showing controls
            cursorObj = Instantiate(cursorPrefab, GameObject.FindWithTag("MainCanvas").transform);
            offset = new Vector3(cursorObj.GetComponent<RectTransform>().rect.width * this.gameObject.transform.localScale.x * -1.1f, 0f, 0f);
            menuCursor = (MenuCursor) cursorObj.AddComponent(cursorType);
        }

        public void Destroy(){
            Destroy(cursorObj);
            Destroy(this.gameObject);
        }
    }
}