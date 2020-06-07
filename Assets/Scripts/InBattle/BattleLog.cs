using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InBattle{    
    public class BattleLog : MonoBehaviour
    {
        public Text text;
        List<string> messageLog;
        void Start()
        {
            Clear();
        }

        void Update()
        {
            
        }

        public void Clear(){
            messageLog = new List<string>();
            UpdateText();
        }

        public void Log(string message){
            messageLog.Add(message);
            UpdateText();
        }

        public void Log(List<string> messages){
            messageLog.AddRange(messages);
            UpdateText();
        }

        void UpdateText(){
            string log = "";
            foreach (string message in messageLog){
                log += message + "\n";
            }
            log = log.TrimEnd(Environment.NewLine.ToCharArray());
            text.text = log;
            StartCoroutine(GoToBottom());
        }

        private IEnumerator GoToBottom(){
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            Scrollbar scrollbar = this.gameObject.transform.Find("Log").Find("Scrollbar").GetComponent<Scrollbar>();
            scrollbar.value = 0;
        }
        
        public static BattleLog GetLog(){
            return GameObject.FindWithTag("Log").GetComponent<BattleLog>();
        }

    }
}