# AutoCopilot

An onboard copilot program for automating functions and devices on your drone. 

Easily compose, transmit, and execute sets of instructions on your DJI or MAVLink drone to control custom devices or drone behaviour using an onboard companion computer (Raspberry Pi or other device) based on real-time telemetry. 

Send instructions simultaneously to multiple drones with ease. Rely on the mission to continue to operate predictably with partial or total signal loss. 

Or, if the instruction framework doesn't cover your use-case, incorporate custom functions and behaviour using the included APIs. 

*This is an open source work-in-progress based on my own unreleased drone automation software, currently used in real-world operations.*

# Objectives

* Create an onboard computer API that provides a generic interface for both DJI and MAVlink based drones to receive basic telemetry and issue simple commands. 
* Without any additional scripting the onboard computer software can run as a receiver and handler for drone instructions sent by the ground station API. 
* Provide a ground station API for creating and synchronizing instructions with a drone's onboard computer running the companion software. 

# Ground Station Code Examples
*This project is a work-in-progress. The code examples represent the intended functionality, not what is yet available.*

## Activate/Deactivate a servo in an area
```C#
// An InstructionSequence only triggers instructions after the one before it has been completed.
// Because it is looping, after all instructions have been triggered the sequence will loop from the start

var instructions = new InstructionSequence(looping=true) {
	new Instruction(
		Conditions.InsidePolygon(area.Polygon),
		Actions.Log("Activating Servo"),
		Actions.SetMavlinkServo(9, 2000)),

	new Instruction(
		Conditions.OutsidePolygon(area.Polygon),
		Actions.Log("Deactivating Servo"),
		Actions.SetMavlinkServo(9, 1000))
};

Copilot.SetInstructions(instructions);
```

## Activate in an area with a time-limit for the servo
```C#
// Instruction groups by default are looping and will not wait for instructions to complete in sequence
var instructions = new InstructionGroup("ServoActivationArea") {

	new InstructionSequence("ActivationLoop", looping=true) {
		new Instruction(
			Conditions.InsidePolygon(areaPolygon),
			Actions.Log("Activating Servo"),
			Actions.StartTimer("ActivationTimer"),
			Actions.SetMavlinkServo(9, 2000)),

		new Instruction(
			Conditions.OutsidePolygon(areaPolygon),
			Actions.Log("Deactivating Servo"),
			Actions.StopTimer("ActivationTimer"),
			Actions.SetMavlinkServo(9, 1000))
	},

	new Instruction("TimerCompleteInstruction"
		Conditions.TimerPassed("ActivationTimer", 60)
		Actions.SetMavlinkServo(9, 1000),
		Actions.StopTimer("ActivationTimer"),
		Actions.Log("Passed activation timer limit - stopping servo and returning home."),
		// If an instruction or instruction group is marked as Finished, it won't be visited anymore after this point
		Actions.FinishInstruction("ServoActivationArea")
		Actions.InitiateReturnHome())
    
  	// The instruction exector will visit ActivationLoop to see if there's any update
	// then visit TimerCompleteInstruction to see if there's any update,
	// then loop from the start
}

Copilot.SetInstructions(instructions);
```

## Activate the servo in many polygons in sequence
```C#
// These will be common groups of actions, so make them into action lists to keep things clean
var StopServoActions = new List<CopilotAction> {
	Actions.StopTimer("ActivationTimer"),
	Actions.Log("Deactivating Servo"),
	Actions.SetMavlinkServo(9, 1000) 
};

var StartServoActions = new List<CopilotAction> {
	Actions.StartTimer("ActivationTimer"),
	Actions.Log("Activating Servo"),
	Actions.SetMavlinkServo(9, 2000) 
};

// With FromCollection we can use a lambda expression to convert a list of objects into multiple instruction sequences
var ActivationSequence = InstructionSequence.FromCollection<ActivationArea>(ActivationAreas,
	area => new InstructionSequence(area.Name) {
		  new InstructionGroup() {
			  new InstructionSequence(looping=true) {
				  new Instruction(
					  Conditions.InsidePolygon(area.Polygon),
					  StartServoActions),

				  new Instruction(
					  Conditions.OutsidePolygon(area.Polygon)
					  StopServoActions),
			  },

			  new Instruction(
				  Conditions.ProximityToPoint(area.EndPoint, 10),
				  StopServoActions,
				  Actions.Log("Finished " + area.Name),
				  Actions.FinishInstruction(area.Name))

			  new Instruction("TimerCompleteInstruction"
				  Conditions.TimerPassed("ActivationTimer", 60),
				  StopServoActions,
				  Actions.Log("Passed activation timer limit - returning home."),
				  Actions.FinishInstruction(area.Name)
				  Actions.InitiateReturnHome())
		  }
	});

// We now want the very last instruction in the sequence to return the drone home
ActivationSequence.AddToEnd(
	new Instruction(
		Actions.Log("Finished all activation area instructions. Returning home."),
		Actions.InitiateReturnHome())
	);

Copilot.SetInstructions(ActivationSequence);
```

