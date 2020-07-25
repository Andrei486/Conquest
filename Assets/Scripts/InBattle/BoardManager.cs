using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Objects;

namespace InBattle{
	public class BoardManager : MonoBehaviour
	{
		public int rows;
		public int columns;
		public int currentTurn;
		public int turnLimit = 100;
		public EndCondition victory = EndCondition.ON_ROUT;
		public EndCondition defeat = EndCondition.ON_ROUT;
		public List<BoardSpace> destinationSpaces;
		public List<BoardSpace> defendedSpaces;
		GameObject board;
		public string modelName;
		public string mapName;
		BattleMenu menu;
		public bool locked = false;
		public bool movingUnit = false;
		Cursor cursor;
		public Cursor moveCursor;
		public UnitAffiliation phase;
		public BoardSpace[,] boardSpaces;
		public GameObject[,] boardTiles;
		public HighlightColor[] highlightColors;
		public GameObject cursorPrefab;
		public GameObject boardTilePrefab;
		public GameObject skillHitPrefab;
		public GameObject directionArrowPrefab;
		private List<PlayerController> toSave = new List<PlayerController>();
		private List<GameObject> units;
		public List<PlayerController> players{
			get{
				return (from unit in units select unit.GetComponent<PlayerController>()).ToList();
			}
		}
		private List<BoardSpace> spaces;
		private bool outlinesOn = true;
		private ControlsManager controls;
		private UIController uI;
		private BattleLog log;
		[SerializeField]
		private AutoMoveController autoMoveController;
		public ArmyManager armyManager = new ArmyManager();

		// Start is called before the first frame update
		void Start()
		{
			board = this.gameObject;
			controls = ControlsManager.GetControls();
			menu = BattleMenu.GetMenu();
			uI = UIController.GetUI();
			log = BattleLog.GetLog();
			InitializeMap(spaces);
			InitializeUnits(units);
			menu.cursor = Instantiate(cursorPrefab, this.transform).GetComponent<Cursor>();
			cursor = menu.cursor;
			ControlsManager.GetControls().board = this.gameObject;
		}

		// Update is called once per frame
		void Update()
		{
			if (locked){
				return;
			}
			if (Input.GetKeyDown(controls.GetCommand(Command.TOGGLE_GRID))){
				outlinesOn = !outlinesOn;
				ToggleOutlines(outlinesOn);
			}
			if (Input.GetKeyDown(controls.GetCommand(Command.TOGGLE_INFO))){
				if (uI.ShowingInfo()){
					uI.ClearUnitInfo();
					cursor.locked = false;
				} else {
					BoardSpace space = GetHoveredSpace();
					if (space.occupyingUnit != null){
						uI.ShowUnitInfo(space.occupyingUnit.GetComponent<PlayerController>());
						cursor.locked = true;
					}
					
				}
			}
		}

		public void SetSpaces(List<BoardSpace> spaces){
			this.spaces = spaces;
		}
		public void SetUnits(List<GameObject> units){
			this.units = units;
		}
		
		public BoardSpace GetSpace(Vector2 position){
			/**Returns the BoardSpace at the (x, y) coordinates specified by position.
			If the coordinates are out of bounds, returns null instead.*/
			if (IsWithinBounds(position)){
				return this.boardSpaces[(int) Math.Round(position.x), (int) Math.Round(position.y)];
			} else {
				return null;
			}
		}

		public BoardSpace GetHoveredSpace(){
			/**Returns the BoardSpace currently hovered over by the cursor.!--*/
			return GetSpace(cursor.position);
		}

		public GameObject GetTile(BoardSpace space){
			/**Returns the tile of the BoardSpace at the (x, y) coordinates specified by position.
			If the coordinates are out of bounds, returns null instead.*/
			Vector2 position = space.boardPosition;
			if (IsWithinBounds(position)){
				return this.boardTiles[(int) Math.Round(position.x), (int) Math.Round(position.y)];
			} else {
				return null;
			}
		}
		
