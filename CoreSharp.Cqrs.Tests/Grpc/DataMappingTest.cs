using System.Collections.Generic;
using Xunit;
using System.Reflection;
using CoreSharp.Cqrs.Grpc.Contracts;
using System.Linq;
using CoreSharp.Cqrs.Resolver;
using AutoMapper;
using CoreSharp.Cqrs.Tests.Grpc.Samples.Models;
using System;

namespace CoreSharp.Cqrs.Tests.Grpc
{
    public class DataMappingTest
    {

        [Fact]
        public void TestDateTimeMap()
        {

            // channel map
            var (mapper, contracts) = CreateMapper();

            // test obj
            var user = new User
            {
                Id = 1,
                Name = "test",
                Status = 1,
                Roles = null,
                LocationTime = DateTimeOffset.Parse("2021-01-01 20:22:23.232 +01:00"),
                LastLogin = null,
                UserCreated = DateTime.Parse("2021-01-01 20:22:23.232"),
                UserActivated = null
            };

            // map through channel
            var chUser = mapper.Map(user, typeof(User), contracts[typeof(User)]);
            var resultUser = (User) mapper.Map(chUser, contracts[typeof(User)], typeof(User));

            // result check
            Assert.Equal(user.Id, resultUser.Id);
            Assert.Equal(user.Name, resultUser.Name);
            Assert.Equal(user.Status, resultUser.Status);
            Assert.Equal(user.Roles, resultUser.Roles);
            Assert.Equal(user.UserCreated, resultUser.UserCreated);
            Assert.Equal(user.UserActivated, resultUser.UserActivated);
            Assert.Equal(user.LocationTime, resultUser.LocationTime);
            Assert.Equal(user.LastLogin, resultUser.LastLogin);

        }

        private (IMapper, IReadOnlyDictionary<Type, Type>) CreateMapper()
        {
            var contractsAssemblies = new List<Assembly>() { typeof(DataMappingTest).Assembly };
            var cqrs = contractsAssemblies.SelectMany(CqrsInfoResolverUtil.GetCqrsDefinitions).ToList();
            var cqrsAdapter = new CqrsContractsAdapter(cqrs, "test");
            var mapper = cqrsAdapter.CreateMapper();
            return (mapper, cqrsAdapter.GetContracts());
        }

    }
}
