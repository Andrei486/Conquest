using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public sealed class ControlsManager
{
    static ControlsManager controls;
    //constants for directional control modes
    public const int WASD = 0;
    public const int ARROWS = 1;

    Dictionary<Command, KeyCode> keyMappings;

    public static ControlsManager GetControls(){
        /**Returns the single instance of ControlsManager, creating one if necessary.*/
        if (ControlsManager.controls == null){
            ControlsManager.controls = new ControlsManager();
        }
        return controls;
    }

    private ControlsManager(){
        //set up key mappings
        keyMappings = new Dictionary<Command, KeyCode>();
        keyMappings.Add(Command.CONFIRM, KeyCode.Space);
        keyMappings.Add(Command.BACK, KeyCode.Backspace);
        keyMappings.Add(Command.MENU, KeyCode.X);
        keyMappings.Add(Command.TOGGLE_INFO, KeyCode.RightControl);
        this.SetCameraControls(ControlsManager.WASD);
        this.SetMoveControls(ControlsManager.ARROWS);
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
        if (keyMappings.Keys.Contains(command)){ //if the command actually exists
            if (!keyMappings.Values.Contains(newKey)){ //if the key isn't already used
                keyMappings.Add(command, newKey);
            } else {
                Debug.Log(string.Format("The key {0} is in use", newKey));
            }
        } else {
            Debug.Log("The command stated does not exist.");
        }
    }

    public KeyCode GetCommand(Command command){
        return keyMappings[command];
    }
}

public enum Command{
    CONFIRM, MENU, BACK,
    TOGGLE_INFO,
    CAMERA_UP, CAMERA_DOWN, CAMERA_LEFT, CAMERA_RIGHT,
    MOVE_UP, MOVE_DOWN, MOVE_LEFT, MOVE_RIGHT
}