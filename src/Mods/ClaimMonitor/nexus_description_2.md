[size=6][b]Milex GMS1 Claim Monitor[/b][/size]
[size=4][i]Real-Time Warning HUD, Mat Tracking & Fleet Fuel Gauges for Gold Mining Simulator[/i][/size]

[hr]

[size=5][b]What Does This Mod Do?[/b][/size]

Have you ever spent an hour happily digging paydirt, only to walk over to your wash plant and realize your Miner's Moss mats reached 100% full 20 minutes ago—meaning tons of precious gold was washed right down the drain[cite: 4]?

Or had your wash plant grind to a halt because a suspension spring snapped on your shaker, a duplex jig bucket overflowed, or a conveyor feed stopped unnoticed[cite: 1, 4]?

[b]Milex GMS1 Claim Monitor[/b] keeps you in full control of your claim[cite: 4]. It puts a customizable, live warning HUD on your screen that tracks your wash plant mats, equipment durability, feeding chains, generators, and vehicle fleet in real time[cite: 1, 2, 4]. Additionally, it includes an integrated Diagnostics Inspector overlay that lets you view all live sensor data across all your active claims[cite: 1].

[hr]

[size=5][b]Highlights[/b][/size]

[list]
[*] [b]Live Mat & Container Fill Tracking (Never Lose Gold Again):[/b] Tracks the exact fill percentage of Miner's Moss mats, nugget trap crates, and Jig/Planter buckets across all wash plant tiers (Setup 1 Hog Pan, Setup 2 Mobile/Mini, Setup 3 Modular Plants, and Setup 4 Orange Beast)[cite: 1, 4, 5]. Gives you an early yellow warning when mats reach 90% and a pulsing red alarm when they hit 100% so you can clean them out in time[cite: 4, 5].
[*] [b]Mechanical Breakdown & Wear Warnings:[/b] Tracks physical durability on suspension springs, hydraulic hoses, motors, drive belts, and conveyor buckets[cite: 1, 5]. Warns you well before parts fail (default: 20%) and triggers immediate critical alarms if a component is missing, unbolted, or destroyed[cite: 1, 4, 5].
[*] [b]Feeding Chain & Conveyor Monitoring:[/b] Keeps an eye on stationary hoppers and conveyor elevators, tracking hopper fill levels, power supply, and drive belt/bucket integrity[cite: 1, 5].
[*] [b]Fleet Fuel Gauges & Dual-Tank Support:[/b] Adds clean vertical fuel gauges right into the top vehicle switcher bar[cite: 4, 5]! Displays fuel levels for all excavators, loaders, trucks, and handles dual-tank vehicles (drive engine + conveyor fuel on Frankenstein and Cordylus)[cite: 3, 4, 5].
[*] [b]Smart Standby (No False Alarms):[/b] Disconnected spare equipment or inactive setups in your yard are recognized as on standby and will not trigger annoying warnings or clutter your screen[cite: 4].
[*] [b]"Warnings Only" Mode (Clean Screen):[/b] Prefer an immersive screen? Turn on "Warnings Only" mode[cite: 4]. The HUD stays completely invisible while everything is running smoothly, and only pops up when something actually needs your attention[cite: 4].
[*] [b]Diagnostics Inspector Overlay:[/b] A built-in live inspector window detailing equipment status, parts wear, fill levels, and topology scan timers with filter tabs for all setups and vehicles[cite: 1].
[*] [b]Movable & Compact:[/b] Drag the warning window anywhere on your screen, or toggle Compact Mode to collapse it into a slim status banner[cite: 2, 4].
[/list]

[hr]

[size=5][b]Quick Install Guide[/b][/size]

[list=1]
[*] Make sure you have [b]BepInEx 5 (x64)[/b] and [b]Milex GMS1 CoreMod[/b] installed[cite: 4].
[*] Place [font=Courier New]Milex_GMS1_ClaimMonitor.dll[/font] into your [font=Courier New]\Gold Rush The Game\BepInEx\plugins\[/font] folder[cite: 4].
[*] Start the game, load your claim, and your warning HUD will appear automatically[cite: 4]!
[/list]

[hr]

[size=5][b]In-Game Controls[/b][/size]

[list]
[*] [b][font=Courier New]Insert[/font]:[/b] Opens the Milex Mod Menu[cite: 4]. Click [b]Claim Monitor[/b] to adjust warning thresholds, HUD appearance, or enable the Diagnostics Inspector[cite: 1, 4].
[*] [b]Click & Drag Header:[/b] Move the Warning HUD window anywhere on your screen (position is saved automatically)[cite: 2, 4].
[*] [b]Diagnostics Overlay Controls:[/b] If enabled in settings, view machine breakdowns by category (Setup 1–4, Vehicles, Alerts) and trigger a manual rescan anytime[cite: 1].
[/list]

