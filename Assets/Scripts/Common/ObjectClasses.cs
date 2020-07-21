using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverters;
using InBattle;

namespace Objects
{

    /**Represents a single space on the game board.*/
	[Serializable]
    public class BoardSpace {
		[JsonConverter(typeof(VectorConverter))]
		public Vector2 boardPosition;
		[JsonConverter(typeof(VectorConverter))]
		public Vector3 anchorPosition;
		[JsonProperty(ItemConverterType = typeof(VectorConverter))]
		public Vector3[] corners;
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

		public void SetHighlightMaterial(BoardManager board, HighlightType type){
			Material material = null;
			foreach (HighlightColor hcolor in board.highlightColors){
				if (hcolor.htype == type){
					material = hcolor.material;
					break;
				}
			}
			GameObject tile = board.GetTile(this);
			tile.GetComponent<MeshRenderer>().material.color = material.color;
		}
	}
	
	[Serializable]
	public class Attack{
		
		public float basePower;
		public float accuracy;
		public const float RANDOM_VARIANCE = 0.1f;
		//relative to user, in board space, when user faces upwards.
		[JsonConverter(typeof(VectorConverter))]
		public Vector2 targetPosition = new Vector2(0, 0);
		[JsonConverter(typeof(VectorConverter))]
		public Vector2 knockbackPosition = new Vector2(0, 0);

		public float CalculateAverageDamage(Health user, Health target){
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
			return Math.Max(0f, Math.Min(100.0f, effectiveAccuracy)); //accuracy cannot be higher than 100%
		}

		public (float, float) CalculateDamageRange(Health user, Health target){
			/**Returns the miminum and maximum damage that can be dealt by this attack,
			in a display-friendly format.!--*/
			double minDamage = Math.Round(CalculateAverageDamage(user, target) * (1f - RANDOM_VARIANCE), 2);
			double maxDamage = Math.Round(CalculateAverageDamage(user, target) * (1f + RANDOM_VARIANCE), 2);
			return ((float) minDamage, (float) maxDamage);
		}

		public float CalculateEffectiveDamage(Health user, Health target){
			/**Returns the average damage dealt to the target.!--*/
			return CalculateAverageDamage(user, target) * CalculateHitChance(user, target) / 100.0f;
		}

