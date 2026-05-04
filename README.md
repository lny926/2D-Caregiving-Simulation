# 2D Caregiving Simulation

## Overview
This project simulates a caregiving environment with multiple nurses handling dynamic and routine tasks.

The system has evolved from a basic simulation into an **experiment-driven simulation framework**, supporting controllable parameters, reproducible experiments, and extended statistical analysis.

---

## Features

###  Simulation Core
- U-shaped ward layout with dual corridors and central nurse station
- Multi-agent system (multiple nurses)
- State machine: Idle / Moving / Working / Returning
- Waypoint-based path system

---

###  Task System
- Dynamic task generation (Light / Medium / Heavy)
- Burst generation (1每4 tasks per cycle)
- Time-window-based workload (Morning / Midday / Evening)

#### Task Duration Model (Updated)

**Previous design (fixed):**
- Light: 5  
- Medium: 10  
- Heavy: 15  

**Current design (random intervals):**
- Light: 2每5 minutes  
- Medium: 6每15 minutes  
- Heavy: 12每30 minutes  

---

###  Routine Task System
- Periodic tasks (e.g., medication)
- Multiple rooms triggered simultaneously
- UI indicators (active / inactive)
- Workflow:
  - Return to nurse station
  - Resource pickup
  - Deliver to room

---

###  Escalation System
- Randomized escalation threshold
- Multi-stage escalation:
  - Light ↙ Medium  
  - Medium ↙ Heavy  
  - Heavy ↙ Secondary Call  

---

###  Scheduling System

Supported strategies:
- FCFS  
- Shortest Distance  
- Priority First  
- AI Score  

#### AI Score Model
Score = w1 * PriorityScore 
      + w2 * WaitingTimeScore 
      - w3 * DistanceCost 
      - w4 * FatigueCost



**Components:**
- PriorityScore:
  - Light = 1  
  - Medium = 2  
  - Heavy = 3  

- WaitingTimeScore ≦ waiting time  
- DistanceCost ≦ path length  
- FatigueCost ≦ nurse fatigue  

**Weights:**
- w1: priority  
- w2: waiting time  
- w3: distance penalty  
- w4: fatigue penalty  

Weights are manually defined (heuristic) and can be tuned through experiments.

**Relation to rule-based strategies:**
- FCFS ↙ waiting time dominant  
- Shortest Distance ↙ distance dominant  
- Priority First ↙ priority dominant  

**Future direction:**
- Reinforcement Learning (RL) for policy learning  
- AI Score used as baseline  

---

###  Experiment System
- Configurable start time (e.g., 08:00)
- Multi-day simulation
- Adjustable simulation speed (time scale)
- Fixed random seed (reproducibility)
- UI-based start / stop control
- Automatic system reset before experiment

---

###  Path & Movement System
- Removal of shared BaseCenter
- Per-nurse station & exit points
- Dynamic path construction
- Room-to-room transitions supported

---

###  Statistics System

**Basic metrics:**
- Total tasks
- Completed tasks
- Waiting tasks
- Average waiting time

**Extended metrics:**
- Max waiting time
- P95 waiting time
- Completion rate
- Total travel distance
- Escalation breakdown
- Routine task statistics

---

## Current Status
- Stable simulation system completed
- Experiment framework implemented
- Routine task system integrated
- AI Score scheduling implemented

---

## Known Limitations
- Time system not fully unified across modules
- No CSV export yet
- No batch experiment support

---

## Controls
- Click room to generate tasks
- Start / Stop experiment buttons
- Strategy selection buttons

---

## Future Work
- Unified simulation time system
- CSV-based data export
- Reinforcement Learning scheduling
- Multi-agent coordination
- Advanced performance metrics