[hr]

[size=5][b]Settings & Customization[/b][/size]

You can change everything in the in-game menu ([font=Courier New]Insert[/font]) or in:
[font=Courier New]\Gold Rush The Game\BepInEx\config\Milex_GMS1_ClaimMonitor.cfg[/font][cite: 4]

[list]
[*] [font=Courier New]HudEnabled[/font] (Default: [font=Courier New]true[/font]): Master switch for the on-screen Warning HUD[cite: 4].
[*] [font=Courier New]HudOnlyShowWarnings[/font] (Default: [font=Courier New]false[/font]): Keeps the HUD hidden until an alert or warning is triggered[cite: 4].
[*] [font=Courier New]HudCompactMode[/font] (Default: [font=Courier New]false[/font]): Turns the HUD into a compact single-line banner[cite: 2, 4].
[*] [font=Courier New]MatWarningThreshold[/font] (Default: [font=Courier New]90%[/font]): Choose at what mat percentage the early warning triggers[cite: 4, 5].
[*] [font=Courier New]ComponentWearWarningThreshold[/font] (Default: [font=Courier New]20%[/font]): Durability percentage that triggers a repair alert before parts break[cite: 4, 5].
[*] [font=Courier New]VehicleLowFuelThreshold[/font] (Default: [font=Courier New]20%[/font]): Choose at what fuel percentage the vehicle low-fuel alert triggers[cite: 4, 5].
[*] [font=Courier New]ShowFuelInVehicleSwitcher[/font] (Default: [font=Courier New]true[/font]): Displays vertical fuel gauges in the top vehicle bar[cite: 4, 5].
[*] [font=Courier New]EnableDebugGroup[/font] (Default: [font=Courier New]false[/font]): Enables the detailed in-game Diagnostics Inspector window[cite: 1].
[*] [font=Courier New]ScanIntervalSeconds[/font] (Default: [font=Courier New]2.0s[/font]): Frequency of fast live state polling[cite: 2, 3].
[/list]

[hr]

[size=5][b]Installation & Requirements[/b][/size]

