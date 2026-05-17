# Good and Bad Tests

## Good Tests

**Integration-style**: Test through real interfaces, not mocks of internal parts.

```csharp
// GOOD: Tests observable behavior
[Fact]
public async Task UserCanCheckoutWithValidCart()
{
    var cart = CreateCart();
    cart.Add(product);
    var result = await checkout.ExecuteAsync(cart, paymentMethod);
    result.Status.Should().Be(CheckoutStatus.Confirmed);
}
```

Characteristics:

- Tests behavior users/callers care about
- Uses public API only
- Survives internal refactors
- Describes WHAT, not HOW
- One logical assertion per test

## Bad Tests

**Implementation-detail tests**: Coupled to internal structure.

```csharp
// BAD: Tests implementation details
[Fact]
public async Task CheckoutCallsPaymentServiceProcess()
{
    var mockPayment = new Mock<IPaymentService>();
    await checkout.ExecuteAsync(cart, payment);
    mockPayment.Verify(p => p.ProcessAsync(cart.Total), Times.Once);
}
```

Red flags:

- Mocking internal collaborators
- Testing private methods
- Asserting on call counts/order
- Test breaks when refactoring without behavior change
- Test name describes HOW not WHAT
- Verifying through external means instead of interface

```csharp
// BAD: Bypasses interface to verify
[Fact]
public async Task CreateUserSavesToDatabase()
{
    await userService.CreateAsync(new CreateUserRequest { Name = "Alice" });
    var row = await dbContext.Users.FirstOrDefaultAsync(u => u.Name == "Alice");
    row.Should().NotBeNull();
}

// GOOD: Verifies through interface
[Fact]
public async Task CreateUserMakesUserRetrievable()
{
    var user = await userService.CreateAsync(new CreateUserRequest { Name = "Alice" });
    var retrieved = await userService.GetAsync(user.Id);
    retrieved.Name.Should().Be("Alice");
}
```
