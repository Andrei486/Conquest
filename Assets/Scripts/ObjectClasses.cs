using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverters;

namespace Objects
{

    /**Represents a single space on the game board.*/
	[Serializable]
    public class BoardSpace {
		[JsonConverter(typeof(VectorConverter))]
		public Vector2 boardPosition;
		[JsonConverter(typeof(VectorConverter))]
		public Vector3 anchorPosition;
		[JsonIgnore]
		public GameObject occupyingUnit;
		public string spriteName;
		public int moveCost = 1;
		public const float BOARD_SIZE = 2.0f;
		public bool impassable = false;
		public bool voidSpace = false;
		public float GetHeight(){
			return this.anchorPosition.y;
		}
	}
	
	[Serializable]
	public class Attack{
		
		public float basePower;
		public float accuracy;
		//relative to user, in board space, when user faces upwards.
		[JsonConverter(typeof(VectorConverter))]
		public Vector2 targetPosition = new Vector2(0, 0);
		[JsonConverter(typeof(VectorConverter))]
		public Vector2 knockbackPosition = new Vector2(0, 0);

		public float CalculateDamage(Health user, Health target){
			/**Returns the damage that would be dealt on a successful hit by this attack,
			used by the unit user on the unit target.*/
			float damageDealt;
			float totalOffense = basePower /100 * user.attackPower;
			float totalDefense = target.defense;
			damageDealt = (1 + user.level / 10f) * totalOffense/totalDefense;
			return damageDealt;
		}
		public float CalculateHitChance(Health user, Health target){
			/**Returns the effective hit chance of this attack, used by the unit user
			on the unit target, which is standing on the space targetSpace.*/
			float effectiveAccuracy = accuracy;
			return Math.Min(100.0f, effectiveAccuracy); //accuracy cannot be higher than 100%
		}
		public void VisualizeTarget(GameObject parent, Health user, BoardSpace target){
			/**Creates a visualization for an attack targeting a unit, as a child of the parent GameObject.*/
			BoardManager board = BoardManager.GetBoard();
			GameObject targetUnit = target.occupyingUnit;
			PlayerController targetPc = targetUnit.GetComponent<PlayerController>();
			Health targetHealth = targetUnit.GetComponent<Health>();
			MeshRenderer renderer = targetUnit.GetComponent<MeshRenderer>();
			float damage = CalculateDamage(user, targetHealth);
			float accuracy = CalculateHitChance(user, targetHealth);
			bool lethal = (damage >= targetHealth.currentHealth) && (accuracy > 0); //if attack would reduce HP to zero and can hit, it may be lethal

			GameObject vis = UnityEngine.Object.Instantiate(board.skillHitPrefab, parent.transform);
			Vector3 topOfUnit = Vector3.up * renderer.bounds.extents.y  + renderer.GetComponent<MeshRenderer>().bounds.center;
			UnityEngine.Camera mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<UnityEngine.Camera>();
			vis.transform.position = mainCamera.WorldToScreenPoint(topOfUnit);
			
			Text damageText = vis.transform.Find("Damage").GetComponent<Text>();
			Text hitText = vis.transform.Find("Hit Chance").GetComponent<Text>();

			hitText.text = ((int) accuracy).ToString() + "%";
			damageText.text = Math.Round(damage, 2).ToString();

			if (lethal){
				damageText.color = new Color(128, 0, 0);
			}
		}
	}
	
	[Serializable]
	public enum SkillType{
		MELEE,
		RANGED,
		SUPPORT
	}
	
	[Serializable]
	public struct SkillTypeInfo{
		/**Allows for input through the Inspector for the menus.*/
		public SkillType type;
		public Sprite emblemSprite;
	}

	[Serializable]
	public struct PlayerInfo{
		/**Information used to recreate a player while saving/loading.*/
		public PlayerController playerController;
		public Health health;
	}
	
	[Serializable]
	public enum UnitAction{
		SKILL,
		MOVE,
		WAIT
	}

	[Serializable]
	public enum UnitAffiliation{
		PLAYER,
		ENEMY,
		ALLY,
		OTHER
	}
	
	[Serializable]
	public struct ActionInfo{
		/**Allows for input through the Inspector for the menus.*/
		public UnitAction action;
		public Sprite emblemSprite;
	}
	
	[Serializable]
	public class Skill{
		
		public string name;
		public SkillType skillType;
		public bool reusable = false;
		public List<Attack> attacks;
		public int actionCost = 1;
		public int bulletCost = 0;
		public int moveCost = 0;
		//relative to user, in board space, when user faces upwards.
		[JsonConverter(typeof(VectorConverter))]
		public Vector2 movePosition = new Vector2(0, 0); 
		
