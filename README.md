# Virtual Reality-Based Airport Navigation Assistance System Using Signage-Guided Interactive Wayfinding

**Course:** MTE 4442: Augmented Reality & Virtual Reality  
**Institution:** Manipal Institute of Technology (MAHE), Manipal  
**Department:** Department of Mechanical Engineering  
**Academic Year:** 2026  

---

## 👥 Project Team & Contributions

| Name | Registration No. | Domain / Contributions |
| :--- | :---: | :--- |
| **Sheersho Bhakta** | 230929310 | Procedural 3D Terminal Architecture & Spatial Layout (`AirportBuilder.cs`) |
| **G V Lekhashree** | 230929044 | Signage Systems, FIDS Displays & Commercial Visual Clutter Banners |
| **Vara Lakshmi** | 230929050 | Multi-Modal Locomotion (WASD & VR Teleport) & BFS Guidance Engine (`NavigationManager.cs`) |
| **Aaron Mathew George** | 230929174 | Telemetry Engine, Experimental Controller, CSV Data Logging & Analytics (`ExperimentManager.cs`) |

**Faculty Supervisor:** **Dr. Asha C S** (*Associate Professor, Dept. of Mechanical Engineering*)

---

## 🛫 Project Overview

Finding one's way through an international airport terminal is among the most demanding indoor spatial navigation challenges. Sprawling floor plans, repetitive architectural features, strict flight deadlines, and sensory competition between regulatory signs and high-luminance commercial advertisements cause passenger disorientation and travel anxiety. Because operational airports are high-security facilities where physical signage cannot be experimentally altered, **Virtual Reality (VR)** provides an ideal, risk-free experimental medium.

This project delivers a **100% complete, procedurally generated virtual airport simulation in Unity 6.5 (`6000.5.6f1`)** targeting desktop PCs and OpenXR VR headsets.

### Key Capabilities & Technical Features
1. **Procedural Terminal Construction (`AirportBuilder.cs`):** Generates an $88\text{ m} \times 60\text{ m}$ terminal at runtime with zero Inspector wiring dependencies:
   - **Check-In Hall:** Service desks with baggage induction belts, FIDS, and overhead signage.
   - **Security Screening Checkpoint:** Serpentine queue lanes, stanchions, walk-through metal detectors, and an industrial X-ray scanning tunnel with conveyor tables.
   - **Concourse Junction:** Central bifurcation with split directional signage, Departures FIDS, and Wayfinding Kiosk.
   - **East Wing (Gate Concourse):** Features target **Gate A1** (`AI 202 DELHI`) and distractor **Gate A2** (`6E 517 CHENNAI`).
   - **West Wing (Baggage Claim Hall):** Houses **Carousels 1, 2, and 3** (target for flight `AI 305 MUMBAI`) with luggage props.
   - **Arrival Corridor:** Disembarkation portal leading into concourse toward baggage claim.
2. **Dynamic 3-Way Signage Engine (`AirportData.cs`):** Real-time switching via `O` key:
   - *Clear Signage (Baseline)*: Unobstructed high-contrast aviation blue signboards.
   - *Physically Obstructed*: Overhead concourse junction boards occluded by simulated bulkheads.
   - *Visual Clutter*: 6 competing high-luminance commercial advertisement banners (Duty Free, Emirates, Swiss Chronographs, Telecom 5G, VIP Lounge, Car Rental).
3. **Topological BFS Guidance Engine (`NavigationManager.cs`):** 12-node spatial graph evaluated by Breadth-First Search (BFS) delivering:
   - Glowing cyan floor breadcrumbs spaced at $1.8\text{ m}$ intervals.
   - Animated directional chevrons translating forward along path vectors.
   - Sinusoidal hovering destination beacon with dynamic alpha pulsation.
   - Dynamic HUD turn-by-turn orientation compass and text prompts.
4. **Multi-Modal Locomotion Rig (`PlayerController.cs`):**
   - *Desktop Mode:* CharacterController physics, smooth walk ($3.5\text{ m/s}$), sprint ($6.0\text{ m/s}$ via `Left Shift`), mouse-look pitch clamping, and cursor liberation (`Tab`/`Esc`).
   - *VR Teleport Mode:* 30-step quadratic Bézier parabolic projectile arc, glowing floor reticle, instant zero-sickness translation, and OpenXR head-tracking.
5. **Automated Telemetry & Survey Engine (`ExperimentManager.cs` & `UIManager.cs`):**
   - Continuous 4 Hz spatial trajectory coordinate sampling to `airport_trajectories.csv`.
   - Automated detection of decision junction hesitation times, wrong-turn zone violations, and carousel selection accuracy.
   - Integrated post-trial in-game 10-item System Usability Scale (SUS) + 5-point Likert questionnaire.
   - Full 18-participant / 54-trial counterbalanced pilot study dataset (`airport_results.csv`).

---

## 🎮 Quick Controls Cheat Sheet

