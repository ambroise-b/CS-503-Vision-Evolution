# CS-503 Project

# Vision Evolution: Natural Selection shaped by environment

## Installation

1. Install Unity Hub
2. Download the Unity editor version:2022.3.25f1
3. clone the project `git clone git@github.com:ambor1011/CS-503-project.git`
4. cd into the cloned git repository and run `git lfs pull` to pull the large files
5. Open the project in Unity Hub, this will install the dependencies
6. install requirements `pip install -r requirements.txt`

## Running the project

1. In unity you need to build the project, with the development build option
2. place the built project in the `Scripts` folder
3. copy `ant_config.yaml` file from the `Assets/config` folder to the `Scripts/` folder
4. change the name of the executable in `vision_evolve.py` to match the name of the built project.
5. run `python vision_evovle.py` to launch an experiment.

## Structure

The main folder of interest are:

- Assets
  - Scripts: contains all the unity c# source code:
  - Scenes: contains the unity scenes
  - config: contains the ant agent configuration file `ant_config.yaml`
- Results: contains the results of our different runs
- Scripts: contains the python code to interact with the unity environment
