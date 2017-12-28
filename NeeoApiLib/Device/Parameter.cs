using Home.Neeo.Models;

namespace Home.Neeo.Device
{
    public class Parameter
    {
        public Parameter (ComponentType type, string name, string label = null, Controller controller = null)
        {
            Type = type;
            Label = label;
            Name = name;
            Controller = controller;
        }
        public Parameter(ComponentType type, string name, double rangeLow, double rangeHigh, string unit, string label = null, Controller controller = null)
        {
            Type = type;
            Label = label;
            RangeLow = rangeLow;
            RangeHigh = rangeHigh;
            Unit = unit;
            Name = name;
            Controller = controller;
        }
        public Parameter(ComponentType type, string name, string label, string size, string imageUrl, Controller controller = null)
        {
            Type = type;
            Label = label;
            Size = size;
            ImageUrl = imageUrl;
            Name = name;
            Controller = controller;
        }
        public ComponentType    Type        { get; }
        public string           Name        { get; }
        public string           Label       { get; }
        public double           RangeLow    { get; }
        public double           RangeHigh   { get; }
        public string           Unit        { get; }
        public string           Size        { get; }
        public string           ImageUrl    { get; }
        public Controller       Controller  { get; set; }
    }
}