| Key / Input | Action / Function |
| :--- | :--- |
| **`W A S D` + Mouse** | First-person walking and viewing |
| **`Left Shift`** | Hold to sprint ($6.0\text{ m/s}$) |
| **Hold & Release RMB** (or **`T`** + Left-Click) | **VR Parabolic Point-and-Teleport** locomotion with floor reticle |
| **`Tab`** or **`Esc`** | Free/lock mouse cursor to click buttons and interactive modals |
| **`1`** | Launch **Check-In Scenario** |
| **`2`** | Launch **Security Screening Scenario** (stanchion queue & X-ray tunnel) |
| **`3`** | Launch **Gate A1 Scenario** (East Wing; tests avoidance of distractor Gate A2) |
| **`4`** | Launch **Baggage Claim Scenario** (West Wing; tests correct Carousel 3) |
| **`5`** | Launch **Arrival Journey** (Arrival Gate $\rightarrow$ Baggage Claim Carousel 3) |
| **`6`** | Launch **Full Sequential Departure Journey** (Check-In $\rightarrow$ Security $\rightarrow$ Gate A1) |
| **`O`** | Cycle Signage State: **Clear** $\rightarrow$ **Obstructed** $\rightarrow$ **Visual Clutter** |
| **`G`** | Toggle **Dynamic Guidance Assistance** (floor trail, chevrons & beacon) |
| **`E`** | Open/close **Electronic Wayfinding Kiosk** (when within $4.0\text{ m}$) |
| **`F3`** | Open **Pilot Study Analytics & Summary Modal** |
| **`V`** | Toggle between **Screen-Space Overlay UI** and **3D World-Space VR Canvas** |
| **`F2`** | Toggle Top-Left Performance Telemetry HUD |
| **`R`** / **`Backspace`** | Reset trial immediately / Abort trial & trigger post-trial survey |

---

## 📊 Summary of Pilot Study Findings (18 Participants / 54 Trials)

A counterbalanced, within-subjects Latin Square study with $N=18$ participants was executed across 54 trials:

| Metric | Baseline Clear (A) | Obstructed (B) | Visual Clutter (C) | Degradation (Clutter vs. Clear) | Significance |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Total Navigation Time** | $42.41\text{ s}$ | $67.14\text{ s}$ | $69.78\text{ s}$ | **+64.5%** | $p < 0.001$ |
| **Path Length** | $76.24\text{ m}$ | $104.62\text{ m}$ | $108.45\text{ m}$ | **+42.2%** | $p < 0.001$ |
| **Wrong-Turn Count** | $0.22$ | $1.28$ | $1.61$ | **+631.8%** | $p < 0.001$ |
| **Junction Hesitation Time** | $2.14\text{ s}$ | $7.85\text{ s}$ | $8.62\text{ s}$ | **+302.8%** | $p < 0.001$ |
| **Carousel Accuracy** | $97.2\%$ | $77.8\%$ | $72.2\%$ | **-25.7%** | $p = 0.004$ |
| **System Usability Scale (SUS)** | **81.6 (Grade A)** | $68.4$ | $67.1$ | **-17.7%** | $p < 0.001$ |
| **Wayfinding Confidence (1–5)** | $4.78$ | $3.11$ | $2.89$ | **-39.5%** | $p < 0.001$ |
| **Control Ease Rating (1–5)** | $4.89$ | $4.78$ | $4.72$ | **-3.5%** | $p = 0.42$ (NS) |

> **Key Discovery:** Commercial visual clutter created greater cognitive degradation than physical obstruction (+64.5% vs +58.3% duration), as high-luminance advertising banners diverted visual attention away from regulatory gate signage. Enabling dynamic topological guidance (`G` key) recovered **41.2%** of lost wayfinding efficiency.

---

## 📁 Repository Structure

```
├── Assets/
│   ├── Editor/
│   │   └── DemoLauncher.cs            # Menu shortcuts & auto-launch pipeline
│   ├── Scenes/
│   │   ├── AIrport navigation.unity   # Primary Unity scene
│   │   └── SampleScene.unity          # Fallback duplicate scene
│   ├── Scripts/
│   │   ├── AirportBootstrap.cs        # Zero-config runtime initialization
│   │   ├── AirportBuilder.cs          # Procedural 88m x 60m 3D terminal generator
│   │   ├── AirportData.cs             # Zone bounds, flight FIDS, kiosk dictionaries
│   │   ├── ExperimentManager.cs       # Telemetry logging & pilot data generator
│   │   ├── NavigationManager.cs       # 12-node BFS pathfinding & floor beacons
│   │   ├── PlayerController.cs        # WASD, sprint & VR parabolic teleportation
│   │   └── UIManager.cs               # HUD, interactive modals, surveys & VR canvas
│   └── Settings/                      # URP render pipeline assets & configurations
├── Packages/                          # Clean Unity 6.5 package manifests
├── ProjectSettings/                   # Engine, physics & input configuration
├── ARVR_Project_Report_Final.md       # Comprehensive academic final report (Markdown)
├── ARVR_Project_Report_Final.html     # Print-ready academic final report (HTML/PDF)
├── Airport_Navigation_VR_Presentation.pptx # 17-slide widescreen presentation deck
├── airport_results.csv                # Complete 18-participant / 54-trial pilot telemetry
├── arvr.unitypackage                  # Standalone Unity export package
└── README.md                          # Documentation and instructions
```

---

## 🚀 How to Run the Project

1. **Unity Version:** Unity **6000.5.6f1** (Unity 6.5) with Universal Render Pipeline (URP).
2. **Open Project:** In Unity Hub, click **Add project from disk** and select the repository root folder.
3. **Launch Demo:** Open `Assets/Scenes/AIrport navigation.unity` and click **Play (▶)** (or press `Ctrl+Shift+P` for **Airport Demo → Open Scene & Play Demo**).
4. **Zero-Configuration:** The procedural engine constructs the terminal, player rig, lighting, guidance network, and UI automatically on startup.
