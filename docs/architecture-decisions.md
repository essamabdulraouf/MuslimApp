# Architecture decisions

Decisions taken while building M1 that a reader of the code would otherwise have to reverse-engineer, and the
places where the implementation deliberately departs from the foundation specification.

Each entry records the decision, why it was taken, and what it costs.

---

## 1. The prayer engine is written here rather than taken from a library

**Decision.** `PrayerCalculationEngine` implements the solar position from Jean Meeus, *Astronomical Algorithms*
(2nd edition), instead of depending on an existing prayer-times package.

**Why.** The specification requires an automated test matrix across cities, dates, conventions, juristic methods,
time-zone transitions, daylight-saving boundaries, manual adjustments and polar latitudes. That matrix only has
value if it tests code we own and can fix. A third-party library would have made the tests a test of someone
else's release cadence, and prayer-time accuracy is the product's central trust claim.

**Cost.** Roughly four hundred lines of astronomy that must be maintained, and a standing rule that the
coefficients in `Astronomical`, `SolarCoordinates` and `SolarTime` are constants from a published source and may
never be adjusted to make a timetable match an expectation. Where a timetable looks wrong, the bug is in the
settings, the time zone or the convention — not in the nutation series.

---

## 2. Instants are stored as UTC ticks, not as text

**Decision.** A model-wide value converter maps every `DateTimeOffset` to a `long` of UTC ticks
(`DateTimeOffsetToUtcTicksConverter`, applied in `MuslimDbContext.ConfigureConventions`).

**Why.** EF Core's default SQLite mapping stores a `DateTimeOffset` as text including its offset, and SQLite
cannot order or compare those values. This surfaced as a real failure:
`SQLite does not support expressions of type 'DateTimeOffset' in ORDER BY clauses`, hit by the open-tasbih-session
lookup and the Quran bookmark list. Fixing it in the two queries would have left the next `OrderBy` to rediscover
it, so it is fixed in the storage model where no entity can opt out.

**Cost.** The original UTC offset is not preserved — only the instant is. That is acceptable because no displayed
local time is ever read back from storage: every clock time the user sees is recalculated from coordinates and
the configured zone. A test (`The_instant_survives_a_round_trip_even_though_the_offset_does_not`) states the
trade-off so it is not mistaken for a bug, and another (`Cached_days_can_be_ordered_and_filtered_in_the_database`)
guards the regression.

---

## 3. There is no `PrayerAdjustments` table

**Decision.** The six per-prayer minute corrections are columns on `PrayerSettings`. The separate adjustments
table listed in the schema section of the specification is not created.

**Why.** The specification describes the same data twice — once as adjustment fields on the settings entity and
once as its own table. Implementing both would create two places to write a correction and a way for them to
disagree. The settings row was chosen because the adjustments are part of one user's single configuration, are
always read together with the convention they modify, and are never queried independently.

**Cost.** A future requirement for per-location or per-profile adjustments would need a schema change. That is
preferable to shipping an ambiguity.

---

## 4. `ThemeSettings` and `LanguageSettings` are projections, not tables

**Decision.** They are records derived from `AppSettings.Theme` and `AppSettings.Language`, and are not mapped.

**Why.** The schema section names seventeen tables and none of them is a theme or language table, while
`AppSettings` already holds both values. Mapping them anyway would duplicate the user's choice and invite drift.

**Cost.** None identified. They exist as types because the presentation layer reads a shaped value
(`IsRightToLeft`, `FollowsSystem`) rather than re-deriving it in several places.

---

## 5. Notification delivery is deferred to M3

**Decision.** `INotificationService` is defined and the permission is requested during onboarding, but the
implementation registered in M1 (`DeferredNotificationService`) records the request and does nothing else.

**Why.** Doing this properly on Android means exact-alarm policy, doze mode, rescheduling after reboot and a
dedicated audio channel for the adhan; on iOS it means the notification centre's scheduling limits. That is a
milestone, not a footnote to the foundation. Establishing the contract now keeps the scheduler out of the view
models when it arrives.

**Cost.** A user can grant notification permission and receive nothing. To avoid that being a silent lie, the
"More" screen lists notifications and adhan audio as in development rather than offering toggles, and the
placeholder service is named for what it is.

---

## 6. Device adapters live in the mobile project

**Decision.** `MauiLocationService`, `MauiPermissionService`, `MauiConnectivityService` and `AppPresentation` are
in `MuslimApp.Mobile`, not `MuslimApp.Infrastructure`.

