[size=6][b]Milex GMS1 Production Tuner[/b][/size]
[size=4][i]Comprehensive Speed, Capacity & Vehicle Tuning for Gold Mining Simulator[/i][/size]

[hr]

[size=5][b]Overview[/b][/size]

The [b]Milex GMS1 Production Tuner[/b] gives you complete control over work speeds, material capacities, hydraulics, and logistics in [i]Gold Mining Simulator[/i]. Tailor your mining operations to your preferred playstyle—whether you want a realistic pace or high-speed mining with massive payloads.

All 29 parameters can be adjusted live in-game with smooth continuous sliders via the Milex Mod Menu ([font=Courier New]Insert[/font]) with zero game restarts required.

[quote][b]Requirement:[/b] This mod requires [b]Milex GMS1 CoreMod[/b] to be installed.[/quote]

[hr]

[size=5][b]Features & Capabilities[/b][/size]

[size=4][b]1. Hand Tools & Mobile Wash Plants[/b][/size]
[list]
[*] [b]Shovel (1-Scoop Filling):[/b] Fills your shovel to 100% in a single scoop. Calibrated so that 1 shovel scoop fills 1 bucket to 100%, speeding up early prospecting.
[*] [b]Buckets & Standalone Hog Pan:[/b] Scale the dirt capacity of handheld buckets and portable Hog Pans.
[*] [b]Mobile Wash Plants:[/b] Independently adjust dirt capacity and washing speed on mobile wash plants.
[*] [b]Automatic Spill Protection:[/b] When pouring dirt from buckets into a gold pan, pouring stops automatically once the pan is full to prevent lost dirt or gold.
[/list]

[size=4][b]2. Heavy Machinery & Vehicles[/b][/size]
[list]
[*] [b]Excavator Hydraulics & Speeds:[/b] Independently adjust boom lift speed, stick reach, turret rotation, and bucket tilt for fluid, responsive vehicle handling.
[*] [b]Increased Bucket Payloads:[/b] Scale paydirt capacities on small and large excavators, front wheel loaders, backhoes, and dump trucks.
[*] [b]Front Loader Lifting Boost:[/b] Automatically enhances loader arm hydraulic power so enlarged payloads lift effortlessly without front-end dipping.
[*] [b]Heavy Conveyor Belts:[/b] Independently adjust dirt capacity and transport speeds on Frankenstein and Cordylus conveyor systems.
[*] [b]Excavator Chassis Stabilization:[/b] Anchors the undercarriage firmly to the ground whenever an excavator is parked or has its handbrake engaged, keeping the machine solidly planted while digging.
[*] [b]In-Place Vehicle Recovery:[/b] If a vehicle rolls over on steep pit terrain, entering the cabin gently sets it upright on its tracks directly where you were working.
[/list]

[size=4][b]3. Wash Plant Modules[/b][/size]
[list]
[*] [b]Feeder Hoppers & Conveyor Buckets:[/b] Scale raw paydirt capacity on hoppers and throughput on conveyor elevators.
[*] [b]Stationary Wash Plants:[/b] Scale dirt buffer capacity and washing speed across shakers and trommels.
[*] [b]Synchronized T3–T5 Setup Slider:[/b] A single master slider scales all interconnected components of stationary wash plants in [b]100% lockstep[/b]: Sluice Miner's Moss mats, Sluice Nugget Trap grates, Duplex Jig buckets, and attached tailing Hog Pan.
[*] [b]Synchronized T6 Orange Beast Slider:[/b] Scales the 20 giant Orange Beast mats and internal gold recovery in lockstep.
[/list]

[size=4][b]4. Fine Processing Equipment[/b][/size]
[list]
[*] [b]Gold Nuggetator:[/b] Adjust washing speed to rapidly separate nuggets from paydirt.
[*] [b]Magnetite Separator:[/b] Independently adjust processing speed and magnetite hopper capacity.
[*] [b]Wave Table:[/b] Independently adjust shaking speed and concentrate bed capacity.
[/list]

