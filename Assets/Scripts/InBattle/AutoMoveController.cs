using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Objects;

namespace InBattle{
    public class AutoMoveController : MonoBehaviour{
        [SerializeField]
        BoardManager board;
        bool movingUnit = false;
        void Start(){
            board = GetComponent<BoardManager>();
        }

        float ScoreMove(PlayerController unit, BoardSpace start, BoardSpace end, int[,] distanceGrid){
            /**Returns a calculated score for the potential move of unit from start to end, taking into account potential
            actions that follow. This takes into account:
            -whether the unit is advancing or retreating, considering the unit's HP;
            -the potential damage or defeat of enemies arising from the use of a skill from the target space.
            This score can be either positive or negative, with more positive scores being preferred.!--*/
            float distanceDifference = distanceGrid[(int) end.boardPosition.x, (int) end.boardPosition.y] - distanceGrid[(int) start.boardPosition.x, (int) start.boardPosition.y];
            float movementMod = (unit.autoMove.retreatHP - (unit.health.currentHealth / unit.health.maxHealth)) * distanceDifference * unit.autoMove.moveModCoefficient;
            (_, float skillMod) = SelectBestSkill(unit, end); //only consider the best action to take, not multiple
            Debug.Log(movementMod + skillMod);
            return movementMod + skillMod;
        }
        (AutoMoveAction, float) SelectBestMove(PlayerController unit, BoardSpace start, int[,] distanceGrid){
            /**Returns the best course of action for the unit unit (a move and a potential action), and the corresponding score.!--*/
            float bestScore = -Mathf.Infinity;
            BoardSpace bestSpace = null;
            //first, find the best space to move to
            Debug.Log(unit.remainingMove);
            foreach (BoardSpace space in unit.GetAccessibleSpaces((int) start.boardPosition.x, (int) start.boardPosition.y)){
                if (ScoreMove(unit, start, space, distanceGrid) > bestScore){
                    bestScore = ScoreMove(unit, start, space, distanceGrid);
                    bestSpace = space;
                }
            }
            //then find the best action, moving to that space
            return SelectBestSkill(unit, bestSpace);
        }

        float ScoreSkill(PlayerController unit, Skill skill, BoardSpace space, Quaternion direction){
            /**Returns a calculated score for the potential use of the skill by unit pointing in the given direction.
            This takes into account:
            -the damage dealt to targets of the skill, in percentage of each target's max HP;
            -the hit chance of each attack with a target, as a multiplier to the above damage;
            -the potential defeat of targets from this skill, ally or enemy;
            -whether each target defeated is a commander.
            This score can be either positive or negative, with more positive scores being preferred.!-- */
            float totalScore = 0;
            float damageScore;
            float defeatScore;
            float friendlyModifier;
            float commanderModifier;
            Vector2 newPosition;
            BoardSpace targetSpace;
            PlayerController targetPc;
            foreach (Attack attack in skill.attacks){
                damageScore = 0;
                defeatScore = 0;
                newPosition = space.boardPosition + attack.targetPosition.Rotate(direction.eulerAngles.y);
                targetSpace = board.GetSpace(newPosition);
                if (board.IsWithinBounds(newPosition) && targetSpace.occupyingUnit != null){
                    targetPc = targetSpace.occupyingUnit.GetComponent<PlayerController>();
                    friendlyModifier = (targetPc.affiliation == unit.affiliation) ? -1 : 1; //replace this with graph check for affiliation conflict
                    commanderModifier = (targetPc.isCommander) ? unit.autoMove.commanderBonus : 1;
                    damageScore = attack.CalculateAverageDamage(unit.health, targetPc.health) / targetPc.health.maxHealth * friendlyModifier;
                    defeatScore = attack.IsPotentiallyLethal(unit.health, targetPc.health) * commanderModifier * friendlyModifier;
                }
                totalScore += damageScore + defeatScore;
            }
            return totalScore;
        }

        Dictionary<AutoMoveAction, float> ScoreAllSkills(PlayerController unit, BoardSpace space){
            /**Returns a dictionary where the keys are valid actions for unit after moving to space, and the
            value for each key is the score for that action.!--*/
            Dictionary<AutoMoveAction, float> skillScores = new Dictionary<AutoMoveAction, float>();
            foreach (Skill skill in unit.GetUsableSkills()){
                foreach (Quaternion direction in skill.GetDirections(space)){
                    skillScores.Add(new AutoMoveAction(space, skill, direction), ScoreSkill(unit, skill, space, direction));
                }
            }
            return skillScores;
        }