[b]Requirements:[/b]
[list]
[*] [i]Gold Mining Simulator[/i] on Steam[cite: 4].
[*] [url=https://github.com/BepInEx/BepInEx/releases]BepInEx 5 (version 5.4.21 or newer, x64)[/url][cite: 4].
[*] [b]Milex GMS1 CoreMod (v1.4.0 or newer)[/b] (installed in [font=Courier New]\Gold Rush The Game\BepInEx\plugins\[/font])[cite: 4].
[/list]

[b]Step-by-Step Installation:[/b]
[list=1]
[*] Make sure BepInEx 5 and Milex CoreMod are installed[cite: 4].
[*] Download this mod[cite: 4].
[*] Place [font=Courier New]Milex_GMS1_ClaimMonitor.dll[/font] into your [font=Courier New]\Gold Rush The Game\BepInEx\plugins\[/font] folder[cite: 4].
[*] Launch the game and start mining[cite: 4]!
[/list]

[b]Uninstallation:[/b]
Delete [font=Courier New]Milex_GMS1_ClaimMonitor.dll[/font] from [font=Courier New]\Gold Rush The Game\BepInEx\plugins\[/font][cite: 4]. Your save games remain completely untouched[cite: 4].

[hr]

[size=5][b]Changelog[/b][/size]

[b]Version 1.0.14[/b]
[list]
[*] [b]Reworked the complete scanning routine in the backgroudn from scratch to ensure high performance and low resource usage.
[*] [b]Double-Buffered Scanning Engine:[/b] Background topology discovery and polling now update data atomically through working buffers, completely eliminating UI flickering, card redraw resets, and micro-stutter[cite: 3].
[*] [b]Smart Scene & Savegame Lifecycle:[/b] Diagnostic scanner now automatically purges all cached claims, parts, and vehicles when returning to the Main Menu[cite: 3]. Scans automatically stay dormant until a world is loaded and wait 4 in-game seconds after loading finishes to allow all claim objects and vehicle controllers to fully spawn[cite: 3].
[*] [b]Diagnostics Inspector Overlay:[/b] Added a comprehensive in-game diagnostics window (configurable via Milex Mod Menu) displaying live topology timers, active alerts, full vehicle inventories, and equipment breakdowns for Setups 1 through 4[cite: 1, 3].
[*] [b]Dual-Tank & Conveyor Fleet Support:[/b] Added conveyor engine fuel monitoring and dirt volume tracking for Frankenstein Excavator and Cordylus belts, alongside standard vehicle chassis fuel tanks[cite: 3].
[*] [b]Stationary Feeding Chain Monitoring:[/b] Added tracking for stationary hoppers and conveyor elevators, monitoring hopper dirt volumes, drive belt integrity, and elevator bucket wear[cite: 3].
[*] [b]Cursor & Input Handling:[/b] Fixed mouse cursor unlock behavior in the debug overlay, ensuring the player character and camera remain unaffected while interacting with the UI[cite: 1].
[/list]

[b]Version 1.0.11[/b]
[list]
[*] Added missing, detached, and unbolted part detection: The Warning HUD now immediately alerts you with a critical alarm if a part is missing, unbolted, or broken on your wash plant (e.g. Glacier Creek suspension springs, drive belts, chains, motors)[cite: 4].
[*] Smart setup isolation: Only machinery actively mounted and connected to your running wash plant setup is monitored[cite: 4]. Spare, loose, or stored parts lying in your yard or vehicles will never trigger false warnings[cite: 4].
[*] Added feeder hopper and conveyor drive belt failure tracking to prevent unnoticed gravel feed stoppages[cite: 4].
[/list]

[b]Version 1.0.10[/b]
[list]
[*] Optimized background claim scanning performance and memory efficiency while working on large claims[cite: 4].
[/list]

[b]Version 1.0.9[/b]
[list]
[*] HUD and claim scanner now automatically sleep during loading screens and main menus to preserve system performance[cite: 4].
[/list]

[b]Version 1.0.8[/b]
[list]
[*] Improved conveyor monitoring: disconnected or unused conveyors on new claims no longer trigger false warnings[cite: 4].
[*] Improved window height handling on smaller screen resolutions[cite: 4].
[/list]

[b]Version 1.0.7[/b]
[list]
[*] Added intelligent connection detection for water pumps and generators (disconnected spare equipment on standby will never trigger false alarms)[cite: 4].
[*] Filtered individual power generator breaker buttons from spamming wear warnings[cite: 4].
[*] Added dynamic window auto-height in both compact and normal modes to fit active alerts tightly without text cutoff[cite: 4].
[*] Added diesel fuel monitoring for Mini Wash Plant engines[cite: 4].
[/list]

[b]Version 1.0.6[/b]
[list]
[*] Added comprehensive wear and breakdown early-warning system for all physical wear parts (nozzles, belts, chains, filters, pump mechanisms)[cite: 4].
[*] Added configurable wear warning threshold (default 20%) to alert you before parts break down[cite: 4].
[/list]

[b]Version 1.0.5[/b]
[list]
[*] Overhauled water supply detection on stationary wash plants (Glacier Creek, Derocker) with specific reasons for water failure[cite: 4].
[/list]

[b]Version 1.0.4[/b]
[list]
[*] Fixed generator and pump running state validation so unpowered machines are accurately reported[cite: 4].
[*] Corrected requirement profiles for Duplex Jigs and Gravel Pumps (electric only, no water required)[cite: 4].
[/list]

[b]Version 1.0.3[/b]
[list]
[*] Positioned vehicle fuel status indicators in front of each vehicle card in the vehicle quick-switcher[cite: 4].
[*] Added Orange Beast setup presence detection to prevent ghost warnings on claims without Tier 6 equipment[cite: 4].
[/list]

[b]Version 1.0.2[/b]
[list]
[*] Added support for Tier 6 Orange Beast wash plants, power cables, and water supply lines[cite: 4].
[*] Added vehicle quick-switcher fuel status overlay[cite: 4].
[/list]

[b]Version 1.0.1[/b]
[list]
[*] Added Tier 5 Gravel Pump support alongside Tier 4 Duplex Jigs[cite: 4].
[*] Added configurable scan interval setting (default 3.0s)[cite: 4].
[/list]

[b]Version 1.0.0[/b]
[list]
[*] Initial release featuring the live tactical Warning HUD (Nominal / Warning / Critical)[cite: 4].
[*] Real-time Sluice Box Miner's Moss mat fill tracking with early warning at 90% and urgent alarm at 100%[cite: 4].
[*] Machinery wear, generator fuel, water pump, and vehicle fleet monitoring[cite: 4].
[*] Support for full card view, compact badge view, and "Warnings Only" mode[cite: 4].
[/list]