[size=4][b]5. Logistics & Fuel Infrastructure[/b][/size]
[list]
[*] [b]Mobile Fuel Trailer:[/b] Scales mobile fuel trailer capacity up to [b]10,000 L[/b] (Vanilla: 1,000 L, Default: 3,000 L).
[*] [b]Stationary Claim Fuel Tank:[/b] Scales stationary tank capacity up to [b]100,000 L[/b] (Vanilla: 10,000 L, Default: 30,000 L).
[*] [b]Fuel Hose Reach:[/b] Extra-long flexible hoses on both the trailer and claim tank reach your machines easily with high-strength locking nozzles.
[*] [b]Magnetite Trailer:[/b] Scales payload capacity of the magnetite transport trailer.
[*] [b]Trailer Fast-Travel Reconnection:[/b] Trailers stay hitched to your pickup after fast travel, with hitch levers remaining fully functional for manual unhitching anytime.
[/list]

[size=4][b]Smart Container Sizing (Cascade Protection)[/b][/size]
[list]
[*] When [font=Courier New]AutoScaleDependentInputs[/font] is enabled, downstream equipment (Hog Pan, Wave Table, Separator, Trailer) automatically stays at least as large as your bucket multiplier to prevent material overflow.
[/list]

[hr]

[size=5][b]Quick Install Guide[/b][/size]

[list=1]
[*] Make sure you have [b]BepInEx 5 (x64)[/b] and [b]Milex GMS1 CoreMod[/b] installed.
[*] Place [font=Courier New]Milex_GMS1_ProductionTuner.dll[/font] into your [font=Courier New]\Gold Rush The Game\BepInEx\plugins\[/font] folder.
[*] Start the game, press [font=Courier New]Insert[/font], click [b]Production Tuner[/b] in the sidebar, and adjust your sliders!
[/list]

[hr]

[size=5][b]In-Game Controls[/b][/size]

[list]
[*] [b][font=Courier New]Insert[/font]:[/b] Opens the mod menu. Select [b]Production Tuner[/b] to view all sliders.
[*] [b]Category Tabs:[/b] Quickly jump between [i]Hand Tools[/i], [i]Vehicles[/i], [i]Wash Plants[/i], [i]Fine Processing[/i], and [i]Trailers[/i].
[*] [b]Continuous Sliders:[/b] Smooth sliders let you set exact multipliers (from 0.5x up to 10.0x, and up to 20.0x for processing equipment).
[*] [b]Reset Category Button:[/b] Reverts only the current category back to its default values.
[*] [b]Sidebar Active Toggle:[/b] Temporarily disable the mod to restore 100% vanilla behavior without restarting the game.
[/list]

[hr]

[size=5][b]Configuration Reference[/b][/size]

All multipliers can be adjusted in-game ([font=Courier New]Insert[/font]) or in:
[font=Courier New]\Gold Rush The Game\BepInEx\config\Milex_GMS1_ProductionTuner.cfg[/font]

[list]
[*] [b]Shovel Scoop Size (Default: [font=Courier New]6.0x[/font]):[/b] Fills shovel in 1 scoop; 1 scoop = 1 full bucket.
[*] [b]Bucket Capacity (Default: [font=Courier New]2.0x[/font]):[/b] Dirt capacity of handheld buckets.
[*] [b]Handheld Hog Pan (Default: [font=Courier New]6.6x[/font]):[/b] Capacity of the portable early-game Hog Pan.
[*] [b]Excavator Bucket Size (Default: [font=Courier New]3.0x[/font]):[/b] Dirt payload for small and large excavator buckets.
[*] [b]Excavator Boom / Arm Speed (Default: [font=Courier New]2.0x[/font]):[/b] Hydraulic speed for boom lifting and stick reach.
[*] [b]Excavator Swing Speed (Default: [font=Courier New]2.0x[/font]):[/b] Cabin rotation responsiveness.
[*] [b]Wheel Loader Capacity (Default: [font=Courier New]3.0x[/font]):[/b] Loader bucket capacity with automatic lift boost.
[*] [b]Dump Truck Bed (Default: [font=Courier New]3.0x[/font]):[/b] Payload capacity for the heavy rock truck.
[*] [b]Heavy Conveyor Belts (Default: [font=Courier New]3.0x[/font]):[/b] Earth throughput and transport speed on heavy belts.
[*] [b]T3–T5 Synchronized Setup (Default: [font=Courier New]5.0x[/font]):[/b] Scales Sluice mats, grates, Jig buckets, and tailing Hog Pan in lockstep.
[*] [b]Orange Beast Setup (Default: [font=Courier New]2.0x[/font]):[/b] Scales T6 giant mats and gold recovery in lockstep.
[*] [b]Mobile Fuel Trailer (Default: [font=Courier New]3.0x[/font] = 3,000 L):[/b] Scales fuel trailer capacity up to 10,000 Liters (Vanilla: 1,000 L).
[*] [b]Claim Fuel Tank (Default: [font=Courier New]3.0x[/font] = 30,000 L):[/b] Scales stationary tank capacity up to 100,000 Liters (Vanilla: 10,000 L).
[*] [b]Fuel Hose Length (Default: [font=Courier New]5.0x[/font]):[/b] Extra-long flexible hoses with high-strength locking nozzles.
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
[*] Ensure BepInEx 5 and Milex CoreMod are installed in your game directory.
[*] Download this mod.
[*] Place [font=Courier New]Milex_GMS1_ProductionTuner.dll[/font] into your [font=Courier New]\Gold Rush The Game\BepInEx\plugins\[/font] folder.
[*] Launch the game!
[/list]

