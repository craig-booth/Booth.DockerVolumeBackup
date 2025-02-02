﻿using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Test.Fixtures.Mocks
{
    internal class ServiceScopeFactoryMock : IServiceScopeFactory
    {
        private IServiceScope _Scope;
        private IServiceProvider _ServiceProvider;
        public ServiceScopeFactoryMock()
        {
            _Scope = Substitute.For<IServiceScope>();
            _ServiceProvider = Substitute.For<IServiceProvider>();

            RegisterService<IServiceScopeFactory>(this);

            _Scope.ServiceProvider.Returns(_ServiceProvider);
        }

        public void RegisterService<T>(object implementation)
        {
            _ServiceProvider.GetService(typeof(T)).Returns(implementation);
        }

        public IServiceScope CreateScope()
        {
            return _Scope;
        }
    }
}
