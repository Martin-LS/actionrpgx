# How to Add a New Character to the Game
### A plain-English guide for humans

This walks you through creating a new character (player or enemy) from scratch and getting it into the game. You don't need to be an expert in any of these tools — each step tells you exactly what to do.

The full technical spec is in `docs/asset-pipeline.md` if you need it. This guide is just the human version.

---

## What you'll need installed

- **ComfyUI** — the AI image/3D generation tool. Runs locally on your machine.
- **Blender** — for cleaning up and preparing the 3D model.
- **A web browser** — for the Mixamo step (free website, just needs an account).
- **Cline** (VS Code extension) + **OpenRouter account** — for running the cleanup scripts. This is the "cheap AI" that does the boring Blender work so you don't have to.
- **Godot** — already open with the project loaded.

---

## Overview

The process has 5 stages:

1. **You draw a silhouette** of the character (a simple black shape)
2. **ComfyUI turns it into a 3D mesh** (an AI-generated 3D model)
3. **Cline cleans it up in Blender** (runs the pipeline scripts for you)
4. **You upload it to Mixamo** to get animations
5. **Claude wires it into Godot** so it works in the game

You only do steps 1 and 4 yourself. Everything else is handled by a tool or an AI.

---

## Step 1 — Draw the Silhouette (You)

**What it is:** A simple black-filled outline of the character on a white background. Think of it like a shadow puppet.

**Why:** This is how you tell the AI what shape you want. The AI is very good at following a silhouette — if you give it a wide squat shape, you get a wide squat character. This is your main creative input.

**How to do it:**

1. Open any drawing tool — even Microsoft Paint works. Photoshop, Figma, Procreate, anything.
2. Make a white canvas, roughly square (e.g. 512×512 pixels).
3. Draw the character's **front view** as a solid black silhouette. No detail needed — just the overall shape.
4. Save it as `front.png`.
5. Do the same for the **side view** (looking at the character from the right). Save as `side.png`.

**Tips:**
- Check `docs/visuals-style.md` for silhouette rules — e.g. demons should be wide and low, undead should be tall and thin.
- Rough is fine. You're not drawing art, you're drawing a shape.
- The front and side should be roughly the same height on the canvas.

---

## Step 2 — Generate the 3D Mesh (ComfyUI)

**What it is:** ComfyUI is a tool that runs AI models on your computer. In this step it takes your two silhouette images and generates a 3D model from them.

**Why:** Instead of spending hours modelling a character by hand in Blender, the AI builds a rough version in about a minute. It won't be perfect, but it's a solid starting point.

**How to do it:**

1. Open ComfyUI in your browser (usually at `http://127.0.0.1:8188` when running locally).
2. Load the Hunyuan3D-2 workflow. (If you haven't set this up yet — Hunyuan3D-2 is a node you install in ComfyUI. Search "ComfyUI Hunyuan3D" for install instructions — it's a one-time setup.)
3. In the workflow, find the image input nodes and load your `front.png` and `side.png`.
4. Hit **Queue Prompt** (the run button).
5. Wait — it takes 1–3 minutes depending on your GPU.
6. When it's done, download the output file. It will be a `.glb` or `.obj` file — that's your 3D mesh.

**If you're not happy with the result:**
- The most common fix is to refine your silhouette and re-run. Make the shape clearer and try again.
- You can also take a screenshot of the 3D output, paint over it to adjust the shape, and use that as a new input.
- You'll usually get something acceptable in 1–2 tries.

---

## Step 3 — Clean Up in Blender (Cline does this)

**What it is:** The AI-generated mesh is rough — too many polygons, wrong shading, no colours. This step cleans it up to match the game's visual style.

**Why:** Game engines need clean, optimised models. The AI output needs to be simplified and styled before it's usable.

**What actually happens:**
- The model gets simplified to a low polygon count (this gives it the blocky look)
- Flat shading is applied (no smooth curves — every face is a solid flat colour)
- Colours are assigned based on body region (skin, shirt, pants, boots)

**How to do it:**

