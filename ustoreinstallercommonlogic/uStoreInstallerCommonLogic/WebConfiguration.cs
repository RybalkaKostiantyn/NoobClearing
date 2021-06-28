using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Text;
using System.Web.Configuration;
using System.Xml;

namespace uStoreInstallerCommonLogic
{
    public class Section : ConfigurationSection
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public String Name
        {
            get { return (String)base["Name"]; }
            set { base["Name"] = value; }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public String Type
        {
            get { return (String)base["type"]; }
            set { base["type"] = value; }
        }
    }

    public class BindingBase
    {
        protected ConfigurationElement _binding;
        protected ServiceModelSectionGroup _serviceModelSectionGroup;

        public BindingBase(ConfigurationElement configurationElement, ServiceModelSectionGroup serviceModelSectionGroup)
        {
            _binding = configurationElement;
            _serviceModelSectionGroup = serviceModelSectionGroup;
        }

        public virtual void WriteConfig()
        {
            throw new NotImplementedException();
        }
    }

    public class WebHttpBinding : BindingBase
    {
        public const string WEB_CONFIG_SECTION_NAME = @"webHttpBinding";
        public WebHttpBinding(WebHttpBindingElement configurationElement, ServiceModelSectionGroup serviceModelSectionGroup) : base(configurationElement, serviceModelSectionGroup) { }

        public override void WriteConfig()
        {
            WebHttpBindingCollectionElement webhttp = (WebHttpBindingCollectionElement)_serviceModelSectionGroup.Bindings[WEB_CONFIG_SECTION_NAME];
            webhttp.Bindings.Add(_binding as WebHttpBindingElement);
        }
    }

    public class BasicHttpBinding : BindingBase
    {
        public const string WEB_CONFIG_SECTION_NAME = @"basicHttpBinding";
        public BasicHttpBinding(BasicHttpBindingElement configurationElement, ServiceModelSectionGroup serviceModelSectionGroup) : base(configurationElement, serviceModelSectionGroup) { }

        public override void WriteConfig()
        {
            BasicHttpBindingCollectionElement basichttp = (BasicHttpBindingCollectionElement)_serviceModelSectionGroup.Bindings[WEB_CONFIG_SECTION_NAME];
            basichttp.Bindings.Add(_binding as BasicHttpBindingElement);
        }
    }

    public class WebConfiguration
    {
        public class SystemServiceModel
        {
            public ServiceModelSectionGroup ServiceModelSectionGroup { get; set; }
            public SystemServiceModel(ServiceModelSectionGroup serviceModelSectionGroup)
            {
                ServiceModelSectionGroup = serviceModelSectionGroup;
            }

            public void AddService(
                string serviceName,
                string serviceBehaviorConfiguration,
                List<EndpointBehaviorElement> endpointBehaviors,
                List<ServiceBehaviorElement> serviceBehaviors,
                List<BindingBase> bindings,
                List<ServiceEndpointElement> serviceEndpoints,
                List<ChannelEndpointElement> clientEndpoints)
            {
                foreach (var behavior in endpointBehaviors)
                {
					// hot decision
					try
					{
						ServiceModelSectionGroup.Behaviors.EndpointBehaviors.Add(behavior);
					}
					catch { }
                }

                foreach (var behavior in serviceBehaviors)
                {
                    ServiceModelSectionGroup.Behaviors.ServiceBehaviors.Add(behavior);
                }

                foreach (var binding in bindings)
                {
                    binding.WriteConfig();
                }

                foreach (var clientEndpoint in clientEndpoints)
                {
                    try
                    {
                        var key = String.Format("contractType:{0};name:{1}", clientEndpoint.Contract, clientEndpoint.Name);

                        if (ServiceModelSectionGroup.Client.Endpoints.ContainsKey(key))
                        {
                            ServiceModelSectionGroup.Client.Endpoints.RemoveAt(key);
                        }

                        ServiceModelSectionGroup.Client.Endpoints.Add(clientEndpoint);
                    }
                    catch { }
                }

                ServiceElement srv = null;
                if (ServiceModelSectionGroup.Services.Services.ContainsKey(serviceName))
                {
                    ServiceModelSectionGroup.Services.Services.RemoveAt(serviceName);
                }

                srv = new ServiceElement(serviceName);

                if (!String.IsNullOrWhiteSpace(serviceBehaviorConfiguration))
                {
                    srv.BehaviorConfiguration = serviceBehaviorConfiguration;
                }

                foreach (var endpoint in serviceEndpoints)
                {
                    srv.Endpoints.Add(endpoint);
                }

                ServiceModelSectionGroup.Services.Services.Add(srv);
            }

