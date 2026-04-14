# 2D Caregiving Simulation

A Unity-based 2D caregiving simulation for aged care task scheduling, multi-nurse coordination, and strategy comparison.

## Overview

This project simulates a U-shaped aged care environment with multiple nurses, room-based task generation, configurable scheduling strategies, and real-time statistics tracking.

The current system focuses on building a modular simulation environment that supports:
- multi-agent nurse behavior
- dynamic task generation
- different scheduling strategies
- task escalation over time
- real-time UI and performance statistics

This project is designed as a foundation for future reinforcement learning based nurse scheduling and AI-assisted decision explanation.

---

## Demo

A playable demo build is available in the Releases section.

> Go to **Releases** to download the demo package.

---

## Tech Stack

- **Unity**
- **C#**
- **TextMeshPro**
- **Rule-based scheduling strategies**
- **Modular simulation framework for future RL integration**

---

## Current System Design

## Table 1: Overview of Implemented Features

| Module | Feature | Status | Description & Configuration |
|------|--------|--------|-----------------------------|
| **Simulation Environment** | U-shaped Care Layout | Completed | Dual-wing corridors with a central nurse station layout |
| | Room Node System | Completed | Each room can generate tasks; a RoomPoint is placed at the entrance of each room; RoomPoint is positioned at the center of the doorway; used as task trigger and navigation target; avoids entering room interiors to simplify movement |
| | Path System (Waypoint) | Completed | Nurses move along predefined waypoints; waypoints are evenly distributed along the center of corridors |
| **Multi-Agent System** | Multi-Nurse System (6 agents) | Completed | Supports parallel execution of multiple nurses; default number is 6; initial positions are located at the nurse station; scalable to N agents |
| | Auto Positioning (Nurse Station) | Completed | Multiple standing positions are defined at the nurse station; each nurse has a fixed initial position |
| | State Machine (Idle / Moving / Working / Returning) | Completed | Full behavioral workflow implemented |
| **Task Generation System** | Automatic Task Generation | Completed | Tasks are randomly generated |
| | 24-Hour Simulation Time | Completed | Simulated 24-hour cycle; time speed adjustable (e.g. 1x / 2x / 5x); task generation depends on time periods |
| | Time Windows (Morning / Afternoon / Night) | Completed | Different time periods generate different task patterns |
| | Adjustable Generation Frequency | Completed | Tasks are randomly distributed across rooms; only one active task per room at a time; generation probability is configurable |
| **Task System** | Task Classification (Light / Medium / Heavy) | Completed | Three task categories: Light / Medium / Heavy |
| | Task Duration | Completed | Each task type has different execution time: Light = 5, Medium = 10, Heavy = 15 (time units) |
| | Task Color Visualization | Completed | Tasks are visually distinguished in UI: Green (Light), Yellow (Medium), Red (Heavy) |
| **Task Escalation Mechanism** | Timeout Escalation | Completed | Tasks automatically escalate if waiting time exceeds threshold (e.g. Light ¡ú Medium at 10, Medium ¡ú Heavy at 15) |
| | Priority Increase | Completed | Task priority increases linearly over time |
| **Scheduling Strategies** | FCFS (First-Come First-Serve) | Completed | Tasks are processed in order of arrival |
| | Shortest Distance | Completed | Selects the nearest task based on path distance |
| | Priority First | Completed | Prioritizes higher urgency tasks |
| | Strategy Switching UI | Completed | Real-time switching between strategies |
| **Statistics System** | Total Tasks | Completed | Real-time tracking |
| | Completed Tasks | Completed | Real-time tracking |
| | Waiting Tasks | Completed | Tracks only unprocessed tasks |
| | Average Waiting Time | Completed | Key performance metric |
| | Task Type Distribution | Completed | Statistics for Light / Medium / Heavy |
| | Total Travel Distance | Completed | Measures nurse movement cost |
| **UI System** | Time Display | Completed | Displays current simulation time |
| | Strategy Buttons | Completed | Allows switching between scheduling strategies |
| | Reset Button | Completed | Resets simulation |
| | Statistics Panel | Completed | Displays real-time system metrics |
| **System Control** | Reset Mechanism | Completed | On reset: all tasks cleared; time reset; nurses return to initial positions |
| | Nurse Reset Positioning | Completed | Ensures all nurses correctly return to starting positions |

---

## Table 2: Partially Implemented / Optimizable Modules

