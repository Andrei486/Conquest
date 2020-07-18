using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using InBattle;

public class ControlsManager: MonoBehaviour
{
    static ControlsManager controls;
    public GameObject menuItem;
    public GameObject board;
    public bool showingControls = false;
    public bool showingPopup = false;
    public const int COLUMN_HEIGHT = 10;
    float menuItemStackHeight;
    float menuItemRowWidth;
    //constants for directional control modes
    public const int WASD = 0;
    public const int ARROWS = 1;
    GameObject cursorPrefab;
    GameObject menuCursor;
    GameObject controlsMenu;
    public Dictionary<Command, KeyCode> keyMappings;
    List<KeyCode> validCodes;
    public bool canShowMenu = true;

    void Start(){
        KeyCode[] allKeys = (KeyCode[]) Enum.GetValues(typeof(KeyCode));
        validCodes = new List<KeyCode>(allKeys); //for now all keys are valid
        cursorPrefab = PrefabManager.GetPrefabs().menuCursorPrefab;
        menuItemStackHeight = menuItem.GetComponent<RectTransform>().rect.height * menuItem.transform.localScale.y * 1.1f;
        menuItemRowWidth = menuItem.GetComponent<RectTransform>().rect.width * menuItem.transform.localScale.x * 1.1f;

        //set up cursor for showing controls
        menuCursor = Instantiate(cursorPrefab, this.transform);
        menuCursor.AddComponent(typeof(ControlsMenuCursor));

        controlsMenu = GameObject.FindWithTag("MainCanvas").transform.Find("Controls Menu").gameObject;
        

        //load key mappings
        LoadControls();
    }

    void Update(){
        if (!showingPopup){
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
        }
        
        if (showingControls && !showingPopup){
            if (Input.GetKeyDown(GetCommand(Command.MOVE_DOWN))){
                menuCursor.GetComponent<MenuCursor>().MoveDown();
            }
            if (Input.GetKeyDown(GetCommand(Command.MOVE_UP))){
                menuCursor.GetComponent<MenuCursor>().MoveUp();
            }
        }

        if (!showingControls && Input.GetKeyDown(GetCommand(Command.RESET_CONTROLS))){
            keyMappings = DefaultControls();
        }
    }

    public void SetCameraControls(Dictionary<Command, KeyCode> mappings, int mode){
        /**Set the camera controls to the specified mode.*/
        if (mode == ControlsManager.WASD){
            mappings.Add(Command.CAMERA_UP, KeyCode.W);
            mappings.Add(Command.CAMERA_DOWN, KeyCode.S);
            mappings.Add(Command.CAMERA_LEFT, KeyCode.A);
            mappings.Add(Command.CAMERA_RIGHT, KeyCode.D);
        } else {
            mappings.Add(Command.CAMERA_UP, KeyCode.UpArrow);
            mappings.Add(Command.CAMERA_DOWN, KeyCode.DownArrow);
            mappings.Add(Command.CAMERA_LEFT, KeyCode.LeftArrow);
            mappings.Add(Command.CAMERA_RIGHT, KeyCode.RightArrow);
        }
    
    }
    public void SetMoveControls(Dictionary<Command, KeyCode> mappings, int mode){
        /**Set the movement controls to the specified mode.*/
        if (mode == ControlsManager.WASD){
            mappings.Add(Command.MOVE_UP, KeyCode.W);
            mappings.Add(Command.MOVE_DOWN, KeyCode.A);
            mappings.Add(Command.MOVE_LEFT, KeyCode.S);
            mappings.Add(Command.MOVE_RIGHT, KeyCode.D);
        } else {
            mappings.Add(Command.MOVE_UP, KeyCode.UpArrow);
            mappings.Add(Command.MOVE_DOWN, KeyCode.DownArrow);
            mappings.Add(Command.MOVE_LEFT, KeyCode.LeftArrow);
            mappings.Add(Command.MOVE_RIGHT, KeyCode.RightArrow);
        }
    }

    public Dictionary<Command, KeyCode> DefaultControls(){
        Dictionary<Command, KeyCode> mappings = new Dictionary<Command, KeyCode>();
        mappings.Add(Command.CONFIRM, KeyCode.Space);
        mappings.Add(Command.BACK, KeyCode.Backspace);
        mappings.Add(Command.MENU, KeyCode.X);
        mappings.Add(Command.QUICKSAVE, KeyCode.Equals);
        mappings.Add(Command.QUICKLOAD, KeyCode.Minus);
        mappings.Add(Command.TOGGLE_INFO, KeyCode.RightControl);
        mappings.Add(Command.RESET_CONTROLS, KeyCode.R);
        mappings.Add(Command.TOGGLE_GRID, KeyCode.G);
        mappings.Add(Command.ZOOM_IN, KeyCode.N);
        mappings.Add(Command.ZOOM_OUT, KeyCode.M);
        this.SetCameraControls(mappings, ControlsManager.WASD);
        this.SetMoveControls(mappings, ControlsManager.ARROWS);
        return mappings;
    }

    public void LoadControls(){
        string controlsData = Application.persistentDataPath + "/SaveData/controlsData.txt";
        string json = File.ReadAllText(controlsData);
        keyMappings = JsonConvert.DeserializeObject<Dictionary<Command, KeyCode>>(json);
        
        //fill in any missing commands
        Dictionary<Command, KeyCode> defaults = DefaultControls();
        foreach (Command command in defaults.Keys){
            if (!keyMappings.ContainsKey(command)){
                keyMappings.Add(command, defaults[command]);
            }
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
        int row = 0;
        int column = 0;
        foreach (Command command in keyMappings.Keys){
            GameObject item = Instantiate(menuItem, controlsMenu.transform);
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
        BoardManager.GetBoard().locked = true;
        CreateControlsMenu();
        menuCursor.GetComponent<MenuCursor>().LinkMenu(controlsMenu);
        showingControls = true;
    }
    public void HideControlsMenu(){
        BoardManager.GetBoard().locked = false;
        menuCursor.GetComponent<MenuCursor>().UnlinkMenu();
        foreach (Transform item in controlsMenu.transform){
            Destroy(item.gameObject);
        }
        showingControls = false;
        SaveManager.GetSaveManager().SaveControls(); //save the controls whenever the menu is closed.
    }

    public IEnumerator SetCommandPopup(Command command){
        menuCursor.GetComponent<MenuCursor>().locked = true;
        this.showingPopup = true;
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
        this.showingPopup = false;
        menuCursor.GetComponent<MenuCursor>().locked = false;
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
    [Description("Reset Controls")] RESET_CONTROLS,
    [Description("Show/Hide Grid")] TOGGLE_GRID,
    [Description("Quicksave")] QUICKSAVE, [Description("Quickload")] QUICKLOAD,
    [Description("Cam. Tilt Up")]CAMERA_UP, [Description("Cam. Tilt Down")]CAMERA_DOWN,
    [Description("Cam. Turn Left")] CAMERA_LEFT, [Description("Cam. Turn Right")] CAMERA_RIGHT,
    [Description("Move Up")] MOVE_UP, [Description("Move Down")] MOVE_DOWN,
    [Description("Move Left")] MOVE_LEFT, [Description("Move Right")] MOVE_RIGHT,
    [Description("Zoom In")] ZOOM_IN, [Description("Zoom Out")] ZOOM_OUT, 
}