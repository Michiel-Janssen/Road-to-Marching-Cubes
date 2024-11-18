# Road to Marching Cubes

## Overview
This repository documents my journey toward understanding and implementing the Marching Cubes algorithm. Marching Cubes is a widely used algorithm for generating isosurfaces from volumetric data, often applied in medical imaging and computer graphics. Through this process, I explore the foundational concepts and gradually build up the knowledge necessary to implement the algorithm from scratch.

The goal of this project is not only to implement Marching Cubes but also to gain a deeper understanding of its applications, mathematical foundations, and performance optimizations.

## Setup
Clone the project using ```git clone https://github.com/Michiel-Janssen/Road-to-Marching-Cubes.git```

After that open it in unity, go to the MarchingCubes scene and press play.

## My journey
Week 1:
It started with basic understanding of the marching cube algorithm and implementating some variations of it. After that I found out about the computing shader and computing buffer idea, helping out with optimization of the big computations marching cubes will have (Putting CPU calculations on the GPU). So I first made a little example scene where I can see the difference in FPS with computing shaders/buffers and without. Now that I understand this principle I went back to my basic marching cube implementation and mixed it with the computing shaders. After I got this working I started making a terraforming tool and fly camera to use to terraform my generated terrain. Next was implementing LOD's into it so I can start on my infinite terrain. Before that a made a simple UI and visual to make terraforming a bit user friendlier.

Week 2:
I went to implement an infinite generation system. So now I achieved a fixed and infinite generation system. I am not yet happy with the noise so I will be working on that. Because of pc issues I stopped working on this for now and start learning about shader graphs and HLSL code. My idea is to prototype in shader graph and then later when I am happy to write it in HLSL myself for optimization purposes. My current code for this watershader is for now on this repo (https://github.com/Michiel-Janssen/Water-Shader).

Week 3:
After struggling another week with a worst laptop, I finally got mine back and could work faster again. I developed a scalable system to procedurally place vegatation on my terrain using the poisson disc sampling algortihm. I also used the Unity scriptable objects to give each type of vegatation some different properties. For example, adding noise or tilt to my vegetation type to get more randomised vegetation. It is also possible now to generate multiple vegetation types at once. Secondly, I changed some noise properties for my terrain generation so I now have more realistic terrain. Lastly, I developed a heightbased shader with textures to texture my terrain.

## Resources
Below is a list of papers, articles, and websites that have helped me throughout this journey:

- **NVIDEA, Generating Complex Procedural Terrains Using the GPU**: https://developer.nvidia.com/gpugems/gpugems3/part-i-geometry/chapter-1-generating-complex-procedural-terrains-using-gpu
- **Paul Bourke, Paper Marching Cubes**: http://paulbourke.net/geometry/polygonise/
- **Lorensen Cline, Paper Marching Cubes**: https://people.eecs.berkeley.edu/~jrs/meshpapers/LorensenCline.pdf
- **Sebastian Lague, Youtuber that explains the marching cube topic**: https://www.youtube.com/@SebastianLague
- **Kyle Halladay, Computing Shaders in unity**: https://kylehalladay.com/blog/tutorial/2014/06/27/Compute-Shaders-Are-Nifty.html
- **PolyCoding, Website with marching cubes explanation**: https://polycoding.net/marching-cubes/
- **catlikecoding, Computing Shaders**: https://catlikecoding.com/unity/tutorials/basics/compute-shaders/
- **Unity docs shader graph**: https://docs.unity3d.com/6000.0/Documentation/Manual/shader-graph.html
- **Habib's water shader**: https://enjoyphysics.cn/%E6%96%87%E4%BB%B6/soft/Water/thesis.pdf
- **Poisson disc sampling**: https://gameidea.org/2023/12/27/poisson-disk-sampling/

Feel free to browse through the resources and explore the code examples within the repository as I continue my progress toward mastering Marching Cubes.
