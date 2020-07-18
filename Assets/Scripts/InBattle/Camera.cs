using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objects;

namespace InBattle{	
	public class Camera : MonoBehaviour
	{
		public Cursor cursor;
		public float timeToRotate = 0.2f;
		public float timeToZoom = 0.1f;
		public float zoomStep = 0.25f;
		public bool locked{
			get{
				return rotating || zooming;
			}
		}
		private bool rotating = false;
		private bool zooming = false;
		int zAngle = 30; //degrees
		int currentZoom = 100; //percentage
		ControlsManager controls;
		private float defaultZoomScale;
		// Start is called before the first frame update
		void Start()
		{
			cursor = GameObject.FindGameObjectsWithTag("Cursor")[0].GetComponent<Cursor>();
			controls = ControlsManager.GetControls();
			defaultZoomScale = transform.localPosition.magnitude;
		}

		// Update is called once per frame
		void Update()
		{
			if (cursor.rotationLocked){
				return;
			}
			if (cursor.moving || this.locked || cursor.movedTemporary){ //to prevent camera going off-center, do not turn if already moving or turning
				return;
			}
			Vector3 rotationPivot = cursor.transform.position;
			if (Input.GetKeyDown(controls.GetCommand(Command.CAMERA_LEFT))){
				StartCoroutine(GradualRotation(rotationPivot, Vector3.up, 90));
			}
			if (Input.GetKeyDown(controls.GetCommand(Command.CAMERA_RIGHT))){
				StartCoroutine(GradualRotation(rotationPivot, Vector3.down, 90));
			}
			if (Input.GetKeyDown(controls.GetCommand(Command.CAMERA_DOWN))){
				if (zAngle >= 15){
					StartCoroutine(GradualRotation(rotationPivot, Vector3.Cross(this.transform.position - rotationPivot, Vector3.down), 15));
					zAngle -= 15;
				}
			}
			if (Input.GetKeyDown(controls.GetCommand(Command.CAMERA_UP))){
				if (zAngle <= 60){
					StartCoroutine(GradualRotation(rotationPivot, Vector3.Cross(this.transform.position - rotationPivot, Vector3.up), 15));
					zAngle += 15;
				}
			}

			if (Input.GetKeyDown(controls.GetCommand(Command.ZOOM_IN))){
				if (currentZoom < 200){
					StartCoroutine(Zoom((currentZoom + 25f) / (float) currentZoom));
					currentZoom += 25;
				}
			}

			if (Input.GetKeyDown(controls.GetCommand(Command.ZOOM_OUT))){
				if (currentZoom > 25){
					StartCoroutine(Zoom((currentZoom - 25f) / (float) currentZoom));
					currentZoom -= 25;
				}
			}
		}
		
		public IEnumerator GradualRotation(Vector3 point, Vector3 axis, int angle){
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

		public IEnumerator Zoom(float zoomFactor){
			Debug.Log(zoomFactor);
			zooming = true;
			Vector3 endPosition = zoomFactor * transform.localPosition;
			Vector3 startPosition = transform.localPosition;
			float t = 0;
			
			while (t < 1){
				t += Time.deltaTime / timeToZoom;
				transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
				yield return new WaitForEndOfFrame();
			}

			zooming = false;
			yield return null;
		}
	}
}