using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Pt_BrepTopoloyPlugin
{

    public class PtBrepTopoloyPluginComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PtBrepTopoloyPluginComponent()
          : base("Pt_BrepTopoloyPlugin", "Nickname",
              "Description",
              "Extra", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("SpderPostonsPts", "PoPts", "SpderPostonsPts", GH_ParamAccess.list);
            pManager.AddBrepParameter("ListPanels", "LiPa", "ListPanels", GH_ParamAccess.list);
            pManager.AddNumberParameter("spiFootLength", "SpFLen", "spiFootLength", GH_ParamAccess.item);
            pManager.AddNumberParameter("spiCenHeight", "SpCenHe", "spiCenHeight", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("PanelsConnectivity", "PaCon", "PanelsConnectivity", GH_ParamAccess.tree);
            pManager.AddLineParameter("SpiderAxesLst", "SpAx", "SpiderAxesLst", GH_ParamAccess.tree);
            pManager.AddLineParameter("RoutelAxesLst", "RoAx", "RoutelAxesLst", GH_ParamAccess.tree);
            pManager.AddVectorParameter("RoutelVecAxesLst", "RoVecAx", "RoutelVecAxesLst", GH_ParamAccess.tree);
            pManager.AddVectorParameter("SpiderNormal", "SpNo", "SpiderNormal", GH_ParamAccess.list);
            pManager.AddPointParameter("SpiderNewCenter", "SpNCen", "SpiderNewCenter", GH_ParamAccess.list);
            pManager.AddPointParameter("RoutelPositionTree", "RoPosTr", "RoutelPositionTree", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //PlaceHolder
            List<Point3d> SpderPostonsPts = new List<Point3d>();
            List<Brep> ListPanels = new List<Brep>();
            double spiFootLength = double.NaN;
            double spiCenHeight = double.NaN;

            if (!DA.GetDataList(0, SpderPostonsPts)) { return; }
            if (!DA.GetDataList(1, ListPanels)) { return; }
            if (!DA.GetData(2, ref spiFootLength)) { return; }
            if (!DA.GetData(3, ref spiCenHeight)) { return; }

            //nstantiate Class
            CreateSpiderAxisCorrection Spider = new CreateSpiderAxisCorrection();
            //Asseign Varriables
            Spider.ListPtsPositions = SpderPostonsPts;
            Spider.ListSurfaces = ListPanels;
            List<List<Line>> SpiAxes = new List<List<Line>>();
            List<List<Line>> RoutelAxes = new List<List<Line>>();
            List<Vector3d> SpiNorm = new List<Vector3d>();
            List<List<Vector3d>> RoNormalsPerPt = new List<List<Vector3d>>();
            List<Point3d> newCenList = new List<Point3d>();
            List<List<Point3d>> RoPos = new List<List<Point3d>>();
            double RoutelHei = 0.2;

            //SpiderWireFrame(List < Brep > listsrf, List < Point3d > PtsPositions, double SpiSideLen, double SpiHeight, double RoutleHeiht, out List < List < Line >> RoutelPerPt, out List < Vector3d > NormalPerPt)

            SpiAxes = Spider.SpiderWireFrame01(ListPanels, SpderPostonsPts, spiFootLength, spiCenHeight, RoutelHei, out RoutelAxes, out RoNormalsPerPt, out SpiNorm, out newCenList, out RoPos);
            Grasshopper.DataTree<Line> AxesTree = new Grasshopper.DataTree<Line>();

            for (int i = 0; i < SpiAxes.Count; i++)
            {
                AxesTree.AddRange(SpiAxes[i], new Grasshopper.Kernel.Data.GH_Path(i));
            }

            Grasshopper.DataTree<Line> RoutelTree = new Grasshopper.DataTree<Line>();

            for (int i = 0; i < RoutelAxes.Count; i++)
            {
                RoutelTree.AddRange(RoutelAxes[i], new Grasshopper.Kernel.Data.GH_Path(i));
            }

            Grasshopper.DataTree<Vector3d> RoutelVecTree = new Grasshopper.DataTree<Vector3d>();

            for (int i = 0; i < RoutelAxes.Count; i++)
            {
                RoutelVecTree.AddRange(RoNormalsPerPt[i], new Grasshopper.Kernel.Data.GH_Path(i));
            }

            //SpiderFaceTopology
            List<List<int>> ConnPanels = new List<List<int>>();
            List<List<Brep>> BPerNode = new List<List<Brep>>();
            ConnPanels = Spider.FaceNumPerPt(ListPanels, SpderPostonsPts, out BPerNode);

            Grasshopper.DataTree<int> Connfaces = new Grasshopper.DataTree<int>();
            Grasshopper.DataTree<Brep> ConnBPerNode = new Grasshopper.DataTree<Brep>();

            for (int i = 0; i < ConnPanels.Count; i++)
            {
                Connfaces.AddRange(ConnPanels[i], new Grasshopper.Kernel.Data.GH_Path(i));
            }
            for (int i = 0; i < ConnPanels.Count; i++)
            {
                ConnBPerNode.AddRange(BPerNode[i], new Grasshopper.Kernel.Data.GH_Path(i));
            }

            //SpiderRoutelVectorsTree
            Grasshopper.DataTree<Vector3d> RoVecPerSpider = new Grasshopper.DataTree<Vector3d>();

            for (int i = 0; i < ConnPanels.Count; i++)
            {
                RoVecPerSpider.AddRange(RoNormalsPerPt[i], new Grasshopper.Kernel.Data.GH_Path(i));
            }

            //RoutelPositionsTree
            Grasshopper.DataTree<Point3d> RoPosPerSpider = new Grasshopper.DataTree<Point3d>();

            for (int i = 0; i < ConnPanels.Count; i++)
            {
                RoPosPerSpider.AddRange(RoPos[i], new Grasshopper.Kernel.Data.GH_Path(i));
            }

            DA.SetDataTree(0, Connfaces);
            DA.SetDataTree(1, AxesTree);
            DA.SetDataTree(2, RoutelTree);
            DA.SetDataTree(3, RoutelVecTree);
            DA.SetDataList(4, SpiNorm);
            DA.SetDataList(5, newCenList);
            DA.SetDataTree(6, RoPosPerSpider);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{661f1122-e330-4ee3-be15-38ee7cdb8ef7}"); }
        }
    }
}