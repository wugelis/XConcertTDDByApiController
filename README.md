# 在 LINQPad 上寫單元測試

上一次的 LINQPad 驅動開發，小弟粗略的展示了在 LINQPad 下，完全沒有任何外部參考的情況下測試一個 ApiController 的方式，而讀者們如果有仔細看清楚的話，應該會發現這個 ApiController 就是目前最新文章中所使用的 API Framework V2裡使用的 OutsideBaseApiController 

在這個範例裡面，再展示一個如何手刻一個 FakeOutsideAPIController 物件以便達成 Isolation Unit Test ApiController 的目的。

#測試程式碼可以撰寫如下：

	// Arrange
	Func<OutsideAPIController, Task<string>> expression2 = c => c.GetIdentityUser();
	string expected = "\"gelis\"";
	string actual;
	
	// Act
	actual = await RunAPIController.GetJSON<string>(expression2);
	
	// Assert
	(actual == expected).Dump();