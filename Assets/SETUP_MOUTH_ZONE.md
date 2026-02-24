# Setting Up the Mouth Zone Trigger (Time-Bomb Bite)

The game now uses a **zone-trigger time-bomb mechanic** instead of distance calculations.

## How It Works

1. **Warning**: Mouth closes slightly when the finger gets close (distance-based, no timer yet).
2. **Danger**: The moment the finger enters the MouthZone trigger, a very short countdown begins.
3. **Action**: You must grab the ring and exit the mouth before the timer hits zero, or it snaps shut!

---

## Unity Setup Instructions

### Step 1: Create the MouthZone GameObject

1. In the Hierarchy, **right-click** on your **Monster GameObject**
2. Select **Create Empty**
3. Rename it to **`MouthZone`**

### Step 2: Add the Trigger Collider

1. Select the `MouthZone` object
2. In the Inspector, click **Add Component**
3. Add a **Box Collider 2D** (or **Polygon Collider 2D** for custom shapes)
4. **Check** the **"Is Trigger"** checkbox
5. Resize/reposition the collider so it perfectly covers the **dark area inside the monster's open mouth**

   - Use the **Edit Collider** button to adjust the bounds
   - Make sure it only covers the inside of the mouth, not the teeth or outside area

### Step 3: Create and Assign the MouthZone Tag

1. Select the `MouthZone` object
2. In the Inspector, click the **Tag** dropdown (top section, below the name)
3. Select **"Add Tag..."**
4. Click the **+** button
5. Type `MouthZone` as the tag name
6. Go back to the `MouthZone` object in Hierarchy
7. Set its **Tag** to **`MouthZone`**

### Step 4: Ensure the FingerTip Has a Collider

The `FingerTip` object (child of the hand) needs a trigger collider to detect the MouthZone:

1. Select the **FingerTip** GameObject
2. If it doesn't have a **Collider2D**, add one:
   - **Circle Collider 2D** works best for a finger tip
   - **Check** the **"Is Trigger"** checkbox
   - Adjust the radius to match the finger size

---

## Inspector Settings (MonsterController)

After setup, tune these values in the Inspector:

- **Bite Countdown**: `0.15` to `0.25` seconds (how fast the bite happens after entering)
- **Min Open To Allow Bite**: `0.55` to `0.7` (bite only triggers if mouth is open enough)
- **Warning Distance**: `1.6` (how close before mouth warns)
- **Warning Close Amount**: `0.35` (how much the mouth closes during warning)

---

## Testing

1. Enter **Play Mode**
2. Drag the hand toward the monster's mouth
3. Watch the **Console** for debug messages:
   - "Finger entered mouth! Time-bomb countdown started."
   - "Finger exited mouth! Countdown reset."
   - "BITE! Time's up!"

If the messages don't appear, check:
- MouthZone has the correct **Tag**
- MouthZone collider is set to **Is Trigger**
- FingerTip has a **Collider2D** set to **Is Trigger**
- Both objects are on compatible layers (not ignoring each other in Physics2D settings)

---

## Code Changes Summary

The logic has been updated in `MonsterController.cs`:

- **Removed**: Distance-based `mouthDangerRadius` checks
- **Added**: `OnTriggerEnter2D` / `OnTriggerExit2D` for zone detection
- **Added**: `fingerInsideMouth` boolean flag
- **Changed**: Timer now starts only when trigger is entered, resets when exited
- **Kept**: Warning distance behavior (slight close when near, but not inside)

Enjoy the new time-bomb gameplay! 🕐💥
