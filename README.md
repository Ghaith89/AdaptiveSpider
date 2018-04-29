# AdaptiveSpider

This Componant works only inside grasshopper (Algorithmic Modelling For Rhino3D)
The Componant creates 3D spider Geometry needed to connect a facade panel.

**Componant's Inputs:**
1. SpderPostonsPts: Node which connects multiple point and it supposes to be a point3d geometry
2. ListPanels: list of panel geometry that shares the same corner
3. spiFootLength: length of the spider feet
4. spiCenHeight: the distance between the spider center and the node

**Componant's Outputs:**
1. PanelsConnectivity: Indecies of panels that are connected to the same node 
2. SpiderAxesLst: 2D Geometry of the spider which describes the orientation of its 3D elements
3. RoutelAxesLst: list of lines which describe the orientation of the spider's routel
4. RoutelVecAxesLst: list of vectors which describe the orientation of the spider's routel
5. SpiderNormal: Spider central axe which is the average of the normals of the connected panels
6. SpiderNewCenter: spider center which translated from the position points with the spiCenHeight.
7. RoutelPositionTree: List of point 3D which describes the routel positions over the panels
