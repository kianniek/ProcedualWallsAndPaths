# Unity Procedural Path and Wall Generation

<!-- Skill and Language Badges -->
<p>
  <img src="https://img.shields.io/badge/C%23-v9.0-blue?style=flat&logo=csharp&logoColor=white" alt="C#">
  <img src="https://img.shields.io/badge/Unity-2022.3-blueviolet?style=flat&logo=unity&logoColor=white" alt="Unity">
  <img src="https://img.shields.io/badge/Shader%20Graph-grey?style=flat&logo=unity&logoColor=white" alt="Shader Graph">
  <img src="https://img.shields.io/badge/Visual%20Studio-2022-blue?style=flat&logo=visual-studio&logoColor=white" alt="Visual Studio">
  <img src="https://img.shields.io/badge/Windows-11-lightgrey?style=flat&logo=windows&logoColor=white" alt="Windows 11">
</p>

This Unity project demonstrates the procedural generation of walls and pathways, inspired by techniques such as Wave Function Collapse and marching cubes. The implementation allows users to draw splines, which are then used to generate walls and paths that smoothly follow the defined curves.

## Features

- **Spline-Based Path Drawing:** Users can define control points to create smooth, curved paths using spline interpolation.
- **Procedural Wall Generation:** Walls are generated along the user-defined splines, adapting to the curvature and length of each segment.
- **Pathway Creation:** In addition to walls, the system supports the generation of pathways that follow the drawn splines.

## Getting Started

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/yourusername/procedural-path-wall-generation.git
   ```
2. **Open in Unity:**
   - Ensure you have Unity 2022.3 LTS or later installed.
   - Open the project folder in Unity.
3. **Run the Scene:**
   - Open the main scene located in the `Assets/Scenes` directory.
   - Press the Play button to start the application.

## Usage

- **Drawing Paths:**
  - Click to place control points in the scene.
  - The system will generate a spline connecting these points.
- **Generating Walls or Paths:**
  - Choose between wall or pathway generation modes.
  - The system will procedurally generate the selected structure along the spline.

## Technical Details

The project utilizes spline interpolation algorithms to create smooth curves from user-defined control points. Procedural generation techniques are then applied to construct walls and pathways that conform to these curves. For an in-depth exploration of the algorithms and methodologies used, refer to the article:

[Procedurally Generated Walls and Pathways](https://summit-2324-sem2.game-lab.nl/2024/02/28/procedualy-generated-walls-and-pathways/)

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your enhancements.