		public void VisualizeTarget(BoardSpace space, GameObject player, Quaternion unitRotation){
			/**Shows a visualization for the area affected by the skill on the board.*/
			foreach (GameObject vis in GameObject.FindGameObjectsWithTag("Visualization")){
				UnityEngine.Object.Destroy(vis);
			}
			PlayerController pc = player.GetComponent<PlayerController>();
			Health h = player.GetComponent<Health>();
			
			GameObject skillVisual = new GameObject(); //empty object to child tiles to, and destroy later
			skillVisual.tag = "Visualization";
			skillVisual.name = "Skill Visualization";
			GameObject hitVisual = new GameObject("Hit Visualization"); //empty object childed to Canvas for hit visuals
			hitVisual.transform.parent = GameObject.Find("Canvas").transform;
			hitVisual.tag = "Visualization";
			
			BoardManager board = BoardManager.GetBoard();
			GameObject skillTile = board.skillTilePrefab;
			GameObject tile;
			Vector2 newPosition;
			foreach (Attack attack in attacks){
				newPosition = space.boardPosition + (Vector2) (unitRotation * new Vector3(attack.targetPosition.x, attack.targetPosition.y, 0));
				
				if (!board.IsWithinBounds(newPosition)){
					continue;
				}
				tile = UnityEngine.Object.Instantiate(skillTile, skillVisual.transform);	
				tile.transform.position = board.GetSpace(newPosition).anchorPosition + new Vector3(0, 0.1f, 0);
				tile.GetComponent<MeshRenderer>().materials = new Material[] {tile.GetComponent<MeshRenderer>().materials[2]};
				
				if (board.GetSpace(newPosition).occupyingUnit != null){
					attack.VisualizeTarget(hitVisual, h, board.GetSpace(newPosition));
				}
				
				if (attack.knockbackPosition != new Vector2(0, 0)){
					newPosition = space.boardPosition + (Vector2) (unitRotation * new Vector3(attack.knockbackPosition.x, attack.knockbackPosition.y, 0));
					if (!board.IsWithinBounds(newPosition)){
						continue;
					}
					tile = UnityEngine.Object.Instantiate(skillTile, skillVisual.transform);
					tile.transform.position = board.GetSpace(newPosition).anchorPosition + new Vector3(0, 0.1f, 0);
					tile.GetComponent<MeshRenderer>().materials = new Material[] {tile.GetComponent<MeshRenderer>().materials[1]};
				}
			}
			
			if(this.movePosition != new Vector2(0,0)){
				tile = UnityEngine.Object.Instantiate(skillTile, skillVisual.transform);
				newPosition = space.boardPosition + (Vector2) (unitRotation * new Vector3(movePosition.x, movePosition.y, 0));
				tile.transform.position = board.GetSpace(newPosition).anchorPosition + new Vector3(0, 0.1f, 0);
				tile.GetComponent<MeshRenderer>().materials = new Material[] {tile.GetComponent<MeshRenderer>().materials[0]};
			}
		}
		
		public bool IsValid(BoardSpace space, Quaternion direction){
			/**Returns true if using this skill from space in chosen direction is a valid move.*/
			return (IsMoveValid(space, direction) && IsTargetValid(space, direction));
		}
		
		public bool IsMoveValid(BoardSpace space, Quaternion direction){
			/**Returns true if and only if unit's skill-related movement is not hindered by obstacles.
			It is assumed that movement occurs only in a straight line.*/
			if (this.movePosition == Vector2.zero){
				return true; // if there is no movement it cannot be blocked
			}
			
			BoardManager board = BoardManager.GetBoard();
			PlayerController pc = space.occupyingUnit.GetComponent<PlayerController>();
			float start;
			float end;
			float currentHeight = space.GetHeight();
			BoardSpace nextSpace;
			BoardSpace endSpace = board.GetSpace(space.boardPosition + (Vector2) (direction * new Vector3(movePosition.x, movePosition.y, 0)));
			
			if (endSpace.occupyingUnit != null){ //cannot move to an occupied space
				return false;
			}
			
			if (space.boardPosition.x == endSpace.boardPosition.x){
				start = space.boardPosition.y;
				end = endSpace.boardPosition.y;
				for (float y = start + 1; y < end; y++){
					nextSpace = board.GetSpace(new Vector2(space.boardPosition.x, y));
					if (nextSpace.impassable || Math.Abs(currentHeight - nextSpace.GetHeight()) > pc.jumpHeight){
						return false; //there is something preventing movement through it
					} else {
						currentHeight = nextSpace.GetHeight();
					}
				}
				
			} else if (space.boardPosition.y == endSpace.boardPosition.y){ 
				start = space.boardPosition.x;
				end = endSpace.boardPosition.x;
				for (float x = start + 1; x < end; x++){
					nextSpace = board.GetSpace(new Vector2(x, space.boardPosition.y));
					if (nextSpace.impassable || Math.Abs(currentHeight - nextSpace.GetHeight()) > pc.jumpHeight){
						return false; //there is something preventing movement through it
					} else {
						currentHeight = nextSpace.GetHeight();
					}
				}
			}
			return true; //reached the end space without obstruction
			
		}
		public bool IsTargetValid(BoardSpace space, Quaternion direction){
			/**Returns true if and only if the attack will hit at least one targetable entity.*/
			BoardManager board = BoardManager.GetBoard();
			Vector2 newPosition;
			foreach (Attack attack in this.attacks){
				newPosition = space.boardPosition + (Vector2) (direction * new Vector3(attack.targetPosition.x, attack.targetPosition.y, 0));
				if (!board.IsWithinBounds(newPosition)){
					continue;
				}
				if (board.GetSpace(newPosition).occupyingUnit != null){
					return true;
				}
			}
			return false;
		}
		