1. Open the character's `.blend` file in Blender (or start a new one and import the mesh).
2. Open **Cline** in VS Code (it's in the left sidebar).
3. Tell Cline something like: *"Run the three pipeline scripts in `assets/models/characters/pipeline/` on this Blender scene. The character type is [player/enemy/boss]. The character name is [name]."*
4. Cline will run the scripts one by one and tell you if anything needs fixing.
5. If Cline flags a warning (e.g. "this mesh had no bone weights") it will ask you to check it. Usually it can fix it itself — just tell it to proceed.

**Save the `.blend` file** when done. It goes in `assets/models/characters/src/{character_name}.blend`.

---

## Step 4 — Get Animations from Mixamo (You)

**What it is:** Mixamo is a free Adobe website that automatically adds a skeleton (rig) to your character and gives you access to hundreds of pre-made animations — walking, running, attacking, dying, etc.

**Why:** Animating characters by hand is extremely time-consuming. Mixamo does it automatically and for free. All Mixamo animations use the same bone structure, so once we set it up once, every future character gets the same animation library for free.

**How to do it:**

1. In Blender, export your cleaned mesh as a `.fbx` file. (File → Export → FBX. Make sure "Selected Objects" is ticked if you only want the mesh, not the armature.)
2. Go to [mixamo.com](https://www.mixamo.com) and sign in (free Adobe account).
3. Click **Upload Character** and upload your `.fbx` file.
4. Mixamo will auto-rig it — it'll ask you to place a few markers (chin, wrists, elbows, groin). Place them roughly and hit Next.
5. Preview the result — the character should move naturally. If limbs are bending backwards, the markers were off — go back and adjust.
6. Once happy, download the rigged character as **FBX** (not GLB).
7. If you want specific animations (e.g. a run cycle, attack animation), find them in Mixamo's library, select your character, and download each one as FBX too.

**After Mixamo — tell Claude:**
Once you have the Mixamo FBX files, hand them to Claude (me). I'll do the retargeting step in Blender — this connects the Mixamo animations to the game's shared armature so they work with all characters.

---

## Step 5 — Wire it into Godot (Claude does this)

**What it is:** The final step — getting the character into the game engine so it actually works.

**What Claude does:**
- Sets up the Godot scene file with the correct nodes
- Wires up the animation system
- Connects the character to the game's combat and movement code
- Tests it in-game and fixes any issues

**What you do:**
Just tell me (Claude) the character is ready and what type it is (player / which enemy type). I'll handle the rest and let you know when it's working in-game.

---

## Quick Checklist

Use this to track where you are with a character:

- [ ] Front silhouette drawn and saved as PNG
- [ ] Side silhouette drawn and saved as PNG
- [ ] ComfyUI generated a 3D mesh you're happy with
- [ ] Cline ran the three cleanup scripts successfully
- [ ] `.blend` saved to `assets/models/characters/src/{name}.blend`
- [ ] Mixamo rigged the character and you downloaded the FBX
- [ ] Handed Mixamo FBX to Claude for retargeting
- [ ] Claude confirmed the character is working in Godot

---

## Who does what — at a glance

| Step | Who |
|---|---|
| Draw silhouettes | You |
| Generate 3D mesh | ComfyUI (you run it) |
| Clean up mesh + apply colours | Cline (cheap AI, runs automatically) |
| Upload to Mixamo + download animations | You |
| Retarget animations to game armature | Claude |
| Wire into Godot scene | Claude |
| Test in-game | Claude (you give feedback) |

---

## Common questions

**Do I need to know how to use Blender?**
Not really. Cline handles Blender for you. You might need to open Blender to check the result looks right, but you won't need to touch any Blender controls.

**What if the ComfyUI output looks terrible?**
Re-draw your silhouette with clearer shapes and try again. The AI follows the silhouette closely — a clearer input almost always gives a better output.

**What if Cline can't fix a problem in Blender?**
Escalate it to Claude (me). Some issues need judgment that Cline can't handle.

**How long does the whole process take?**
For a new enemy: roughly 30–45 minutes, most of which is waiting for ComfyUI to generate and Mixamo to process. Your active time is maybe 15 minutes.
