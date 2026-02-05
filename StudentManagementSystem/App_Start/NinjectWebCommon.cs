using System;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;
using StudentManagementSystem.Models;    
using StudentManagementSystem.Repository; 
using StudentManagementSystem.Services;   

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(StudentManagementSystem.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(StudentManagementSystem.App_Start.NinjectWebCommon), "Stop")]

namespace StudentManagementSystem.App_Start
{
    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        private static void RegisterServices(IKernel kernel)
        {
            // DATABASE-FIRST BINDING
            kernel.Bind<SMSDBNEWEntities>().ToSelf().InRequestScope();
            kernel.Bind<IStudentRepository>().To<StudentRepository>();
            kernel.Bind<IStudentService>().To<StudentService>();
        }
    }
}