		public static Skill GetSkillByName(string skillName, TextAsset skillData){
			/**Gets the skill with the name skillName from the skillData JSON file.*/
			List<Skill> skillList = JsonConvert.DeserializeObject<List<Skill>>(skillData.text, new SkillConverter());
			foreach(Skill skill in skillList){
				if (skill.name == skillName){
					return skill;
				}
			}
			return null;
		}
		
		internal class SkillConverter : JsonConverter{
			/**A class to convert JSON representations of skills to Skill objects.*/
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer){
				throw new System.NotImplementedException(); //can't use to serialize json
			}
			
			public override bool CanConvert(Type type){
				if (type == typeof(Skill) || type == typeof(List<Skill>) || type == typeof(Attack) || type == typeof(List<Attack>)){
					return true;
				} else {
					return false;
				}
			}
			
			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer){
				JsonTextReader newReader;
				JArray array;
				switch(objectType){
					case Type t when t == typeof(Skill):
						Skill skill = new Skill();
						JObject skillInfo = JObject.Load(reader);
						skill.name = skillInfo["name"].Value<string>();
						skill.skillType = (SkillType) Enum.Parse(typeof(SkillType), skillInfo["skillType"].Value<string>());
						skill.reusable = skillInfo["reusable"].Value<bool>();
						skill.actionCost = skillInfo["actionCost"].Value<int>();
						skill.bulletCost = skillInfo["bulletCost"].Value<int>();
						skill.moveCost = skillInfo["moveCost"].Value<int>();
						if (skillInfo.ContainsKey("movePosition")){
							skill.movePosition = new Vector2(skillInfo["movePosition"][0].Value<int>(), skillInfo["movePosition"][1].Value<int>());
						}
						newReader = new JsonTextReader(new StringReader(skillInfo["attacks"].ToString()));
						skill.attacks = this.ReadJson(newReader, typeof(List<Attack>), existingValue, serializer) as List<Attack>;	
						
						return skill;
					case Type t when t == typeof(Attack):
						Attack attack = new Attack();
						JObject attackInfo = JObject.Load(reader);
						attack.basePower = attackInfo["basePower"].Value<float>();
						attack.accuracy = attackInfo["basePower"].Value<float>();
						attack.targetPosition = new Vector2(attackInfo["targetPosition"][0].Value<int>(), attackInfo["targetPosition"][1].Value<int>());
						if (attackInfo.ContainsKey("knockbackPosition")){
							attack.knockbackPosition = new Vector2(attackInfo["knockbackPosition"][0].Value<int>(), attackInfo["knockbackPosition"][1].Value<int>());
						} else {
							attack.knockbackPosition = new Vector2(0, 0);
						}
						return attack;
					case Type t when t == typeof(List<Skill>):
						List<Skill> skills = new List<Skill>();
						array = JArray.Load(reader);
						foreach (JToken token in array){
							newReader = new JsonTextReader(new StringReader(token.ToString()));
							skills.Add(this.ReadJson(newReader, typeof(Skill), existingValue, serializer) as Skill); 
						}
						return skills;
					case Type t when t == typeof(List<Attack>):
						List<Attack> attacks = new List<Attack>();
						array = JArray.Load(reader);
						foreach (JToken token in array){
							newReader = new JsonTextReader(new StringReader(token.ToString()));
							attacks.Add(this.ReadJson(newReader, typeof(Attack), existingValue, serializer) as Attack); 
						}
						return attacks;
					default:
						return null;
				}
			}
		}
	}
}
