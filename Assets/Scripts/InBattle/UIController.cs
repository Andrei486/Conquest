using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Objects;

namespace InBattle{
    public class UIController: MonoBehaviour
    {
        public GameObject unitInfoPrefab;
        public GameObject unitSummaryPrefab;
        private GameObject unitInfoBox;
        private GameObject unitSummaryBox;
        private ControlsManager controls;
        private BoardManager board;

        void Start(){
            controls = ControlsManager.GetControls();
        }
        void Update(){
        }
        public GameObject ShowUnitInfo(PlayerController unit){
            /**Shows detailed information about the specified unit.!--*/
            if (unitInfoBox != null){
                ClearUnitInfo(); //remove previous unit info if it exists
            }
            Transform section;

            unitInfoBox = Instantiate(unitInfoPrefab, this.gameObject.transform);
            UIController.SetTheme(unitInfoBox.transform, unit.affiliation);
            //fill general info
            section = unitInfoBox.transform.Find("General");
            section.Find("Unit Name").GetComponent<Text>().text = unit.name;
            section.Find("Class Title").GetComponent<Text>().text = unit.classTitle;
            section.transform.Find("Level").GetComponent<Text>().text = unit.health.level.ToString();

            //show sprites if possible
            if (unit.unitSprite != null){
                section.Find("Unit Emblem").GetComponent<Image>().sprite = unit.unitSprite;
            } else {
                section.Find("Unit Emblem").GetComponent<Image>().enabled = false;
            }

            if (unit.armySprite != null){
                section.Find("Affiliation Emblem").GetComponent<Image>().sprite = unit.armySprite;
            } else {
                section.Find("Affiliation Emblem").GetComponent<Image>().enabled = false;
            }

            //fill stats
            section = unitInfoBox.transform.Find("Stats");
            section.Find("Current HP").GetComponent<Text>().text = string.Format("{0}/{1}",
                                                                                Math.Ceiling(unit.health.currentHealth),
                                                                                Math.Ceiling(unit.health.maxHealth));
            section.transform.Find("HP Bar").GetComponent<Slider>().value = unit.health.currentHealth / unit.health.maxHealth;
            section.Find("Attack").GetComponent<Text>().text = unit.health.attackPower.ToString();
            section.Find("Defense").GetComponent<Text>().text = unit.health.defense.ToString();
            section.Find("Movement").GetComponent<Text>().text = unit.moveRange.ToString();

            //fill remaining actions, move, etc
            section = unitInfoBox.transform.Find("Remaining");
            section.Find("Remaining Movement").GetComponent<Text>().text = string.Format("{0}/{1}",
                                                                                        unit.remainingMove,
                                                                                        unit.moveRange);
            section.Find("Remaining Actions").GetComponent<Text>().text = string.Format("{0}/{1}",
                                                                                        unit.remainingActions,
                                                                                        unit.maxActions);
            section.Find("Remaining Bullets").GetComponent<Text>().text = string.Format("{0}/{1}",
                                                                                        unit.bullets,
                                                                                        unit.maxBullets);
            return unitInfoBox;
        }

        public GameObject ShowUnitSummary(PlayerController unit){
            /**Shows a brief summary of the unit's stats and information.
            Use when hovering over a unit.!--*/
            if (unitSummaryBox != null){
                Destroy(unitSummaryBox); //remove previous unit summary if it exists
            }
            unitSummaryBox = Instantiate(unitSummaryPrefab, GameObject.FindWithTag("MainCanvas").transform);
            if (!unit.turnEnded){
                UIController.SetTheme(unitSummaryBox.transform, unit.affiliation);
            }

            unitSummaryBox.transform.Find("Unit Name").GetComponent<Text>().text = unit.name;
            unitSummaryBox.transform.Find("Current HP").GetComponent<Text>().text = string.Format("{0}/{1}",
                                                                                                Math.Ceiling(unit.health.currentHealth),
                                                                                                Math.Ceiling(unit.health.maxHealth));
            unitSummaryBox.transform.Find("HP Bar").GetComponent<Slider>().value = unit.health.currentHealth / unit.health.maxHealth;
            unitSummaryBox.transform.Find("Unit Name").GetComponent<Text>().text = unit.name;
            unitSummaryBox.transform.Find("Level").GetComponent<Text>().text = unit.health.level.ToString();

            unitSummaryBox.transform.Find("Movement").GetComponent<Text>().text = string.Format("{0}/{1}",
                                                                                        unit.remainingMove,
                                                                                        unit.moveRange);
            unitSummaryBox.transform.Find("Actions").GetComponent<Text>().text = string.Format("{0}/{1}",
                                                                                        unit.remainingActions,
                                                                                        unit.maxActions);

            //show sprites if possible
            if (unit.unitSprite != null){
                unitSummaryBox.transform.Find("Unit Emblem").GetComponent<Image>().sprite = unit.unitSprite;
                unitSummaryBox.transform.Find("Unit Emblem").GetComponent<Image>().enabled = true;
            } else {
                unitSummaryBox.transform.Find("Unit Emblem").GetComponent<Image>().enabled = false;
            }

            if (unit.armySprite != null){
                unitSummaryBox.transform.Find("Affiliation Emblem").GetComponent<Image>().sprite = unit.armySprite;
                unitSummaryBox.transform.Find("Affiliation Emblem").GetComponent<Image>().enabled = true;
            } else {
                unitSummaryBox.transform.Find("Affiliation Emblem").GetComponent<Image>().enabled = false;
            }
            return unitSummaryBox;
        }

        public GameObject ShowUnitSummary(BoardSpace space){
            if (space.occupyingUnit == null){
                //if no unit, clear the current summary box and don't replace it with anything
                Destroy(unitSummaryBox);
                return null;
            } else {
                //if there is a unit, show its summary
                unitSummaryBox = ShowUnitSummary(space.occupyingUnit.GetComponent<PlayerController>());
                return unitSummaryBox;
            }
        }

        public static UIController GetUI(){
            return GameObject.FindWithTag("Global").GetComponent<UIController>();
        }

        public static void SetTheme(Transform uiElement, UnitAffiliation theme){
            /**Recursively sets the theme of the given UI element and all its children to match
            the given UnitAffiliation's theme.!-- */

            //set color of image as needed
            if (uiElement.CompareTag("UIBorder")){
                uiElement.GetComponent<Image>().color = theme.BorderColor();
            }
            if (uiElement.CompareTag("UIBackground")){
                uiElement.GetComponent<Image>().color = theme.BgColor();
            }
            //look for children that need to be recolored.
            if (uiElement.childCount == 0){
                return;
            }
            foreach (Transform uiChild in uiElement){
                SetTheme(uiChild, theme);
            }
            return;
        }

        public bool ShowingInfo(){
            return (unitInfoBox != null);
        }

        public void ClearUnitInfo(){
            Destroy(unitInfoBox);
        }
    }}