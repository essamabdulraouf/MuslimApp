# MuslimApp

A free, ad-free, offline-first Islamic companion app: prayer times, adhan, azkar, Quran, qibla, tasbih and duas.
Arabic first, English optional. Built with .NET MAUI for Android and iOS.

No accounts. No analytics. No tracking. Nothing about you leaves your device.

## Principles

These are constraints, not aspirations. They decide what the code is allowed to do.

- **Offline first.** Every feature works with the network off. Connectivity is consulted only for genuinely
  optional extras, such as turning coordinates into a city name for display.
- **Private by construction.** There is no account, no server-side state, and no behavioural analytics. The app
  does not log coordinates, reading positions, prayer history or tasbih history.
- **Religious content is not editable by software.** Quranic text is stored as revealed and is never reformatted,
  "corrected" or generated. Azkar and duas carry their source, reference and authenticity grading, and nothing
  ungraded is presented as authentic.
- **Prayer times are a trust feature.** The calculation is an in-house implementation of published astronomical
  algorithms, isolated behind one interface, and covered by tests across cities, seasons, conventions, juristic
  methods, time-zone transitions and polar latitudes.
- **Expected failures are states, not crashes.** No location fix, no permission, no database — each is something
  the user is shown and offered a way out of.

## Solution layout

```
MuslimApp.sln
├── src/
│   ├── MuslimApp.Domain           entities, enums, value types, interfaces — no dependencies at all
│   ├── MuslimApp.Application      use cases, DTOs, platform abstractions — no UI, no persistence
│   ├── MuslimApp.Infrastructure   EF Core + SQLite, prayer engine, qibla, content validation
│   └── MuslimApp.Mobile           .NET MAUI app: design system, shell, pages, view models, device adapters
└── tests/
    ├── MuslimApp.Domain.Tests
    ├── MuslimApp.Application.Tests
    └── MuslimApp.Infrastructure.Tests
```

The dependency rule runs one way only: `Domain ← Application ← Infrastructure` and `Domain ← Application ←
Mobile`. `MuslimApp.Domain` references no packages and no projects, and a test asserts that — it must never learn
about MAUI, EF Core, SQLite, HTTP or a platform SDK.

Device adapters (location, permissions, connectivity) live in `MuslimApp.Mobile` rather than
`MuslimApp.Infrastructure`, because they depend on MAUI Essentials and Infrastructure is kept free of any UI
framework so it stays usable from a test host or a future API.

## Prerequisites

- .NET SDK 8.0.400 or later on the 8.0 feature band (pinned in `global.json`)
- MAUI workloads: `dotnet workload install maui-android ios`
- JDK 17 and the Android SDK (API 34, build-tools 34) for the Android head
- macOS, or a paired Mac, to run or package the iOS head

## Building

The whole solution builds on Windows, macOS and Linux:

```bash
dotnet build MuslimApp.sln
```

If the build reports `XA5300: The Android SDK directory could not be found`, point it at your SDK. Visual Studio
sets this for you; a bare command line may not:

```powershell
$env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
dotnet build MuslimApp.sln -p:AndroidSdkDirectory="$env:ANDROID_HOME"
```

A single head:

```bash
dotnet build src/MuslimApp.Mobile/MuslimApp.Mobile.csproj -f net8.0-android
dotnet build src/MuslimApp.Mobile/MuslimApp.Mobile.csproj -f net8.0-ios
```

The iOS head compiles anywhere, but running it on a simulator or device, and signing it, needs macOS or a paired
Mac.

## Testing

```bash
dotnet test tests/MuslimApp.Domain.Tests/MuslimApp.Domain.Tests.csproj
dotnet test tests/MuslimApp.Application.Tests/MuslimApp.Application.Tests.csproj
dotnet test tests/MuslimApp.Infrastructure.Tests/MuslimApp.Infrastructure.Tests.csproj
```

`dotnet test` cannot be given several projects at once, so run them individually or point it at a solution
filter. The repository tests create a real temporary SQLite file and apply the real migrations, so they exercise
the shipped schema rather than an in-memory approximation.

## Prayer calculation

`IPrayerCalculationService` is pure, synchronous and side-effect free: coordinates, a date and settings in, a
timetable out. The implementation follows Jean Meeus, *Astronomical Algorithms* (2nd edition) for the solar
position, and applies the published Fajr and Isha conventions of Umm al-Qura, the Muslim World League, the
Egyptian General Survey Authority, Karachi, ISNA and the Gulf region, plus a user-defined custom option.

The published angles are pinned by tests. Changing one has to be a deliberate act with a source, not a quiet
tweak to make a timetable match expectation.

Asr uses a shadow ratio of 1 for Shafi'i, Maliki and Hanbali, and 2 for Hanafi. Above roughly 48.5° latitude,
where the sun may never reach the twilight angle, one of three high-latitude rules applies, and beyond that the
nearest-latitude rule keeps the timetable defined rather than failing.

## Database

SQLite through EF Core, created and migrated on first launch by `IDatabaseInitializer`. Instants are stored as
UTC ticks so SQLite can sort and compare them; see the architecture notes for why.

To work with migrations:

```bash
dotnet ef migrations add <Name> --project src/MuslimApp.Infrastructure
```

A design-time factory supplies a throwaway `MuslimApp.design.db`, so no device or app configuration is needed.

## Status

M1 — Foundation, plus the first vertical slice, is complete:

- Clean-architecture solution with the dependency rule enforced by test
- Domain model, enums, value types and repository interfaces
- SQLite schema with migrations, seeded category and preset structure, and repositories
- Prayer calculation engine and qibla calculator, with a broad test matrix
- Content package manifest validation with SHA-256 hashing and schema gating
- Design system as centralised XAML tokens; no page contains a literal colour
- Shell navigation with six named tabs, RTL by default, Arabic and English
- First-launch flow: welcome, location (with a manual city fallback and a skip), calculation method,
  notifications, ready
- Home and Prayer screens driven end to end by Location → Prayer Engine → UI
- Settings for language, theme, location, calculation method and juristic method

Not yet built, and visible as such in the app rather than hidden: azkar, Quran, qibla compass, tasbih, and local
notification delivery. See [docs/architecture-decisions.md](docs/architecture-decisions.md) for the decisions and
deviations taken along the way.

## Licence and content

Application code is this repository's own. Quranic text, hadith, azkar and dua content are religious material
carrying their own provenance; each content package records its source and authenticity grading, and packages are
validated before they are allowed to replace what is already trusted.