            public EndpointBehaviorElement CreateEndpointBehavior(string name, List<BehaviorExtensionElement> behaviorElements)
            {
                if (ServiceModelSectionGroup.Behaviors.EndpointBehaviors.ContainsKey(name))
                {
                    ServiceModelSectionGroup.Behaviors.EndpointBehaviors.RemoveAt(name);
                }

                EndpointBehaviorElement behavior = new EndpointBehaviorElement(name);

                foreach (var element in behaviorElements)
                {
                    if (!behavior.Contains(element))
                    {
                        behavior.Add(element);
                    }
                }

                return behavior;
            }

            public ServiceBehaviorElement CreateServiceBehavior(string name, List<BehaviorExtensionElement> behaviorElements)
            {
                if (ServiceModelSectionGroup.Behaviors.ServiceBehaviors.ContainsKey(name))
                {
                    ServiceModelSectionGroup.Behaviors.ServiceBehaviors.RemoveAt(name);
                }

                ServiceBehaviorElement behavior = new ServiceBehaviorElement(name);

                foreach (var element in behaviorElements)
                {
                    if (!behavior.Contains(element))
                    {
                        behavior.Add(element);
                    }
                }

                return behavior;
            }

            public ServiceEndpointElement CreateServiceEndpointElement(Uri address, string bindingConfiguration,
                string behaviorConfiguration, string binding, string contract, IdentityElement identity = null)
            {
                var endpoint = new ServiceEndpointElement
                {
                    Address = address,
                    Binding = binding,
                    Contract = contract
                };

                if (!String.IsNullOrWhiteSpace(bindingConfiguration))
                {
                    endpoint.BindingConfiguration = bindingConfiguration;
                }

                if (!String.IsNullOrWhiteSpace(behaviorConfiguration))
                {
                    endpoint.BehaviorConfiguration = behaviorConfiguration;
                }

                if (identity != null)
                {
                    endpoint.Identity.Dns.Value = identity.Dns.Value;
                }

                return endpoint;
            }

            public ChannelEndpointElement CreateChannelEndpointElement(string name, EndpointAddress address, string contractType, string binding, string bindingConfiguration)
            {
                ChannelEndpointElement endpoint = new ChannelEndpointElement(address, contractType);

                endpoint.Binding = binding;
                endpoint.BindingConfiguration = bindingConfiguration;
                endpoint.Name = name;

                return endpoint;
            }

            public WebHttpBindingElement CreateWebHttpBinding(string bindingName, WebHttpSecurityElement security = null, XmlDictionaryReaderQuotasElement readerQuotas = null)
            {
                WebHttpBindingCollectionElement webhttp = ServiceModelSectionGroup.Bindings[WebHttpBinding.WEB_CONFIG_SECTION_NAME] as WebHttpBindingCollectionElement;

                if (webhttp.Bindings.ContainsKey(bindingName))
                {
                    webhttp.Bindings.RemoveAt(bindingName);
                }

                WebHttpBindingElement bind = new WebHttpBindingElement(bindingName);

                if (security != null)
                {
                    bind.Security.Mode = security.Mode;
                    bind.Security.Transport.ClientCredentialType = security.Transport.ClientCredentialType;
                    bind.Security.Transport.ProxyCredentialType = security.Transport.ProxyCredentialType;
                    bind.Security.Transport.Realm = security.Transport.Realm;
                }

                if (readerQuotas != null)
                {
                    bind.ReaderQuotas.MaxDepth = readerQuotas.MaxDepth;
                    bind.ReaderQuotas.MaxStringContentLength = readerQuotas.MaxStringContentLength;
                    bind.ReaderQuotas.MaxArrayLength = readerQuotas.MaxArrayLength;
                    bind.ReaderQuotas.MaxBytesPerRead = readerQuotas.MaxBytesPerRead;
                    bind.ReaderQuotas.MaxNameTableCharCount = readerQuotas.MaxNameTableCharCount;
                }

                return bind;
            }

