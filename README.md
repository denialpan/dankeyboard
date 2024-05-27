# dankeyboard

## (BETA) Personal mouse and keyboard activity logger. Completely free and open-source alternative to other commercial loggers.

# FAQ:

## Privacy & Safety (READ)

### How do I know that my personal data is not compromised or sent anywhere?

This project's intent is to be a **completely free** alternative to commercial logging software. This definition of **completely free** includes not collecting any data from you, even generally regarded safe information such as telemetry, running build version, or operating system. Your privacy and data is completely yours and not modified, altered, or compromised in any way. **To ensure that all data is yours and only yours**:

- All releases, debugs, and individual commits from this official github page are completely written by only me with the intent that **none** of them will have code to add the capability for the program to access the internet.
- Data that this program receives from you is solely individual keyboard input and mouse input.
  - There is no algorithm or string detection to try to piece a concurrent input together.
  - There is no detection for what brand of hardware or software you are running.
  - There is no detection for what programs are running on your computer.
  - There is no detection of related cookies, cache, session data, etc. 
  - No files are modified, read, or written to **except** for these listed [here](#how-are-my-keyboard-and-mouse-input-saved).
- No elevated administrative powers are required for this program to function.
- The program and its related folder and files are free to permanently delete at any point.

### How are my keyboard and mouse input detected? 

This program uses `user32.dll` and `kernel32.dll`, both files which are provided on every fresh install of a Windows machine at `C:\Windows\System32` to detect when a key press or mouse click occurs. This method is chosen to detect exactly when either occur, no matter how fast the keyboard or mouse is used. Hooking onto these `.dll` files are only used and can be found in [KeyboardHook.cs](https://github.com/denialpan/dankeyboard/blob/c7c06ae4195f77d519585cf3c89514e8027e0c60/src/keyboard/KeyboardHook.cs#L281) and [MouseHook.cs](https://github.com/denialpan/dankeyboard/blob/c7c06ae4195f77d519585cf3c89514e8027e0c60/src/mouse/MouseHook.cs#L174). No modifications of the original `.dll` files are performed.

### Why did you choose this input detection method/I don't trust your detection method at all.

This detection method certainly requires a high level of trust, but as the sole developer and it being and always will be personal learning project, these `.dll` files will not be used maliciously. Because of the nature of this program to be inobtrusive and to run as a background process, hooking at a low-level is required, as opposed to alternative methods of only detecting key states while the program is focused. 

General key detection is still the same as with other GUI programs, utilizing the official [Windows API](https://learn.microsoft.com/en-us/windows/win32/inputdev/keyboard-input), where key and mouse input which will be the [same on every Windows machine](https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes), but the only addition for input to be detected while the program is minimized/out of focus is for the `.dll` files to be used.

Furthermore, there are no methods to save the literal text or string recorded, i.e. delineating the difference between a capital `A` and lowercase `a`. Only the key value provided by the [Windows API](https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes) are used.

### VirusTotal detected something malicious!

[VirusTotal](https://www.virustotal.com/) is a very good tool and using it is recommended, but similar to all virus detection tools, it will occasionally have false positives. Unfortunately, the behavior of this program is inherently similar to malicious keyloggers and other malware, especially due to the low level input detection method mentioned above. However, the similarity between this program and other keyloggers/commercial software ends here. 

### How are my keyboard and mouse input saved?

While running, **only five files** are created that the program read and writes to, excluding itself. They can be found in the `.\dankeyboard_data` or navigating to `File -> Open Current Data Directory`:
- **keys.csv** - saves key and the amount of times it has been pressed
- **mouse.csv** - saves mouse click and the amount of times it has been pressed
- **combination.csv** - saves combination type and the amount of times it has been invoked (i.e. Ctrl + C, Shift+)
- **mouse_coordinates.bin** - saves the relative coordinate that any mouse click has been performed to a monitor.
  - saves the x coordinate
  - saves the y coordinate
  - saves the monitor it was performed on as an integer
- **config.xml** - saves user configuration. Below is a list of ALL (current) settings. More to come.
  - color scheme of keyboard heatmap
  - color scheme of mouse heatmap

**All above files listed and its respective data can be read, modified, or deleted. Deletion of `.\dankeyboard_data` removes all data that the program has generated and modified. You are free to delete `dankeyboard.exe` and the ENTIRE root folder it is running in at any time without any system issues relating to above `.dll` files.** 

**All other files not mentioned are automatically generated by [Visual Studio publish profiles](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/visual-studio-publish-profiles?view=aspnetcore-8.0#publish-profiles) to make this program easy to share.**

### How do I stop this program from recording my inputs?

There are two definitive ways to terminate the program's entire process and recording of input:

- Clicking the `X` button on the top right of the program window.
- Right clicking on the icon in the taskbar system tray and selecting `Terminate`.

Terminating the process through `Task Manager` is also viable, but some recorded input activity may be lost. [See here for why](#Does-this-program-use-a-lot-of-CPU-and-RAM).

## General 

### Does this program use a lot of CPU and RAM?

This program performs no calculations or writing to files when minimized/running in the background. This is intentional to avoid performing computationally intensive tasks, primarily type `double` division and the updating of heatmaps. This also means that heatmaps do not update until the program is brought back into focus.

_Methods of when heatmaps update may be added as user settings in future releases._

### How can I minimize the program to be a background process?

- Click the `-` button on the top right of the program window.

### The program is not updating colors or heatmaps when I type or click!

This is intended, as the program is currently set to only update heatmaps and _**save your input activity**_ when the program is either:

- Brought back into focus from being minimized.
- Clicking the `X` button on the top right of the program window.
- Terminated by right clicking the program icon in the taskbar system tray and clicking "Terminate"

_This also means that terminating the program through `Task Manager` may result in input activity loss. There is no saving method tied to ending a process from outside._

# Install

### Download 

To ensure that you download a safe version, only download from this repository's releases page. Each release will have its respective **SHA256** and a VirusTotal listed as well. 

### Build and Compile

1. Download or clone the repository.
2. Open `dankeyboard.sln` in Visual Studio.
3. In the solution explorer, right click the solution project.
4. Select `Build Solution`
5. In the solution explorer, right click the `dankeyboard` application file.
6. Select `Publish...`
7. Follow the instructions to compile locally on your machine.

# Contribution/Suggestions

Pull requests will heavily reviewed to keep the safety and integrity of this project. 

If there is a feature you'd like to see or a suggestion you have, make a pull request with the suggestion as the title; no code is necessary.
