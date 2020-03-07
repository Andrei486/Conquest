using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Objects;

public class Health : MonoBehaviour
{
	public float maxHealth;
	public float currentHealth;
	public int level;
	public float attackPower;
	public float defense;
	BoardManager board;
	PlayerController pc;
	Vector2 boardPosition;
    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.FindGameObjectsWithTag("Board")[0].GetComponent<BoardManager>();
		pc = this.gameObject.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        this.boardPosition = pc.boardPosition;
    }
	
	public void UseSkill(Skill skill, Quaternion direction){
		/**Makes this unit use the specified skill in the chosen direction.
		Assumes that the skill command is valid.*/
		board.SkillMoveUnit(skill, board.GetSpace(boardPosition), direction);
		Vector2 newPosition;
		foreach (Attack attack in skill.attacks){
			newPosition = boardPosition + (Vector2) (direction * new Vector3(attack.targetPosition.x, attack.targetPosition.y, 0));
			if (board.IsWithinBounds(newPosition) && board.GetSpace(newPosition).occupyingUnit != null){
				UseAttack(attack, board.GetSpace(newPosition).occupyingUnit.GetComponent<PlayerController>().health, direction);
			}
		}
		
	}
	
	void UseAttack(Attack attack, Health target, Quaternion direction){
		System.Random rng = new System.Random();
		float effectiveAccuracy = attack.accuracy;
		if (rng.NextDouble() >= effectiveAccuracy / 100.0f){ //attack misses
			return;
		}
		float damageDealt;
		float totalOffense = attack.basePower /100 * this.attackPower;
		float totalDefense = target.defense;
		damageDealt = (1 + level / 10f) * totalOffense/totalDefense;

		target.currentHealth -= damageDealt;
		Debug.Log("Dealt " + damageDealt + " damage to " + target.pc.name);
		
		if (attack.knockbackPosition != Vector2.zero){
			Vector2 knockbackPosition = target.boardPosition + (Vector2) (direction * (attack.knockbackPosition - attack.targetPosition));
			board.KnockbackMoveUnit(board.GetSpace(target.boardPosition), board.GetSpace(knockbackPosition), false);
		}
	}
}
