<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

async Task Main()
{
	//Func<OutsideAPIController, Task<IEnumerable<Person>>> expression = c => c.GetPersons();
	//await RunAPIController.GetJSON<Person>(expression).Dump();

	Func<OutsideAPIController, Task<string>> expression2 = c => c.GetIdentityUser();
	//await RunAPIController.GetJSON<string>(expression2).Dump();
	
	
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

#region 測試目標

public class OutsideAPIController : OutsideBaseApiController
{
	private readonly IUriExtensions _uriExtensions;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IUserService _userService;
	/// <summary>
	/// 
	/// </summary>
	/// <param name="logger"></param>
	/// <param name="userService"></param>
	/// <param name="uriExtensions"></param>
	/// <param name="httpContextAccessor"></param>
	public OutsideAPIController(
		ILogger<OutsideBaseApiController> logger,
		IUserService userService,
		IUriExtensions uriExtensions,
		IHttpContextAccessor httpContextAccessor)
		: base(logger, userService, httpContextAccessor)
	{
		_userService = userService;
		_uriExtensions = uriExtensions;
		_httpContextAccessor = httpContextAccessor;
	}

	/// <summary>
	/// 範例程式（需要驗證）
	/// </summary>
	/// <returns></returns>
	[NeedAuthorize]
	[HttpGet]
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
					Name = "Gelis Wu"
				}
		});
	}

	/// <summary>
	/// 取得 Current Identity Id
	/// </summary>
	/// <returns></returns>
	[NeedAuthorize]
	[APIName("GetIdentityId")]
	[ApiLogException]
	[ApiLogonInfo]
	public async Task<decimal?> GetIdentityId()
	{
		return await Task.FromResult(_userService.IdentityId);
	}

	/// <summary>
	/// 取得 Current Identity Id
	/// </summary>
	/// <returns></returns>
	[NeedAuthorize]
	[APIName("GetIdentityUser")]
	[ApiLogException]
	[ApiLogonInfo]
	public async Task<string> GetIdentityUser()
	{
		return await Task.FromResult(_userService.IdentityUser);
	}
}

#endregion

public class OutsideBaseApiController
{
	public OutsideBaseApiController() {}
	public OutsideBaseApiController(ILogger<OutsideBaseApiController> logger, IUserService userService, IHttpContextAccessor httpContextAccessor) {}
}
public interface IHttpContextAccessor { }
public interface IUriExtensions { }
public interface IUserService 
{
	string IdentityUser { get; }
	decimal? IdentityId { get; }
}
public interface ILogger<T> { }

public class Logger<T> : ILogger<T> { }
public class HttpContextHandler : IHttpContextAccessor { }
public class UriExtension : IUriExtensions { }
public class UserService : IUserService
{
	private string _identityUser = "gelis";
	public string IdentityUser { get => _identityUser; private set => _identityUser = value; }

	public decimal? _identityId;
	public decimal? IdentityId { get => _identityId; private set => _identityId = value; }
}
public class NeedAuthorize : Attribute { }
public class APIName : Attribute 
{
	public APIName(string methodName) {}
}
public class ApiLogException : Attribute { }
public class ApiLogonInfo : Attribute { }
public class HttpGet : Attribute { }

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