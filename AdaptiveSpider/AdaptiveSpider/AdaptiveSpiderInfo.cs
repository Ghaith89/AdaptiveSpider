using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace AdaptiveSpider
{
    public class AdaptiveSpiderInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "AdaptiveSpider";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("904d8f62-fafa-40f3-aa67-6b27f90b485b");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Microsoft";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
