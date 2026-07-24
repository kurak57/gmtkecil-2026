# 📄 Game Design Document: Yandere's Pager

---

# 1. Game Identity

| Item | Detail |
|------|--------|
| **Title** | Yandere's Pager |
| **Genre** | Survival Horror, Rhythm-Puzzle, Typing |
| **Theme** | Yandere, Retro Pager, Psychological Thriller, Home Invasion |
| **Target Platform** | Web / Mobile (PC Compatible) |

---

# 2. Story Premise & Narrative

You are in your bedroom at midnight.

Your highly obsessive girlfriend (**Yandere**) forces you to constantly exchange messages using an old Morse-code pager she gave you. She hates being ignored.

The terrifying part is that from her messages, you realize she isn't at her own house.

She is walking toward your house.

She steps into your front yard.

She opens your front door.

She slowly approaches your bedroom.

The only way to stop her from breaking in is to reply to every message **perfectly** and **before her patience runs out**.

---

# 3. Core Gameplay Loop

The gameplay is divided into two phases.

## A. Narrative & Choice Phase (Time Freeze)

1. Pager receives a dialogue.
2. Time completely stops.
3. Player reads the message.
4. Player chooses one reply:
   - 🟢 Green
   - 🔴 Red
5. Once selected:
   - Freeze ends
   - Morse transmission begins
   - Timer starts

## B. Action Phase (Morse Input)

- Tap → Dot (`.`)
- Hold → Dash (`-`)

If the player stops typing for **more than 1.5 seconds**, Patience drains **2× faster**.

Completing the word resets the Patience Bar.

Wrong Morse or time running out costs **1 HP**.

---

# 4. Word Choice System (Risk vs Reward)

| Choice | Nature | Gameplay Effect | Story Effect |
|---------|--------|----------------|--------------|
| 🟢 Green | Safe / Compliant | Shorter word. Normal patience drain. | Leads toward the Bad Ending. |
| 🔴 Red | Rebellious / Risky | Longer word. Instantly loses 20% patience and drains 1.5× faster. | Grants +1 Battery on success and is required for the True Survival Ending. |

---

# 5. Progression Scale

| Phase | Shift | Hazard | Description |
|--------|------|---------|-------------|
| 1 | 1–2 | Normal Chatting | Standard typing rhythm. |
| 2 | 3–4 | Panic Blur | Screen blur and pager glitches. |
| 3 | 5–6 | Pager Reboot | Hold button to reboot pager. |
| 4 | 7–8 | DON'T MOVE! | Release input while she is nearby. |
| 5 | 9–10 | Paranoia (Boss) | Ignore fake glitch words. |

---

# 6. Upgrade System

| Upgrade | Effect |
|----------|--------|
| Spare Battery | +1 Max HP and restore 1 HP |
| Extra Padlock | Patience drains 15% slower |
| Sweet Talker | Restore 20% Patience after every successful word |
| Auto Reply | One auto-complete per Shift |

---

# 7. Character Expression States

- 👀 Idle / Watching
- 😊 Affectionate
- 😟 Impatient
- 😈 Hostile / Jealous
- 🤫 Eerie Command
- 💥 Enraged Strike

---

# 8. 10-Shift Storyline

See the full dialogue table from the original GDD for all 30 pager conversations and Green/Red word choices.
