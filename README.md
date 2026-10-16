# Syntax Error Academy (SEA)

A gamified C# learning platform built with **ASP.NET Web Forms**. Students work through levelled quizzes, earn points and coins, climb a leaderboard and spend coins on avatar cosmetics and wallpapers. Instructors write quizzes; administrators approve them and manage the platform.

Group project for **CT050-3-2 Web Applications (WAPP)** at Asia Pacific University of Technology & Innovation (APU).

## Features

### Student
- **CTF-style dashboard**: Levels 1–5 in a side rail. Finishing one quiz in a level unlocks the next level.
- **Personal grade roadmap**: an avatar pin slides along a tier track, with a "Level Up!" animation and a coin reward for each tier.
- **Four question types**: multiple choice, tap-to-match (Duolingo-style), code-token puzzles and essays.
- **AI features**: essays are marked by AI and students can ask for hints. Both go through n8n webhooks, so the app never holds an AI provider key.
- **Daily challenge**: one puzzle a day, plus a streak tracker.
- **Leaderboard**: a podium for the top three with full-body avatars, a trophy, medals and confetti.
- **Item shop and avatar system**: layered SVG avatar cosmetics and CSS wallpapers, with live previews. Each slot can have one item equipped.
- **Minigames**: Byte Catcher, Stack Tower and Target Shot, written in vanilla JS on canvas with WebAudio sound.
- **Profile**: edit details, change password, see quiz history.
- Dark/light theme and a responsive mobile layout.

### Instructor
- Dashboard with a status badge on each quiz (approved, pending or rejected).
- Drag-and-drop quiz builder for every question type.
- View quizzes and reject reasons, and review each student's attempts with right and wrong answers highlighted.
- Instructor shop, profile and equipped wallpaper.

### Administrator
- Manage users (students, instructors, admins), with cascade delete.
- Approve or reject quizzes. Every reject reason is logged.
- Item shop management: upload files, paste SVGs or use CSS presets.
- Quiz reports and summary reports.

## Tech stack
- ASP.NET Web Forms (Web Application project), C#, .NET Framework 4.7.2
- ADO.NET with parameterised queries
- SQL Server LocalDB. The `.mdf` file sits in `App_Data` and is attached with `AttachDbFilename`.
- Vanilla JavaScript and CSS. The games and avatars use no external libraries.
- n8n webhooks for AI essay grading and hints

## Getting started

**Requirements:** Visual Studio 2022 with the *ASP.NET and web development* workload, which includes SQL Server Express LocalDB.

1. Clone the repo and open `WAPP Assignment.slnx`.
2. Build the solution. Visual Studio restores the NuGet packages for you.
3. Run with IIS Express (F5) and start from `Login.aspx`.

The database is `App_Data/SyntaxErrorAcademy.mdf`. LocalDB creates the transaction log automatically the first time the database is attached.

### Enabling the AI features (optional)
AI essay grading and quiz hints are **off by default** because no webhook URLs are included in this repo. Everything else works without them; the AI parts just show a "not configured" message.

To turn them on, connect your own AI API. The app never calls an AI provider directly. It POSTs JSON to a webhook, so any AI model works (Gemini, OpenAI, etc.).

1. Build two webhook endpoints (e.g. two n8n workflows: **Webhook → AI model → Respond to Webhook**), following the contract below. Your AI API key lives inside the workflow, not in this app.
2. Copy `WAPP Assignment/secrets.config.example` to `WAPP Assignment/secrets.config`. This file is git-ignored.
3. Fill in the values and restart the app:

```xml
<add key="N8nEssayWebhookUrl" value="https://your-host/webhook/sea-essay-grade" />
<add key="N8nHintWebhookUrl"  value="https://your-host/webhook/sea-hint" />
<add key="N8nAuthToken"       value="" />  <!-- optional: sent as the Authorization header -->
```

**Webhook contract**

| Endpoint | App sends (POST, JSON) | Webhook replies (JSON) |
|---|---|---|
| Essay grading | `{ "question": "...", "expected": "...", "answer": "..." }` | `{ "verdict": "Correct" \| "Wrong", "feedback": "one short sentence" }` |
| Quiz hint | `{ "question": "..." }` | `{ "hint": "..." }` |

The reply can be a bare object or an array with one object, which is n8n's default.

### Demo accounts
All demo passwords are `123456`.

| Role | Email |
|---|---|
| Administrator | `admin@gmail.com` |
| Instructor | `jas@gmail.com`, `tan@gmail.com`, `wang@gmail.com`, `tay@gmail.com` |
| Student | any `@sea.edu` account in the database, e.g. `eexin@sea.edu` |

You can also create a new student account on the Sign Up page.

## Project structure
```
WAPP Assignment.slnx
WAPP Assignment/
├── *.aspx / *.aspx.cs        Pages: Student*, Instructor*, Admin*, Quiz, Games, ...
├── Avatar.cs                 Layered SVG avatar renderer
├── Wallpaper.cs              Equipped wallpaper lookup (students)
├── InstructorTheme.cs        Equipped wallpaper lookup (instructors)
├── Streak.cs                 Daily streak tracking
├── Security.cs               Password hashing
├── EssayGrader.cs, AiHint.cs n8n integrations
├── HintHandler.ashx          JSON endpoint for quiz hints
├── Styles/                   One stylesheet per area (dashboard, quiz, admin, ...)
├── Assets/                   Logo, game scripts, grading JS
└── App_Data/                 SyntaxErrorAcademy.mdf
```

## Notes
- Passwords are stored as unsalted SHA-256 hashes, which keeps things simple for the course. A production app should use a salted, slow hash such as PBKDF2 or bcrypt.
- This is a coursework project and is not hardened for production.

## Team
- Tan Ee Xin
- Tay Boon Tao
- Wang Jim Loong
- Yeoh Yoong Ning