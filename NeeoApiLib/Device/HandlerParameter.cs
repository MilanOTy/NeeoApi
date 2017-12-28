using Home.Neeo.Models;

namespace Home.Neeo.Device
{
    public class HandlerParameter
    {
        public HandlerParameter(ComponentType type, Controller controller)
        {
            ComponentType = type;
            Controller = controller;
        }
        public ComponentType ComponentType;
        public Controller Controller;
    }
}