		void AddTile(BoardSpace space){
			/**Shows a board tile corresponding to a BoardSpace.*/
			
			GameObject tile = Instantiate(boardTilePrefab, this.transform);
			//move the tile up a bit so it looks better
			tile.transform.Translate(Vector3.up * 0.1f * BoardSpace.BOARD_SIZE);
			LineRenderer lrenderer = tile.GetComponent<LineRenderer>();
			MeshRenderer mrenderer = tile.GetComponent<MeshRenderer>();
			lrenderer.positionCount = 4;
			lrenderer.SetPositions(space.corners);
			Mesh mesh = new Mesh();
			mesh.vertices = space.corners;
			mesh.triangles = new int[]{2, 1, 0, 0, 3, 2};
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			tile.GetComponent<MeshFilter>().mesh = mesh;
			lrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mrenderer.receiveShadows = false;
			mrenderer.enabled = false;
			boardTiles[(int) space.boardPosition.x, (int) space.boardPosition.y] = tile;
		}

		void ToggleOutlines(bool enabled){
			foreach (GameObject tile in boardTiles){
				tile.GetComponent<LineRenderer>().enabled = enabled;
			}
		}

		public void HighlightSpaces(HashSet<BoardSpace> spaces){
			foreach (BoardSpace space in boardSpaces){
				GetTile(space).GetComponent<MeshRenderer>().enabled = false;
			}
			if (spaces != null){
				foreach (BoardSpace space in spaces){
					GetTile(space).GetComponent<MeshRenderer>().enabled = true;
				}
			}
		}
		
		public bool IsWithinBounds(Vector2 position){
			/**Returns true if the (x, y) coordinates specified by position are within bounds for the grid,
			ie there is a space at that position. Otherwise returns false.*/
			int x = (int) Math.Round(position.x);
			int y = (int) Math.Round(position.y);
			if (x < 0 || x > columns - 1 || y < 0 || y > rows - 1){
				return false;
			} else {
				return true;
			}
		}
		
		public void MoveUnit(BoardSpace start, BoardSpace end){
			/**Moves the unit on the BoardSpace start to the BoardSpace end, if any.*/
			if (start.occupyingUnit == null){
				Debug.Log("no unit to move");
				return;
			}
			if (end.occupyingUnit != null){
				Debug.Log("cannot move to a used space");
				return;
			}
			PlayerController pc = start.occupyingUnit.GetComponent<PlayerController>();
			if (!pc.CanMove()){
				return;
			}
			end.occupyingUnit = start.occupyingUnit;
			start.occupyingUnit = null;
			StartCoroutine(pc.FollowPath(CollapsePath(pc.GetShortestPath(Vector2Int.RoundToInt(end.boardPosition)), pc)));
			// end.occupyingUnit.transform.position = end.anchorPosition; //lerp here
			pc.boardPosition = end.boardPosition;
			pc.remainingMove = pc.moveGrid[(int) end.boardPosition.x, (int) end.boardPosition.y];
			pc.previousAction = UnitAction.MOVE;
			pc.hasActed = true;
			// if there was a temporary move player then remove it
			if (end.occupyingUnit.transform.Find("Temporary") != null){
				Destroy(end.occupyingUnit.transform.Find("Temporary").gameObject);
			}
			cursor.selectedSpace = end;
			BattleLog.GetLog().Log(pc.unitName + " moved.");
		}
		
		public void MoveUnit(Vector2 start, Vector2 end){
			/**Moves the unit on the BoardSpace at coordinates start to the BoardSpace at coordinates end, if any.*/
			MoveUnit(this.GetSpace(start), this.GetSpace(end));
		}

		public bool CanMoveThrough(Vector2Int previous, Vector2Int next, PlayerController pc){
			/**Returns true if and only if a unit could move directly from previous to next
			without going through the omitted middle point, assuming that the movement is possible at all.!--*/
			int minX = Math.Min(previous.x, next.x);
			int maxX = Math.Max(previous.x, next.x);
			int minY = Math.Min(previous.y, next.y);
			int maxY = Math.Max(previous.y, next.y);

			float minHeight = GetSpace(previous).GetHeight();
			float maxHeight = minHeight;
			float height;
			BoardSpace space;
			for (int i = minX; i <= maxX; i++){
				for (int j = minY; j <= maxY; j++){
					space = GetSpace(new Vector2Int(i, j));
					height = space.GetHeight();
					minHeight = (height < minHeight) ? height : minHeight;
					maxHeight = (height > maxHeight) ? height : maxHeight;
					if (space.impassable
                        || (space.occupyingUnit != null && !armyManager.IsFriendly(space.occupyingUnit.GetComponent<PlayerController>().affiliation, pc.affiliation))){
						return false; //can't go through if any space is impassable
					}
				}
			}
			if (maxHeight - minHeight > pc.jumpHeight){
				return false; //can't go through if the terrain is too uneven
			}
			return true;
		}

