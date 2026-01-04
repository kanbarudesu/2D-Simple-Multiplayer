# 2D-Simple-Multiplayer

A simple **2D side-scroller multiplayer / co-op game** built with **Unity**, using **Netcode for GameObjects** for networking and **Unity Lobby + Relay** for player connection and session management.

The game focuses on light competitive co-op gameplay where players complete tasks while sabotaging each other.

## Gameplay Overview

- Each player must complete **3 mini-games / tasks**
- When a player completes a task:
  - Other players can **sabotage** it
  - The sabotaged task must be **recompleted**
  - The difficulty of the task is **reduced**
- The **first player to complete all 3 tasks** wins the game

## Features

- **2D Side-Scroller Gameplay**
- **Multiplayer / Co-op**
  - Built with Unity Netcode for GameObjects
  - Host / client architecture
- **Unity Lobby**
  - Create and join game sessions
- **Unity Relay**
  - Enables online play without direct peer-to-peer connection
- **Task & Sabotage System**
  - Multiple mini-games / tasks
  - Player-driven sabotage mechanics

## Demo

[![Quiz Game Demo](https://img.youtube.com/vi/W0TB3Xr5p9w/0.jpg)](https://www.youtube.com/watch?v=W0TB3Xr5p9w)

## Technologies Used

- Unity
- C#
- Netcode for GameObjects
- Unity Lobby
- Unity Relay

## Project Purpose

This project was created to:
- Learn **multiplayer game development** in Unity
- Use **Netcode for GameObjects** in a real gameplay scenario
- Implement **Lobby and Relay services**
- Design simple competitive co-op mechanics
