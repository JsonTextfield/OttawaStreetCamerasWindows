using System;
using System.Text.RegularExpressions;

namespace OttawaStreetCameras
{
    public abstract class BilingualObject : IComparable<BilingualObject>
    {
        protected string name, nameFr;
        Regex rgx = new Regex("[^a-zA-Z0-9 -]");

        public override string ToString()
        {
            return GetName();
        }

        public string GetName()
        {
            return (Windows.System.UserProfile.GlobalizationPreferences.Languages[0].Contains("fr")) ? nameFr : name;
        }

        public string GetSortableName()
        {
            //Regex rgx = new Regex("\\W");
            return rgx.Replace(GetName(), "");
        }

        public int CompareTo(BilingualObject other)
        {
            return GetSortableName().CompareTo(other.GetSortableName());
        }
    }
}
