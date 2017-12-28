using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Home.Neeo.Device.Brain
{
    internal class BrainModule
    {
 
        private static readonly IRestClient   _restClient;
        private static Notification           _notification;
        private static NotificationMapping    _notificationMapping;
        private static readonly ILogger       _logger;

        static BrainModule ()
        {
            _restClient = NEEOEnvironment.RestClient;
            _logger = NEEOEnvironment.Logger;
        }
        public static Task<bool> Start (NEEOConf conf, string adapterName)
        {
            var urlPrefix = UrlBuilder.BuildBrainUrl(conf.Brain, null, conf.BrainPort);
            _notification = new Notification(_restClient, urlPrefix, _logger);
            _notificationMapping = new NotificationMapping(_restClient, urlPrefix, adapterName, _logger);
            return Register.RegisterAdapterOnTheBrain(urlPrefix, conf.BaseUrl, adapterName);
        }
        public static Task<bool> Stop(NEEOConf conf, string adapterName)
        {
            var urlPrefix = UrlBuilder.BuildBrainUrl(conf.Brain);
            _notification = null;
            _notificationMapping = null;
            return Register.UnregisterAdapterOnTheBrain(urlPrefix, adapterName);
        }

        public static async Task<bool> SendNotification(NEEONotification msg, string deviceId)
        {
            if (_notification == null)
            {
                _logger.LogWarning("Brain | server not started, ignore notification");
                return false;
            }
            string notificationKey = await _notificationMapping.GetNotificationKey(msg.UniqueDeviceId, deviceId, msg.Component);
            if (notificationKey != null)
            {
                _logger.LogDebug($"Brain | notificationKey {notificationKey}");
                Notification.MessageData notificationData = new Notification.MessageData { Type = notificationKey, Data = msg.Value };
                return await _notification.Send(notificationData);
            }
            return false;
        }
        // Send Raw Message
        public static async Task<bool> SendNotification(Notification.MessageData msg)
        {
            if (_notification == null)
            {
                _logger.LogWarning("Brain | server not started, ignore notification");
                return false;
            }
            _logger.LogDebug($"Brain | msgtype {msg.Type}");
            return await _notification.Send(msg);
        }
    }
}

