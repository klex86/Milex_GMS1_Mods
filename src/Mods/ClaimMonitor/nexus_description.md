[size=6][b]Milex GMS1 Claim Monitor[/b][/size]
[size=4][i]Real-Time Warning HUD, Mat Tracking & Fleet Fuel Gauges for Gold Mining Simulator[/i][/size]

[hr]

[size=5][b]What Does This Mod Do?[/b][/size]

Have you ever spent an hour happily digging paydirt, only to walk over to your wash plant and realize your Miner's Moss mats reached 100% full 20 minutes ago—meaning tons of precious gold was washed right down the drain?

Or had your wash plant grind to a halt because your generator silently ran out of diesel or a trommel drive chain snapped without you noticing?

[b]Milex GMS1 Claim Monitor[/b] keeps you in full control of your claim. It puts a customizable, live warning HUD on your screen that tracks your wash plant mats, equipment durability, water pumps, generator fuel, and vehicle fleet in real time.

[hr]

[size=5][b]Highlights[/b][/size]

[list]
[*] [b]Live Mat Fill Tracking (Never Lose Gold Again):[/b] Tracks the exact fill percentage of Miner's Moss mats, nugget traps, and Jig buckets across all wash plant setups (Tier 2 Mobile, Tier 3–5 Stationary, and Tier 6 Orange Beast). Gives you an early yellow warning when mats reach 90% and a pulsing red alarm when they hit 100% so you can clean them out in time.
[*] [b]Breakdown & Wear Warnings:[/b] Warns you before belts, chains, motors, or gravel pumps break down so you can fix them before your entire mining operation stalls.
[*] [b]Generator & Water Pump Alerts:[/b] Keeps an eye on the diesel level in your main claim generator and alerts you if water pumps run low on fuel or lose water pressure.
[*] [b]Fleet Fuel Gauges:[/b] Adds clean vertical fuel bars right into the top vehicle switcher bar! You can see the exact fuel level of every excavator, loader, and dump truck on your claim without having to get into each cabin.
[*] [b]Smart Standby (No False Alarms):[/b] Spare water pumps without hoses or extra generators parked in your yard are recognized as on standby. They won't trigger annoying warnings or clutter your screen.
[*] [b]"Warnings Only" Mode (Clean Screen):[/b] Prefer a clean, immersive screen? Turn on "Warnings Only" mode. The HUD stays completely invisible while everything is running smoothly, and only pops up when something actually needs your attention (like low fuel or full mats).
[*] [b]Movable & Compact:[/b] Drag the warning window anywhere on your screen, or click the minimize button to collapse it into a tiny single-line status badge.
[/list]

[hr]

[size=5][b]Quick Install Guide[/b][/size]

[list=1]
[*] Make sure you have [b]BepInEx 5 (x64)[/b] and [b]Milex GMS1 CoreMod[/b] installed.
[*] Place [font=Courier New]Milex_GMS1_ClaimMonitor.dll[/font] into your [font=Courier New]\Gold Rush The Game\BepInEx\plugins\[/font] folder.
[*] Start the game, load your claim, and your warning HUD will appear automatically when machines are running!
[/list]

[hr]

[size=5][b]In-Game Controls[/b][/size]

[list]
[*] [b][font=Courier New]Insert[/font]:[/b] Opens the Milex menu. Click [b]Claim Monitor[/b] to adjust warning thresholds and HUD appearance.
[*] [b]Click & Drag Header:[/b] Move the Warning HUD window anywhere on your screen.
[*] [b][font=Courier New][ - Minimize ][/font]:[/b] Collapses the HUD into a compact status badge (or expands it back).
[/list]

[hr]

[size=5][b]Settings & Customization[/b][/size]

You can change everything in the in-game menu ([font=Courier New]Insert[/font]) or in:
[font=Courier New]\Gold Rush The Game\BepInEx\config\Milex_GMS1_ClaimMonitor.cfg[/font]

[list]
[*] [font=Courier New]HudEnabled[/font] (Default: [font=Courier New]true[/font]): Master switch for the on-screen Warning HUD.
[*] [font=Courier New]HudOnlyShowWarnings[/font] (Default: [font=Courier New]false[/font]): Keeps the HUD hidden until an alert or warning is triggered.
[*] [font=Courier New]HudCompactMode[/font] (Default: [font=Courier New]false[/font]): Turns the HUD into a compact single-line badge.
[*] [font=Courier New]MatWarningThreshold[/font] (Default: [font=Courier New]90%[/font]): Choose at what mat percentage the early warning triggers.
[*] [font=Courier New]VehicleLowFuelThreshold[/font] (Default: [font=Courier New]15%[/font]): Choose at what fuel percentage the vehicle low-fuel alert triggers.
[*] [font=Courier New]ComponentWearWarningThreshold[/font] (Default: [font=Courier New]20%[/font]): Durability percentage that triggers a repair alert before parts break.
[*] [font=Courier New]ShowFuelInVehicleSwitcher[/font] (Default: [font=Courier New]true[/font]): Displays fuel gauges in the top vehicle bar.
[/list]

