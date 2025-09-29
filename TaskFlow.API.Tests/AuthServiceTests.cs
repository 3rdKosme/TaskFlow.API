using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using TaskFlow.API.Data;
using TaskFlow.API.DTOs;
using TaskFlow.API.Models;
using TaskFlow.API.Services;


namespace TaskFlow.API.Tests

{
    public class AuthServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private IConfiguration GetTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "this_is_my_super_secret_jwt_key_must_be_at_least_32_chars_long" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task RegisterAsync_WithNewEmail_CreatesUserAndReturnsToken()
        {
            using var context = GetInMemoryDbContext("RegisterDb");
            var config = GetTestConfiguration();
            var service = new AuthService(context, config);

            var registerDto = new RegisterDto { Email = "test@example.com", Password = "password123" };

            var token = await service.RegisterAsync(registerDto);

            token.Should().NotBeNullOrEmpty();
            context.Users.Should().ContainSingle(u => u.Email == "test@example.com");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsToken()
        {
            using var context = GetInMemoryDbContext("LoginDb");
            var config = GetTestConfiguration();

            var user = new User
            {
                Email = "test@example.com"
            };
            user.PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>()
                .HashPassword(user, "password123");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new AuthService(context, config);
            var loginDto = new LoginDto { Email = "test@example.com", Password = "password123" };

            var token = await service.LoginAsync(loginDto);

            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsException()
        {
            using var context = GetInMemoryDbContext("InvalidPasswordDb");
            var config = GetTestConfiguration();

            var user = new User
            {
                Email = "test@example.com"
            };
            user.PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>()
                .HashPassword(user, "correct_password");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new AuthService(context, config);
            var loginDto = new LoginDto { Email = "test@example.com", Password = "wrong_password" };

            Func<Task> act = async () => await service.LoginAsync(loginDto);

            await act.Should().ThrowAsync<Exception>().WithMessage("Incorrect Email or password.");
        }
    }
}

//{
//    public class AuthServiceTests
//    {
//        [Fact]
//        public async Task RegisterAsync_WithNewEmail_CreatesUserAndReturnsToken()
//        {
//            var mockContext = new Mock<ApplicationDbContext>();
//            var mockConfig = new Mock<IConfiguration>();
//            mockConfig.SetupGet(x => x["Jwt:Key"]).Returns("this_is_my_super_secret_jwt_key_must_be_at_least_32_chars_long");

//            var users = new List<User>().AsQueryable();
//            var mockUsers = new Mock<DbSet<User>>();
//            mockUsers.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
//            mockUsers.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
//            mockUsers.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
//            mockUsers.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

//            mockContext.Setup(c => c.Users).Returns(mockUsers.Object);
//            mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

//            var service = new AuthService(mockContext.Object, mockConfig.Object);

//            var registerDto = new RegisterDto { Email = "test@example.com", Password = "password123" };

//            var token = await service.RegisterAsync(registerDto);

//            token.Should().NotBeNullOrEmpty();
//            mockContext.Verify(c => c.Users.Add(It.IsAny<User>()), Times.Once);
//            mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
//        }

//        [Fact]
//        public async Task LoginAsync_WithValidCredentials_ReturnsToken()
//        {
//            var mockContext = new Mock<ApplicationDbContext>();
//            var mockConfig = new Mock<IConfiguration>();
//            mockConfig.SetupGet(x => x["Jwt:Key"]).Returns("this_is_my_super_secret_jwt_key_must_be_at_least_32_chars_long");

//            var user = new User
//            {
//                Id = 1,
//                Email = "test@example.com",
//                PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>().HashPassword(new User(), "password123")
//            };

//            var users = new List<User> { user }.AsQueryable();
//            var mockUsers = new Mock<DbSet<User>>();
//            mockUsers.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
//            mockUsers.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
//            mockUsers.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
//            mockUsers.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

//            mockContext.Setup(c => c.Users).Returns(mockUsers.Object);

//            var service = new AuthService(mockContext.Object, mockConfig.Object);
//            var loginDto = new LoginDto { Email = "test@example.com", Password = "password123" };

//            var token = await service.LoginAsync(loginDto);

//            token.Should().NotBeNullOrEmpty();
//        }

//        [Fact]
//        public async Task LoginAsync_WithInvalidPassword_ThrowsException()
//        {
//            var mockContext = new Mock<ApplicationDbContext>();
//            var mockConfig = new Mock<IConfiguration>();
//            mockConfig.SetupGet(x => x["Jwt:Key"]).Returns("this_is_my_super_secret_jwt_key_must_be_at_least_32_chars_long");

//            var user = new User
//            {
//                Id = 1,
//                Email = "test@example.com",
//                PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>().HashPassword(new User(), "correct_password")
//            };

//            var users = new List<User> { user }.AsQueryable();
//            var mockUsers = new Mock<DbSet<User>>();
//            mockUsers.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
//            mockUsers.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
//            mockUsers.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
//            mockUsers.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

//            mockContext.Setup(c => c.Users).Returns(mockUsers.Object);

//            var service = new AuthService(mockContext.Object, mockConfig.Object);
//            var loginDto = new LoginDto { Email = "test@example.com", Password = "wrong_password" };

//            Func<Task> act = async () => await service.LoginAsync(loginDto);

//            await act.Should().ThrowAsync<Exception>().WithMessage("Incorrect Email or password.");

//        }
//    }
//}