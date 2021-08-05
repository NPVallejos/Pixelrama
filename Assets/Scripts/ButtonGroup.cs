using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonGroup : MonoBehaviour
{
    // Can contain an arbitrary number of buttons
    // Buttons toggling on and off with cause subsequent buttons to turn on and off
    public List<Button> buttons;
    public int currentIndex = 0;

    // WARNING: The order in which you place your buttons inside the List AND the currentIndex will mess with the switch tiles
    // It is strongly suggested that the button that should be up at start be placed last in the list and that the ucrrentIndex be set to the last element in the list
    private void Start() {
        if (buttons != null) {
            for (int i = 0; i < buttons.Count; i++) {
                if (i < currentIndex || i > currentIndex) {
                    buttons[i].ToggleButtonDown(false);
                }
                else {
                    buttons[i].ToggleButtonUp(true);
                }
            }
        }
    }

    private void FixedUpdate() {
        if (buttons.Count != 0) {
            if (buttons[currentIndex].isActivated) {
                currentIndex = (currentIndex + 1) % buttons.Count;
                buttons[currentIndex].ToggleButtonUp(false);
            }
        }
    }
}
