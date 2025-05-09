// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityEditor.Connect
{
    [InitializeOnLoad]
    internal class ServicesRepository
    {
        static readonly ServicesRepository k_Instance;

        public static ServicesRepository instance => k_Instance;

        static readonly Dictionary<string, SingleService> k_Services = new Dictionary<string, SingleService>();

        static List<string> s_InitializedServices = new List<string>();

        const string k_serviceFlagsKey = "service_flags";

        const string k_NotInitializedMsg = "Service {0} was not initialized. See the console for more details.";

        static ServicesRepository()
        {
            k_Instance = new ServicesRepository();
        }

        ServicesRepository()
        {
            InitializeServicesHandlers();
        }

        internal static void InitializeServicesHandlers()
        {
            if (s_InitializedServices.Count < k_Services.Count)
            {
                foreach (var serviceKey in k_Services.Keys)
                {
                    if (!s_InitializedServices.Contains(serviceKey))
                    {
                        var service = k_Services[serviceKey];
                        try
                        {
                            service.InitializeServiceEventHandlers();
                            s_InitializedServices.Add(serviceKey);
                        }
                        catch (Exception ex)
                        {
                            NotificationManager.instance.Publish(service.notificationTopic, Notification.Severity.Error, string.Format(L10n.Tr(k_NotInitializedMsg), L10n.Tr(service.title)));
                            Debug.LogException(ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new service in the services window list.
        /// Sorting and duplicates are already handled.
        /// Each service has the responsibility to add itself to this list.
        /// This should not be called by a service that isn't registering itself.
        /// </summary>
        /// <param name="singleService">Configuration for the service to add</param>
        public static void AddService(SingleService singleService)
        {
            if (!CanAddService(singleService))
                return;

            k_Services.Add(singleService.name, singleService);
        }


        /// <remarks>General rule: do not double add; if Core package exists, do not add services
        /// Exception: Installed services which do not have a hard dependency to Core</remarks>
        static bool CanAddService(SingleService singleService)
        {
            var isServiceAlreadyAdded = k_Services.ContainsKey(singleService.name);

            var isCorePackageRegistered = PackageHelper.IsCorePackageRegistered();
            var servicePackageRegistered = PackageManager.PackageInfo.IsPackageRegistered(singleService.editorGamePackageName);
            var servicePackageHasCoreDependency = PackageHelper.HasCoreDependency(singleService.editorGamePackageName);

            return !isServiceAlreadyAdded &&
                (!isCorePackageRegistered || (servicePackageRegistered && !servicePackageHasCoreDependency));
        }

        public static List<SingleService> GetServices()
        {
            return k_Services.Values.ToList();
        }

        public static void DisableAllServices(bool shouldUpdateApiFlag)
        {
            GetServices().ForEach(service => service.EnableService(false, shouldUpdateApiFlag));
        }

        public static void EnableServicesOnProjectCreation()
        {
            GetServices().ForEach(service =>
            {
                if (service.shouldEnableOnProjectCreation)
                {
                    service.EnableService(true);
                }
            });
        }
    }
}
