---
created: 2022-07-31T12:16:25-04:00
modified: 2022-08-05T17:17:20-04:00
---

# AutoCoPilot

An onboard copilot program for automating functions and devices on your drone. 

Easily compose, transmit, and execute sets of instructions on your DJI or MAVLink drone to control custom devices or drone behaviour using an onboard companion computer (Raspberry Pi or other device) based on real-time telemetry. 

Send instructions simultaneously to multiple drones with ease. Rely on the mission to continue to operate predictably with partial or total signal loss. 

Or, if the instruction framework doesn't cover your use-case, incorporate custom functions and behaviour using the included APIs. 

*This is an open source work-in-progress based on my own unreleased drone automation software, currently used in real-world operations.*

# Objectives

* Create an onboard computer API that provides a generic interface for both DJI and MAVlink based drones to receive basic telemetry and issue simple commands. 
* Without any additional scripting the onboard computer software can run as a receiver and handler for drone instructions sent by the ground station API. 
* Provide a ground station API for creating and synchronizing instructions with a drone's onboard computer running the companion software. 

# Documentation

---

# Mission
## Instructions Collection
* Name
* Sequence True/False
* Optional Condition collection
* Instruction Collection

By default, instruction collections do not wait for the previous instruction to complete, only when flagged as a sequence. 

Instruction collections and composed objects all serialize to JSON 

# Instruction

* Name
* ID
* Last Modification Time
* Conditions collection
* Actions collection

## Conditions
* **DRONE_PROTOCOL** is *Enum*
* **FLIGHT_STATE** is *Enum*
* **MODE_NAME** is *String*
* **AUTO_MODE** is *Bool*
* **RETURN_MODE** is *Bool*
* **DISTANCE_FROM_POINT** is *Metres* with *Coord*
* **DISTANCE_FROM_POLYGON** is *Metres* with *Polygon*
* **INSIDE_POLYGON** is *Bool* with *Polygon*
* **MISSION_STATE** is *String*
* **TIMER_STATE** is *Number* with *Timer Name*
* **RPI_GPIO** is *Bool* with *Pin Number*

## Actions
* **SET_RPI_PWM** to *PW* with *PWM Number* and *HZ*
* **SET_RPI_GPIO** to *Bool* with *Pin Number*
* **SET_MAVLINK_SERVO** to *PW* with *Servo Number*
* **START_TIMER** with *Name*
* **PAUSE_TIMER** with *Name*
* **RESET_TIMER** with *Name*
* **SET_MISSION_STATE** to *String*
* **INITIATE_RETURN_HOME**

# Protocol

## Commands sent to Onboard Copilot
* set_instructions JSON
* execute_instruction NAME
* execute_instructions NAME
* get_state
