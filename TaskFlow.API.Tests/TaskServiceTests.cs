using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskFlow.API.Data;
using TaskFlow.API.DTOs;
using TaskFlow.API.Models;
using TaskFlow.API.Services;
using Xunit;


namespace TaskFlow.API.Tests
{
    public class TaskServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetTasksByUserIdAsync_ReturnsOnlyUserTasks()
        {
            using var context = GetInMemoryDbContext("TaskDb1");

            var user1 = new User { Id = 1, Email = "user1@example.com" };
            var user2 = new User { Id = 2, Email = "user2@example.com" };

            context.Users.AddRange(user1, user2);

            context.Tasks.AddRange(
                new TaskItem { Id = 1, Title = "Task1", UserId = 1 },
                new TaskItem { Id = 2, Title = "Task2", UserId = 1 },
                new TaskItem { Id = 3, Title = "Task3", UserId = 2 }
            );

            await context.SaveChangesAsync();

            var service = new TaskService(context);

            var tasks = await service.GetTasksByUserIdAsync(1);

            tasks.Should().HaveCount(2);
            tasks.All(t => t.UserId == 1).Should().BeTrue();
        }

        [Fact]
        public async Task AddTaskAsync_SavesTaskToDatabase()
        {
            using var context = GetInMemoryDbContext("TaskDb2");

            var user = new User { Id = 1, Email = "user@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new TaskService(context);

            var taskDto = new TaskCreateDto { Title = "New Task", Description = "Desc", UserId = user.Id };

            await service.CreateTaskAsync(taskDto);

            context.Tasks.Should().ContainSingle(t => t.Title == "New Task" && t.UserId == 1);
        }

        [Fact]
        public async Task DeleteTaskAsync_RemovesTaskFromDatabase()
        {
            using var context = GetInMemoryDbContext("TaskDb3");

            var task = new TaskItem { Id = 1, Title = "Task to delete", UserId = 1 };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var service = new TaskService(context);

            await service.DeleteTaskAsync(1);

            context.Tasks.Should().BeEmpty();
        }
    }
}

//namespace TaskFlow.API.Tests
//{
//    public class TaskServiceTests
//    {
//        [Fact]
//        public async Task CreateTaskAsync_ValidDto_CreatesAndReturnsTask()
//        {
//            var mockContext = new Mock<ApplicationDbContext>();
//            var tasks = new List<TaskItem>().AsQueryable();
//            //var mockTasks = CreateMockDbSet(tasks);

//            var mockTasks = new Mock<DbSet<TaskItem>>();
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.Provider).Returns(tasks.Provider);
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.Expression).Returns(tasks.Expression);
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.ElementType).Returns(tasks.ElementType);
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.GetEnumerator()).Returns(tasks.GetEnumerator());


//            mockContext.Setup(c => c.Tasks).Returns(mockTasks.Object);
//            mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

//            var service = new TaskService(mockContext.Object);
//            var dto = new TaskCreateDto { Title = "Test Task", Description = "TestDesc" };

//            var result = await service.CreateTaskAsync(dto, userId: 1);

//            result.Should().NotBeNull();
//            result.Title.Should().Be("Test Task");
//            result.UserId.Should().Be(0);
//        }

//        [Fact]
//        public async Task GetTaskAsync_ExistingTasks_ReturnsFilteredTasks()
//        {
//            var tasks = new List<TaskItem>
//            {
//                new TaskItem {Id = 1, UserId = 1, Title = "Task 1", IsCompleted = false},
//                new TaskItem {Id = 2, UserId = 1, Title = "Task 2", IsCompleted = true},
//                new TaskItem {Id = 3, UserId = 2, Title = "Other user task", IsCompleted = false},
//            }.AsQueryable();

//            var mockContext = new Mock<ApplicationDbContext>();
//            //var mockTasks = CreateMockDbSet(tasks);

//            var mockTasks = new Mock<DbSet<TaskItem>>();
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.Provider).Returns(tasks.Provider);
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.Expression).Returns(tasks.Expression);
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.ElementType).Returns(tasks.ElementType);
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.GetEnumerator()).Returns(tasks.GetEnumerator());



//            mockContext.Setup(c => c.Tasks).Returns(mockTasks.Object);

//            var service = new TaskService(mockContext.Object);

//            var result = await service.GetTasksAsync(userId: 1, isCompleted: false);

//            result.Should().HaveCount(1);
//            result.First().Title.Should().Be("Task 1");
//        }

//        [Fact]
//        public async Task DeleteTaskAsync_OwnTask_DeletesAndReturnsTrue()
//        {
//            var tasks = new List<TaskItem>
//            {
//                new TaskItem { Id = 1, UserId = 1, Title = "My Task"}
//            }.AsQueryable();

//            var mockContext = new Mock<ApplicationDbContext>();
//            //var mockTasks = CreateMockDbSet(tasks);

//            var mockTasks = new Mock<DbSet<TaskItem>>();
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.Provider).Returns(tasks.Provider);
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.Expression).Returns(tasks.Expression);
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.ElementType).Returns(tasks.ElementType);
//            mockTasks.As<IQueryable<TaskItem>>().Setup(m => m.GetEnumerator()).Returns(tasks.GetEnumerator());


//            mockContext.Setup(c => c.Tasks).Returns(mockTasks.Object);
//            mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

//            var service = new TaskService(mockContext.Object);

//            var result = await service.DeleteTaskAsync(id: 1, userId: 1);

//            result.Should().BeTrue();
//            mockContext.Verify(c => c.Tasks.Remove(It.IsAny<TaskItem>()), Times.Once);
//        }

//        //private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
//        //{
//        //    var mockSet = new Mock<DbSet<T>>();
//        //    mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
//        //    mockSet.As<IQueryable<T>>().Setup(m => m.Expressiom).Returns(data.Expression);
//        //    mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
//        //    mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
//        //    return mockSet;
//        //}
//    }
//}