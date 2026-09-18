# Enhanced Vehicle Lighting Controls
## Description
Get more control over your vehicle! This mod allows you to silence sirens with one key press, Toggle your headlights between full and low beams, Toggle the interior light and actually use your indicators!

### *Controller Support*:
This mod supports controller input with the use of a modifier button, which by default is the Left Bumper. To change the button binding, See the [Mod Settings] section of the .ini file.
Keep in mind that whilst holding the modifier button, only some basic driving functions are enabled.

---

## Requirements:
- GTA V (PC)
- AB ScriptHookV [Available Here](http://www.dev-c.com/gtav/scripthookv/)
- ScriptHookVDotNet [Available Here](https://www.gta5-mods.com/tools/scripthookv-net)

---

## Installation
- Navigate to your GTA V root folder.
- Copy the /scripts folder into the directory.

---

## Controls
### Default Bindings:
#### *Keyboard*:
- **Siren Toggle**: Tab
- **Beam Toggle**: CapsLock
- **Interior Light**: I
- **Left Indicator**: Left
- **Right Indicator**: Right
- **Hazard lights**: Down 
    
#### *Controller*:
- **Modifier Button**: LB 
- **Siren Toggle**: DPadDown
- **Beam Toggle**: X
- **Interior Light Toggle**: Y
- **Left Indicator**: DPadLeft
- **Right Indicator**: DPadRight
- **Hazard lights**: DPadUp

#### *Custom Bindings*:
You can change the default controls in the .ini file. 
See '\docs\ControllerButtonList.pdf' for a list of valid button binds.

--- 

## Changelog
- **v0.4.1**: Add ability to toggle interior light with controller. 
- **v0.4.0**: Improved controller support.
- **v0.3.0**: Add hazard light features.
- **v0.2.0**: Add basic controller support.

---

## Known Bugs and Issues:
- Sometimes when the mod first loads the input is unresponsive; To fix, simply reload the script via the ScriptHookVDotNet console (F4 by default). I've only noticed this while using LSPDFR.

---

## Developers and Support:
The source code can be found [on GitHub](https://github.com/MccDev260/EnhancedVehicleLightingControls). 

If you would like to extend or improve this project, feel free to submit a pull request!

To report a bug, open a new issue with the 'bug' label.

If you have an idea for a feature you would like to see in a future release, open a new issue with the 'enhancement' label.

### How to auto copy the .dll to your scripts folder
Working on this project and sick of dragging and dropping the output .dll into your scripts folder for testing?

Find `Local.Build.props.example` file in the repos root.

1. Duplicate the file.

2. Delete `.example` extension so it becomes `Local.Build.props`

3. Open the file and find this property:
```
<PostBuildEvent>
      copy /Y "$(TargetPath)" "Path\To\GTAV\Scripts\Folder"
</PostBuildEvent>
```

4. Replace the `Path\To\GTAV\Scripts\Folder` placeholder with the path to your scripts folder in your GTAV directory. It should look like the following example:
```
<PostBuildEvent>
      copy /Y "$(TargetPath)" "C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V\scripts"
</PostBuildEvent>
```

Now each time the project is built in visual studio, it will auto copy the .dll to that directory.

**Notes and Warnings**

* The example `Local.Build.props.example` file should not be edited directly as Visual Studio does not know it exists, so therefore won't work.
* Your `Local.Build.props` file should **not** be included in commits and should stay local to your machine.
* Remember to reload ScriptHookDotNet from the in-game console with the `Reload()` command after each build.