		public List<BoardSpace> CollapsePath(List<BoardSpace> path, PlayerController pc){
			/**Returns a collapsed version of the path path, along which the
			unit can still move. The original path is modified.!--*/
			List<BoardSpace> toRemove;
			bool removedBend;
			Vector2 previousDirection;
			Vector2 nextDirection;
			if (path.Count <= 2){
				return path; //if the path is too short, do not attempt to reduce it furthers
			}
			//try to smooth bends from the path
			do {
				removedBend = false;
				toRemove = new List<BoardSpace>();
				for (int x=1; x < path.Count - 1; x++){
					previousDirection = path[x].boardPosition - path[x-1].boardPosition;
					nextDirection = path[x+1].boardPosition - path[x].boardPosition;
					if (Vector2.Angle(previousDirection, nextDirection) > 1.0f && CanMoveThrough(Vector2Int.RoundToInt(path[x-1].boardPosition),
																									   Vector2Int.RoundToInt(path[x+1].boardPosition),
																									   pc)){
						toRemove.Add(path[x]);
						x++; //skip the next one, because its previous space (x-1) will have been removed.
						removedBend = true;
					}
				}
				foreach (BoardSpace space in toRemove){
					path.Remove(space);
				}
				if (path.Count <= 2){
					return path; //if the path becomes too short, do not attempt to reduce it furthers
				}
			} while (removedBend);
			return path;
		}
		
		public void TempMoveUnit(BoardSpace start, BoardSpace end){
			/**Creates a temporary visualization of the unit at start and moves it to the end position.*/
			if (start.occupyingUnit == null){
				Debug.Log("no unit to move");
				return;
			}
			if (end.occupyingUnit != null){
				Debug.Log("cannot move to a used space");
				return;
			}
			GameObject tempPlayer = Instantiate(start.occupyingUnit, start.occupyingUnit.transform);
			tempPlayer.transform.localEulerAngles = new Vector3(0, 0, 0);
			tempPlayer.transform.localScale = new Vector3(1, 1, 1);
			tempPlayer.name = "Temporary";
			tempPlayer.GetComponent<PlayerController>().MakeSemiTransparent(true);
			tempPlayer.transform.position = end.anchorPosition;
		}
		
		public void TempMoveUnit(Vector2 start, Vector2 end){
			/**Creates a temporary visualization of the unit at start and moves it to the end position.*/
			TempMoveUnit(this.GetSpace(start), this.GetSpace(end));
		}
		
		public void KnockbackMoveUnit(BoardSpace start, BoardSpace end, bool throughUnits){
			/**Moves a unit from start to end, going through units if specified.
			Used to move a unit due to a skill from applied knockback.*/
			BoardManager board = BoardManager.GetBoard();
			PlayerController pc = start.occupyingUnit.GetComponent<PlayerController>();
			float startPos;
			float endPos;
			float currentHeight = start.GetHeight();
			BoardSpace nextSpace;
			BoardSpace lastAttainable = start;
			
			if (start.boardPosition.x == end.boardPosition.x){
				startPos = start.boardPosition.y;
				endPos = end.boardPosition.y;
				for (float y = startPos + 1; y < endPos; y++){
					nextSpace = board.GetSpace(new Vector2(start.boardPosition.x, y));
					if (nextSpace.impassable || Math.Abs(currentHeight - nextSpace.GetHeight()) > pc.jumpHeight){
						break; //there is something preventing movement through it, stop there
					} else {
						if (nextSpace.occupyingUnit != null && throughUnits == false){
							break; //if cannot move through units, stop
						}
						currentHeight = nextSpace.GetHeight();
						lastAttainable = nextSpace; //keep going
					}
				}
				
			} else if (start.boardPosition.y == end.boardPosition.y){ 
				startPos = start.boardPosition.x;
				endPos = end.boardPosition.x;
				for (float x = startPos + 1; x < endPos; x++){
					nextSpace = board.GetSpace(new Vector2(x, start.boardPosition.y));
					if (nextSpace.impassable || Math.Abs(currentHeight - nextSpace.GetHeight()) > pc.jumpHeight){
						break; //there is something preventing movement through it, stop there
					} else {
						if (nextSpace.occupyingUnit != null && throughUnits == false){
							break; //if cannot move through units, stop
						}
						currentHeight = nextSpace.GetHeight();
						lastAttainable = nextSpace; //keep going
					}
				}
			}
			
			//move to last attainable space along the path
			lastAttainable.occupyingUnit = start.occupyingUnit;
			start.occupyingUnit = null;
			lastAttainable.occupyingUnit.transform.position = lastAttainable.anchorPosition; //lerp here
			pc.boardPosition = lastAttainable.boardPosition;
		}
		
