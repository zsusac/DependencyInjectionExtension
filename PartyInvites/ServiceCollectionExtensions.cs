using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace PartyInvites
{
    public static class ServiceCollectionExtensions
    {
        static IServiceCollection _services;

        static List<Service> _registeredServices;

        struct Service
        {
            public string name;
            public string className;
            public string interfaceName;
            public string lifetime;
        };

        /// <summary>
        /// Register all services defined in services.yml configuration files
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterServices(
            this IServiceCollection services)
        {
            // Set properties shared between class methods
            _services = services;
            _registeredServices = new List<Service>();

            // Find recursively all services.yml files placed below project root directory
            string projectRootPath = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
            string[] files = Directory.GetFiles(projectRootPath, "services.yml", SearchOption.AllDirectories);

            // Read, parse and register services file by file
            foreach (string serviceConfigurationFilePath in files)
            {
                ParseYaml(GetYaml(serviceConfigurationFilePath));
            }

            return services;
        }

        /// <summary>
        /// Read file and return yaml stream
        /// </summary>
        /// <param name="serviceConfigurationFilePath"></param>
        /// <returns></returns>
        private static YamlStream GetYaml(String serviceConfigurationFilePath)
        {
            if (!File.Exists(serviceConfigurationFilePath))
            {
                throw new ArgumentException("Service configuration file with path '" + serviceConfigurationFilePath + "' does not exist.");
            }

            // Setup the input
            StringReader input = new StringReader(File.ReadAllText(serviceConfigurationFilePath));

            // Load the stream
            YamlStream yaml = new YamlStream();
            yaml.Load(input);

            return yaml;
        }

        /// <summary>
        /// Parse yaml file and traverse through defined services
        /// </summary>
        /// <param name="yaml"></param>
        private static void ParseYaml(YamlStream yaml)
        {
            // Examine the stream
            YamlMappingNode mapping =
                (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var child in mapping.Children)
            {
                if (child.Key.ToString() == "services")
                {
                    // Loop through defined services under services node and register
                    foreach (var newServiceYaml in ((YamlMappingNode)child.Value).Children)
                    {
                        RegisterService(newServiceYaml);
                    }
                }
            }
        }

        /// <summary>
        /// Register service defined in yaml file
        /// </summary>
        /// <param name="newServiceYaml"></param>
        private static void RegisterService(KeyValuePair<YamlNode, YamlNode> newServiceYaml)
        {
            IDictionary<YamlNode, YamlNode> serviceDefinition = ((YamlMappingNode)newServiceYaml.Value).Children;

            string serviceName = newServiceYaml.Key.ToString();
            string className = "";
            string interfaceName = "";
            string lifetimeName = "";

            // Set service configuration properties defined in service.yml
            foreach (var newServiceConfigProperty in serviceDefinition)
            {
                switch (newServiceConfigProperty.Key.ToString())
                {
                    case "class":
                        className = newServiceConfigProperty.Value.ToString();
                        break;
                    case "interface":
                        interfaceName = newServiceConfigProperty.Value.ToString();
                        break;
                    case "lifetime":
                        lifetimeName = newServiceConfigProperty.Value.ToString();
                        break;
                    default:
                        break;
                }
            }

            // Create new service struct and check if same service is already defined
            Service newService = CreateServiceStruct(serviceName, className, interfaceName, lifetimeName);

            dynamic classType = Type.GetType(newService.className);
            dynamic interfaceType = Type.GetType(newService.interfaceName);

            if (interfaceType == null)
            {
                throw new ArgumentException("Service interface type '" + newService.interfaceName + "' does not exist.");
            }

            if (classType == null)
            {
                throw new ArgumentException("Service class type '" + newService.className + "' does not exist.");
            }

            // TODO: Check if given service class implements given service interface

            // Register service in .NET Core DI container
            _services.Add(new ServiceDescriptor(interfaceType, classType, GetServiceLifetime(lifetimeName)));

            // Add newly registred service to list of already registred services in all services.yml files
            _registeredServices.Add(newService);
        }

        /// <summary>
        /// Returns ServiceLifetime for given string 
        /// </summary>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        private static ServiceLifetime GetServiceLifetime(string lifetime)
        {
            switch (lifetime.ToLower())
            {
                case "transient":
                    return ServiceLifetime.Transient;
                case "scoped":
                    return ServiceLifetime.Scoped;
                case "singleton":
                    return ServiceLifetime.Singleton;
                default:
                    return ServiceLifetime.Singleton;
            }
        }

        /// <summary>
        /// Create new service struct and check if same service is already defined
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="className"></param>
        /// <param name="interfaceName"></param>
        /// <param name="lifetimeName"></param>
        /// <returns></returns>
        private static Service CreateServiceStruct(string serviceName, string className, string interfaceName, string lifetimeName)
        {
            Service newService;
            newService.name = serviceName;
            newService.className = className;
            newService.interfaceName = interfaceName;
            newService.lifetime = lifetimeName;

            ServiceExist(newService);

            return newService;
        }

        /// <summary>
        /// Check if service with the same name or class type already exist
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private static bool ServiceExist(Service service)
        {
            if (_registeredServices.Exists(registeredService => registeredService.name == service.name))
            {
                throw new ArgumentException("Service with name '" + service.name + "' already exist.");
            }

            if (_registeredServices.Exists(registeredService => registeredService.className == service.className))
            {
                throw new ArgumentException("Service with class '" + service.className + "' already exist.");
            }

            // Multiple services can implement same interface, but cannot be of the same class type

            return false;
        }
    }
}