        (AutoMoveAction, float) SelectBestSkill(PlayerController unit, BoardSpace space){
            /**Returns the best course of action for unit after moving to space, and the corresponding score.!--*/
            Dictionary<AutoMoveAction, float> skillScores = ScoreAllSkills(unit, space);
            float bestScore = 0; //not using a skill means a 0 modifier (nothing happens)
            AutoMoveAction bestAction = new AutoMoveAction(space, null, Quaternion.identity); //in case no skill is possible, can still move
            foreach (KeyValuePair<AutoMoveAction, float> action in skillScores){
                if (action.Value > bestScore){
                    bestScore = action.Value;
                    bestAction = action.Key;
                }
            }
            return (bestAction, bestScore);
        }

        public PlayerController SelectUnitToMove(UnitAffiliation phase, int[,] distanceGrid){
            /**Returns the PlayerController which can move during the given phase with the best
            possible action.!--*/
            //consider only units of that phase, which have not ended their turns yet
            List<PlayerController> validUnits = (from player in board.players where (player.affiliation == phase && !player.turnEnded) select player).ToList();
            if (validUnits.Count == 0){
                return null;
            }
            
            float bestScore = -Mathf.Infinity; //use infinitely negative score to start, that is the least preferred move
            PlayerController bestUnit = null;
            foreach (PlayerController unit in validUnits){
                (_, float score) = SelectBestMove(unit, board.GetSpace(unit.boardPosition), distanceGrid);
                if (score > bestScore){
                    bestScore = score;
                    bestUnit = unit;
                }
            }
            return bestUnit;
        }

        public IEnumerator AutoMoveUnit(PlayerController unit, int[,] distanceGrid){
            /**Automatically moves the unit unit as much as possible or needed, and takes actions during the turn.!--*/
            BoardSpace space;
            AutoMoveAction action;
            movingUnit = true;
            bool turnEnded = false;
            while (!turnEnded){
                space = board.GetSpace(unit.boardPosition);
                (action, _) = SelectBestMove(unit, space, distanceGrid);
                //if the unit should move, have it move
                Debug.Log(action);
                if (action.movementTarget != null && action.movementTarget != space){
                    board.MoveUnit(space, action.movementTarget);
                }
                yield return new WaitWhile(() => board.movingUnit);
                //don't need to check end turn here: if the unit cannot act, skillToUse will be null
                if (action.skillToUse != null){
                    unit.UseSkill(action.skillToUse, action.skillDirection);
                    unit.previousAction = UnitAction.SKILL;
				    unit.hasActed = true;
                }
                //if the best move was to do nothing, end the turn there
                if ((action.movementTarget == null || action.movementTarget == space) && action.skillToUse == null){
                    unit.EndTurn();
                    turnEnded = true;
                } else {
                    turnEnded = unit.EndTurnIfNeeded();
                }
            }
            movingUnit = false;
            yield return null;
        }

        public IEnumerator AutoMovePhase(UnitAffiliation phase){
            /**Automatically moves units of the given phase until that phase is over.!--*/
            bool wasLocked = board.locked;
            board.locked = true;
            int[,] distanceGrid;
            PlayerController toMove;
            while (board.phase == phase){
                distanceGrid = BuildDistanceGrid(phase);
                toMove = SelectUnitToMove(phase, distanceGrid);
                StartCoroutine(AutoMoveUnit(toMove, distanceGrid));
                yield return new WaitWhile(() => movingUnit);
            }
            board.locked = wasLocked;
            yield return null;
        }

        public int[,] BuildDistanceGrid(UnitAffiliation phase){
            /**Returns the distance grid for the given phase.!--*/
            List<int[,]> unitDistanceGrids = new List<int[,]>();
            foreach (PlayerController unit in (from player in board.players where (player.affiliation != phase) select player)){
                unitDistanceGrids.Add(unit.FillDistanceGrid());
            }
            int[,] fullGrid = new int[board.columns, board.rows];
            for (int i = 0; i < board.columns; i++){
                for (int j = 0; j < board.rows; j++){
                    fullGrid[i, j] = (from grid in unitDistanceGrids select grid[i, j]).Min();
                } 
            }
            File.WriteAllLines(@"D:\Users\Andrei\Conquest\Conquest\Assets\Miscellaneous\distanceGrid.txt", fullGrid
            .ToJagged()
            .Select(line => String.Join("\t", line)));

            return fullGrid;
        }
    }

    public static class ArrayExtensions {
    // In order to convert any 2d array to jagged one
    // let's use a generic implementation
    public static T[][] ToJagged<T>(this T[,] value) {
        if (System.Object.ReferenceEquals(null, value))
        return null;

        // Jagged array creation
        T[][] result = new T[value.GetLength(0)][];

        for (int i = 0; i < value.GetLength(0); ++i) 
        result[i] = new T[value.GetLength(1)];

        // Jagged array filling
        for (int i = 0; i < value.GetLength(0); ++i)
        for (int j = 0; j < value.GetLength(1); ++j)
            result[i][j] = value[i, j];

        return result;
    }
    }
}