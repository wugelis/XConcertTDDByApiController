<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

async Task Main()
{
	//Func<OutsideAPIController, Task<IEnumerable<Person>>> expression = c => c.GetPersons();
	//RunAPIController.GetJSON<Person>(expression).Dump();

	Func<OutsideAPIController, Task<string>> expression2 = c => c.GetIdentityUser();
	//RunAPIController.GetJSON<string>(expression2).Dump();
	
	// Arrange
	FakeOutsideAPIController target = new FakeOutsideAPIController();
	string actual;
	string expected = JsonSerializer.Serialize("gelis");
	
	// Act
	actual = await target.GetIdentityUser(expression2);
	
	// Assert
	(actual == expected).Dump();
}

public class FakeOutsideAPIController
{
	public async Task<string> GetIdentityUser(Func<OutsideAPIController, Task<string>> expression)
	{
		return await RunAPIController.GetJSON<string>(expression);
	}
}

public class OutsideAPIController: OutsideBaseApiController
{
	private IUserService _userService;
	private IUriExtensions _uriExtensions;
	private IHttpContextAccessor _httpContextAccessor;
	
	public OutsideAPIController(
			ILogger<OutsideBaseApiController> logger,
			IUserService userService,
			IUriExtensions uriExtensions,
			IHttpContextAccessor httpContextAccessor)
	{
		_userService = userService;
		_uriExtensions = uriExtensions;
		_httpContextAccessor = httpContextAccessor;
	}

	[NeedAuthorize]
	[APIName("GetIdentityUser")]
	[ApiLogException]
	[ApiLogonInfo]
	public async Task<string> GetIdentityUser()
	{
		return await Task.FromResult(_userService.IdentityUser);
	}

	[NeedAuthorize]
	[APIName("GetPersons")]
	[ApiLogException]
	[ApiLogonInfo]
	public async Task<IEnumerable<Person>> GetPersons()
	{
		return await Task.FromResult(new Person[]
		{
				new Person()
				{
					ID = 1,
					Name = "Gelis Wu",
					Title = "資深.NET技術顧問",
					CreateDate = DateTime.Now
				}
		});
	}
}

public class OutsideBaseApiController { }
public interface IHttpContextAccessor { }
public interface IUriExtensions { }
public interface IUserService 
{
	string IdentityUser {get;}
}
public interface ILogger<T> { }

public class Logger<T> : ILogger<T> { }
public class HttpContextHandler : IHttpContextAccessor { }
public class UriExtension : IUriExtensions { }
public class UserService : IUserService
{
	private string _identityUser = "gelis";
	public string IdentityUser { get => _identityUser; private set => _identityUser = value; }
}
public class NeedAuthorize : Attribute { }
public class APIName : Attribute 
{
	public APIName(string methodName) {}
}
public class ApiLogException : Attribute { }
public class ApiLogonInfo : Attribute { }

public class Person
{
	public int ID { get; set; }
	public string Name { get; set; }
	public string Title { get; set; }
	public DateTime? CreateDate {get;set;}
}

public class RunAPIController
{
	public static async Task<string> GetJSON<T>(Func<OutsideAPIController, Task<IEnumerable<T>>> expression)
	{
		IEnumerable<T> result = await expression.Invoke(new OutsideAPIController(new Logger<OutsideBaseApiController>(), new UserService(), new UriExtension(), new HttpContextHandler()));
		var jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions() { WriteIndented = true});
		return jsonResult;
	}

	public static async Task<string> GetJSON<T>(Func<OutsideAPIController, Task<string>> expression)
	{
		var result = await expression.Invoke(new OutsideAPIController(new Logger<OutsideBaseApiController>(), new UserService(), new UriExtension(), new HttpContextHandler()));
		var jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions() { WriteIndented = true });
		return jsonResult;
	}
}