# Road to Marching Cubes

## Overview
This repository documents my journey toward understanding and implementing the Marching Cubes algorithm. Marching Cubes is a widely used algorithm for generating isosurfaces from volumetric data, often applied in medical imaging and computer graphics. Through this process, I explore the foundational concepts and gradually build up the knowledge necessary to implement the algorithm from scratch.

The goal of this project is not only to implement Marching Cubes but also to gain a deeper understanding of its applications, mathematical foundations, and performance optimizations.

## Setup
Clone the project using ```git clone https://github.com/Michiel-Janssen/Road-to-Marching-Cubes.git```

After that open it in unity, go to the MarchingCubes scene and press play.

## My journey
It started with basic understanding of the marching cube algorithm and implementating some variations of it. After that I found out about the computing shader and computing buffer idea, helping out with optimization of the big computations marching cubes will have (Putting CPU calculations on the GPU). So I first made a little example scene where I can see the difference in FPS with computing shaders/buffers and without. Now that I understand this principle I went back to my basic marching cube implementation and mixed it with the computing shaders. After I got this working I started making a terraforming tool and fly camera to use to terraform my generated terrain. Next was implementing LOD's into it so I can start on my infinite terrain. Before that a made a simple UI and visual to make terraforming a bit user friendlier.

## Resources
Below is a list of papers, articles, and websites that have helped me throughout this journey:

- **NVIDEA, Generating Complex Procedural Terrains Using the GPU**: https://developer.nvidia.com/gpugems/gpugems3/part-i-geometry/chapter-1-generating-complex-procedural-terrains-using-gpu
- **Paul Bourke, Paper Marching Cubes**: http://paulbourke.net/geometry/polygonise/
- **Lorensen Cline, Paper Marching Cubes**: https://people.eecs.berkeley.edu/~jrs/meshpapers/LorensenCline.pdf
- **Sebastian Lague, Youtuber that explains the marching cube topic**: https://www.youtube.com/@SebastianLague
- **Kyle Halladay, Computing Shaders in unity**: https://kylehalladay.com/blog/tutorial/2014/06/27/Compute-Shaders-Are-Nifty.html
- **PolyCoding, Website with marching cubes explanation**: https://polycoding.net/marching-cubes/
- **catlikecoding, Computing Shaders**: https://catlikecoding.com/unity/tutorials/basics/compute-shaders/

Feel free to browse through the resources and explore the code examples within the repository as I continue my progress toward mastering Marching Cubes.
