# AR Reward System Setup Guide

## Overview
This system uses a simple script (`ARCubeRewardManager`) attached to each cube prefab to manage AR rewards directly. Each cube works independently and manages its own AR House, Pet, Car, and Tree children based on the user's correct answers.

## Files Created
1. **`ARCubeRewardManager.cs`** - Simple script that manages AR rewards on each cube
2. **Updated `AnswerVerifier.cs`** - Now uses the new system

## Setup Instructions

### Step 1: Add Script to Cube Prefab
1. Open your `NumberCubePrefab` in the Project window
2. Add the `ARCubeRewardManager` component to the prefab

### Step 2: Manual Reference Assignment (Drag & Drop)
1. In the `ARCubeRewardManager` component, you'll see 4 fields:
   - **arHouse** - Drag your AR House object here
   - **arPet** - Drag your AR Pet object here  
   - **arCar** - Drag your AR Car object here
   - **arTree** - Drag your AR Tree object here

2. **Simply drag and drop** each AR reward object from your hierarchy into the corresponding field

### Step 3: Test the System
1. Use the context menu tests on the `ARCubeRewardManager` component:
   - **"Test Activate House"** - Tests House activation
   - **"Test Activate Pet"** - Tests Pet activation
   - **"Test Activate Car"** - Tests Car activation
   - **"Test Activate Tree"** - Tests Tree activation
   - **"Test Counter-Based Activation"** - Tests the full counter system

## How It Works

### Counter-Based Activation
- **1 correct answer** → Activates AR House
- **2 correct answers** → Activates AR Pet  
- **3 correct answers** → Activates AR Car
- **4 correct answers** → Activates AR Tree

### Independent System
- Each cube manages ONLY its own AR rewards
- No cube depends on any other cube
- Each cube is completely self-contained
- Perfect for AR where cubes can be anywhere in 3D space

### Automatic Behavior
- All AR rewards are automatically deactivated at startup
- When player gives correct answer, reward buttons appear
- Clicking reward buttons activates the appropriate reward on the correct cube
- Wrong answers deactivate all rewards and reset the counter

## Testing

### Context Menu Tests
- **`ARCubeRewardManager` → "Test Counter-Based Activation"** - Tests the full system
- **`AnswerVerifier` → "Test Counter-Based AR Reward Activation"** - Tests integration
- **`AnswerVerifier` → "Destroy All Rewards"** - Tests deactivation

### Manual Testing
1. Generate number line using `ARNumberLineGenerator`
2. Give correct answers to see reward buttons appear
3. Click reward buttons to see AR rewards activate on cubes
4. Give wrong answers to see all rewards deactivate

## Benefits of This System

1. **Simple & Clean** - Just drag and drop references, no complex setup
2. **Independent** - Each cube manages its own rewards
3. **Reliable** - No complex searching or auto-setup that could fail
4. **Easy to Debug** - Clear references, easy to see what's assigned
5. **Build-Safe** - Works identically in both Editor and builds
6. **Counter-Based** - Exactly what you requested!

## Troubleshooting

### "AR House reference is null"
- Make sure you dragged the AR House object into the `arHouse` field
- Check that the AR House object is a child of the cube prefab

### "AR rewards not activating"
- Verify all references are assigned in the Inspector
- Check that AR reward objects are children of the cube prefab
- Use the test context menus to debug

### "Rewards appearing in wrong positions"
- AR rewards will appear exactly where you positioned them in the prefab
- No more floating or misplaced rewards

## Next Steps

1. Add `ARCubeRewardManager` to your cube prefab
2. Drag and drop AR reward references
3. Test with context menus
4. Build and test on device

The system is now **simple, clean, and exactly what you requested** - manual drag-and-drop references with independent cube management! 🎉
