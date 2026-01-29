using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models;
using Backend.Application.TodoItems.Queries.GetTodoItemsWithPagination;
using Backend.Application.TodoLists.Queries.GetTodos;
using Backend.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Backend.Application.UnitTests.Common.Mappings;

public class MappingTests
{
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public MappingTests()
    {
        var configExpression = new MapperConfigurationExpression();
        configExpression.AddMaps(Assembly.GetAssembly(typeof(IApplicationDbContext)));
        _configuration = new MapperConfiguration(configExpression, NullLoggerFactory.Instance);

        _mapper = _configuration.CreateMapper();
    }

    [Test]
    public void ShouldHaveValidConfiguration()
    {
        _configuration.AssertConfigurationIsValid();
    }

    [Test]
    [TestCase(typeof(TodoList), typeof(TodoListDto))]
    [TestCase(typeof(TodoItem), typeof(TodoItemDto))]
    [TestCase(typeof(TodoList), typeof(LookupDto))]
    [TestCase(typeof(TodoItem), typeof(LookupDto))]
    [TestCase(typeof(TodoItem), typeof(TodoItemBriefDto))]
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        var instance = GetInstanceOf(source);

        _mapper.Map(instance, source, destination);
    }

    private object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        // Type without parameterless constructor
        return RuntimeHelpers.GetUninitializedObject(type);
    }
}
