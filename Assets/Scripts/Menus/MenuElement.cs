using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Menus{
    public class MenuElement : MonoBehaviour{
        /**Class that represents an element in a menu, whether it be a container for other elements,
        something that can be hovered over or interacted with, or so on.!--*/
        [HideInInspector] public MenuElement parent;
        public MenuElement up;
        public MenuElement down;
        public MenuElement left;
        public MenuElement right;
        [HideInInspector] public List<MenuElement> children;
        public bool isActive = false;
        void Awake(){
            //set parent and children
            parent = (transform.parent.GetComponent<MenuElement>() != null) ? transform.parent.GetComponent<MenuElement>() : null;
            children = new List<MenuElement>();
            foreach (Transform child in transform){
                if (child.GetComponent<MenuElement>() != null){
                    children.Add(child.GetComponent<MenuElement>());
                }
            }
        }
        void Start(){

        }
        void Update(){

        }
        public virtual void OnSelect(MenuCursor cursor){
            /**Called when a MenuCursor selects this element while hovering on it.!--*/
            //default behavior is to move cursor to first valid child
            if (children.Count > 0){
                cursor.activeElement = this;
                cursor.hoveredElement = children[0];
            }
            return;
        }
        public virtual void OnHover(MenuCursor cursor){
            /**Called when a MenuCursor moves onto this element.!--*/
            return;
        }
        public virtual void OnUnhover(MenuCursor cursor){
            /**Called when a MenuCursor moves off of this element.
            Used to undo effects created in OnHover, if any.!--*/
            return;
        }
        public virtual void OnBack(MenuCursor cursor){
            /**Called when a MenuCursor tries to move back while this element is active.!--*/
            if (parent != null){
                cursor.activeElement = parent;
                cursor.hoveredElement = this;
            }
        }

        // private MenuElement FindElementInDirection(Vector2 direction){
        //     /**Returns the MenuElement that should be mapped to a given cursor direction for this element.
        //     The vector direction is expected to be one of Vector2.up, Vector2.down, Vector2.left, and Vector2.right.!--*/
        //     Vector2 thisCenter = GetComponent<RectTransform>().rect.center;
        //     Vector2 siblingCenter;
        //     List<MenuElement> validSiblings = new List<MenuElement>();
        //     foreach (MenuElement sibling in parent.children){
        //         siblingCenter = sibling.GetComponent<RectTransform>().rect.center;

        //     }
        //     return null;
        // }
        
        // private bool NothingBetween(MenuElement target, int precision = 50, float tolerance = 0.1f){
        //     /**Returns true if and only if none of this element's siblings are between it and the target.!--*/
        //     Vector2 thisCenter = GetComponent<RectTransform>().rect.center;
        //     Vector2 targetCenter = target.GetComponent<RectTransform>().rect.center;
        //     Rect siblingRect;
        //     float step = 1.0f / precision;
        //     int errors = 0;
        //     foreach (MenuElement sibling in parent.children){
        //         if (sibling != target){
        //             siblingRect = sibling.GetComponent<RectTransform>().rect;
        //             errors = 0;
        //             for (float t = 0; t <= 1.0f + step; t += step){
        //                 Vector2 point = Vector2.Lerp(thisCenter, targetCenter, t);
        //                 if (siblingRect.Contains(point)){
        //                     errors++;
        //                 }
        //             }
        //             if (errors > tolerance * precision){
        //                 return false; //if too many points are inside other elements, there is something between this and the target
        //             }
        //         }
        //     }
        //     return true; //if nothing was found, nothing in between
        // }
    }
}