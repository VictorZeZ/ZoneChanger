# TimeZoneManager

**TimeZoneManager** is a lightweight Windows console utility (written in C#) that allows you to quickly switch your system’s time zone among a list of popular zones — and restore the previous one when you're done.  
It’s especially useful for games or apps that perform time-zone checks before launch.

---

## ✨ Key Features
- 🧭 Interactive console menu with arrow-key navigation and **ENTER** to apply a selection.  
- 🌍 Quick list of common Windows time zones (London, Berlin/Paris, New York, Los Angeles, Tehran, Delhi, Beijing).  
- 💾 Automatically saves your current system time zone before the first change.  
- 🔁 Easily restore your previous time zone using the `reset` command.  
- ⚙️ Simple CLI with commands: `start`, `reset`, `help`.

---

## 🧱 Requirements
- Windows (with `tzutil.exe` available in PATH — standard on all modern Windows versions).  
- .NET 9 runtime or SDK.  
- Uses modern **C# 13** syntax features.  
- Must be run as **Administrator** to change the system time zone.

---

## 📁 Files and Storage
- **Source file:** `Program.cs`  
- **Saved previous time zone file:**  
  `%LocalAppData%\ZoneChanger\prev_tz.txt`  
  (The app automatically creates the `ZoneChanger` folder under Local Application Data.)

---

## 🚀 Build & Run

From the project directory:

### Run directly (requires .NET SDK):
```bash
dotnet run -- start
```

### Publish a standalone release build (Windows x64):
```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

> ⚠️ If you’re using the published executable, **Run as Administrator** to apply time zone changes.

---

## 🧩 Usage

### Command-line options:
| Command | Description |
|----------|--------------|
| `start` | Launch the interactive time zone selector |
| `reset` | Restore the previously saved time zone |
| `help`  | Show usage information |

### Interactive mode:
- Use **UP/DOWN** arrows to move the selection.  
- Press **ENTER** to apply the selected time zone.  
- Last menu option: **RESET to initial TimeZone** (restores saved zone).  
- Press **ESC** to exit interactive mode.

### Behavior details:
- On first `start`, the tool saves your current time zone using `tzutil /g` to `prev_tz.txt`.  
- Applying a new zone uses `tzutil /s "<TimeZoneId>"`.  
- `reset` re-applies the saved time zone and removes `prev_tz.txt`.

---

## 🛡️ Safety Notes
- Changing the system time zone affects all users and scheduled tasks.  
- Requires Administrator rights — otherwise it will exit with an error.  
- Avoid running on managed or domain-joined systems without permission.

---

## 🧰 Troubleshooting
| Issue | Cause / Solution |
|--------|------------------|
| “This program must be run as Administrator!” | Run the app elevated (Right-click → *Run as administrator*) |
| `tzutil not found` | Make sure you’re on Windows and `tzutil.exe` is available in PATH |
| “No saved previous TimeZone found” | The `prev_tz.txt` file was deleted or never created |

---

## 💡 Future Improvements
- Add a configuration file to customize available time zones.  
- Enumerate and search all installed Windows time zones.  
- Add a `--set <TimeZoneId>` argument for scripting and automation.

---

## 🤝 Contributing
Contributions are welcome!  
Please:
- Keep changes focused and tested on Windows.
- Follow the existing code style.
- Target `.NET 9`.

---

## 📜 License
This project is licensed under the [MIT License](LICENSE).  
You may freely use, modify, and distribute this software under the terms of that license.

---

## 📬 Contact / Attribution
- The app uses the built-in Windows `tzutil.exe` utility only (no network activity).  
- For questions or feature requests, please open an issue in this repository.
