using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float attackPower;
	public float defense;
	public float jumpHeight;
	public int moveRange;
	public int maxBullets;
	public int bullets;
	BoardManager board;
	
    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.FindGameObjectsWithTag("BoardManager")[0].GetComponent<BoardManager>();
		MakeSemiTransparent(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public Vector2[] GetAccessibleSpaces(){
		return null;
	}
	
	public void MakeSemiTransparent(bool enabled){
		Color color = this.gameObject.GetComponent<MeshRenderer>().material.color;
		if (enabled){
			color.a = 0.5f;
		} else {
			color.a = 1.0f;
		}
		this.gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
	}
}
