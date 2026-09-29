// The application layer lives at MuslimApp.Application, which is a sibling of MuslimApp.Mobile under the
// MuslimApp root. Inside this project the bare name "Application" therefore resolves to that namespace and
// shadows Microsoft.Maui.Controls.Application. This alias makes MAUI's Application reachable without every
// reference having to be written out in full.
global using UiApplication = Microsoft.Maui.Controls.Application;
