using AutoFixture;
using Moq;
using ToDoList.Application.DTOs.Tag;
using ToDoList.Application.Services.Implementations;
using ToDoList.Application.Services.Mapping;
using ToDoList.Domain.Entities;
using ToDoList.Domain.Interfaces;
using Xunit;
using FluentAssertions; 
using Task = System.Threading.Tasks.Task;

namespace ToDoList.Tests;

public class TagServiceTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<ITagMapper> _tagMapperMock;
    private readonly TagService _tagService;
    private readonly Fixture _fixture;

    public TagServiceTests()
    {
        _tagRepositoryMock = new Mock<ITagRepository>();
        _tagMapperMock = new Mock<ITagMapper>();
        _tagService = new TagService(
            _tagRepositoryMock.Object,
            _tagMapperMock.Object
        );
        _fixture = new Fixture(); 
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

    }

    [Fact]
    public async Task GetTagByIdAsync_ShouldReturnNull_WhenTagNotFound()
    {
        // Arrange
        int tagId = _fixture.Create<int>();
        _tagRepositoryMock.Setup(repo => repo.GetByIdAsync(tagId))
            .ReturnsAsync((Tag?)null);
        
        // Act
        var result = await _tagService.GetTagByIdAsync(tagId);
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllTagsAsync_ShouldReturnAllTags()
    {
        // Arrange
        var tagList = _fixture.CreateMany<Tag>(2).ToList();

        _tagRepositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(tagList);
        _tagMapperMock.Setup(mapper => mapper.MapToGetDto(It.IsAny<Tag>()))
            .Returns((Tag tag) => new TagGetDto { Id = tag.Id, Name = tag.Name });
        
        // Act
        var result = await _tagService.GetAllTagsAsync();

        // Assert
        result.Should().NotBeNull()
              .And.HaveCount(tagList.Count);
    }
    
    [Fact]
    public async Task CreateTagAsync_ShouldReturnTag_WhenTagCreated()
    {
        // Arrange
        var tagCreateDto = _fixture.Create<TagCreateDto>();

        var existingTag = (Tag?)null; // No existing tag
        var tagEntity = _fixture.Build<Tag>()
                                .With(t => t.Id, 1)
                                .With(t => t.Name, tagCreateDto.Name)
                                .Create();
        var tagGetDto = new TagGetDto { Id = 1, Name = tagEntity.Name };

        _tagRepositoryMock.Setup(repo => repo.GetByNameAsync(tagCreateDto.Name))
                          .ReturnsAsync(existingTag);
        _tagMapperMock.Setup(mapper => mapper.MapToEntity(tagCreateDto))
                      .Returns(tagEntity);
        _tagRepositoryMock.Setup(repo => repo.AddAsync(tagEntity));
        _tagMapperMock.Setup(mapper => mapper.MapToGetDto(tagEntity))
                      .Returns(tagGetDto);

        // Act
        var result = await _tagService.CreateTagAsync(tagCreateDto);

        // Assert
        result.Should().NotBeNull()
              .And.Match<TagGetDto>(x => x.Name == tagCreateDto.Name);
    }

    [Fact]
    public async Task CreateTagAsync_ShouldReturnNull_WhenTagAlreadyExists()
    {
        // Arrange
        var tagCreateDto = new TagCreateDto
        {
            Name = "Existing Tag"
        };

        var existingTag = new Tag { Id = 1, Name = "Existing Tag" };
        _tagRepositoryMock.Setup(repo => repo.GetByNameAsync(tagCreateDto.Name))
                          .ReturnsAsync(existingTag);

        // Act
        var result = await _tagService.CreateTagAsync(tagCreateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTagAsync_ShouldReturnTrue_WhenTagUpdated()
    {
        // Arrange
        int tagId = 1;
        var tagUpdateDto = new TagUpdateDto
        {
            Name = "Updated Tag"
        };

        var existingTag = new Tag { Id = tagId, Name = "Old Tag" };

        _tagRepositoryMock.Setup(repo => repo.GetByIdAsync(tagId))
                          .ReturnsAsync(existingTag);
        _tagMapperMock.Setup(mapper => mapper.MapToEntity(tagUpdateDto, existingTag))
                      .Returns(existingTag);
        _tagRepositoryMock.Setup(repo => repo.UpdateAsync(existingTag))
                          .Returns(Task.CompletedTask);

        // Act
        var result = await _tagService.UpdateTagAsync(tagId, tagUpdateDto);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTagAsync_ShouldReturnFalse_WhenTagNotFound()
    {
        // Arrange
        int tagId = 1;
        var tagUpdateDto = new TagUpdateDto
        {
            Name = "Updated Tag"
        };

        _tagRepositoryMock.Setup(repo => repo.GetByIdAsync(tagId))
                          .ReturnsAsync((Tag?)null);

        // Act
        var result = await _tagService.UpdateTagAsync(tagId, tagUpdateDto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteTagAsync_ShouldReturnTrue_WhenTagDeleted()
    {
        // Arrange
        int tagId = 1;
        var existingTag = new Tag { Id = tagId, Name = "Tag to Delete" };

        _tagRepositoryMock.Setup(repo => repo.GetByIdAsync(tagId))
                          .ReturnsAsync(existingTag);
        _tagRepositoryMock.Setup(repo => repo.DeleteAsync(existingTag))
                          .Returns(Task.CompletedTask);

        // Act
        var result = await _tagService.DeleteTagAsync(tagId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTagAsync_ShouldReturnFalse_WhenTagNotFound()
    {
        // Arrange
        int tagId = 1;
        _tagRepositoryMock.Setup(repo => repo.GetByIdAsync(tagId))
                          .ReturnsAsync((Tag?)null);

        // Act
        var result = await _tagService.DeleteTagAsync(tagId);

        // Assert
        result.Should().BeFalse();
    }
}
