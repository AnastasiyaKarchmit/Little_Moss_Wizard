# Little Moss Wizard

**Little Moss Wizard** is a 2D platformer demo made in Unity as a portfolio project.

The game follows a small wizard exploring a mossy cave, collecting magical ingredients, and using them to brew a final potion. The project is inspired by the atmosphere of **Hollow Knight**, but uses a softer green cave theme.

The main focus of the project is not only the gameplay itself, but also the reusable Unity architecture behind it: application state management, dependency injection, Addressables-based UI loading, clean feature separation, and maintainable gameplay systems.

---

## About the Game

The player explores a mossy cave, collects required plants and potions, activates checkpoints, and reaches the potion altar to complete the demo.

The gameplay is built around responsive 2D platformer movement, exploration, collectibles, and simple interactions.

---

## Technical Foundation

The project is built on top of my reusable Unity base architecture, which is maintained as a separate repository.

The architecture is centered around a main application state machine that controls the high-level flow of the game, such as:

- Bootstrap
- Main Menu
- Gameplay
- Loading / transition flow

Each app state is responsible for its own setup, dependencies, lifetime, and cleanup.

For local flows inside a specific app state, the project uses additional in-state state machines. For example, the Main Menu state contains separate menu and settings states implemented with **Stateless**. This keeps local transitions explicit and separated from the global application flow.

---

## Architecture Highlights

- Reusable base architecture shared between projects
- Main application state machine for global game flow
- In-state state machines powered by **Stateless**
- Dependency injection and lifetime management with **VContainer**
- Async scene/UI flow with **UniTask**
- Addressables-based loading for windows, popups, and UI assets
- Reactive event flow with **R3**
- UI and gameplay animations with **DOTween**
- Unity New Input System support
- Keyboard and gamepad support
- Assembly Definitions for faster compilation and clearer dependencies
- Code structured with SOLID principles in mind

---

## Core Features

- **Architecture-driven project structure**  
  Built on a reusable base architecture with app states, dependency injection, Addressables-based loading, and clear feature separation.

- **2D platformer controller**  
  Handles movement, jumping, dashing, variable jump height, and additional collision protection for tilemap-based levels.

- **Interaction system**  
  Used for collectibles, checkpoints, tutorial hints, and objective-related world objects.

- **Inventory and item system**  
  Stores collected ingredients and potions, and connects them with pickup logic and progression.

- **Progression flow**  
  The player explores the cave, collects required ingredients, activates checkpoints, and reaches the potion altar to complete the demo.

- **Input support**  
  Built with Unity’s New Input System and supports both keyboard and controller input.

---

## Gameplay Systems

### Player Controller

The player logic is split into separate components instead of being handled by one large script.

The player-related systems are responsible for:

- movement
- input handling
- animation updates
- interaction detection
- audio and particle reactions
- health and boost events

This keeps the controller easier to extend, test, and maintain.

---

### Movement System

The movement system supports:

- horizontal acceleration and deceleration
- jump buffering
- coyote time
- variable jump height
- faster falling
- dash movement
- dash cooldown
- protection against getting stuck in tilemap colliders

---

### Inventory and Collectibles

The inventory is used to store collected gameplay items, including ingredients and potions.

Implemented item types include:

- health potion
- jump boost potion
- collectible plants required for the final potion

The item system is connected with pickup popups, inventory display, and final game progression.

---

### Interaction System

World objects can react when the player enters their interaction area.

Current interactions include:

- checkpoints
- collectible items
- tutorial hint triggers
- final potion altar

World-space hints are used to explain mechanics directly inside the level.

---

## Tech Stack

- **Unity**
- **C#**
- **VContainer**
- **Stateless**
- **UniTask**
- **DOTween**
- **R3**
- **Addressables**
- **Unity New Input System**
- **Assembly Definitions**

---

## Project Goals

The project was created to demonstrate practical Unity development skills, including:

- 2D character controller development
- gameplay system architecture
- interaction systems
- inventory and collectible systems
- UI/popup flow
- scene and app-state management
- dependency injection in Unity
- clean and maintainable C# code

---

## Screenshots & GIFs

### Menu Flow

#### Main Menu

<img width="800" alt="Main Menu preview" src="https://github.com/user-attachments/assets/1ecc8eea-17d4-4e73-a12d-a84e4d1bc72e" />

#### Options Menu

<img width="800" alt="Options menu screenshot" src="https://github.com/user-attachments/assets/48b878aa-62a4-4857-a843-66df413d87a4" />

---

### Gameplay

#### Tutorial Hints

<img width="800" alt="Tutorial hint screenshot" src="https://github.com/user-attachments/assets/8320566f-89aa-4943-b7a7-bc40b0c7815a" />

#### Checkpoint

<img width="800" alt="Checkpoint interaction screenshot" src="https://github.com/user-attachments/assets/30d2af94-68a1-4058-812f-fde188c67ce3" />

#### Dashing

<img width="800" alt="Dash gameplay preview" src="https://github.com/user-attachments/assets/7e7a364b-38e1-4e62-8cb6-8ba1bf4d9105" />

---

### Items and Inventory

#### Item Pickup

<img width="800" alt="Item pickup preview" src="https://github.com/user-attachments/assets/1c83bed9-f206-4699-9f42-fb0e96a90f2a" />

#### Jump Booster and Inventory

<img width="800" alt="Jump booster and inventory preview" src="https://github.com/user-attachments/assets/0ba1f662-02fa-4662-9ccd-03a34602aebc" />
