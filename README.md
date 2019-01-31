# UnityStandard
Set of commonly used Unity assets and scripts to start development quickly

## Scripts

### DebugToUI

Add to a UI->Text component to display Debug.Log() messages in-game.
Configure the Text component (recommended):
- Alignment: Left + Bottom
- Horizontal Overflow: Wrap
- Vertical Overflow: Truncate
- Color: Something w/ high contrast in your scene

Hint: Set the Fade Time to zero to disable it.

## Packages

### Giovanni

Contains a simple test character with a basic controller script

## Tips/Tricks/Hacks

### Getting Unity dark mode

1. Install HxD
2. Run in admin-mode and open Unity.exe
3. Search for `84 C0 75 08 33 C0 48 83 C4 20 5B C3 8B 03 48 83 C4 20 5B C3` in hex mode
4. Replace `75` with `74`

Tested with Unity 2018.2.7f1
