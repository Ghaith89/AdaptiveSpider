using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Pt_BrepTopoloyPlugin
{
    class CreateSpiderAxisCorrection
    {

        public List<Brep> ListSurfaces;
        public List<Point3d> ListPtsPositions;

        //Define Outputs
        List<List<Line>> RoutelPerPt = new List<List<Line>>();
        List<Vector3d> NormalPerPt = new List<Vector3d>();

        //SpiderWire01
        public List<List<Line>> SpiderWireFrame01(List<Brep> listsrf, List<Point3d> PtsPositions, double SpiSideLen, double SpiHeight, double RoutleHeiht, out List<List<Line>> RoutelPerPt, out List<List<Vector3d>> NormalPerPt, out List<Vector3d> SpiderNormals, out List<Point3d> NewSpCen, out List<List<Point3d>> RoutelPositions)
        {
            List<List<Brep>> FacePerNode = new List<List<Brep>>();
            FaceNumPerPt(listsrf, PtsPositions, out FacePerNode);
            List<List<Line>> FeetAxePerPt = new List<List<Line>>();
            RoutelPositions = new List<List<Point3d>>();
            NormalPerPt = new List<List<Vector3d>>();
            RoutelPerPt = new List<List<Line>>();
            SpiderNormals = new List<Vector3d>();
            NewSpCen = new List<Point3d>();

            for (int i = 0; i < PtsPositions.Count; i++)
            {
                List<Brep> Faces = FacePerNode[i];
                Point3d SpiPos = PtsPositions[i];
                List<Vector3d> BisecPerNode = BisectorsPerNodeForDualSurfaces(Faces, SpiPos, SpiSideLen);
                int counter = -1;
                List<Vector3d> Normals = new List<Vector3d>();
                List<Point3d> PtsFaces = new List<Point3d>();
                List<Line> RoAxes = new List<Line>();
                foreach (Vector3d n in BisecPerNode)
                {
                    counter++;
                    //n.Unitize();
                    Point3d PoOnFace = SpiPos;
                    Brep Face = Faces[counter];
                    Transform OnFace = Transform.Translation(n);
                    PoOnFace.Transform(OnFace);
                    Vector3d Normal;
                    Point3d PoFa;
                    ComponentIndex coIn;
                    double do01;
                    double do02;
                    double do03 = 50000;
                    Face.ClosestPoint(PoOnFace, out PoFa, out coIn, out do01, out do02, do03, out Normal);
                    Normals.Add(Normal);
                    PtsFaces.Add(PoFa);
                    Normal.Unitize();
                    Transform RoutelAxeTr = Transform.Translation(Normal * 200);
                    Point3d PoRoEd = PoFa;
                    PoRoEd.Transform(RoutelAxeTr);
                    Line RoutelAxe = new Line(PoFa, PoRoEd);
                    RoAxes.Add(RoutelAxe);

                }
                RoutelPositions.Add(PtsFaces);
                Vector3d AvVec = Ave(Normals);
                AvVec.Unitize();
                SpiderNormals.Add(AvVec);
                Transform AveNorm = Transform.Translation(AvVec * SpiHeight);
                Point3d SpiCen = PtsPositions[i];
                SpiCen.Transform(AveNorm);
                NewSpCen.Add(SpiCen);
                List<Line> SpiFeet = new List<Line>();
                NormalPerPt.Add(Normals);
                RoutelPerPt.Add(RoAxes);

                foreach (Point3d n in PtsFaces)
                {
                    Line LegAxe = new Line(SpiCen, n);
                    SpiFeet.Add(LegAxe);
                }
                FeetAxePerPt.Add(SpiFeet);
            }

            return FeetAxePerPt;
        }
        //Calculate the average Vector of The panels
        public Vector3d Ave(List<Vector3d> Vectors)
        {
            Vector3d result = new Vector3d(0, 0, 0);

            for (int i = 0; i < Vectors.Count; i++)
            {
                result += Vectors[i];
            }

            return result / Vectors.Count;
        }

        //List Of Face indecies that are connected to one spider
        public List<List<int>> FaceNumPerPt(List<Brep> ListSurfaces, List<Point3d> ListPtsPositions, out List<List<Brep>> BrepPerNode)
        {
            List<Point3d[]> PointsperBrep = GetBrepPts(ListSurfaces);
            List<List<int>> ConFacesPerNode = new List<List<int>>();
            BrepPerNode = new List<List<Brep>>();

            foreach (Point3d i in ListPtsPositions)
            {
                int counter = -1;

                List<int> connectedFaces = new List<int>();
                List<Brep> ConnectedBreps = new List<Brep>();
                ConFacesPerNode.Add(connectedFaces);
                foreach (Point3d[] n in PointsperBrep)
                {
                    counter++;
                    foreach (Point3d l in n)
                    {
                        if (i.DistanceTo(l) <= Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                        {
                            connectedFaces.Add(counter);
                            ConnectedBreps.Add(ListSurfaces[counter]);
                        }
                    }

                }
                BrepPerNode.Add(ConnectedBreps);

            }

            return ConFacesPerNode;
        }

        //Get The Brep corner Points
        public List<Point3d[]> GetBrepPts(List<Brep> FacadeSurfaces)
        {
            List<Point3d[]> BrepsPts = new List<Point3d[]>();

            foreach (Brep i in FacadeSurfaces)
            {
                Point3d[] brepPts;
                brepPts = i.DuplicateVertices();
                BrepsPts.Add(brepPts);
            }

            return BrepsPts;
        }

        //Get The Bsectors Of the Panel
        public List<Line> Bisector(Brep face)
        {
            Point3d[] BrepPts = face.DuplicateVertices();
            Point3d po01 = BrepPts[0];
            List<Point3d> poNew = new List<Point3d>();
            poNew.AddRange(BrepPts);
            poNew.Add(po01);

            Curve[] BrepCurves = face.DuplicateEdgeCurves();
            Curve curve5 = BrepCurves[0];

            List<Curve> liBrepCurves = new List<Curve>();
            liBrepCurves.AddRange(BrepCurves);
            liBrepCurves.Add(curve5);

            List<List<Point3d>> CurveStEd = new List<List<Point3d>>();
            List<Line> Bisectorperface = new List<Line>();

            List<Line> bb = new List<Line>();

            foreach (Curve i in liBrepCurves)
            {
                List<Point3d> lstPted = new List<Point3d>();
                Point3d post = i.PointAtLength(0.1);
                double ilen = i.GetLength();
                Point3d poed = i.PointAtLength(ilen - 0.1);
                lstPted.Add(post);
                lstPted.Add(poed);

                CurveStEd.Add(lstPted);
            }
            int counter = -1;
            for (int i = 1; i < poNew.Count; i++)
            {
                //int id =
                counter++;
                int counter2 = -1;
                counter2++;
                Point3d Stpt = CurveStEd[counter][counter2 + 1];
                Point3d Edpt = CurveStEd[counter + 1][counter2];
                Line dia = new Rhino.Geometry.Line(Stpt, Edpt);
                Point3d diapt = dia.PointAt(0.5);
                int pt = i;
                Line Bisectorli = new Line(poNew[pt], diapt);
                Bisectorperface.Add(Bisectorli);
                bb.Add(dia);
            }

            return Bisectorperface;
        }

        public List<Vector3d> BisectorsPerNode(List<Brep> ConnectedFaces, Point3d Node)
        {
            List<Curve> BeamPerNode = new List<Curve>();
            List<Vector3d> BisecVec = new List<Vector3d>();
            foreach (Brep i in ConnectedFaces)
            {
                Curve[] ListCurves = i.DuplicateNakedEdgeCurves(true, false);
                List<Curve> FaceDoubleEdges = new List<Curve>();
                foreach (Curve n in ListCurves)
                {
                    Point3d StPt = n.PointAtStart;
                    Point3d EdPt = n.PointAtEnd;
                    if (Node.DistanceTo(StPt) <= Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                    {
                        Line CorrBeam = new Line(Node, EdPt);
                        BeamPerNode.Add(CorrBeam.ToNurbsCurve());
                        FaceDoubleEdges.Add(CorrBeam.ToNurbsCurve());
                    }
                    if (Node.DistanceTo(EdPt) <= Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                    {
                        Line CorrBeam = new Line(Node, StPt);
                        BeamPerNode.Add(CorrBeam.ToNurbsCurve());
                        FaceDoubleEdges.Add(CorrBeam.ToNurbsCurve());
                    }
                }
                Curve FrstCrv = FaceDoubleEdges[0];
                Curve ScndCrv = FaceDoubleEdges[1];
                Point3d FstPt = FrstCrv.PointAtLength(0.1);
                Point3d SndtPt = ScndCrv.PointAtLength(0.1);
                Line intrmediate = new Line(FstPt, SndtPt);
                Point3d mid = intrmediate.PointAt(0.5);
                Vector3d Bisec = mid - Node;
                Bisec.Unitize();
                BisecVec.Add(Bisec);
            }

            return BisecVec;


        }

        public List<Vector3d> BisectorsPerNodeForDualSurfaces(List<Brep> ConnectedFaces, Point3d Node, double SpiSideLen)
        {
            List<Curve> BeamPerNode = new List<Curve>();
            List<Vector3d> BisecVec = new List<Vector3d>();
            foreach (Brep i in ConnectedFaces)
            {
                Curve[] ListCurves = i.DuplicateNakedEdgeCurves(true, false);
                List<Curve> FaceDoubleEdges = new List<Curve>();
                foreach (Curve n in ListCurves)
                {
                    Point3d StPt = n.PointAtStart;
                    Point3d EdPt = n.PointAtEnd;
                    if (Node.DistanceTo(StPt) <= Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                    {
                        Line CorrBeam = new Line(Node, EdPt);
                        BeamPerNode.Add(CorrBeam.ToNurbsCurve());
                        FaceDoubleEdges.Add(CorrBeam.ToNurbsCurve());
                    }
                    if (Node.DistanceTo(EdPt) <= Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                    {
                        Line CorrBeam = new Line(Node, StPt);
                        BeamPerNode.Add(CorrBeam.ToNurbsCurve());
                        FaceDoubleEdges.Add(CorrBeam.ToNurbsCurve());
                    }
                }
                Curve FrstCrv = FaceDoubleEdges[0];
                Curve ScndCrv = FaceDoubleEdges[1];
                Point3d FstPt = FrstCrv.PointAtLength(0.1);
                Point3d SndtPt = ScndCrv.PointAtLength(0.1);
                Line intrmediate = new Line(FstPt, SndtPt);
                Point3d mid = intrmediate.PointAt(0.5);
                Point3d Corrmid;
                Vector3d Normal;
                ComponentIndex coIn;
                double do01;
                double do02;
                double do03 = 50000;

                Vector3d Bisec = mid - Node;
                Bisec.Unitize();
                Vector3d CorrBisec = Bisec * SpiSideLen;
                Transform midTra = Transform.Translation(CorrBisec);
                mid.Transform(midTra);

                i.ClosestPoint(mid, out Corrmid, out coIn, out do01, out do02, do03, out Normal);

                Vector3d FiBisec = Corrmid - Node;


                BisecVec.Add(FiBisec);
            }

            return BisecVec;


        }
    }
}
