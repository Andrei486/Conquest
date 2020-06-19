using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Objects;


namespace InBattle
{
	[Serializable]
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
			board = BoardManager.GetBoard();
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
				Debug.Log(newPosition);
				if (board.IsWithinBounds(newPosition) && board.GetSpace(newPosition).occupyingUnit != null){
					UseAttack(attack, board.GetSpace(newPosition).occupyingUnit.GetComponent<PlayerController>().health, direction);
				}
			}
			BoardManager.GetBoard().RemoveDeadUnits();
			BoardManager.GetBoard().CheckEndMission();
		}
		
		void UseAttack(Attack attack, Health target, Quaternion direction){
			System.Random rng = new System.Random();
			float effectiveAccuracy = attack.CalculateHitChance(this, target);
			if (rng.NextDouble() >= effectiveAccuracy / 100.0f){ //attack misses
				BattleLog.GetLog().Log("The attack missed " + target.pc.name + "...");
				Debug.Log("attack missed");
				return;
			}
			float damageDealt = attack.CalculateDamage(this, target);

			target.currentHealth -= damageDealt;
			BattleLog.GetLog().Log("Dealt " + damageDealt + " damage to " + target.pc.name);
			Debug.Log("damage dealt");
			
			if (attack.knockbackPosition != Vector2.zero){
				Vector2 knockbackPosition = target.boardPosition + (Vector2) (direction * (attack.knockbackPosition - attack.targetPosition));
				board.KnockbackMoveUnit(board.GetSpace(target.boardPosition), board.GetSpace(knockbackPosition), false);
			}
		}
	}
}