		public float IsPotentiallyLethal(Health user, Health target){
			float maxDamage = CalculateAverageDamage(user, target) * (1.0f + RANDOM_VARIANCE);
			return (maxDamage >= target.currentHealth) && (CalculateHitChance(user, target) > 0f) ? 1 : 0;
		}
		public void VisualizeTarget(Transform parent, Health user, BoardSpace target){
			/**Creates a visualization for an attack targeting a unit, as a child of the parent GameObject.*/
			BoardManager board = BoardManager.GetBoard();
			GameObject targetUnit = target.occupyingUnit;
			PlayerController targetPc = targetUnit.GetComponent<PlayerController>();
			Health targetHealth = targetUnit.GetComponent<Health>();
			float damage = CalculateAverageDamage(user, targetHealth);
			float accuracy = CalculateHitChance(user, targetHealth);
			bool lethal = (damage >= targetHealth.currentHealth) && (accuracy > 0); //if attack would reduce HP to zero and can hit, it may be lethal

			GameObject vis = UnityEngine.Object.Instantiate(board.skillHitPrefab, parent);
			Vector3 topOfUnit = Vector3.up * targetPc.playerRenderer.bounds.extents.y  + targetPc.playerRenderer.bounds.center;
			UnityEngine.Camera mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<UnityEngine.Camera>();
			vis.transform.position = mainCamera.WorldToScreenPoint(topOfUnit);
			vis.tag = "Visualization";
			
			Text minText = vis.transform.Find("Minimum").GetComponent<Text>();
			Text maxText = vis.transform.Find("Maximum").GetComponent<Text>();
			Text hitText = vis.transform.Find("Hit Chance").GetComponent<Text>();

			hitText.text = ((int) accuracy).ToString() + "%";
			(float minDamage, float maxDamage) = CalculateDamageRange(user, targetHealth);
			minText.text = minDamage.ToString();
			if (minDamage >= targetHealth.currentHealth){
				minText.color = new Color(128, 0, 0);
			}
			maxText.text = maxDamage.ToString();
			if (maxDamage >= targetHealth.currentHealth){
				maxText.color = new Color(128, 0, 0);
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
	public enum HighlightType{
		MOVE,
		ATTACK,
		KNOCKBACK
	}

	[Serializable]
	public struct HighlightColor{
		public HighlightType htype;
		public Material material;
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

	public static class Extensions
	{
		public static T Next<T>(this T src) where T : struct
		{
			if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

			T[] Arr = (T[])Enum.GetValues(src.GetType());
			int j = Array.IndexOf<T>(Arr, src) + 1;
			return (Arr.Length==j) ? Arr[0] : Arr[j];            
		}

		public static Vector2 Rotate(this Vector2 v2, float angle)
		{
			/**Rotates the vector by angle degrees COUNTERCLOCKWISE.!--*/
			angle *= Mathf.Deg2Rad;
			float x = Mathf.Cos(angle) * v2.x - Mathf.Sin(angle) * v2.y;
			float y = Mathf.Sin(angle) * v2.x + Mathf.Cos(angle) * v2.y;
			return new Vector2(x, y);
		}

		public static Color BorderColor(this UnitAffiliation affiliation){
			Color color;
			switch (affiliation){
				case UnitAffiliation.PLAYER:
					color = new Color(0f, 0.2f, 0.9f); //blue
					break;
				case UnitAffiliation.ENEMY:
					color = new Color(0.8f, 0.1f, 0.1f); //red
					break;
				case UnitAffiliation.ALLY:
					color = new Color(0.3f, 0.7f, 0.1f); //forest green
					break;
				case UnitAffiliation.OTHER:
					color = new Color(0.8f, 0.7f, 0f); //golden yellow
					break;
				default:
					color = Color.black;
					break;
			}
			return color;
		}

		public static Color BgColor(this UnitAffiliation affiliation){
			Color color;
			switch (affiliation){
				case UnitAffiliation.PLAYER:
					color = new Color(0.85f, 0.9f, 1f); //light blue
					break;
				case UnitAffiliation.ENEMY:
					color = new Color(1f, 0.85f, 0.85f); //light red
					break;
				case UnitAffiliation.ALLY:
					color = new Color(0.85f, 1f, 0.85f); //light green
					break;
				case UnitAffiliation.OTHER:
					color = new Color(1f, 0.9f, 0.8f); //golden yellow
					break;
				default:
					color = Color.white;
					break;
			}
			return color;
		}

		public static T NextOf<T>(this IList<T> list, T item)
		{
			var indexOf = list.IndexOf(item);
			return list[indexOf == list.Count - 1 ? 0 : indexOf + 1];
		}
		public static T PreviousOf<T>(this IList<T> list, T item)
		{
			var indexOf = list.IndexOf(item);
			return list[indexOf == 0 ? list.Count - 1 : indexOf - 1];
		}

		public static bool ApproximatelyEqual(this Quaternion quatA, Quaternion quatB, float acceptableRange = (float)(1e-5))
		{
			return 1 - Mathf.Abs(Quaternion.Dot(quatA, quatB)) < acceptableRange;
		}  
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
			
			BoardManager board = BoardManager.GetBoard();
			GameObject hitVisual = new GameObject();
			hitVisual.tag = "Visualization";
			hitVisual.transform.parent = GameObject.FindWithTag("MainCanvas").transform;
			Vector2 newPosition;
			HashSet<BoardSpace> affectedSpaces = new HashSet<BoardSpace>();
			foreach (Attack attack in attacks){
				newPosition = space.boardPosition + attack.targetPosition.Rotate(unitRotation.eulerAngles.y);
				
				if (!board.IsWithinBounds(newPosition)){
					continue;
				}
				affectedSpaces.Add(board.GetSpace(newPosition));
				board.GetSpace(newPosition).SetHighlightMaterial(board, HighlightType.ATTACK);
				
				if (board.GetSpace(newPosition).occupyingUnit != null){
					attack.VisualizeTarget(hitVisual.transform, h, board.GetSpace(newPosition));
				}
				
				if (attack.knockbackPosition != new Vector2(0, 0)){
					newPosition = space.boardPosition + attack.knockbackPosition.Rotate(unitRotation.eulerAngles.y);
					if (!board.IsWithinBounds(newPosition)){
						continue;
					}
					affectedSpaces.Add(board.GetSpace(newPosition));
					board.GetSpace(newPosition).SetHighlightMaterial(board, HighlightType.KNOCKBACK);
				}
			}
			
			if(this.movePosition != new Vector2(0,0)){
				newPosition = space.boardPosition + movePosition.Rotate(unitRotation.eulerAngles.y);
				affectedSpaces.Add(board.GetSpace(newPosition));
				board.GetSpace(newPosition).SetHighlightMaterial(board, HighlightType.MOVE);
			}
			board.HighlightSpaces(affectedSpaces);
		}

		public List<Quaternion> GetDirections(BoardSpace space){
			/**Returns the list of valid rotations for this skill used at the given space.!--*/
			List<Quaternion> valid = new List<Quaternion>();
			Quaternion stepRotation = Quaternion.AngleAxis(90, Vector3.up);
			Quaternion rotation;
			for (int i=0; i<=3; i++){
				rotation = Quaternion.identity;
				for (int j = 0; j < i; j++){
					rotation *= stepRotation;
				}
				if (IsValid(space, rotation)){
					valid.Add(rotation);
				}
			}
			return valid;
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
			BoardSpace endSpace = board.GetSpace(space.boardPosition + movePosition.Rotate(direction.eulerAngles.y));
			
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
				newPosition = space.boardPosition + attack.targetPosition.Rotate(direction.eulerAngles.y);
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

		private Quaternion SpaceToPlane(Quaternion rotation){
			Vector3 euler = rotation.eulerAngles;
			euler = new Vector3(euler.x, euler.z, euler.y);
			return Quaternion.Euler(euler);
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
						attack.accuracy = attackInfo["accuracy"].Value<float>();
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

	[Serializable]
	public struct Mission{
		public string name;
		public string filename;
		public bool completed;
		public bool replayable;
		public List<string> required;
	}

	[Serializable]
	public struct AutoMoveInfo{
		/**Defines how this unit should be automatically moved.!--*/
		public float moveModCoefficient;
        public float commanderBonus;
        public float retreatHP;
	}

	[Serializable]
	public struct AutoMoveAction{
		/**Defines a course of action for automatic movement: a unit would move to the space
		movementTarget and then use skillToUse in the direction skillDirection.!--*/
		public BoardSpace movementTarget;
		public Skill skillToUse;
		public Quaternion skillDirection;

		public AutoMoveAction(BoardSpace movementTarget, Skill skillToUse, Quaternion skillDirection){
			this.movementTarget = movementTarget;
			this.skillToUse = skillToUse;
			this.skillDirection = skillDirection;
		}
	}
}