## Add failsafe instructions to handle unexpected behaviour
```C#
var FailsafeInstructions = new InstructionGroup("Failsafes") {
	// Without the copilot interface, we can't issue commands to the drone.
	// But we can set a flag to stop everything when the interface comes back.
	new Instruction("Interface Connection Lost Failsafe",
		Conditions.AllOf(Conditions.AutoMode(true), Conditions.InterfaceStatus(false), Conditions.ReturnMode(false)),
		Actions.SetFlag("InterfaceDropped", true),
		Actions.Log("Copilot interface connection was lost while the drone was on autopilot.")),

	// If the copilot interface comes back after being dropped, we want to call off the mission
	new Instruction("Interface Connection Regained",
		Conditions.AllOf(Conditions.GetFlag("InterfaceDropped", true), Conditions.InterfaceStatus(true), Conditions.AutoMode(true), Conditions.ReturnMode(false)),
		StopServoActions,
		Actions.SetFlag("InterfaceDropped", false),
		Actions.InitiateReturnHome()),

	// If the pilot takes control of the drone, make sure the servo stops
	new Instruction("Pilot Took Control Failsafe",
		Conditions.AutoMode(false),
		StopServoActions,
		Actions.Log("Pilot took control of the drone.")),

	// If the drone initiates a return home, make sure the servo stops if it wasn't already stopped
	new Instruction("Unexpected Return Home Failsafe",
		Conditions.AllOf(Conditions.ReturnMode(true), Conditions.TimerState("ActivationTimer", true)),
		StopServoActions,
		Actions.Log("Drone started returning unexpectedly while on mission.")),

	// If the drone stops moving for a few seconds while the servo is active, stop it
	new Instruction("Stopped While Activated Failsafe",
		Conditions.AllOf(Conditions.StoppedTimePassed(3), Conditions.TimerState("ActivationTimer", true)),
		StopServoActions,
		Actions.Log("Drone stopped moving while activated."))
};

// Add conditions to the instruction sequence so it will only be visited while the drone is:
// In auto-mode, not returning, and not stopped
ActivationSequence.AddConditions(Conditions.AutoMode(true), Conditions.ReturnMode(false), Conditions.StoppedTimeBelow(3));
Copilot.SetInstructions(FailsafeInstructions, ActivationSequence);
```
## Use CopilotStages to group more complicated instruction sets
```C#
// Like InstructionSequences, InstructionGroups can be given conditions

var InstructionSet = new InstructionGroup() {
	// Only move into the READY stage if the interface is online
	new InstructionGroup(Conditions.CopilotStage("NOT READY")) {
			new Instruction(
			Conditions.InterfaceStatus(true),
			Actions.SetCopilotStage("READY")),
		},

	new InstructionGroup(Conditions.CopilotStage("READY")) {
		// Reset ActivationTimer to 0 if it had any time on the clock
		new Instruction(
			Conditions.AllOf(Conditions.FlightMode(FlightMode.OnGround), Conditions.TimerPassed("ActivationTimer", 0)),
			Actions.StopTimer("ActivationTimer"),
			Actions.SetCopilotStage("ActivationTimer")),

		// Return to NOT READY if the interface drops
		new Instruction(
			Conditions.InterfaceStatus(false),
			Actions.SetCopilotStage("NOT READY")),

		// Once the drone is in the air, in auto-mode, and not stopped, we enter the ON MISSION stage
		new Instruction(
			Conditions.AllOf(Conditions.FlightMode(FlightMode.InAir), Conditions.AutoMode(true), Conditions.StoppedTimeAt(0)),
			Actions.SetCopilotStage("ON MISSION"))
	},

	new InstructionGroup(Conditions.SetCopilotStage("ON MISSION")) {
		// Return to NOT READY if the interface drops
		new Instruction(
			Conditions.InterfaceStatus(false),
			Actions.SetCopilotStage("NOT READY")),

		// Return to READY if the drone leaves auto-mode, enters a return mode, or stops moving for a while
		new Instruction(
			Conditions.AnyOf(Conditions.AutoMode(false), Conditions.ReturnMode(true), Conditions.StoppedTimePassed(3)),
			Actions.SetCopilotStage("READY")),

		ActivationSequence
	}
};

Copilot.SetInstructions(FailsafeInstructions, InstructionSet);
Copilot.SetDefaultCopilotStage("NOT READY");
```