            public BasicHttpBindingElement CreateBasicHttpBinding(string bindingName, BasicHttpSecurityElement security = null, XmlDictionaryReaderQuotasElement readerQuotas = null)
            {
                BasicHttpBindingCollectionElement basehttp = ServiceModelSectionGroup.Bindings[BasicHttpBinding.WEB_CONFIG_SECTION_NAME] as BasicHttpBindingCollectionElement;

                if (basehttp.Bindings.ContainsKey(bindingName))
                {
                    basehttp.Bindings.RemoveAt(bindingName);
                }

                BasicHttpBindingElement bind = new BasicHttpBindingElement(bindingName);

                bind.MaxBufferSize = 2147483647;
                bind.MaxReceivedMessageSize = 2147483647;

                if (security != null)
                {
                    bind.Security.Mode = security.Mode;
                    bind.Security.Transport.ClientCredentialType = security.Transport.ClientCredentialType;
                    bind.Security.Transport.ProxyCredentialType = security.Transport.ProxyCredentialType;
                    bind.Security.Transport.Realm = security.Transport.Realm;
                }

                if (readerQuotas != null)
                {
                    bind.ReaderQuotas.MaxDepth = readerQuotas.MaxDepth;
                    bind.ReaderQuotas.MaxStringContentLength = readerQuotas.MaxStringContentLength;
                    bind.ReaderQuotas.MaxArrayLength = readerQuotas.MaxArrayLength;
                    bind.ReaderQuotas.MaxBytesPerRead = readerQuotas.MaxBytesPerRead;
                    bind.ReaderQuotas.MaxNameTableCharCount = readerQuotas.MaxNameTableCharCount;
                }

                return bind;
            }
        }

        public SystemServiceModel ServiceModel { get; set; }

        public string uStoreConnectionString { get; private set; }
        public AppSettingsSection AppSettingsSection { get; private set; }
        public HttpHandlersSection HttpHandlersSection { get; private set; }
        public ConnectionStringSettingsCollection ConnectionStringSettingsCollection { get; private set; }
        public ConfigurationSectionGroupCollection ConfigurationSectionGroupCollection { get; private set; }
        public OutputCacheSettingsSection OutputCacheSettingsSection { get; private set; }
		public ScriptingWebServicesSectionGroup WebServicesSection { get; set; }

        public Configuration _configuration;
        private string _configPath;

        public WebConfiguration(string configPath)
        {
            _configPath = configPath;
            var configFile = new FileInfo(configPath);
            var vdm = new VirtualDirectoryMapping(configFile.DirectoryName, true, configFile.Name);
            var wcfm = new WebConfigurationFileMap();
            wcfm.VirtualDirectories.Add("/", vdm);
            _configuration = WebConfigurationManager.OpenMappedWebConfiguration(wcfm, "/");

            AppSettingsSection = (AppSettingsSection)_configuration.GetSection("appSettings");
            HttpHandlersSection = (HttpHandlersSection)_configuration.GetSection("system.web/httphandlers");
            ServiceModel = new SystemServiceModel((ServiceModelSectionGroup)_configuration.SectionGroups["system.serviceModel"]);
            ConnectionStringSettingsCollection = _configuration.ConnectionStrings.ConnectionStrings;
            ConfigurationSectionGroupCollection = _configuration.SectionGroups;
            OutputCacheSettingsSection = (OutputCacheSettingsSection)_configuration.GetSection("system.web/caching/outputCacheSettings");
			WebServicesSection = (ScriptingWebServicesSectionGroup)_configuration.GetSectionGroup("system.web.extensions/scripting/webServices");

            ServiceModel.ServiceModelSectionGroup.ServiceHostingEnvironment.AspNetCompatibilityEnabled = true;
            ServiceModel.ServiceModelSectionGroup.ServiceHostingEnvironment.MultipleSiteBindingsEnabled = true;

			if (ConnectionStringSettingsCollection["uStore"] != null)
			{
				uStoreConnectionString = ConnectionStringSettingsCollection["uStore"].ConnectionString;
			}
        }

