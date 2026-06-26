# CyberSecurity Awareness Bot

A C# WinForms desktop chatbot that teaches everyday users how to stay safe online — covering password security, phishing, and safe browsing — through a dark-themed chat interface with a quiz mini-game, task management, and activity logging.

Built for **PROG6221** as a progressive project: a console-based chatbot (Part 1) evolved into this full WinForms GUI (Parts 2–3).

![Platform](https://img.shields.io/badge/platform-Windows-blue) ![Framework](https://img.shields.io/badge/.NET-WinForms-purple)

## Features

- **Chat-based interface** — ask about password security, phishing, or safe browsing and get keyword-matched responses
- **Quiz mini-game** — type `quiz` to test your cybersecurity knowledge
- **Task management** — type `tasks` to add, view, and manage cybersecurity action items
- **Activity log** — type `activity log` to review recent actions taken in the session
- **Sentiment detection** — the bot adapts its tone based on detected user sentiment
- **Voice greeting** — plays `greeting.wav` on startup
- **Typing animation** — bot responses appear with a simulated typing effect
- **Username memory** — remembers the user's name for the rest of the session
- **Dark-themed UI** — black/dark chat window with a red "Quit" button accent

## Getting Started

### Prerequisites
- Windows 10/11
- [.NET SDK](https://dotnet.microsoft.com/download) (or Visual Studio 2022 with the **.NET Desktop Development** workload)

### Running the project
```bash
git clone https://github.com/karabo-10/CyberGUI.git
cd CyberGUI
dotnet build
dotnet run
```
Or open the solution in Visual Studio and press **F5**.

On launch, the bot will greet you and ask for your name before you can start chatting.

## Usage

| Command | Description |
|---|---|
| `quiz` | Starts the cybersecurity mini-game |
| `tasks` | Opens task management |
| `activity log` | Shows recent actions |
| *(free text)* | Ask about password security, phishing, or safe browsing |

UI controls:
- **Send** — submits the typed message (or press Enter in the input box)
- **Clear** — clears the chat window
- **Quit** — exits the application

## Project Structure

```
CyberGUI/
├── Form1.cs              # Main form logic (chat handling, commands, quiz, tasks)
├── Form1.Designer.cs     # WinForms designer-generated layout (rtbChat, txtInput, buttons)
├── Program.cs            # Application entry point
├── greeting.wav          # Startup voice greeting audio
└── README.md
```

Key controls on the main form:
- `rtbChat` — read-only RichTextBox displaying the conversation
- `txtInput` — text box for user input
- `btnSend` / `btnClear` / `btnQuit` — action buttons, anchored to the bottom-right of the window

## Notes

- This project favors small, targeted code changes over large rewrites — see commit history for incremental fixes (layout, control overlap, etc.).
- Built and tested as part of academic coursework; not intended for production security guidance.

## License

Educational project — for academic use under PROG6221 coursework.
