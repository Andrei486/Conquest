using System.Collections;
using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsManager: MonoBehaviour
{
    static ControlsManager controls;
    public GameObject menuItem;
    public GameObject board;
    public bool showingControls = false;
    public const int COLUMN_HEIGHT = 10;
    float menuItemStackHeight;
    float menuItemRowWidth;
    //constants for directional control modes
    public const int WASD = 0;
    public const int ARROWS = 1;
    public GameObject cursorPrefab;
    GameObject menuCursor;
    Dictionary<Command, KeyCode> keyMappings;
    List<KeyCode> validCodes;

    void Start(){
        KeyCode[] allKeys = (KeyCode[]) Enum.GetValues(typeof(KeyCode));
        validCodes = new List<KeyCode>(allKeys); //for now all keys are valid
        menuItemStackHeight = menuItem.GetComponent<RectTransform>().rect.height * menuItem.transform.localScale.y * 1.1f;
        menuItemRowWidth = menuItem.GetComponent<RectTransform>().rect.width * menuItem.transform.localScale.x * 1.1f;

        //set up cursor for showing controls
        menuCursor = Instantiate(cursorPrefab, this.transform);
        menuCursor.AddComponent(typeof(ControlsMenuCursor));

        //set up key mappings
        keyMappings = new Dictionary<Command, KeyCode>();
        keyMappings.Add(Command.CONFIRM, KeyCode.Space);
        keyMappings.Add(Command.BACK, KeyCode.Backspace);
        keyMappings.Add(Command.MENU, KeyCode.X);
        keyMappings.Add(Command.QUICKSAVE, KeyCode.Equals);
        keyMappings.Add(Command.QUICKLOAD, KeyCode.Minus);
        keyMappings.Add(Command.TOGGLE_INFO, KeyCode.RightControl);
        this.SetCameraControls(ControlsManager.WASD);
        this.SetMoveControls(ControlsManager.ARROWS);
    }

    void Update(){
        if (Input.GetKeyDown(GetCommand(Command.MENU))){
            if (!showingControls){
                ShowControlsMenu();
            }
            else {
                HideControlsMenu();
            }
        }
        if (Input.GetKeyDown(GetCommand(Command.BACK))){
            HideControlsMenu();
        }
        if (showingControls && !menuCursor.GetComponent<MenuCursor>().locked){
            if (Input.GetKeyDown(GetCommand(Command.MOVE_DOWN))){
                menuCursor.GetComponent<MenuCursor>().MoveDown();
            }
            if (Input.GetKeyDown(GetCommand(Command.MOVE_UP))){
                menuCursor.GetComponent<MenuCursor>().MoveUp();
            }
        }
    }

    public void SetCameraControls(int mode){
        /**Set the camera controls to the specified mode.*/
        if (mode == ControlsManager.WASD){
            keyMappings.Add(Command.CAMERA_UP, KeyCode.W);
            keyMappings.Add(Command.CAMERA_DOWN, KeyCode.S);
            keyMappings.Add(Command.CAMERA_LEFT, KeyCode.A);
            keyMappings.Add(Command.CAMERA_RIGHT, KeyCode.D);
        } else {
            keyMappings.Add(Command.CAMERA_UP, KeyCode.UpArrow);
            keyMappings.Add(Command.CAMERA_DOWN, KeyCode.DownArrow);
            keyMappings.Add(Command.CAMERA_LEFT, KeyCode.LeftArrow);
            keyMappings.Add(Command.CAMERA_RIGHT, KeyCode.RightArrow);
        }
       
    }
    public void SetMoveControls(int mode){
        /**Set the movement controls to the specified mode.*/
        if (mode == ControlsManager.WASD){
            keyMappings.Add(Command.MOVE_UP, KeyCode.W);
            keyMappings.Add(Command.MOVE_DOWN, KeyCode.A);
            keyMappings.Add(Command.MOVE_LEFT, KeyCode.S);
            keyMappings.Add(Command.MOVE_RIGHT, KeyCode.D);
        } else {
            keyMappings.Add(Command.MOVE_UP, KeyCode.UpArrow);
            keyMappings.Add(Command.MOVE_DOWN, KeyCode.DownArrow);
            keyMappings.Add(Command.MOVE_LEFT, KeyCode.LeftArrow);
            keyMappings.Add(Command.MOVE_RIGHT, KeyCode.RightArrow);
        }
    }

    public void SetCommand(Command command, KeyCode newKey){
        /**Sets the key for the specified command to newKey, if the command exists and newKey is not in use.!--*/
        if (keyMappings.Keys.Contains(command)){ //if the command actually exists
            if (!keyMappings.Values.Contains(newKey)){ //if the key isn't already used: cannot map one key to multiple
                keyMappings[command] = newKey;
            } else {
                Debug.Log(string.Format("The key {0} is in use.", newKey));
            }
        } else {
            Debug.Log("The command specified does not exist.");
        }
    }

    public KeyCode GetCommand(Command command){
        return keyMappings[command];
    }

    public static ControlsManager GetControls(){
        /**Returns the single instance of ControlsManager, creating one if necessary.*/
        return GameObject.FindWithTag("Global").GetComponent<ControlsManager>();
    }

    public void CreateControlsMenu(){
        GameObject menu = this.transform.GetChild(0).gameObject;
        int row = 0;
        int column = 0;
        foreach (Command command in keyMappings.Keys){
            GameObject item = Instantiate(menuItem, menu.transform);
            item.transform.Translate(new Vector3(menuItemRowWidth * column, menuItemStackHeight * row, 0f)); //stack items properly
            item.transform.Find("Command Name").GetComponent<Text>().text = ControlsManager.GetCommandName(command);
            item.transform.Find("Current Keybind").GetComponent<Text>().text = keyMappings[command].ToString();
            row++;
            //start a new column if at max number of items
            if (row == COLUMN_HEIGHT){
                row = 0;
                column++;
            }
        }
    }

    public void ShowControlsMenu(){
        BoardManager.SetLock(true);
        CreateControlsMenu();
        menuCursor.GetComponent<MenuCursor>().LinkMenu(this.transform.GetChild(0).gameObject);
        showingControls = true;
    }
    public void HideControlsMenu(){
        BoardManager.SetLock(false);
        menuCursor.GetComponent<MenuCursor>().UnlinkMenu();
        foreach (Transform item in this.transform.GetChild(0)){
            Destroy(item.gameObject);
        }
        showingControls = false;
    }

    public IEnumerator SetCommandPopup(Command command){
        menuCursor.GetComponent<MenuCursor>().locked = true;
        GameObject popup = Instantiate(menuItem, this.transform);
        popup.transform.Find("Command Name").GetComponent<Text>().text = "Enter new key";
        popup.transform.Find("Current Keybind").GetComponent<Text>().text = "";
        yield return null;
        while (!Input.anyKeyDown){
            yield return null;
        }
        foreach (KeyCode key in validCodes){
            if (Input.GetKeyDown(key)){
                SetCommand(command, key);
                break;
            }
        }
        Destroy(popup);
        menuCursor.SetActive(true);
        //update the menu
        HideControlsMenu();
        ShowControlsMenu();
    }

    public static string GetCommandName(Command command){
        FieldInfo fi = command.GetType().GetField(command.ToString());

        DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

        if (attributes.Any())
        {
            return attributes.First().Description;
        }

        return command.ToString();
    }

    public static Command? GetCommandByName(string name){
        foreach (Command command in GetControls().keyMappings.Keys){
            FieldInfo fi = command.GetType().GetField(command.ToString());
            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            if (attributes.First().Description == name){
                return command;
            }
        }
        return null;
    }
}

public enum Command{
    
    [Description("Select/Confirm")] CONFIRM,
    [Description("Open Menu")] MENU,
    [Description("Back/Cancel")] BACK,
    [Description("Show/Hide Details")] TOGGLE_INFO,
    [Description("Quicksave")] QUICKSAVE, [Description("Quickload")] QUICKLOAD,
    [Description("Cam. Tilt Up")]CAMERA_UP, [Description("Cam. Tilt Down")]CAMERA_DOWN,
    [Description("Cam. Turn Left")] CAMERA_LEFT, [Description("Cam. Turn Right")] CAMERA_RIGHT,
    [Description("Move Up")] MOVE_UP, [Description("Move Down")] MOVE_DOWN,
    [Description("Move Left")] MOVE_LEFT, [Description("Move Right")] MOVE_RIGHT
}