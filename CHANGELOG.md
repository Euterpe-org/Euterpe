| Versions        | | | |
|-----------------| ------ | ------ | ------ |
|  |  |  | [v1.3.0](#v130) |
| [v1.2.7](#v127) | [v1.2.6](#v126) | [v1.2.5](#v125) | [v1.2.4](#v124) |
| [v1.2.3](#v123) | [v1.2.2](#v122) | [v1.2.1](#v121) | [v1.2.0](#v120) |
| [v1.1.2](#v112) | [v1.1.0](#v110) | [v1.0.9](#v109) | [v1.0.8](#v108) |
| [v1.0.7](#v107) | [v1.0.6](#v106) | [v1.0.5](#v105) | [v1.0.4](#v104) |
| [v1.0.3](#v103) | [v1.0.2](#v102) | [v1.0.1](#v101) | [v1.0.0](#v100) |


---

## v1.3.0

### Changes

1. Mod description localization (Chinese Simplified, Chinese Traditional, Hungarian, Russian, Spanish)
2. Use system default font on first launch

---

## v1.2.7

### Bug Fixes

1. Fix buttons in mod manage page not working

---

## v1.2.6

### Changes

1. Translation Updates (Fully translated into Korean and Russian)
2. About page update (Add translators)
3. Dependency updates

---

## v1.2.5

### Changes

1. Fully translated into Chinese Traditional, Hungarian, Korean, and Spanish
2. Slightly redesign chart download page
3. Adjust UI's height
4. Use ItemsRepeater in chart download page, now it loads at least 10 times faster
5. Only check once for ConfigFolder and Charts folder exist

---

## v1.2.4

### Changes
1. **With the "Chart" tab, you can sort, filter, search and download charts directly**
2. If you set the download source to custom, you can use the custom mod links
3. Mod Tools can load mods when offline but can't download mods, of course

### Bug Fixes

1. Fix the saved language not working bug
2. Add bug report button in mod window
3. Fix the window cannot be dragged and move
4. Fix no and don't ask again option for installing mod tools mod

---

## v1.2.3

### Bug Fixes

1. Adjust version text position
2. Fix about tab name doesn't change when switching languages
3. Fix [#130](https://github.com/MDModsDev/MuseDashModToolsUI/issues/130) Setting.json now saved when exiting the program, and won't cause errors when saving
4. Fix MelonLoader install

---

## v1.2.2

### Changes

1. Translation Updates(Hungarian, Spanish, Russian)
2. Now on first launch it will try to auto detect your game path
3. Prevent multiple launch so you can only have one MDMT instance running at the same time
4. Use System.Text.Json instead of Newtonsoft.json, which is faster
5. Delete log files if there are more than 60 in Logs folder
6. Fix for recursive initialize tabs, largely improve performance
7. Add About page instead of popup, add translator credits

---

## v1.2.1

### Bug Fixes

1. Fix check update & read MuseDash.exe version causes programs not working on Linux
2. Setting page use scroll viewer to have more space
3. Try to fix auto updater for Linux
4. Bump Material.Avalonia to 3.0.0
5. Some translation update (Traditional Chinese & Spanish)

---

## v1.2.0

### Changes

1. Add more tabs so that users can switch between different tabs (Current tabs are ModManage, LogAnalysis and Settings)
2. You can change languages and font at runtime in the Settings tab
3. You can configure various settings on your own (Download Source, pop up to ask you to enable other mods etc)
4. Log analyzer for detecting pirate, incorrect MelonLoader version, and outdated mods version
5. You can toggle the download prerelease button for future prerelease download

---

## v1.1.2

### Changes

1. Update Translation for Chinese Traditional, Hungarian, Russian, Spanish

---

## v1.1.0

### Bug Fixes

1. Shows red name for incompatible mod and blue for duplicated mod
2. Fix update program doesn't work (I forgot to write .zip at the end)
3. Now, except github itself, muse dash mod tools have 2 more github mirror links to download files.
4. Add file system watcher to refresh the mod list when there's something change with the mods in the folder (this also causes many refreshes when install/uninstall/toggle mods which is fixed now)
5. Localization (now English and Chinese Simplified only)

---

## v1.0.9

### Changes

1. change github mirror to gitee and ghproxy
2. localization for download window message

---

## v1.0.8

### Changes

1. Support Chinese Simplified Localization

---

## v1.0.7

### Changes

1. Add file system watcher to monitor the file changes in mods folder and refresh the mod list.
2. The update shows the GitHub release info



### Bug fixes

Fix choose path command.

---

## v1.0.6

### Changes

Add one more github mirror link

---

## v1.0.5

### Bug Fixes

Fix for you can reinstall the incompatible mod after uninstalling it

---

## v1.0.4

### Bug Fixes

Fix updater github download link

---

## v1.0.3

### Bug Fixes

Fix description, launch exe after update

---

## v1.0.2

### Bug Fixes

Fix update program doesn't work

---

## v1.0.1

### Changes

display red with incompatible mod name
ask whether you want to update the ui at once

---

## v1.0.0

First Release