# Documentation

## InstructionGroup
Base class for collections of instructions.
#### Constructors
```C#
InstructionGroup(string name, Condition groupCondition = Conditions.NoCondition(), executionOrder = ExecutionOrder.Parallel)
InstructionGroup(Condition groupCondition, string name = "", executionOrder = InstructionExecutionOrder.Parallel)
```
#### Properties
* UUID
* ExecutionOrder
  * Parallel (default) **The intructions will be visited one at a time, but the exector won't wait for previous instructions to complete to move on.**
  * SingleSequence **The instructions are executed one at a time, holding until each one completes in sequence. After all are instructions are complete the group will not be revisited.**
  * RepeatingSequence **The instructions are executed one at a time, holding until each one completes in sequence. After all are instructions are complete the group will restart from the beginning.**
* *(Optional)* Name
* Completed *(True/False: When the instruction is marked as completed it won't be revisited.)*

## InstructionSequence
Simple wrapper class for sequence InstructionGroups to improve readability
#### Constructors
```C#
InstructionSequence(string name, Condition groupCondition = Conditions.NoCondition(), bool looping = false) : 
	InstructionGroup(name, groupCondition, looping ? ExecutionOrder.RepeatingSequence : ExecutionOrder.SingleSequence)
InstructionSequence(Condition groupCondition, string name = "", bool looping = false) : 
	InstructionGroup(groupCondition, name, looping ? ExecutionOrder.RepeatingSequence : ExecutionOrder.SingleSequence)
```

## Instruction
Instruction for the Copilot to trigger an action either based on conditions or as soon as the instruction is reached.
#### Constructors
```C#
Instruction(params CopilotAction[] actions)
Instruction(List<CopilotAction> actionList, params CopilotAction[] actions)
Instruction(Condition, params CopilotAction[] actions)
Instruction(Condition, List<CopilotAction> actionList, params CopilotAction[] actions)
Instruction(List<Condition>, params CopilotAction[] actions)
Instruction(List<Condition>, List<CopilotAction> actionList, params CopilotAction[] actions)
```
#### Properties
* UUID
* *(Optional)* Name
* Conditions collection
* Actions collection
* Triggered *True/False*
* LastTriggeredTime
* LastTriggeredPosition

## Conditions
* InsidePolygon(*polygon*)
* OutsidePolygon(*polygon*)
* ProximityToPoint(*point*, *metres*)
* TimerState(*timer name*, *running: true/false*)
* TimerPassed(*timer name*, *seconds*)
* AutoMode(*enabled: true/false*)
* InterfaceStatus(*online: true/false*)
* ReturnMode(*enabled: true/false*)
* FlightMode(*Stopped, OnGround, InAir*)
* StoppedTimePassed(*seconds*)
* GetFlag(*name*, *set: true/false*)
* CopilotStage(*name*)

## Actions
* Log(*text*)
* SetMavlinkServo(*servo number*, *PWM*)
* SetRPIPWM(*PWM number*, *PWM*)
* SetRPIGPIO(*PIN number*, *true/false*)
* StartTimer(*name*)
* StopTimer(*name*)
* ResetTimer(*name*)
* InitiateReturnHome()
* FinishInstruction(*name*)
* SetFlag(*name*, *true/false*)
* SetCopilotStage(*name*)

## Protocol

#### Commands sent to Onboard Copilot
* set_instructions JSON
* execute_instruction NAME
* execute_instructions NAME
* get_state
