﻿using Castle.MicroKernel.Registration;
using Plus.Configuration.Startup;
using Plus.Dependency;
using Plus.Domain.Uow;
using Plus.Event.Bus;
using Plus.Modules;
using Plus.Runtime.Validation.Interception;
using System;
using System.IO;
using System.Linq.Expressions;

namespace Plus
{
    /// <summary>
    /// Plus Leadership Module
    /// </summary>
    public class PlusLeadershipModule : PlusModule
    {
        public override void PreInitialize()
        {
            IocManager.AddConventionalRegistrar(new BasicConventionalRegistrar());
            ConfigureCaches();
            AddIgnoredTypes();
            AddMethodParameterValidators();
        }

        public override void Initialize()
        {
            foreach (Action value in ((PlusStartupConfiguration)Configuration).ServiceReplaceActions.Values)
            {
                value();
            }

            IocManager.IocContainer.Install(new EventBusInstaller(IocManager));


            IocManager.RegisterAssembly(typeof(PlusLeadershipModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            RegisterMissingComponents();
        }

        public override void Shutdown()
        {
        }

        private void RegisterMissingComponents()
        {
            if (!IocManager.IsRegistered<IGuidGenerator>())
            {
                IocManager.IocContainer.Register(
                    Component
                        .For<IGuidGenerator, SequentialGuidGenerator>()
                        .Instance(SequentialGuidGenerator.Instance)
                );
            }

            IocManager.RegisterIfNot<IUnitOfWork, NullUnitOfWork>(DependencyLifeStyle.Transient);
            IocManager.RegisterIfNot<IUnitOfWorkFilterExecuter, NullUnitOfWorkFilterExecuter>();
        }

        private void AddMethodParameterValidators()
        {
            Configuration.Validation.Validators.Add<DataAnnotationsValidator>();
            Configuration.Validation.Validators.Add<ValidatableObjectValidator>();
        }

        private void AddIgnoredTypes()
        {
            var commonIgnoredTypes = new[]
            {
                typeof(Stream),
                typeof(Expression)
            };

            foreach (var ignoredType in commonIgnoredTypes)
            {
                Configuration.Validation.IgnoredTypes.AddIfNotContains(ignoredType);
            }

            var validationIgnoredTypes = new[] { typeof(Type) };
            foreach (var ignoredType in validationIgnoredTypes)
            {
                Configuration.Validation.IgnoredTypes.AddIfNotContains(ignoredType);
            }
        }

        private void ConfigureCaches()
        {

        }
    }
}