namespace Home.Neeo.Device.Handler
{
    internal class HandlerModule
    {
        static internal RequestHandler Build (DataBase deviceDatabase)
        {
            return new RequestHandler(deviceDatabase);
        }
    }
}
