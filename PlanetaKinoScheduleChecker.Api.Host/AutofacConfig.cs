using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using PlanetaKinoScheduleChecker.Api.Host.Controllers;

namespace PlanetaKinoScheduleChecker.Api.Host
{
    internal class AutofacConfig
    {
        public static void ConfigureDi(HttpConfiguration config)
        {
            var resolver = new AutofacWebApiDependencyResolver(GetContainer(config));
            config.DependencyResolver = resolver;
        }

        private static IContainer GetContainer(HttpConfiguration config)
        {
            var builder = new ContainerBuilder();
            builder.RegisterWebApiFilterProvider(config);
            RegisterApiControllers(builder);
            var customTypes = AppDomain.CurrentDomain.GetAllCustomTypes().ToList();
            RegisterCustomTypes(customTypes, builder);
            return builder.Build();
        }

        private static void RegisterApiControllers(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(
                    typeof(TestController).Assembly)
                .Where(t => !t.IsAbstract && typeof(ApiController).IsAssignableFrom(t))
                .InstancePerRequest();
        }

        private static void RegisterCustomTypes(IEnumerable<Type> types, ContainerBuilder builder)
        {
            foreach (var type in types.Where(ShouldRegisterType))
            {
                builder.RegisterType(type)
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .PropertiesAutowired()
                    .InstancePerRequest();
            }
        }

        private static bool ShouldRegisterType(Type type)
        {
            if (type.IsAbstract)
            {
                return false;
            }

            if (type.IsDefined(typeof(ExportAttribute), false))
            {
                return true;
            }

            if (type.BaseType != null && type.BaseType.IsDefined(typeof(InheritedExportAttribute), true))
            {
                return true;
            }

            return type.GetInterfaces().Any(i => i.IsDefined(typeof(InheritedExportAttribute), true));
        }
    }

    public static class AppDomainExtensions
    {
        private const string AllAssembliesKey = "AppDomainExtensions.AllCustomAssembliesKey";
        private const string AllTypesKey = "AppDomainExtensions.AllCustomTypesKey";
        private static readonly string[] DefaultAssemblies = { "PlanetaKinoScheduleChecker." };

        public static IEnumerable<Type> GetAllCustomTypes(this AppDomain domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }

            // We cache types with the domain for performance
            var types = (IEnumerable<Type>)domain.GetData(AllTypesKey);
            if (types != null)
            {
                return types;
            }

            types = GetAllCustomAssemblies(domain)
                .SelectMany(asm => asm.GetTypes())
                .ToList();

            domain.SetData(AllTypesKey, types);
            return types;
        }

        public static Assembly[] GetAllCustomAssemblies(this AppDomain domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }

            var assemblies = (Assembly[])domain.GetData(AllAssembliesKey);
            if (assemblies != null)
            {
                return assemblies;
            }

            var assemblyDirs = GetAssemblyDirectories(domain.SetupInformation);

            // Find all .dll and .exe files in directories used to load assemblies
            var extensions = new[] { "*.dll", "*.exe" };
            var assemblyFiles = assemblyDirs
                .SelectMany(dir => extensions.SelectMany(ext => Directory.GetFiles(dir, ext)))
                .Where(file => file.Contains(".vshost") == false);

            assemblies = assemblyFiles
                .Where(f => IsCustomAssembly(Path.GetFileName(f)))
                .Select(Assembly.LoadFrom)
                .Concat(domain.GetAssemblies().Where(asm => IsCustomAssembly(asm.GetName().Name)))
                .Distinct()
                .ToArray();

            domain.SetData(AllAssembliesKey, assemblies);
            return assemblies;
        }

        // Returns a list of all the directories used to search for assemblies
        public static IEnumerable<string> GetAssemblyDirectories(this AppDomainSetup setupInformation)
        {
            // Unless PrivateBinPathProbe is non-null, we always look in ApplicationBase
            // http://msdn.microsoft.com/en-us/library/system.appdomainsetup.privatebinpathprobe(v=vs.110).aspx
            if (setupInformation.PrivateBinPathProbe == null)
            {
                yield return setupInformation.ApplicationBase;
            }

            // We also search in a set of subdirectories listed in PrivateBinPath.
            // This is, e.g., the bin directory in ASP.NET, or test directories in nunit.
            if (string.IsNullOrEmpty(setupInformation.PrivateBinPath) == false)
            {
                foreach (var subdir in setupInformation.PrivateBinPath.Split(';'))
                {
                    yield return Path.Combine(setupInformation.ApplicationBase, subdir);
                }
            }
        }

        private static bool IsCustomAssembly(string name)
        {
            return DefaultAssemblies.Any(assembly => name.StartsWith(assembly, StringComparison.OrdinalIgnoreCase));
        }
    }
}