using Home.Neeo.Device;
using Home.Neeo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home.Neeo.Interfaces
{
    public interface IRequestHandler
    {
        // /db/search
        List<DataBase.SearchItem>   DBSearchDevice      (string query);
        // /db/<deviceId>
        DataBase.DeviceEntry        DBGetDevice         (int id);

        // /device/<adapterId>/discover
        Task<NEEODiscoveredDevice[]>    DeviceDiscover  (string adapterId);
        // /device/<adapterid>/<component>/<deviceid>
        Task<HandlerParameter>      GetHandler          (string adapterId, string componentId);
        Task<object>                DeviceGetValue      (HandlerParameter handler, string deviceId);
        Task<object>                DeviceGetValue      (string adapterId, string componentId, string deviceId);
        // /device/<adapterid>/<component>/<deviceid>/<value>
        Task<bool>                  DeviceSetValue      (string adapterId, string componentId, string deviceId, string value);

        // /<adapterName>/subscribe/<deviceId>/<eventUriPrefix>
        Task<bool>                  Subscribe           (string adapterId, string deviceId, string eventUriPrefix);
        // /<adapterName>/unsubscribe/<deviceId>
        Task<bool>                  Unsubscribe         (string adapterId, string deviceId);

        Task<object>                HandleRequest       (string path);
    }
}
