using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Objects;
using InBattle;

namespace JsonConverters{
    public class VectorConverter : JsonConverter{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer){
            JObject toWrite = new JObject();
            switch(value.GetType()){
                case Type t when t == typeof(Vector2):
                    Vector2 v2 = (Vector2) value;
                    toWrite.Add("x", v2.x);
                    toWrite.Add("y", v2.y);
                    break;

                case Type t when t == typeof(Vector3):
                    Vector3 v3 = (Vector3) value;
                    toWrite.Add("x", v3.x);
                    toWrite.Add("y", v3.y);
                    toWrite.Add("z", v3.z);
                    break;
                case Type t when t == typeof(Quaternion):
                    Quaternion q = (Quaternion) value;
                    toWrite.Add("x", q.x);
                    toWrite.Add("y", q.y);
                    toWrite.Add("z", q.z);
                    toWrite.Add("w", q.w);
                    break;
				case Type t when t == typeof(Color):
                    Color c = (Color) value;
                    toWrite.Add("r", c.r);
                    toWrite.Add("g", c.g);
                    toWrite.Add("b", c.b);
                    toWrite.Add("a", c.a);
                    break;
            }
            toWrite.WriteTo(writer);
			return;
		}
		public override bool CanConvert(Type type){
			if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Quaternion) || type == typeof(Color)){
				return true;
			} else {
				return false;
			}
		}
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer){
            JObject read = JObject.Load(reader);
            switch(objectType){
                case Type t when t == typeof(Vector2):
                    Vector2 v2 = new Vector2();
                    v2.x = read["x"].Value<float>();
                    v2.y = read["y"].Value<float>();
                    return v2;
                
                case Type t when t == typeof(Vector3):
                    Vector3 v3 = new Vector3();
                    v3.x = read["x"].Value<float>();
                    v3.y = read["y"].Value<float>();
                    v3.z = read["z"].Value<float>();
                    return v3;
                
                case Type t when t == typeof(Quaternion):
                    Quaternion q = new Quaternion();
                    q.x = read["x"].Value<float>();
                    q.y = read["y"].Value<float>();
                    q.z = read["z"].Value<float>();
                    q.w = read["w"].Value<float>();
                    return q;
				case Type t when t == typeof(Color):
                    Color c = new Color();
                    c.r = read["r"].Value<float>();
                    c.g = read["g"].Value<float>();
                    c.b = read["b"].Value<float>();
                    c.a = read["a"].Value<float>();
                    return c;
            }
            
			return null;
		}
	}

    public class BoardConverter : JsonConverter{
		/**A class which converts JSON representations of BoardSpaces to BoardSpace objects.*/
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer){
            BoardManager board = ((GameObject) value).GetComponent<BoardManager>();
			
			//serialize the spaces and units on them
            //JArray spaces = new JArray();
			JArray units = new JArray();
            foreach (BoardSpace space in board.boardSpaces){
                //spaces.Add(JObject.Parse(JsonConvert.SerializeObject(space)));
				if (space.occupyingUnit != null){
					units.Add(JObject.Parse(JsonConvert.SerializeObject(space.occupyingUnit, new PlayerConverter())));
				}
            }
            /*
			JObject toWrite = new JObject(
				new JProperty("rows", board.rows),
				new JProperty("columns", board.columns),
				new JProperty("spaces", spaces),
				new JProperty("units", units),
				new JProperty("modelName", board.modelName)
			);
			*/
			JObject missionInfo = new JObject(
				new JProperty("units", units),
				new JProperty("currentTurn", board.currentTurn),
				new JProperty("currentPhase", board.phase),
				new JProperty("map", board.mapName),
				new JProperty("armyManager", JObject.Parse(JsonConvert.SerializeObject(board.armyManager)))
			);

			missionInfo.WriteTo(writer);
		}
		
		public override bool CanConvert(Type type){
			if (type == typeof(GameObject)){
				return true;
			} else {
				return false;
			}
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer){

			GameObject boardObject = UnityEngine.Object.Instantiate(SaveManager.GetSaveManager().boardPrefab);
			BoardManager board = boardObject.GetComponent<BoardManager>();
			JObject boardInfo = JObject.Load(reader);

			//set dimensions
			board.columns = boardInfo["columns"].Value<int>();
			board.rows = boardInfo["rows"].Value<int>();
			board.modelName = boardInfo["modelName"].Value<string>();

			//initialize and show the map
			List<BoardSpace> spaces = new List<BoardSpace>();
			foreach (JToken item in boardInfo["spaces"].Children()){
				spaces.Add(JsonConvert.DeserializeObject<BoardSpace>(item.ToString()));
			}
			board.SetSpaces(spaces);

			//initialize the units

			return boardObject;
		}
	}
    public class PlayerConverter : JsonConverter{
		/**A class which converts JSON representations of players to player GameObjects and vice versa.*/
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer){
			GameObject player = (GameObject) value;
			PlayerController pc = player.GetComponent<PlayerController>();
			JObject toWrite = JObject.Parse(JsonConvert.SerializeObject(pc));
			toWrite.Add(new JProperty("skillNames", new JArray(from skill in pc.skillList select skill.name)));
			toWrite.WriteTo(writer);
		}
		
		public override bool CanConvert(Type type){
			if (type == typeof(GameObject)){
				return true;
			} else {
				return false;
			}
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer){

			BoardManager board = BoardManager.GetBoard();
			JObject playerInfo = JObject.Load(reader);
			//need to instantiate specific model instead, from json attribute.
			GameObject modelObject = (GameObject) Resources.Load("UnitAesthetics/Models/" + playerInfo["modelName"]);
			GameObject player = UnityEngine.Object.Instantiate(modelObject, board.transform);
			PlayerController pc = player.AddComponent<PlayerController>();
			Health h = player.AddComponent<Health>();
			foreach (JToken skillName in playerInfo["skillNames"]){
				pc.skillList.Add(Skill.GetSkillByName(skillName.ToString()));
			}
			playerInfo.Remove("skillNames");
			JsonConvert.PopulateObject(playerInfo["health"].ToString(), h);
			playerInfo.Remove("health");
			JsonConvert.PopulateObject(playerInfo.ToString(), pc);

			// pc.unitName = playerInfo["name"].Value<string>();
			// pc.modelName = playerInfo["model"].Value<string>();
			// pc.affiliation = (UnitAffiliation) Enum.Parse(typeof(UnitAffiliation), playerInfo["affiliation"].Value<string>());
			// pc.jumpHeight = playerInfo["jumpHeight"].Value<float>();
			// pc.moveRange = playerInfo["moveRange"].Value<int>();
			// pc.remainingMove = pc.moveRange;
			// pc.maxActions = playerInfo["maxActions"].Value<int>();
			// pc.remainingActions = pc.maxActions;
			// pc.boardPosition = JsonConvert.DeserializeObject<Vector2>(playerInfo["boardPosition"].ToString());
			// pc.maxBullets = playerInfo["maxBullets"].Value<int>();
			// pc.bullets = playerInfo["bullets"].Value<int>();
			// pc.turnEnded = playerInfo["turnEnded"].Value<bool>();
			// pc.aggressionGroup = playerInfo["aggressionGroup"].Value<int>();
			// pc.aggressive = playerInfo["aggressive"].Value<bool>();
			// pc.autoMove = JsonConvert.DeserializeObject<AutoMoveInfo>(playerInfo["autoMove"].ToString());

			// h.attackPower = playerInfo["attackPower"].Value<float>();
			// h.defense = playerInfo["defense"].Value<float>();
			// h.level = playerInfo["level"].Value<int>();
			// h.maxHealth = playerInfo["maxHealth"].Value<float>();
			// h.currentHealth = playerInfo["currentHealth"].Value<float>();

			return player;
		}
	}

	public class SkillConverter : JsonConverter{
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