		/// <summary>
		/// Method makes updating rough like an icebreaker. 
		/// Before execution this method necessarily to call Save() method. 
		/// If you have not called it yet.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="allowUsers"></param>
        public void Add_Location(string path, params string[] allowUsers)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_configPath);
            var location = doc.SelectSingleNode(String.Format(@"configuration/location[@path='{0}']", path));
            if (location == null)
            {
                XmlElement newElem = doc.CreateElement("location");
                XmlAttribute newAttr = doc.CreateAttribute("path");
                newAttr.Value = path;
                newElem.Attributes.Append(newAttr);

                StringBuilder sb = new StringBuilder();
                sb.Append(@"<system.web><authorization>");
                foreach (var au in allowUsers)
                {
                     sb.Append(String.Format(@"<allow users=""{0}""/>", au));
                }
                sb.Append(@"</authorization></system.web>");

                newElem.InnerXml = sb.ToString();
                doc.DocumentElement.AppendChild(newElem);
                doc.PreserveWhitespace = false;
                doc.Save(_configPath);
            }
        }

		/// <summary>
		/// Method makes updating rough like an icebreaker. 
		/// Before execution this method necessarily to call Save() method. 
		/// If you have not called it yet.
		/// </summary>
		/// <param name="path"></param>
        public void Remove_Location(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_configPath);
            var configuration = doc.SelectSingleNode(@"configuration");
            var location = doc.SelectSingleNode(String.Format(@"configuration/location[@path='{0}']", path));
            if (location != null)
            {
                configuration.RemoveChild(location);
                doc.PreserveWhitespace = false;
                doc.Save(_configPath);
            }
        }

		/// <summary>
		/// Method makes updating rough like an icebreaker. 
		/// Before execution this method necessarily to call Save() method. 
		/// If you have not called it yet.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="preCondition"></param>
        public void Add_SystemWebServer_Module(string name, string type, string preCondition = null)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_configPath);

            var module = doc.SelectSingleNode(String.Format(@"configuration/system.webServer/modules/add[@name='{0}']", name));

            if (module == null)
            {
                XmlElement newElem = doc.CreateElement("add");

                XmlAttribute newAttrName = doc.CreateAttribute("name");
                newAttrName.Value = name;

                if (!String.IsNullOrWhiteSpace(preCondition))
                {
                    XmlAttribute newAttrPreCondition = doc.CreateAttribute("preCondition");
                    newAttrPreCondition.Value = preCondition;
                    newElem.Attributes.Append(newAttrPreCondition);
                }

                XmlAttribute newAttrType = doc.CreateAttribute("type");
                newAttrType.Value = type;

                newElem.Attributes.Append(newAttrName);
                newElem.Attributes.Append(newAttrType);

                var modules = doc.SelectSingleNode(@"configuration/system.webServer/modules");
                if (modules == null)
                {
                    modules = doc.CreateElement("modules");

                    XmlAttribute attr = doc.CreateAttribute("runAllManagedModulesForAllRequests");
                    attr.Value = "true";

                    modules.Attributes.Append(attr);

                    var systemWebServer = doc.SelectSingleNode(@"configuration/system.webServer");
                    systemWebServer.AppendChild(modules);
                }

                modules.AppendChild(newElem);

                doc.PreserveWhitespace = false;
                doc.Save(_configPath);
            }
        }

		/// <summary>
		/// Method makes updating rough like an icebreaker. 
		/// Before execution this method necessarily to call Save() method. 
		/// If you have not called it yet.
		/// </summary>
        public void Add_SystemWebServer_Security_RequestFiltering_Verbs()
        {
            string[] verbs = { "GET", "POST", "PUT", "DELETE" };
            bool isSave = false;

            XmlDocument doc = new XmlDocument();
            doc.Load(_configPath);

            var requestFiltering = doc.SelectSingleNode(@"configuration/system.webServer/security/requestFiltering");
            var verbElems = doc.SelectSingleNode(@"configuration/system.webServer/security/requestFiltering/verbs");
            if (verbElems == null)
            {
                verbElems = doc.CreateElement("verbs");
                requestFiltering.AppendChild(verbElems);
            }

            foreach (var verb in verbs)
            {
                var verbElem = doc.SelectSingleNode(String.Format(@"configuration/system.webServer/security/requestFiltering/verbs/add[@verb='{0}']", verb));
                if (verbElem == null)
                {
                    XmlElement newElem = doc.CreateElement("add");

                    XmlAttribute newAttrVerb = doc.CreateAttribute("verb");
                    newAttrVerb.Value = verb;

                    XmlAttribute newAttrAllowed = doc.CreateAttribute("allowed");
                    newAttrAllowed.Value = "true";

                    newElem.Attributes.Append(newAttrVerb);
                    newElem.Attributes.Append(newAttrAllowed);

                    verbElems.AppendChild(newElem);
                    isSave = true;
                }
            }

            if (isSave)
            {
                doc.Save(_configPath);
            }
        }

		/// <summary>
		/// Method makes updating rough like an icebreaker. 
		/// Before execution this method necessarily to call Save() method. 
		/// If you have not called it yet.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="verb"></param>
		/// <param name="path"></param>
		/// <param name="type"></param>
        public void Add_SystemWebServer_Handler(string name, string verb, string path, string type)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_configPath);

            var handler = doc.SelectSingleNode(String.Format(@"configuration/system.webServer/handlers/add[@name='{0}']", name));

            if (handler == null)
            {
                XmlElement newElem = doc.CreateElement("add");

                XmlAttribute newAttrName = doc.CreateAttribute("name");
                newAttrName.Value = name;

                XmlAttribute newAttrVerb = doc.CreateAttribute("verb");
                newAttrVerb.Value = verb;

                XmlAttribute newAttrPath = doc.CreateAttribute("path");
                newAttrPath.Value = path;

                XmlAttribute newAttrType = doc.CreateAttribute("type");
                newAttrType.Value = type;

                newElem.Attributes.Append(newAttrName);
                newElem.Attributes.Append(newAttrVerb); 
                newElem.Attributes.Append(newAttrPath);
                newElem.Attributes.Append(newAttrType);

                var handlers = doc.SelectSingleNode(@"configuration/system.webServer/handlers");
                handlers.AppendChild(newElem);

                doc.PreserveWhitespace = false;
                doc.Save(_configPath);
            }
        }

		/// <summary>
		/// Method makes updating rough like an icebreaker. 
		/// Before execution this method necessarily to call Save() method. 
		/// If you have not called it yet.
		/// </summary>
		/// <param name="validateIntegratedModeConfiguration"></param>
        public void Add_SystemWebServer_Validation(string validateIntegratedModeConfiguration)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_configPath);

            var systemWebServer = doc.SelectSingleNode(@"configuration/system.webServer");
            var validation = doc.SelectSingleNode(@"configuration/system.webServer/validation");

            if (validation == null)
            {
                validation = doc.CreateElement("validation");
                XmlAttribute attr = doc.CreateAttribute("validateIntegratedModeConfiguration");
                attr.Value = validateIntegratedModeConfiguration;
                validation.Attributes.Append(attr);

                systemWebServer.AppendChild(validation);
            }
            else
            {
                XmlAttribute attr = validation.Attributes["validateIntegratedModeConfiguration"];
                attr.Value = validateIntegratedModeConfiguration;
            }

            doc.PreserveWhitespace = false;
            doc.Save(_configPath);
        }

		/// <summary>
		/// Method makes updating rough like an icebreaker. 
		/// Before execution this method necessarily to call Save() method. 
		/// If you have not called it yet.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="xmlDef"></param>
		/// <param name="xml"></param>
        public void Add_SectionGroup(string name, string xmlDef, string xml)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(_configPath);

            var group = doc.SelectSingleNode(String.Format(@"configuration/configSections/sectionGroup[@name='{0}']", name));
            var groups = doc.SelectSingleNode(@"configuration/configSections");

            if (group == null)
            {
                StringBuilder buildDEF = new StringBuilder();
                buildDEF.AppendLine().AppendFormat(@"  <sectionGroup name=""{0}"">", name);
                buildDEF.AppendLine(xmlDef);
                buildDEF.AppendLine("  </sectionGroup>");
                groups.InnerXml += buildDEF.ToString();
            }

            var elem = doc.SelectSingleNode(String.Format(@"configuration/{0}", name));
            var configuration = doc.SelectSingleNode(@"configuration");
            if (elem == null)
            {
                StringBuilder buildXML = new StringBuilder();
                buildXML.AppendLine().AppendFormat(@"  <{0}>", name);
                buildXML.AppendLine(xml);
                buildXML.AppendFormat(@"  </{0}>", name).AppendLine();
                configuration.InnerXml += buildXML.ToString();
            }

            doc.Save(_configPath);
        }

		/// <summary>
		/// Method makes updating rough like an icebreaker. 
		/// Before execution this method necessarily to call Save() method. 
		/// If you have not called it yet.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultOutgoingResponseFormat"></param>
        public void Add_SystemServiceModel_StandardEndpoints_WebHttpEndpoint_StandardEndpoint(string name, string defaultOutgoingResponseFormat)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_configPath);

            var configuration = doc.SelectSingleNode(@"configuration");

            var systemServiceModel = doc.SelectSingleNode(@"configuration/system.serviceModel");
            if (systemServiceModel == null)
            {
                systemServiceModel = doc.CreateElement("system.serviceModel");
                configuration.AppendChild(systemServiceModel);
            }

            var standardEndpoints = configuration.SelectSingleNode(@"system.serviceModel/standardEndpoints");
            if (standardEndpoints == null)
            {
                standardEndpoints = doc.CreateElement("standardEndpoints");
                systemServiceModel.AppendChild(standardEndpoints);
            }

            var webHttpEndpoint = standardEndpoints.SelectSingleNode(@"webHttpEndpoint");
            if (webHttpEndpoint == null)
            {
                webHttpEndpoint = doc.CreateElement("webHttpEndpoint");
                standardEndpoints.AppendChild(webHttpEndpoint);
            }

            var standardEndpoint = webHttpEndpoint.SelectSingleNode(String.Format(@"standardEndpoint[@name='{0}']", ""));
            if (standardEndpoint == null)
            {
                XmlElement newElem = doc.CreateElement("standardEndpoint");

                XmlAttribute newAttrName = doc.CreateAttribute("name");
                newAttrName.Value = name;

                XmlAttribute newAttrHelpEnabled = doc.CreateAttribute("helpEnabled");
                newAttrHelpEnabled.Value = "true";

                XmlAttribute newAttrAutomaticFormatSelectionEnabled = doc.CreateAttribute("automaticFormatSelectionEnabled");
                newAttrAutomaticFormatSelectionEnabled.Value = "true";

                XmlAttribute newAttrDefaultOutgoingResponseFormat = doc.CreateAttribute("defaultOutgoingResponseFormat");
                newAttrDefaultOutgoingResponseFormat.Value = defaultOutgoingResponseFormat;

                XmlAttribute newAttrMaxBufferSize = doc.CreateAttribute("maxBufferSize");
                newAttrMaxBufferSize.Value = "2147483647";

                XmlAttribute newAttrMaxReceivedMessageSize = doc.CreateAttribute("maxReceivedMessageSize");
                newAttrMaxReceivedMessageSize.Value = "2147483647";

                newElem.Attributes.Append(newAttrName);
                newElem.Attributes.Append(newAttrHelpEnabled);
                newElem.Attributes.Append(newAttrAutomaticFormatSelectionEnabled);
                newElem.Attributes.Append(newAttrDefaultOutgoingResponseFormat);
                newElem.Attributes.Append(newAttrMaxBufferSize);
                newElem.Attributes.Append(newAttrMaxReceivedMessageSize);

                webHttpEndpoint.AppendChild(newElem);

                doc.PreserveWhitespace = false;
                doc.Save(_configPath);
            }
        }

        public void Add_SystemServiceModel_ClientEndpoint(ChannelEndpointElement clientEndpoint)
        {
            var key = String.Format("contractType:{0};name:{1}", clientEndpoint.Contract, clientEndpoint.Name);

            if (ServiceModel.ServiceModelSectionGroup.Client.Endpoints.ContainsKey(key))
            {
                ServiceModel.ServiceModelSectionGroup.Client.Endpoints.RemoveAt(key);
            }

            ServiceModel.ServiceModelSectionGroup.Client.Endpoints.Add(clientEndpoint);
        }

		/// <summary>
		/// Method makes updating rough like an icebreaker. 
		/// Before execution this method necessarily to call Save() method. 
		/// If you have not called it yet.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="allowUsers"></param>
		public void Add_SystemNet_Settings_ServicePointManager(bool expect100Continue)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(_configPath);

			var configuration = doc.SelectSingleNode(@"configuration");

			var systemNet = configuration.SelectSingleNode(@"system.net");
			if (systemNet == null)
			{
				systemNet = doc.CreateElement(@"system.net");
				configuration.AppendChild(systemNet);
			}

			var settings = systemNet.SelectSingleNode(@"settings");
			if (settings == null)
			{
				settings = doc.CreateElement(@"settings");
				systemNet.AppendChild(settings);
			}

			var servicePointManager = settings.SelectSingleNode(@"servicePointManager");
			if (servicePointManager == null)
			{
				servicePointManager = doc.CreateElement(@"servicePointManager");
				XmlAttribute expect100ContinueAttr = doc.CreateAttribute(@"expect100Continue");
				expect100ContinueAttr.Value = expect100Continue.ToString().ToLower();
				servicePointManager.Attributes.Append(expect100ContinueAttr);
				settings.AppendChild(servicePointManager);
			}
			
			doc.PreserveWhitespace = false;
			doc.Save(_configPath);
		}

        /// <summary>
        /// Method makes updating rough like an icebreaker. 
        /// Before execution this method necessarily to call Save() method. 
        /// If you have not called it yet.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="publicKeyToken"></param>
        /// <param name="oldVersion"></param>
        /// <param name="newVersion"></param>
        public void Add_Runtime_AssemblyBinding(string name, string publicKeyToken, string oldVersion, string newVersion)
        {
            var doc = new XmlDocument();
            doc.Load(_configPath);

            var config = doc.SelectSingleNode(@"configuration");

            var runtime = config.SelectSingleNode(@"runtime");
            if (runtime == null)
            {
                runtime = doc.CreateElement("runtime");
                config.AppendChild(runtime);
            }

            XmlNode assemblyBinding = null;
            foreach (XmlNode c in runtime.ChildNodes)
            {
                if (c.Name == "assemblyBinding") 
                { 
                    assemblyBinding = c;
                    break;
                }
            }

            if (assemblyBinding == null)
            {
                assemblyBinding = doc.CreateElement("assemblyBinding");

                var xmlns = doc.CreateAttribute("xmlns");
                xmlns.Value = "urn:schemas-microsoft-com:asm.v1";
                assemblyBinding.Attributes.Append(xmlns);

                runtime.AppendChild(assemblyBinding);
            }

            XmlNode assemblyIdentity = null;
            foreach(XmlNode c in assemblyBinding.ChildNodes)
            {
                foreach (XmlNode cc in c.ChildNodes)
                {
                    if (cc.Name == "assemblyIdentity")
                    {
                        var nameAttr = cc.Attributes["name"];
                        if (nameAttr != null && nameAttr.Value == name)
                        {
                            assemblyIdentity = cc;
                            break;
                        }
                    }
                }

                if (assemblyIdentity != null) { break; }
            }

            if (assemblyIdentity == null)
            {
                var dependentAssembly = doc.CreateElement("dependentAssembly");
                
                var xmlns = doc.CreateAttribute("xmlns");
                xmlns.Value = "urn:schemas-microsoft-com:asm.v1";
                dependentAssembly.Attributes.Append(xmlns);

                assemblyIdentity = doc.CreateElement("assemblyIdentity");
              
                var nameAttr = doc.CreateAttribute("name");
                nameAttr.Value = name;
                assemblyIdentity.Attributes.Append(nameAttr);

                var publicKeyTokenAttr = doc.CreateAttribute("publicKeyToken");
                publicKeyTokenAttr.Value = publicKeyToken;
                assemblyIdentity.Attributes.Append(publicKeyTokenAttr);

                var cultureAttr = doc.CreateAttribute("culture");
                cultureAttr.Value = "neutral";
                assemblyIdentity.Attributes.Append(cultureAttr);

                dependentAssembly.AppendChild(assemblyIdentity);
                assemblyBinding.AppendChild(dependentAssembly);

                var bindingRedirect = doc.CreateElement("bindingRedirect");

                var oldVersionAttr = doc.CreateAttribute("oldVersion");
                oldVersionAttr.Value = oldVersion;
                bindingRedirect.Attributes.Append(oldVersionAttr);

                var newVersionAttr = doc.CreateAttribute("newVersion");
                newVersionAttr.Value = newVersion;
                bindingRedirect.Attributes.Append(newVersionAttr);

                dependentAssembly.AppendChild(bindingRedirect);

                doc.PreserveWhitespace = false;
                doc.Save(_configPath);
            }
        }

        public void Save()
        {
            _configuration.Save();
        }
    }

    public static class WebConfigurationExtensions
    {
        public static void Add(this AppSettingsSection appSettingsSection, string key, string value)
        {
            if (appSettingsSection != null)
            {
                if (appSettingsSection.Settings.AllKeys.Contains(key))
                {
                    appSettingsSection.Settings.Remove(key);
                }

                appSettingsSection.Settings.Add(key, value);
            }
        }

        public static void Add(this ConnectionStringSettingsCollection connectionStringSettingsCollection, string name, string connectionString, string providerName)
        {
            ConnectionStringSettings constr = new ConnectionStringSettings(name, connectionString);
            constr.ProviderName = providerName;

            if (connectionStringSettingsCollection[name] != null)
            {
                connectionStringSettingsCollection.Remove(name);
            }

            connectionStringSettingsCollection.Add(constr);
        }

        public static void Add(this OutputCacheSettingsSection outputCacheSettingsSection, string name, int duration)
        {
            OutputCacheProfile outputCacheProfile = new OutputCacheProfile(name);
            outputCacheProfile.Duration = duration;
            outputCacheProfile.VaryByParam = "none";

            if(outputCacheSettingsSection.OutputCacheProfiles.AllKeys.Contains(name))
            {
                outputCacheSettingsSection.OutputCacheProfiles.Remove(name);
            }

            outputCacheSettingsSection.OutputCacheProfiles.Add(outputCacheProfile);
        }
    }
}