**Why.** They depend on MAUI Essentials. Infrastructure is deliberately free of any UI framework so that it can
be referenced by a test host, a console tool or the optional API without dragging MAUI in. The abstractions they
implement (`ILocationService`, `IPermissionService`, `IConnectivityService`) live in Application, so the
application layer still has no idea a phone is involved.

**Cost.** The mobile project holds a little more than pure presentation. The boundary that matters — Application
depends on abstractions only — is intact.

---

## 7. Light and dark use `AppThemeBinding`, not swapped dictionaries

**Decision.** One `Colors.xaml` holds both palettes and every style resolves through `AppThemeBinding`. Theme
choice is applied by setting `Application.UserAppTheme`.

**Why.** It is the platform's own mechanism, so "follow the system" works without any code, and every bound
colour updates live. Swapping resource dictionaries would require pages to use `DynamicResource` throughout and
would still not track the system setting on its own.

**Cost.** Pages must use style keys rather than colours, which is required anyway: no page contains a literal
colour.

---

## 8. Onboarding is one page with a step machine

**Decision.** The first-launch flow is a single `OnboardingPage` driven by `OnboardingViewModel.Step`, rather than
six pages on a navigation stack.

**Why.** The flow branches — a granted location skips the city picker, a denial or a failed fix lands on it — and
earlier answers change later defaults, such as a chosen city suggesting its region's calculation convention. One
state machine expresses that; six pages would express it as parameters passed between them.

**Cost.** One larger XAML file. Back navigation is explicit in `BackCommand` instead of being free.

---

## 9. Every permission step can be declined

**Decision.** Location and notifications can both be refused or skipped, and onboarding still completes.

**Why.** The specification requires it, and it is the right behaviour: a user unwilling to share a position must
still get a usable app. Declining location leaves the timetable unconfigured, and the home and prayer screens
render a `PermissionRequired` state that names the problem and offers the city picker. A built-in city list
(`CityCatalog`) exists so that path is genuinely complete rather than a consolation.

**Cost.** Two more states to design and keep working. The city list is curated data that must be verified, which
is why it sits in the application layer with tests over its coordinates, time zones and suggested conventions.

---

## 10. The timetable is recalculated for display, and the cache is write-through

**Decision.** `PrayerService` always calculates the day it is asked for and writes the result to
`PrayerTimes` afterwards, rather than reading the cache first.

**Why.** A day costs well under a millisecond. Reading the cache for display would add no measurable speed and
would create a way to show a stale timetable after a location or settings change. The cached rows exist for a
consumer that cannot recalculate on demand — the notification scheduler — and are cleared by `InvalidateAsync`.

**Cost.** A write on every read of today. Cache write failures are logged and swallowed, because a cache is
derived data and must never stop the user seeing prayer times.

---

## 11. Hijri dates use Umm al-Qura with hard-coded month names

**Decision.** `HijriDate` formats through `UmAlQuraCalendar`, with Arabic and English month names written out in
the source rather than read from culture data.

**Why.** Umm al-Qura is the civil calendar of Saudi Arabia and therefore the one the primary audience sees on
official documents; the tabular `HijriCalendar` can differ by a day. ICU's Arabic month names vary between
platforms and versions, and a date shown beside prayer times should not be spelled differently on Android and
iOS.

**Cost.** Month names are not translatable by adding a culture. This is display convenience only: the app makes no
claim about the start of a month, which is determined by sighting and not by arithmetic.

---

## 12. Time is read in exactly one place

**Decision.** Nothing outside `SystemDateTimeProvider` reads `DateTime.Now`. Domain and Application take
`IDateTimeProvider`.

**Why.** "Which prayer is next", "is this row passed" and "what is today in the configured zone" are all
clock-dependent, and all three need to be testable at a fixed instant — including across daylight-saving
transitions, which cannot be reproduced against the real clock.

**Cost.** One more constructor parameter in several places.

---

## 13. `Result<T>` for expected failures, exceptions for defects

**Decision.** No location fix, no permission, and no stored configuration are returned as `Result` failures with
stable error codes. Exceptions are reserved for programming errors.

**Why.** The specification requires that expected failures never crash and that the UI can render Loading, Empty,
Error, Offline and PermissionRequired. Codes such as `location.permission_denied` let a view model choose a state
without matching on message text, and the UI resolves the code to a translated message rather than displaying the
developer-facing one.

**Cost.** Callers must check results. That is the point.
