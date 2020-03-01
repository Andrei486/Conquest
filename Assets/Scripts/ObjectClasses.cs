using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Objects{
	
	/**Represents a single space on the game board.*/
	public class BoardSpace {
		
		public Vector2 boardPosition;
		public Vector3 anchorPosition;
		public GameObject occupyingUnit;
		public Sprite sprite;
		public Color pillarColor;
		public int moveCost = 1;
		public const float BOARD_SIZE = 2.0f;
		public bool impassable = false;
		
		public float GetHeight(){
			return this.anchorPosition.y;
		}
	}
	
	public class Attack{
		
		public float basePower;
		public float accuracy;
		//relative to user, in board space, when user faces upwards.
		public Vector2 targetPosition = new Vector2(0, 0);
		public Vector2 knockbackPosition = new Vector2(0, 0);
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
	public enum UnitAction{
		SKILL,
		MOVE,
		WAIT
	}
	
	[Serializable]
	public struct ActionInfo{
		/**Allows for input through the Inspector for the menus.*/
		public UnitAction action;
		public Sprite emblemSprite;
	}
	
	public class Skill{
		
		public string name;
		public SkillType skillType;
		public bool reusable = false;
		public List<Attack> attacks;
		public int actionCost = 1;
		public int bulletCost = 0;
		public int moveCost = 0;
		//relative to user, in board space, when user faces upwards.
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
			BoardManager board = GameObject.FindGameObjectsWithTag("Board")[0].GetComponent<BoardManager>();
			GameObject skillTile = board.skillTilePrefab;
			GameObject tile;
			Vector2 newPosition;
			foreach (Attack attack in attacks){
				newPosition = new Vector2((int) (space.boardPosition.x), (int) (space.boardPosition.y)) + (Vector2) (unitRotation * new Vector3(attack.targetPosition.x, attack.targetPosition.y, 0));
				if (!board.IsWithinBounds(newPosition)){
					continue;
				}
				tile = UnityEngine.Object.Instantiate(skillTile, skillVisual.transform);	
				tile.transform.position = board.GetSpace(newPosition).anchorPosition + new Vector3(0, 0.1f, 0);
				tile.GetComponent<MeshRenderer>().materials = new Material[] {tile.GetComponent<MeshRenderer>().materials[2]};
				if (attack.knockbackPosition != new Vector2(0, 0)){
					newPosition = new Vector2((int) (space.boardPosition.x), (int) (space.boardPosition.y)) + (Vector2) (unitRotation * new Vector3(attack.knockbackPosition.x, attack.knockbackPosition.y, 0));
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
				newPosition = new Vector2((int) (space.boardPosition.x), (int) (space.boardPosition.y)) + (Vector2) (unitRotation * new Vector3(movePosition.x, movePosition.y, 0));
				tile.transform.position = board.GetSpace(newPosition).anchorPosition + new Vector3(0, 0.1f, 0);
				tile.GetComponent<MeshRenderer>().materials = new Material[] {tile.GetComponent<MeshRenderer>().materials[0]};
			}
		}
		
		public bool IsValid(BoardSpace space, Vector2 direction){
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
