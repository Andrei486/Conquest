using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objects;

namespace InBattle{	
	public class Camera : MonoBehaviour
	{
		public Cursor cursor;
		public float timeToRotate = 0.2f;
		public bool rotating = false;
		float zAngle = 30.0f;
		ControlsManager controls;
		// Start is called before the first frame update
		void Start()
		{
			cursor = GameObject.FindGameObjectsWithTag("Cursor")[0].GetComponent<Cursor>();
			controls = ControlsManager.GetControls();
		}

		// Update is called once per frame
		void Update()
		{
			if (cursor.rotationLocked){
				return;
			}
			if (cursor.moving || this.rotating || cursor.movedTemporary){ //to prevent camera going off-center, do not turn if already moving or turning
				return;
			}
			Vector3 rotationPivot = cursor.transform.position;
			if(Input.GetKeyDown(controls.GetCommand(Command.CAMERA_LEFT))){
				StartCoroutine(GradualRotation(rotationPivot, Vector3.up, 90.0f));
			}
			if(Input.GetKeyDown(controls.GetCommand(Command.CAMERA_RIGHT))){
				StartCoroutine(GradualRotation(rotationPivot, Vector3.down, 90.0f));
			}
			if(Input.GetKeyDown(controls.GetCommand(Command.CAMERA_DOWN))){
				if (zAngle >= 15.0f){
					StartCoroutine(GradualRotation(rotationPivot, Vector3.Cross(this.transform.position - rotationPivot, Vector3.down), 15.0f));
				zAngle -= 15.0f;
				}
			}
			if(Input.GetKeyDown(controls.GetCommand(Command.CAMERA_UP))){
				if (zAngle <= 60.0f){
					StartCoroutine(GradualRotation(rotationPivot, Vector3.Cross(this.transform.position - rotationPivot, Vector3.up), 15.0f));
				zAngle += 15.0f;
				}
			}
		}
		
		public IEnumerator GradualRotation(Vector3 point, Vector3 axis, float angle){
			/**Rotates camera by angle degrees around axis with constant speed.*/
			//assuming framerate remains (approximately) constant
			rotating = true;
			float rotationPerFrame = angle / timeToRotate * Time.deltaTime;
			float totalRotation = 0f;
			
			while (totalRotation < angle - 0.5f){ //it is better to undershoot so error correction is less noticeable
				totalRotation += rotationPerFrame;
				this.gameObject.transform.RotateAround(point, axis, rotationPerFrame);
				yield return new WaitForEndOfFrame();
			}
			
			this.gameObject.transform.RotateAround(point, axis, angle - totalRotation); //account for errors produced
			
			rotating = false;
		}
	}
}