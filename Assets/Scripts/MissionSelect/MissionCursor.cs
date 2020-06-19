using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Objects;

namespace MissionSelect{
    public class MissionCursor : MenuCursor
    {
        ControlsManager controls;
		MissionManager missions;
        // Start is called before the first frame update
        protected override void Start()
        {
            controls = ControlsManager.GetControls();
			missions = MissionManager.GetManager();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
			if (this.menu != null){
				if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_DOWN))){
					MoveDown();
				}
				if (Input.GetKeyDown(controls.GetCommand(Command.MOVE_UP))){
					MoveUp();
				}
				if (Input.GetKeyDown(controls.GetCommand(Command.CONFIRM))){
					SelectItem(currentItem);
				}
			}
        }

        override public void SelectItem(GameObject item){
            Mission selected = (Mission) missions.FindByName(item.transform.Find("Name").GetComponent<Text>().text);
			missions.StartMission(selected);
	    }
    }
}