		public void SkillMoveUnit(Skill skill, BoardSpace start, Quaternion direction){
			/**Moves the unit from space start according to the movement of skill used in direction.
			Assumes that the movement is possible.*/
			if (skill.movePosition == Vector2.zero){
				return; //don't need to move the unit
			}
			BoardSpace end = GetSpace(start.boardPosition + (Vector2) (direction * (Vector3) skill.movePosition));
			PlayerController pc = start.occupyingUnit.GetComponent<PlayerController>();
			end.occupyingUnit = start.occupyingUnit;
			start.occupyingUnit = null;
			end.occupyingUnit.transform.position = end.anchorPosition; //lerp here
			pc.boardPosition = end.boardPosition;
			cursor.selectedSpace = end;
		}
		
		public void RefreshUnits(UnitAffiliation toRestore){
			/**Called at the end of turn to allow all units a new turn.*/
			for (int i = 0; i < columns; i++){
				for (int j = 0; j < rows; j++){
					BoardSpace space = GetSpace(new Vector2(i, j));
					if (space.occupyingUnit != null){
						PlayerController pc = space.occupyingUnit.GetComponent<PlayerController>();
						if (pc.affiliation == toRestore){
							RefreshUnit(pc);
						}
					}
				}
			}
		}

		private void RefreshUnit(PlayerController pc){
			pc.turnEnded = false;
			pc.previousAction = UnitAction.WAIT;
			pc.hasActed = false;
			pc.remainingActions = pc.maxActions;
			pc.remainingMove = pc.moveRange;
			Debug.Log("refreshed " + pc.unitName);
		}

		public bool AdvancePhase(){
			List<PlayerController> actingUnits = new List<PlayerController>(from player in players
				where player.affiliation == phase
				select player);
			foreach (PlayerController unit in actingUnits){
				if (!unit.turnEnded){
					//at least one unit has not acted but still can
					//Debug.Log("unit can still act");
					return false;
				}
			}
			//all units that can act on this phase have acted
			RemoveDeadUnits();
			BattleLog log = BattleLog.GetLog();
			log.Log(phase + " phase ended.");
			RefreshUnits(phase);
			do {
				phase = phase.Next();
			} while (!(from player in players select player.affiliation).Contains(phase)); //if the phase is empty, go to the next one immediately
			log.Log(phase + " phase started.");
			if (phase == UnitAffiliation.ENEMY){
				StartCoroutine(autoMoveController.AutoMovePhase(phase));
			}
			return true;
		}

		private bool ArmyDefeated(UnitAffiliation army){
			List<PlayerController> standingUnits = new List<PlayerController>(from player in players
				where player.affiliation == army
				select player);
			return (standingUnits.Count == 0);
		}

		public void CheckEndMission(){
			/**Ends the mission if needed.*/
			if (IsVictory(UnitAffiliation.PLAYER, victory, destinationSpaces) || IsVictory(UnitAffiliation.ENEMY, defeat, defendedSpaces)){
				EndMission(); 
			}
		}

		private void EndMission(){
			/**Ends the current mission and exits to the mission select screen, updating the mission's
			status if needed.*/
			bool isVictory = false;

			SaveRemainingUnits();
			if (IsVictory(UnitAffiliation.PLAYER, victory, destinationSpaces)){
				isVictory = true;
			}
			Mission updated = SaveManager.GetSaveManager().currentMission;
			updated.completed = updated.completed || isVictory;
			InfoObject info = InfoObject.Create();
			info.missionToUpdate = updated;
			SceneManager.LoadScene("Scenes/battleselect", LoadSceneMode.Single);
		}

		public void RemoveDeadUnits(){
			for (int x = 0; x < columns; x++){
				for (int y = 0; y < rows; y++){
					BoardSpace space = GetSpace(new Vector2(x, y));
					if (space.occupyingUnit != null){
						PlayerController pc = space.occupyingUnit.GetComponent<PlayerController>();
						if (pc.health.currentHealth <= 0){
							RemoveUnit(pc);
						}
					}
				}
			}
		}

