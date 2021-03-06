﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Objects;
using Newtonsoft.Json;

namespace InBattle
{
	[Serializable] [JsonObject(MemberSerialization.OptIn)]
	public class Health : MonoBehaviour
	{
		[JsonProperty] public float maxHealth;
		[JsonProperty] public float currentHealth;
		[JsonProperty] public int level;
		[JsonProperty] public float attackPower;
		[JsonProperty] public float defense;
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
				newPosition = boardPosition + attack.targetPosition.Rotate(direction.eulerAngles.y);
				Debug.Log(newPosition);
				if (board.IsWithinBounds(newPosition) && board.GetSpace(newPosition).occupyingUnit != null){
					UseAttack(attack, board.GetSpace(newPosition).occupyingUnit.GetComponent<PlayerController>().health, direction);
				}
			}
			BoardManager.GetBoard().RemoveDeadUnits();
			BoardManager.GetBoard().CheckEndMission();
		}
		
		void UseAttack(Attack attack, Health target, Quaternion direction){
			/**Applies the effect of the given attack in the given direction, to the chosen target.!--*/
			System.Random rng = new System.Random();
			float damageDealt = attack.CalculateAverageDamage(this, target);
			if (damageDealt > 0){
				target.pc.Aggressive = true; //attacking a target makes it aggressive
			}
			//check whether attack lands
			float effectiveAccuracy = attack.CalculateHitChance(this, target);
			if (rng.NextDouble() >= effectiveAccuracy / 100.0f){ //attack misses
				BattleLog.GetLog().Log("The attack missed " + target.pc.unitName + "...");
				Debug.Log("attack missed");
				return;
			}
			double damageModifier = (1.0 - Attack.RANDOM_VARIANCE) + (rng.NextDouble() * 2 * Attack.RANDOM_VARIANCE);
			target.currentHealth -= damageDealt * (float) damageModifier;
			BattleLog.GetLog().Log("Dealt " + damageDealt + " damage to " + target.pc.unitName);
			Debug.Log("damage dealt");
			
			if (attack.knockbackPosition != Vector2.zero){
				Vector2 knockbackPosition = target.boardPosition + attack.knockbackPosition.Rotate(direction.eulerAngles.y);
				board.KnockbackMoveUnit(board.GetSpace(target.boardPosition), board.GetSpace(knockbackPosition), false);
			}
		}
	}
}