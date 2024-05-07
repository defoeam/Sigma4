# Sigma4: Connect 4 3D Reinforcement Learning Agent.
This project includes a playable version of the game Connect 4 3D, along with the implimentation of the reinforcement learning(RL) agent.  

## Objective
The primary goal of the Sigma4 project is to develop an RL agent capable of competing against human players in Connect 4 3D.
A secondary goal of this project is to collect data on many games played, to test the hypothesis that player 1, the player that moves first has an advantage over player 2.

## Game Overview
Connect Four 3D is a lesser-known variation of Connect 4. Instead of the typical two-dimensional 6x7 board, 3D Connect 4 consists of a board set up in a 4x4x4 configuration. Similar to Connect 4, each player has their own-colored piece and players alternate in selecting a column on the board to drop their piece into with the goal of getting a four in a row horizontally, vertically, or diagonally in each dimension of the board.

## Installation and Setup
This guide assumes that you have Unity 2022.3.22f1 installed on your machine and have a basic understanding on how to use Unity and the Windows command promt or powershell.
If you plan on training your own model, please follow the required installation steps for the Unity ml-agents toolkit, including the steps to install the anaconda virtual environment, found here: https://unity-technologies.github.io/ml-agents/Learning-Environment-Design-Agents/

### Playing against a pre-trained model:
1. Open the "Sigma4" project folder using Unity.
2. Open the "GameWorld" scene.
3. Use the Hierarch menu to select "GameManager1", the Inspector window should now display its properties.
4. Verify that the "Human Player" and "Visualized Game" checkboxes are checked, and the "Use Random for Agent 2" checkbox is not checked.
5. If you are okay with playing against the default model: 1mil-opScoreHaste, you can press the "Play" button at the top of the window. When it is your turn, you may click on the column that you wish to place your piece in. To play against other versions of the model, continue to step 5.
6. Expand the "GameManger1" object in the Hierarchy view, and select "Agent 2".
7. Select the "Model" property in the Inspector, you should see a list of other available models to choose from.

### Training your own model (Windows 10/11):
1. First, verify that you followed the ml-agents toolkit installation correctly by running the following command in your python virtual environment: "mlagents-learn --help"
2. Open the "Sigma4" project folder using Unity.
3. Open the "Training" scene.
4. Open windows command promt or powershell and activate your python virtual environment and navigate to your "Sigma4" project folder.
5. Run the following command: "mlagents-learn --force ./configurationSelfPlay.yaml", you should see that the process is waiting for a Unity instance to run.
6. Press the "Play" button at the top of the Unity editor window.
7. If you've done everything right, you should start to see updates being written to the terminal every 1000 training steps.
8. Once you've decided that the model has finished training, enter "Ctrl + C" into the terminal to stop the training process. Observe where the finished Sigma4.onnx model file is stored.
9. To play against your fresh model, move the model file to ./Assets/FinishedModels folder.
10. Follow the "Playing against a pre-trained model" guide above. 

## Contact Information
For more information on model training or any queries, please reach out to:
- (Main PoC) Anthony DeFoe, anthony.defoe@ndsu.edu, https://www.linkedin.com/in/amdefoe/
- Joshua Heeren, joshua.heeren@ndsu.edu, https://www.linkedin.com/in/joshuajheeren/
- Gavin Kestner, *todo*
- Jonathan Rivard, *todo*
