# Practice Mode Setup Guide

## 🎯 **Overview**
Practice Mode is a tutorial-based learning system that copies all the mechanics from Mobile Mode but adds:
- **Tutorial System** - Step-by-step learning guide
- **Unlimited Attempts** - No pressure, learn at your own pace
- **Hint System** - Get help when stuck
- **Safe Learning Environment** - Perfect for beginners

## 🚀 **Quick Setup**

### **1. Create Practice Scene**
- Duplicate your `MobileGame` scene
- Rename it to `PracticeGame`
- Save it in `Assets/Scenes/`

### **2. Add PracticeGameManager**
- In your PracticeGame scene, create an empty GameObject
- Name it `PracticeGameManager`
- Add the `PracticeGameManager` script component
- Use **🔧 Auto-Assign All References** context menu to assign all references automatically

### **3. Add TutorialUI**
- Create a UI Canvas for tutorial overlay
- Add the `TutorialUI` script component to your Tutorial Panel
- Use **🔧 Auto-Assign Tutorial UI References** context menu to assign all UI elements

### **4. Update Main Menu**
- Replace `GameModeManager` with `SceneLoader` (much simpler!)
- The main menu just needs buttons that call:
  - `LoadMobileGame()`
  - `LoadARGame()`
  - `LoadPracticeGame()`
  - `LoadMainMenu()`

## 🔧 **Component References**

### **PracticeGameManager Required References:**
```
PlayerController → Your player character
AnswerVerifier → Your answer verification system
UIManager → Your UI management system
GameManager → Your game management system
AudioManager → Your audio system
```

### **TutorialUI Required References:**
```
Tutorial Panel → Main tutorial overlay GameObject
Tutorial Title → TextMeshPro for step titles
Tutorial Description → TextMeshPro for step descriptions
Previous Button → Button for going back
Next Button → Button for going forward
Skip Button → Button to skip tutorial
Close Button → Button to close tutorial
Progress Slider → Slider showing tutorial progress (optional)
```

## 📚 **Tutorial System Features**

### **Default Tutorial Steps:**
1. **Welcome** - Introduction to practice mode
2. **How to Play** - Basic game mechanics
3. **Practice Features** - What makes practice mode special
4. **Ready to Start** - Final confirmation

### **Customizing Tutorial:**
- Edit `tutorialSteps` list in PracticeGameManager
- Add custom images for each step
- Modify step descriptions
- Add action requirements

### **Tutorial Controls:**
- **Previous/Next** - Navigate between steps
- **Skip** - Jump to end of tutorial
- **Close** - Hide tutorial panel
- **Progress Bar** - Shows current position

## 🎮 **Practice Mode Features**

### **Learning-Friendly Settings:**
- ✅ Unlimited attempts on questions
- ✅ Hints available for each question
- ✅ No pressure or time limits
- ✅ Learn from mistakes

### **Hint System:**
- Contextual hints based on operation type
- Progressive hint system
- Customizable hint content
- Audio feedback support

## 🧪 **Testing**

### **Context Menu Tests:**
- **🔧 Auto-Assign All References** - Automatically assigns all missing references
- **🔍 Check Reference Status** - Shows what's assigned and what's missing
- **Test Tutorial System** - Shows first tutorial step
- **Complete Tutorial** - Skips to end
- **Show Hint** - Displays current hint

### **Manual Testing:**
1. Load PracticeGame scene
2. Tutorial should appear automatically
3. Navigate through tutorial steps
4. Test hint system with equations
5. Verify unlimited attempts work

## 🔄 **Integration with Existing Systems**

### **SceneLoader (replaces GameModeManager):**
- Simple scene loading without mode complexity
- Just calls `SceneManager.LoadScene()` for each game type
- Includes button sound effects

### **AnswerVerifier:**
- Works exactly like mobile mode
- Same reward system
- Same equation generation
- Same player movement

### **Audio System:**
- Uses `practice_music` (can be same as mobile)
- Button click sounds
- Tutorial navigation audio

## 📱 **UI Layout Suggestions**

### **Tutorial Panel Design:**
```
┌─────────────────────────────────┐
│           TUTORIAL              │
├─────────────────────────────────┤
│  [Previous] [1/4] [Next]       │
├─────────────────────────────────┤
│  Welcome to Practice Mode!      │
│                                 │
│  This is a safe space to learn │
│  math without pressure...       │
│                                 │
│  [Skip Tutorial] [Close]       │
└─────────────────────────────────┘
```

### **Mobile Layout:**
- Tutorial panel overlays on top
- Semi-transparent background
- Non-blocking to game interaction
- Responsive design for different screen sizes

## 🎯 **Next Steps**

1. **Create PracticeGame scene** from MobileGame
2. **Add PracticeGameManager** with all references (use auto-assign!)
3. **Create TutorialUI** with proper layout (use auto-assign!)
4. **Test tutorial system** using context menus
5. **Customize tutorial content** for your needs
6. **Add practice-specific audio** if desired
7. **Test full flow** from main menu to practice

## 🐛 **Troubleshooting**

### **Tutorial Not Showing:**
- Check `showTutorial` is true
- Verify tutorial panel is assigned
- Check PracticeGameManager is in scene
- Use **🔍 Check Reference Status** to see what's missing

### **Buttons Not Working:**
- Verify button listeners are assigned
- Check PracticeGameManager reference
- Ensure UI elements are active
- Use **🔧 Auto-Assign Tutorial UI** to fix references

### **Practice Mode Not Loading:**
- Verify scene name in SceneLoader
- Check scene is in Build Settings
- Ensure PracticeGameManager exists in scene

### **Missing References:**
- Use **🔧 Auto-Assign All References** context menu
- This will automatically find and assign all components
- Check console for any warnings about missing components

---

**Practice Mode is now ready to provide a safe, tutorial-based learning experience for your math game!** 🎓✨

**No more complex mode switching - just simple scene loading and a focused practice experience!** 🚀
