using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Pt_BrepTopoloyPlugin
{
    class Spider3D
    {
        //Point3d Center = Point3d.Unset;
        //Vector3d Normal = Vector3d.Unset;

        public Brep create3DArm(Vector3d Normal, Line Axe, double Width, double Strt, double Hend)
        {
            Vector3d AxeVec;
            Point3d stPt = Axe.PointAt(0);
            Point3d EdPt = Axe.PointAt(1);
            Point3d mdPt = Axe.PointAt(0.5);

            AxeVec = EdPt - stPt;
            AxeVec.Unitize();
            Vector3d Z = Normal;
            Vector3d X = AxeVec;
            Vector3d Y = Rhino.Geometry.Vector3d.CrossProduct(Z, X);

            double hWidth = Width / 2;

            //Creat End Rectangle
            Transform halWidthpos = Transform.Translation(Y * hWidth);
            Transform halWidthneg = Transform.Translation(Y * -1 * hWidth);

            Point3d PoTrWid = new Point3d(EdPt);
            PoTrWid.Transform(halWidthpos);

            Point3d NeTrWid = new Point3d(EdPt);
            PoTrWid.Transform(halWidthneg);

            Line li = new Line(PoTrWid, NeTrWid);
            Curve liCrv = li.ToNurbsCurve();

            Surface rectEd = Surface.CreateExtrusion(liCrv, Z * Hend);
            Brep rectEdBrep = rectEd.ToBrep();
            Rhino.Geometry.Collections.BrepCurveList BoundaryEd = rectEdBrep.Curves3D;
            Curve[] Rect02 = Rhino.Geometry.Curve.JoinCurves(BoundaryEd);

            Curve rectConnEd = Rect02[0];

            //Creat Strt Rectangle
            Transform halWidthpos01 = Transform.Translation(Y * hWidth);
            Transform halWidthneg01 = Transform.Translation(Y * -1 * hWidth);

            Point3d PoTrWid01 = new Point3d(stPt);
            PoTrWid.Transform(halWidthpos);

            Point3d NeTrWid01 = new Point3d(stPt);
            PoTrWid.Transform(halWidthneg);

            Line li01 = new Line(PoTrWid01, NeTrWid01);
            Curve liCrv01 = li01.ToNurbsCurve();

            Surface rectSt = Surface.CreateExtrusion(liCrv01, Z * Strt);
            Brep rectStBrep = rectSt.ToBrep();
            Rhino.Geometry.Collections.BrepCurveList BoundaryEd01 = rectStBrep.Curves3D;
            Curve[] Rect01 = Rhino.Geometry.Curve.JoinCurves(BoundaryEd01);

            Curve rectConnSt = Rect01[0];

            //Creat 3rd Rectangle
            Vector3d mid = mdPt - EdPt;
            Curve remid = rectConnEd.DuplicateCurve();
            Transform vecmid = Transform.Translation(mid);
            remid.Transform(vecmid);

            //Lofted Surface
            List<Curve> connections = new List<Curve>();
            connections.Add(rectConnEd);
            connections.Add(remid);
            connections.Add(rectConnSt);
            Brep[] LofLeg = Rhino.Geometry.Brep.CreateFromLoft(connections, Point3d.Unset, Point3d.Unset, LoftType.Tight, false);
            Brep finalLeg = LofLeg[0].CapPlanarHoles(0.1);

            return finalLeg;
        }

    }
}