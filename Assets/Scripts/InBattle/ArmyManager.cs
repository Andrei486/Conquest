using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using UnityEngine;
using Objects;
using Newtonsoft.Json;

namespace InBattle{
    public class ArmyManager{
        [JsonProperty]
        Dictionary<UnitAffiliation, HashSet<UnitAffiliation>> conflictGraph = new Dictionary<UnitAffiliation, HashSet<UnitAffiliation>>();

        public bool IsFriendly(PlayerController unit1, PlayerController unit2){
            /**Returns true if and only if unit1's affiliation is friendly to unit2's.
            Friendly affiliations can pass through each other's spaces and do not try to attack each other.!--*/
            return IsFriendly(unit1.affiliation, unit2.affiliation);
        }

        public bool IsFriendly(UnitAffiliation unit1, UnitAffiliation unit2){
            /**Returns true if and only if unit1's affiliation is friendly to unit2's.
            Friendly affiliations can pass through each other's spaces and do not try to attack each other.!--*/
            return !conflictGraph[unit1].Contains(unit2);
        }

        public void SetDefault(){
            conflictGraph = new Dictionary<UnitAffiliation, HashSet<UnitAffiliation>>();
            conflictGraph.Add(UnitAffiliation.PLAYER, new HashSet<UnitAffiliation>{UnitAffiliation.ENEMY, UnitAffiliation.OTHER});
            conflictGraph.Add(UnitAffiliation.ENEMY, new HashSet<UnitAffiliation>{UnitAffiliation.PLAYER, UnitAffiliation.ALLY, UnitAffiliation.OTHER});
            conflictGraph.Add(UnitAffiliation.ALLY, new HashSet<UnitAffiliation>{UnitAffiliation.ENEMY, UnitAffiliation.OTHER});
            conflictGraph.Add(UnitAffiliation.OTHER, new HashSet<UnitAffiliation>{UnitAffiliation.PLAYER, UnitAffiliation.ENEMY, UnitAffiliation.ALLY});
        }
    }
}