[hr]

[size=5][b]Installation & Requirements[/b][/size]

[b]Requirements:[/b]
[list]
[*] [i]Gold Mining Simulator[/i] on Steam.
[*] [url=https://github.com/BepInEx/BepInEx/releases]BepInEx 5 (version 5.4.21 or newer, x64)[/url].
[*] [b]Milex GMS1 CoreMod (v1.4.0 or newer)[/b] (installed in [font=Courier New]\Gold Rush The Game\BepInEx\plugins\[/font]).
[/list]

[b]Step-by-Step Installation:[/b]
[list=1]
[*] Make sure BepInEx 5 and Milex CoreMod are installed.
[*] Download this mod.
[*] Place [font=Courier New]Milex_GMS1_ClaimMonitor.dll[/font] into your [font=Courier New]\Gold Rush The Game\BepInEx\plugins\[/font] folder.
[*] Launch the game and start mining!
[/list]

[b]Uninstallation:[/b]
Delete [font=Courier New]Milex_GMS1_ClaimMonitor.dll[/font] from [font=Courier New]\Gold Rush The Game\BepInEx\plugins\[/font]. Your save games remain completely untouched.

[hr]

[size=5][b]Changelog[/b][/size]

[b]Version 1.0.11[/b]
[list]
[*] Added missing, detached, and unbolted part detection: The Warning HUD now immediately alerts you with a critical alarm if a part is missing, unbolted, or broken on your wash plant (e.g. Glacier Creek suspension springs, drive belts, chains, motors).
[*] Smart setup isolation: Only machinery actively mounted and connected to your running wash plant setup is monitored. Spare, loose, or stored parts lying in your yard or vehicles will never trigger false warnings.
[*] Added feeder hopper and conveyor drive belt failure tracking to prevent unnoticed gravel feed stoppages.
[/list]

[b]Version 1.0.10[/b]
[list]
[*] Optimized background claim scanning performance and memory efficiency while working on large claims.
[/list]

[b]Version 1.0.9[/b]
[list]
[*] HUD and claim scanner now automatically sleep during loading screens and main menus to preserve system performance.
[/list]

[b]Version 1.0.8[/b]
[list]
[*] Improved conveyor monitoring: disconnected or unused conveyors on new claims no longer trigger false warnings.
[*] Improved window height handling on smaller screen resolutions.
[/list]

[b]Version 1.0.7[/b]
[list]
[*] Added intelligent connection detection for water pumps and generators (disconnected spare equipment on standby will never trigger false alarms).
[*] Filtered individual power generator breaker buttons from spamming wear warnings.
[*] Added dynamic window auto-height in both compact and normal modes to fit active alerts tightly without text cutoff.
[*] Added diesel fuel monitoring for Mini Wash Plant engines.
[/list]

[b]Version 1.0.6[/b]
[list]
[*] Added comprehensive wear and breakdown early-warning system for all physical wear parts (nozzles, belts, chains, filters, pump mechanisms).
[*] Added configurable wear warning threshold (default 20%) to alert you before parts break down.
[/list]

[b]Version 1.0.5[/b]
[list]
[*] Overhauled water supply detection on stationary wash plants (Glacier Creek, Derocker) with specific reasons for water failure.
[/list]

[b]Version 1.0.4[/b]
[list]
[*] Fixed generator and pump running state validation so unpowered machines are accurately reported.
[*] Corrected requirement profiles for Duplex Jigs and Gravel Pumps (electric only, no water required).
[/list]

[b]Version 1.0.3[/b]
[list]
[*] Positioned vehicle fuel status indicators in front of each vehicle card in the vehicle quick-switcher.
[*] Added Orange Beast setup presence detection to prevent ghost warnings on claims without Tier 6 equipment.
[/list]

[b]Version 1.0.2[/b]
[list]
[*] Added support for Tier 6 Orange Beast wash plants, power cables, and water supply lines.
[*] Added vehicle quick-switcher fuel status overlay.
[/list]

[b]Version 1.0.1[/b]
[list]
[*] Added Tier 5 Gravel Pump support alongside Tier 4 Duplex Jigs.
[*] Added configurable scan interval setting (default 3.0s).
[/list]

[b]Version 1.0.0[/b]
[list]
[*] Initial release featuring the live tactical Warning HUD (Nominal / Warning / Critical).
[*] Real-time Sluice Box Miner's Moss mat fill tracking with early warning at 90% and urgent alarm at 100%.
[*] Machinery wear, generator fuel, water pump, and vehicle fleet monitoring.
[*] Support for full card view, compact badge view, and "Warnings Only" mode.
[/list]