| Module | Feature | Status | Description |
|------|--------|--------|-------------|
| Task State | waiting / assigned / working | Partially Completed | Basic logic implemented; requires further standardization |
| Experiment Control | Fixed Simulation Duration | Not Completed | Currently runs continuously |
| | Fixed Random Seed | Not Implemented | Limits experiment reproducibility |
| Data Recording | Data Export (CSV) | Not Implemented | Currently only displayed in UI |
| Path System | Multi-Agent Path Optimization | Needs Improvement | Path conflicts exist; optimization needed for entering and exiting nurse station |

---

## Table 3: Not Implemented Features

| Module | Feature | Status | Description |
|------|--------|--------|-------------|
| **AI / RL** | Reinforcement Learning Scheduling | Not Implemented | Currently uses rule-based strategies only |
| | AI Policy Learning | Not Implemented | No learning capability yet |
| **Fatigue Model** | Nurse Fatigue Level | Not Implemented | No stamina or fatigue modeling |
| | Fatigue Affecting Speed | Not Implemented | Not integrated into behavior |
| **Task Interruption Mechanism** | High-Priority Interruptions | Not Implemented | No handling of urgent interruptions |
| **Multi-Agent Collaboration** | Cooperative Tasks (e.g. Heavy Tasks) | Not Implemented | No multi-nurse collaboration |
| **Resource Constraints** | Equipment Limitations | Not Implemented | e.g. wheelchairs, carts |
| | Congestion System | Not Implemented | Corridor crowding not modeled |
| **Advanced Metrics** | Timeout Rate | Not Implemented | Important performance metric |
| | P95 Waiting Time | Not Implemented | Advanced statistical metric |
| | Nurse Utilization | Not Implemented | Workload analysis |
| **Resident Modeling** | Different Care Levels | Not Implemented | Affects task generation probability |
| **Shift System** | Nurse Shift Rotation | Not Implemented | No shift scheduling implemented |

---

## Simulation Configuration

The current simulation environment is designed to be modular and configurable for future experiments.

### Environment Layout
- U-shaped care layout with dual wings
- Central nurse station as the dispatching hub
- Room entrances represented by RoomPoints
- Corridor-based movement using waypoints

### Agents
- Default number of nurses: 6
- Each nurse has a fixed initial position in the nurse station
- Supports parallel task execution
- Agent behavior follows a state machine:
  - Idle
  - Moving
  - Working
  - Returning

### Tasks
- Task categories:
  - Light
  - Medium
  - Heavy
- Task duration:
  - Light = 5
  - Medium = 10
  - Heavy = 15
- Only one active task can exist in a room at one time
- Tasks are generated dynamically based on simulation time windows

### Task Escalation
- Light task escalates to Medium when waiting time reaches 10
- Medium task escalates to Heavy when waiting time reaches 15
- Task priority increases over time

### Scheduling Strategies
- FCFS
- Shortest Distance
- Priority First

### Statistics
- Total tasks
- Completed tasks
- Waiting tasks
- Average waiting time
- Task type distribution
- Total travel distance

---

## Future Work

The next development stage will focus on AI-based scheduling and experiment control.

Planned future extensions include:
- reinforcement learning based nurse scheduling
- AI policy learning
- CSV data export for experiments
- fixed random seed for reproducibility
- fatigue model
- urgent task interruption mechanism
- cooperative heavy-task handling
- congestion modeling
- advanced metrics such as timeout rate, P95 waiting time, and nurse utilization
- resident care-level modeling
- nurse shift scheduling
- LLM-based explanation for scheduling decisions

---

## RL Direction

A future reinforcement learning module is planned to replace or augment the current rule-based scheduling system.

The intended RL setup is:

- **Agent**: nurse scheduler / nurse agent
- **Environment**: aged care simulation environment
- **State**: current nurse location, active tasks, task priority, waiting time
- **Action**: choose the next room/task
- **Reward**: encourage efficient task completion, shorter waiting time, and urgent task prioritization

The goal is to compare RL-based scheduling against existing rule-based baselines such as FCFS, Shortest Distance, and Priority First.

---

## Project Goal

The goal of this project is not only to build a caregiving simulation, but also to create a configurable experimental platform for:
- scheduling strategy comparison
- multi-agent behavior simulation
- AI integration
- reinforcement learning research
- explainable decision support in aged care workflows

---

## Repository Structure

```text
Assets/
Packages/
ProjectSettings/
README.md