# Unity Multiplayer Lobby System

A functional multiplayer lobby and matchmaking system developed as a technical study of Unity's modern networking architecture. This project serves as an educational deep dive into player authentication, lobby management, and real-time synchronization.

## 🛠 Project Overview
This project was primarily informed by tutorials from **CodeMonkey's YouTube channel**. It was instrumental in helping me navigate and implement the latest multiplayer systems available in Unity.

The goal was to move beyond high-level theory and physically implement the handshake between local clients and Unity's backend services.

## 🚀 Key Learning Objectives
* **Unity Services (UGS):** Implementing `Lobby` and `Authentication` packages.
* **Relay Integration:** Understanding how to host games without port forwarding.
* **Architecture:** Utilizing `asmdef` (Assembly Definitions) to organize Editor vs. Runtime logic.
* **UI/UX:** Managing dynamic lobby lists, player ready-states, and naming systems.

## 💻 Technical Stack
* **Engine:** Unity 2022.3+ (URP)
* **Networking:** Unity Lobby Service, Unity Relay, and Netcode for GameObjects.
* **Language:** C#
* **Accessibility:** Integrated `Open-Dyslexic` font support for improved UI readability.

## 📂 Project Structure
* `Assets/LobbyTutorial`: Core logic for authentication and lobby management.
* `Assets/CodeMonkeyTicTacToeFree`: The base gameplay logic and UI templates.
* `Assets/Fonts`: Custom accessibility-focused typography.

## 📝 Credits
Special thanks to [CodeMonkey](https://www.youtube.com/c/CodeMonkeyUnity) for the excellent educational content and assets that made this deep dive possible.