		private void RemoveUnit(PlayerController pc){
			BoardSpace space = GetSpace(pc.boardPosition);
			if (space == cursor.selectedSpace){
				cursor.Deselect();
			}
			space.occupyingUnit = null;
			units.Remove(pc.gameObject);
			log.Log(pc.unitName + " died.");
			// show death dialogue or other effects here if needed
			// for player units, write info back to units file if applicable
			if (pc.saveAfterBattle){
				toSave.Add(pc);
			}
			Destroy(pc.gameObject);
		}

		public static BoardManager GetBoard(){
			return GameObject.FindWithTag("Board").GetComponent<BoardManager>();
		}

		public void InitializeUnits(List<GameObject> units){
			/**Places the specified units at their respective positions.*/
			foreach (GameObject unit in units){
				PlayerController pc = unit.GetComponent<PlayerController>();
				Vector2 position = pc.boardPosition;
				GetSpace(position).occupyingUnit = unit;
				unit.transform.position = GetSpace(position).anchorPosition;
			}
		}

		public void InitializeMap(List<BoardSpace> spaces){
			//find and delete the previous map object if there is one
			GameObject mapInstance = GameObject.FindWithTag("MapModel");
			if (mapInstance != null){
				Destroy(mapInstance);
			}
			//create the new map object
			GameObject modelObject = (GameObject) Resources.Load("MapModels/" + modelName);
			mapInstance = Instantiate(modelObject);
			mapInstance.tag = "MapModel";
			boardSpaces = new BoardSpace[columns, rows];
			boardTiles = new GameObject[columns, rows];
			foreach (BoardSpace space in spaces){
				int i = (int) space.boardPosition.x;
				int j = (int) space.boardPosition.y;
				boardSpaces[i, j] = space;
				AddTile(boardSpaces[i, j]);
			}
			ToggleOutlines(true);
		}

		public void SaveRemainingUnits(){
			PlayerController pc;
			foreach (GameObject unit in units){
				pc = unit.GetComponent<PlayerController>();
				if (pc.saveAfterBattle){
					toSave.Add(pc);
				}
			}
		}

		private bool IsVictory(UnitAffiliation phase, EndCondition conditions, List<BoardSpace> targetSpaces = null){
			/**Returns true if and only if the current board is a victory for the player's army,
			according to the conditions for victory.!--*/
			if (targetSpaces == null){
				targetSpaces = new List<BoardSpace>();
			}
			List<Func<bool>> predicates = new List<Func<bool>>(); //each of these predicates returns true if and only if it is a victory
			IEnumerable<UnitAffiliation> opposingArmies = (from army in (UnitAffiliation[]) Enum.GetValues(typeof(UnitAffiliation))
					where !armyManager.IsFriendly(army, phase) select army);
			if (conditions.HasFlag(EndCondition.ON_ROUT)){ //if all enemy armies are destroyed
				predicates.Add(() => (from army in opposingArmies select ArmyDefeated(army)).All(x => x));
			}
			if (conditions.HasFlag(EndCondition.ON_DEFEAT_COMMANDER)){ //if there is no enemy commander left in any army
				predicates.Add(() => (from army in opposingArmies select (from enemy in players where enemy.affiliation == army && enemy.isCommander select enemy).Count() == 0).All(x => x));
			}
			if (conditions.HasFlag(EndCondition.ON_TURN_LIMIT)){ //if the turn limit has been reached
				predicates.Add(() => currentTurn >= turnLimit);
			}
			if (conditions.HasFlag(EndCondition.ON_REACH_DESTINATION)){ //if anyone has reached the destination
				predicates.Add(() => (from player in players where player.affiliation == phase
                                                       && targetSpaces.Contains(GetSpace(player.boardPosition)) select player).Count() == 0);
			}
			foreach (Func<bool> predicate in predicates){
				if (predicate()){
					return true;
				}
			}
			return false;
		}
		
		public static void ClearVisualization(){
			/**Clears all visualizations for skills, movement, etc.*/
			foreach (GameObject vis in GameObject.FindGameObjectsWithTag("Visualization")){
				Destroy(vis);
			}
			BoardManager.GetBoard().HighlightSpaces(null);
		}

		public IEnumerator WhenUnitStops(Action toDo){
			/**Executes the given action once no unit is moving around the board.!--*/
			yield return new WaitWhile(() => this.movingUnit);
			toDo();
			yield return null;
		}
	}
}