[b]Uninstallation:[/b]
Delete [font=Courier New]Milex_GMS1_ProductionTuner.dll[/font] from [font=Courier New]\Gold Rush The Game\BepInEx\plugins\[/font]. All vehicle physics, speeds, and capacities immediately revert back to 100% normal vanilla.

[hr]

[size=5][b]Changelog[/b][/size]

[b]Version 1.4.14[/b]
[list]
[*] Added excavator chassis stabilization: parked excavators stay solidly anchored on their tracks and will not roll over while you are away hauling dirt.
[*] Added in-place upright recovery: entering an overturned vehicle sets it upright on its tracks directly where you were working.
[/list]

[b]Version 1.4.12[/b]
[list]
[*] Added synchronized wash plant scaling: a single master slider scales all T3-T5 components (Sluice mats, nugget grates, Jig buckets, and tailing Hog Pan) in 100% lockstep.
[*] Added synchronized scaling for the Tier 6 Orange Beast wash plant setup.
[*] Handheld buckets and manual Hog Pans now scale independently from wash plant setups.
[/list]

[b]Version 1.4.11[/b]
[list]
[*] Added trailer fast-travel auto-reconnection: trailers stay hitched to your pickup after fast travel with full hitch lever functionality.
[/list]

[b]Version 1.4.10[/b]
[list]
[*] Improved fuel trailer and stationary tank fuel saving so refueled tanks never lose fuel after reloading a save.
[/list]

[b]Version 1.4.5[/b]
[list]
[*] Added 1-scoop shovel filling: 1 shovel scoop fills 1 full bucket to 100%.
[*] Added automatic spill protection when pouring buckets into a gold pan so dirt and gold are never wasted.
[/list]

[b]Version 1.4.1[/b]
[list]
[*] Extended fuel hose reach on fuel trailers and claim tanks with high-strength locking nozzles.
[/list]

[b]Version 1.4.0[/b]
[list]
[*] Added continuous smooth sliders across all 29 machinery and logistics parameters.
[*] Expanded mobile fuel trailer (up to 10,000 L) and stationary claim tank (up to 100,000 L).
[*] Added smart container sizing (AutoScaleDependentInputs) to automatically prevent dirt overflow.
[/list]

[b]Version 1.3.1[/b]
[list]
[*] Added dump truck driving mass compensation so fully loaded trucks drive smoothly over rough ground.
[/list]

[b]Version 1.3.0[/b]
[list]
[*] Added independent excavator hydraulic speed sliders (Boom/Arm lift, Turret swing, Bucket tilt).
[*] Added heavy conveyor belt speed and dirt capacity controls for Frankenstein and Cordylus belts.
[/list]

[b]Version 1.2.0[/b]
[list]
[*] Added support for all core mining tools, wash plants, vehicles, and logistics equipment.
[*] Added front loader hydraulic lifting boost for heavy payloads.
[/list]

[b]Version 1.1.0[/b]
[list]
[*] Tuned default multipliers for all machines and tools for optimal gameplay pacing.
[/list]

[b]Version 1.0.0[/b]
[list]
[*] Initial release featuring 5 customizable tuning groups (Hand Tools, Vehicles, Wash Plants, Fine Processing, Logistics).
[/list]
